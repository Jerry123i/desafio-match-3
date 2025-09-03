using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    public static class MatchInformationListFunctions
    {
        public static void TryCombineLast(this List<MatchInformation> list)
        {
            if(list.Count <= 1)
                return;

            LinearMatch last = (LinearMatch)list.Last();
            
            if(last.MatchType != MatchType.Linear)
                return;
            
            var match = list.Find(i =>
            {
                if (i == last)
                    return false;

                if (i.MatchType != MatchType.Linear)
                    return false;

                LinearMatch item = (LinearMatch)i;
                
                bool sameDirection = item.Direction == last.Direction;
                bool aligned;
                bool overlap;
            
                if (last.Direction == Direction.Horizontal)
                {
                    aligned = item.y == last.y;
                    overlap = item.x + item.Length >= last.x;
                }
                else
                {
                    aligned = item.x == last.x;
                    overlap = item.y + item.Length >= last.y;
                }
            
                return aligned && overlap && sameDirection;
            
            });
            
            if (match == null)
                return;

            LinearMatch matchTypeCast = (LinearMatch)match;
            
            if (matchTypeCast.Direction == Direction.Horizontal)
                matchTypeCast.Length = last.x + last.Length - matchTypeCast.x;
            else
                matchTypeCast.Length = last.y + last.Length - matchTypeCast.y;
        }

        public static void MarkMatches(this Table<bool> tileBoard, List<MatchInformation> matches)
        {
            if(matches == null)
                return;

            for (int i = 0; i < matches.Count; i++)
            {
                MatchInformation match = matches[i];

                switch (match.MatchType)
                {
                    case MatchType.Linear:
                        tileBoard.MarkMatchesLinear((LinearMatch)match);
                        break;
                    case MatchType.Square:
                        tileBoard.MarkMatchesSquare((SquareMatch)match);
                        break;
                    case MatchType.Pattern:
                        tileBoard.MarkMatchesPattern((PatternMatch)match);
                        break;
                }
            }
            
        }
        
        private static void MarkMatchesLinear(this Table<bool> tileBoard, LinearMatch match)
        {
            
            for (int l = 0; l < match.Length; l++)
            {
                switch (match.Direction)
                {
                    case Direction.Horizontal:
                        tileBoard[match.x + l , match.y] = true;
                        break;
                    case Direction.Vertical:
                        tileBoard[match.x,match.y + l] = true;
                        break;
                }
                    
            }
        }
        
        private static void MarkMatchesSquare(this Table<bool> tileBoard, SquareMatch match)
        {
            
            for (int l = 0; l < match.Size; l++)
                for (int k = 0; k < match.Size; k++)
                    tileBoard[match.x - l, match.y - k] = true;
        }
        
        private static void MarkMatchesPattern(this Table<bool> tileBoard, PatternMatch match)
        {
            for (int j = 0; j < match.Coordinates.Count; j++)
            {
                var coordinate = match.Coordinates[j];
                tileBoard[coordinate.x, coordinate.y] = true;
            }
        }
        
    }
}
