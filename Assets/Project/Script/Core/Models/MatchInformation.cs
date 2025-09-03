using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Gazeus.DesafioMatch3.Models
{
    public enum MatchType { Linear , Square, Pattern}
    public enum Direction {Horizontal, Vertical }

    public abstract class MatchInformation
    {
        public MatchType MatchType { get; set; }
    }
    
    public class LinearMatch : MatchInformation
    {
        public int x { get; set; }
        public int y { get; set; }
        public int Length { get; set; }
        public int TileType { get; set; }
        
        public Direction Direction { get; set; }
        public LinearMatch(Direction direction, int x, int y, int length, int type)
        {
            this.MatchType = MatchType.Linear;
            this.Direction = direction;
            this.x = x;
            this.y = y;
            this.Length = length;
            this.TileType = type;
        }
        
    }

    public class SquareMatch : MatchInformation
    {
        public int x { get; set; }
        public int y { get; set; }
        public int Size { get; set; }
        public int TileType { get; set; }

        public SquareMatch(int x, int y, int size, int type)
        {
            this.MatchType = MatchType.Square;
            this.x = x;
            this.y = y;
            this.Size = size;
            this.TileType = type;
        }
        
    }

    public class PatternMatch : MatchInformation
    {
        public List<Vector2Int> Coordinates { get; set; } 
        
        public PatternMatch(List<Vector2Int> coordinates)
        {
            this.MatchType = MatchType.Pattern;
            this.Coordinates = coordinates;
        }
        
    }
    
}
