

using System;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;

namespace Gazeus.DesafioMatch3.Core
{
    public static class MatchInfoListFunctions 
    {
        public static void AddAndCombine(this List<MatchInformation> list, MatchInformation newInfo)
        {
            if (newInfo.MatchType is MatchType.Square or MatchType.Pattern)
            {
                list.Add(newInfo);
                return;
            }
            
            var match = list.Find(listItem =>
            {
                bool sameType = listItem.TileType == newInfo.TileType;
                bool sameDirection = listItem.MatchType == newInfo.MatchType;
                bool aligned;
                bool overlap;

                if (newInfo.MatchType == MatchType.Horizontal)
                {
                    aligned = listItem.y == newInfo.y;
                    overlap = listItem.x + listItem.Length >= newInfo.x;
                }
                else
                {
                    aligned = listItem.x == newInfo.x;
                    overlap = listItem.y + listItem.Length >= newInfo.y;
                }

                return sameType && aligned && overlap && sameDirection;

            });

            if (match == null)
            {
                list.Add(newInfo);
                return;
            }
            
            if (newInfo.MatchType == MatchType.Horizontal)
                match.Length = newInfo.x + newInfo.Length - match.x;
            else
                match.Length = newInfo.y + newInfo.Length - match.y;

        }

        public static void MarkMatches(this Table<bool> tileBoard, List<MatchInformation> matches)
        {
            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];

                switch (match.MatchType)
                {
                    case MatchType.Horizontal:
                    case MatchType.Vertical:
                        MarkLinearMatch(tileBoard,match);
                        break;
                    case MatchType.Square:
                        MarkSquareMatch(tileBoard, match);
                        break;
                    case MatchType.Pattern:
                        MarkPattern(tileBoard,match);
                        break;
                }
                
            }
        }

        private static void MarkPattern(this Table<bool> tileBoard, MatchInformation pattern)
        {
            for (int j = 0; j < pattern.PatternCoordinates.Count; j++)
            {
                var coordinate = pattern.PatternCoordinates[j];
                tileBoard[coordinate.x, coordinate.y] = true;
            }
        }

        private static void MarkLinearMatch(this Table<bool> tileBoard, MatchInformation info)
        {
            for (int l = 0; l < info.Length; l++)
            {
                switch (info.MatchType)
                {
                    case MatchType.Horizontal:
                        tileBoard[info.x + l, info.y] = true;
                        break;
                    case MatchType.Vertical:
                        tileBoard[info.x, info.y + l] = true;
                        break;
                }
            }
        }

        private static void MarkSquareMatch(this Table<bool> tileBoard, MatchInformation info)
        {
            for (int l = 0; l < info.Length; l++)
                for (int k = 0; k < info.Length; k++)
                    tileBoard[info.x - l, info.y - k] = true;
        }
        
    }
}
