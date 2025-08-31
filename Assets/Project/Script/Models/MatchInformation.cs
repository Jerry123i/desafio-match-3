using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Gazeus.DesafioMatch3.Models
{
    public enum Direction { Horizontal, Vertical }
    
    public class MatchInformation
    {
        public Direction Direction { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int Length { get; set; }
        public int TileType { get; set; }

        public MatchInformation(Direction direction, int x, int y, int length, int type)
        {
            this.Direction = direction;
            this.x = x;
            this.y = y;
            this.Length = length;
            this.TileType = type;
        }

        public override string ToString()
        {
            return $"{Direction} ({x},{y}) Length:{Length}";
        }
    }

    public static class MatchInformationFunctions
    {
        public static void AddAndCombine(this List<MatchInformation> list, MatchInformation newInfo)
        {
            var match = list.Find(listItem =>
            {
                bool sameType = listItem.TileType == newInfo.TileType;
                bool sameDirection = listItem.Direction == newInfo.Direction;
                bool aligned;
                bool overlap;

                if (newInfo.Direction == Direction.Horizontal)
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
            
            if (newInfo.Direction == Direction.Horizontal)
                match.Length = newInfo.x + newInfo.Length - match.x;
            else
                match.Length = newInfo.y + newInfo.Length - match.y;

        }

        public static void MarkMatches(this List<List<bool>> tileBoard, List<MatchInformation> matches)
        {
            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                for (int j = 0; j < match.Length; j++)
                {
                    if (match.Direction == Direction.Horizontal)
                        tileBoard[match.y][match.x + j] = true;
                    else
                        tileBoard[match.y + j][match.x] = true;
                }
                
            }
        }
        
    }
    
}
