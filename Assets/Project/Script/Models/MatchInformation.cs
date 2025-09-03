using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Gazeus.DesafioMatch3.Models
{
    public enum Direction { Horizontal, Vertical , Square, Special}
    
    public class MatchInformation
    {
        public Direction Direction { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int Length { get; set; }
        public int TileType { get; set; }
        
        public List<Vector2Int> SpecialMatchCoordinates { get; set; } //TODO Find another solution for this

        public MatchInformation(Direction direction, int x, int y, int length, int type)
        {
            this.Direction = direction;
            this.x = x;
            this.y = y;
            this.Length = length;
            this.TileType = type;
        }

        public MatchInformation(List<Vector2Int> coordinates)
        {
            this.Direction = Direction.Special;
            SpecialMatchCoordinates = coordinates;
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
            if (newInfo.Direction == Direction.Square || newInfo.Direction == Direction.Special)
            {
                list.Add(newInfo);
                return;
            }
            
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

        public static void MarkMatches(this Table<bool> tileBoard, List<MatchInformation> matches) //TODO add squares here
        {
            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];

                if (match.Direction == Direction.Special)
                {
                    for (int j = 0; j < match.SpecialMatchCoordinates.Count; j++)
                    {
                        var coordinate = match.SpecialMatchCoordinates[j];
                        tileBoard[coordinate.x, coordinate.y] = true;
                    }
                    continue;
                }
                
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
                        case Direction.Square:
                            for (int k = 0; k < match.Length; k++)
                                tileBoard[match.x - l, match.y - k] = true;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                }
                
            }
        }
        
    }
    
}
