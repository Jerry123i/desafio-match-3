
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    public static class PatternModels
    {
        private static List<Table<int>> patterns;

        private static void InitPatterns()
        {
            patterns = new List<Table<int>>();

            Table<int> basePattern = new Table<int>(3, 3, 1);
            
            patterns.Add(Table<int>.CreateCopy(basePattern));
            patterns.Last()[0, 0] = 0;
            patterns.Last()[0, 2] = 0;
            patterns.Last()[2, 0] = 0;
            patterns.Last()[2, 2] = 0;
            
            patterns.Add(Table<int>.CreateCopy(basePattern));
            patterns.Last()[0, 2] = 0;
            patterns.Last()[1, 2] = 0;
            patterns.Last()[2, 2] = 0;
            
            patterns.Add(Table<int>.CreateCopy(basePattern));
            patterns.Last()[2, 0] = 0;
            patterns.Last()[2, 1] = 0;
            patterns.Last()[2, 2] = 0;
            
            patterns.Add(Table<int>.CreateCopy(basePattern));
            patterns.Last()[1, 0] = 0;
            patterns.Last()[1, 1] = 0;
            patterns.Last()[1, 2] = 0;

            patterns.Add(Table<int>.CreateCopy(basePattern));
            patterns.Last()[1, 0] = 0;
            patterns.Last()[1, 1] = 0;
            patterns.Last()[1, 2] = 0;
            
            patterns.Add(Table<int>.CreateCopy(basePattern));
            patterns.Last()[0, 0] = 0;
            patterns.Last()[2, 1] = 0;
            patterns.Last()[0, 2] = 0;
            patterns.Last()[1, 2] = 0;
            patterns.Last()[2, 2] = 0;
            
            patterns.Add(Table<int>.CreateCopy(basePattern));
            patterns.Last()[0, 0] = 0;
            patterns.Last()[2, 0] = 0;
            patterns.Last()[1, 2] = 0;
            
            patterns.Add(Table<int>.CreateCopy(basePattern));
            patterns.Last()[1, 0] = 0;
            patterns.Last()[1, 1] = 0;
            patterns.Last()[1, 2] = 0;
            
            patterns.Add(Table<int>.CreateCopy(basePattern));
            patterns.Last()[0, 1] = 0;
            patterns.Last()[0, 2] = 0;
            patterns.Last()[2, 1] = 0;
            patterns.Last()[2, 2] = 0;

        }

        public static Table<int> GetRandomPattern()
        {
            if(patterns == null)
                InitPatterns();

            return patterns[Random.Range(0, patterns.Count)];

        }
        
    }
}
