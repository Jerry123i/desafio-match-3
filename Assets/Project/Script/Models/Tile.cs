namespace Gazeus.DesafioMatch3.Models
{
    public class Tile
    {
        public int Id { get; set; }
        public int Type { get; set; }

        public override string ToString()
        {
            switch (Type)
            {
                case 0:
                    return $"<color=#0000ff>Blue</color> {Id}";
                case 1:
                    return $"<color=#00ff00>Green</color> {Id}";
                case 2:
                    return $"<color=#ff8c00>Orange</color> {Id}";
                case 3:
                    return $"<color=#ffff00>Yellow</color> {Id}";
                case 4:
                    return $"<color=#ff69b4>Pink</color> {Id}";
                case 5:
                    return $"<color=#800080>Purple</color> {Id}";
                case 6:
                    return $"<color=#ff0000>Red</color> {Id}";
                default:
                    return $"Tile {Id}";
            }
        }
    }
}
