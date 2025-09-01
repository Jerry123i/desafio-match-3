namespace Gazeus.DesafioMatch3.Models
{
    public enum TileType {Blue, Green, Orange, Yellow, Pink, Purple, Red, Gray}
    
    public class Tile
    {
        public int Id { get; set; }
        public int Type { get; set; }

        public override string ToString()
        {
            switch ((TileType)Type)
            {
                case TileType.Blue:
                    return $"<color=#0000ff>Blue</color> {Id}";
                case TileType.Green:
                    return $"<color=#00ff00>Green</color> {Id}";
                case TileType.Orange:
                    return $"<color=#ff8c00>Orange</color> {Id}";
                case TileType.Yellow:
                    return $"<color=#ffff00>Yellow</color> {Id}";
                case TileType.Pink:
                    return $"<color=#ff69b4>Pink</color> {Id}";
                case TileType.Purple:
                    return $"<color=#800080>Purple</color> {Id}";
                case TileType.Red:
                    return $"<color=#ff0000>Red</color> {Id}";
                case TileType.Gray:
                    return $"<color=#555555>Gray</color> {Id}";
                default:
                    return $"Tile {Id}";
            }
        }
    }
}
