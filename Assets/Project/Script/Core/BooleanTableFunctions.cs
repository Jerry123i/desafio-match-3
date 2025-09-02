
using System.Collections.Generic;
using System.Linq;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3
{
    public static class BooleanTableFunctions 
    {
        public static List<List<bool>> CombineBooleanTables(List<List<bool>> listA, List<List<bool>> listB)
        {
            List<List<bool>> newTable = new();
            for (int y = 0; y < listA.Count; y++)
            {
                newTable.Add(new List<bool>(listA[y].Count));
                for (int x = 0; x < listA.Count; x++)
                {
                    bool value;
                    
                    if(listB.Count<y || listB[y].Count < x)
                        value = listA[y][x];
                    else
                        value = listA[y][x] || listB[y][x];
                    
                    newTable[y].Add(value);
                }
            }

            return newTable;
        }
        
        public static void MarkLine(this Table<bool> table, int line)
        {
            for (int i = 0; i < table.Width; i++)
            {
                table[i,line] = true;
            }
        }

        public static void MarkColumn(this Table<bool> table, int column)
        {
            for (int i = 0; i < table.Height; i++)
            {
                table[column,i] = true;
            }
        }

        public static void MarkRadius(this Table<bool> table, int centerX, int centerY, int radius)
        {
            Vector2Int centerVector = new Vector2Int(centerX, centerY);
            
            for (int y = 0; y < table.Height; y++)
            {
                for (int x = 0; x < table.Width; x++)
                {
                    var dist = (new Vector2Int(x, y) - centerVector).magnitude;
                    if (dist <= radius)
                        table[x,y] = true;
                }
            }

        }

        public static void MarkSameType(this Table<bool> table, Table<Tile> referenceBoard, int type)
        {
            for (int y = 0; y < referenceBoard.Height; y++)
            {
                for (int x = 0; x < referenceBoard.Width; x++)
                {
                    if (referenceBoard[x,y].Type == type)
                        table[x,y] = true;
                }
            }
        }

        //Effect for a specific item use
        public static void MarkEarthquakePattern(this Table<bool> table)
        {
            int minSpike = 1;
            int maxSpike = 4;

            List<int> spikes = new List<int>();

            for (int i = 0; i < table.Width; i++)
            {
                int value = Random.Range(minSpike, maxSpike + 1);

                if (i == 0)
                {
                    spikes.Add(value);
                    continue;
                }
                    
                    
                while (value==spikes[i-1])
                    value = Random.Range(minSpike, maxSpike + 1);
                
                spikes.Add(value);
            }

            for (int y = 0; y < table.Height; y++)
            {
                for (int x = 0; x < table.Width; x++)
                {
                    if (table.Height-y <= spikes[x])
                        table[x,y] = true;
                }
            }

        }

        public static void Clear(this Table<bool> table)
        {
            for (int y = 0; y < table.Height; y++)
            {
                for (int x = 0; x < table.Width; x++)
                {
                    table[x,y] = false;
                }
            }
        }
        
    }
}
