
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
        
        public static void MarkLine(this List<List<bool>> table, int line)
        {
            for (int i = 0; i < table[0].Count; i++)
            {
                table[line][i] = true;
            }
        }

        public static void MarkColumn(this List<List<bool>> table, int column)
        {
            for (int i = 0; i < table.Count; i++)
            {
                table[i][column] = true;
            }
        }

        public static void MarkRadius(this List<List<bool>> table, int centerX, int centerY, int radius)
        {
            Vector2Int centerVector = new Vector2Int(centerX, centerY);
            
            for (int y = 0; y < table.Count; y++)
            {
                for (int x = 0; x < table[0].Count; x++)
                {
                    var dist = (new Vector2Int(x, y) - centerVector).magnitude;
                    if (dist <= radius)
                        table[y][x] = true;
                }
            }

        }

        public static void MarkSameType(this List<List<bool>> table, List<List<Tile>> referenceBoard, int type)
        {
            for (int y = 0; y < referenceBoard.Count; y++)
            {
                for (int x = 0; x < referenceBoard.Count; x++)
                {
                    if (referenceBoard[y][x].Type == type)
                        table[y][x] = true;
                }
            }
        }

        //Effect for a specific item use
        public static void MarkEarthquakePattern(this List<List<bool>> table)
        {
            int minSpike = 1;
            int maxSpike = 4;

            int width = table[0].Count;
            List<int> spikes = new List<int>();

            for (int i = 0; i < width; i++)
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

            int height = table.Count;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < table[0].Count; x++)
                {
                    if (height-y <= spikes[x])
                        table[y][x] = true;
                }
            }

        }

        public static void Clear(this List<List<bool>> table)
        {
            for (int y = 0; y < table.Count; y++)
            {
                for (int x = 0; x < table.Count; x++)
                {
                    table[y][x] = false;
                }
            }
        }
        
    }
}
