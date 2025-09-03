using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Gazeus.DesafioMatch3.Models
{
    public enum MatchType { Horizontal, Vertical , Square, Pattern}
    
    public class MatchInformation
    {
        public MatchType MatchType { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int Length { get; set; }
        public int TileType { get; set; }
        
        public List<Vector2Int> PatternCoordinates { get; set; } 

        public MatchInformation(MatchType matchType, int x, int y, int length, int type)
        {
            this.MatchType = matchType;
            this.x = x;
            this.y = y;
            this.Length = length;
            this.TileType = type;
        }

        public MatchInformation(List<Vector2Int> coordinates)
        {
            this.MatchType = MatchType.Pattern;
            PatternCoordinates = coordinates;
        }

        public override string ToString()
        {
            return $"{MatchType} ({x},{y}) Length:{Length}";
        }
    }
    
}
