using System.Collections;
using System.Collections.Generic;
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
        
    }

    public static class MatchInformationFunctions
    {
        public static void AddAndCombine(this List<MatchInformation> list, MatchInformation newInfo)
        {
            var match = list.Find(listItem =>
            {
                bool sameType = listItem.TileType == newInfo.TileType;
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

                return sameType && aligned && overlap;

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
    }
    
}
