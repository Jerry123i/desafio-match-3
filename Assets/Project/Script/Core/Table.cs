namespace Gazeus.DesafioMatch3
{
    public class Table<T>
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        private T[] array;
        
        public Table(int width, int height)
        {
            Width = width;
            Height = height;

            array = new T[width*height];
        }

        public T Get(int x, int y)
        {
            return array[y * Width + x];
        }

        public void Set( T value, int x, int y)
        {
            if(x>=Width)
                return;
            if(y>= Height)
                return;

            array[y * Width + x] = value;
        }
        
    }
}
