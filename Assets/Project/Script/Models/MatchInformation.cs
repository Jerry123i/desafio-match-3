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

        public static void AddAndCombine(this List<MatchInformation> list, MatchInformation info)
        {
            var match = list.Find(listItem =>
            {
                bool rightType = listItem.TileType == info.TileType;
                bool aligned;
                bool overlap;

                if (info.Direction == Direction.Horizontal)
                {
                    aligned = listItem.y == info.y;
                    overlap = listItem.x + listItem.Length >= info.x;
                }
                else
                {
                    aligned = listItem.x == info.x;
                    overlap = listItem.y + listItem.Length >= info.y;
                }

                return rightType && aligned && overlap;

            });

            if (match == null)
            {
                list.Add(info);
                return;
            }
            
            if (info.Direction == Direction.Horizontal)
                match.Length = info.x + info.Length - match.x;
            else
                match.Length = info.y + info.Length - match.y;

        }
    }
    
}
