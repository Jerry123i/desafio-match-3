using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    public static class Utils 
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
    }
}
