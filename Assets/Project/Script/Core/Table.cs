using System;

namespace Gazeus.DesafioMatch3
{
    public class Table<T>
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        private T[] array;
        
        public T this[int x, int y]
        {
            get => array[y * Width + x];
            set
            {
                if(x>=Width)
                    return;
                if(y>=Height)
                    return;
                array[y * Width + x] = value;
            }
        }

        public Table(int width, int height)
        {
            Width = width;
            Height = height;

            array = new T[width*height];
        }
        
        public static Table<T1> Clone<T1>(Table<T1> tableToClone) where T1 : ICloneable
        {
            var newTable = new Table<T1>(tableToClone.Width, tableToClone.Height);

            for (int i = 0; i < tableToClone.array.Length; i++)
            {
                newTable.array[i] = (T1)tableToClone.array[i].Clone();
            }

            return newTable;

        }
        
    }
}
