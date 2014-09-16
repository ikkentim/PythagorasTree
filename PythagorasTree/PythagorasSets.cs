using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;

namespace WindowsFormsApplication1
{
    public static class PythagorasSets
    {
        private static readonly List<IEnumerable<Pythagoras>> Sets = new List<IEnumerable<Pythagoras>>();

        private static IEnumerable<Pythagoras> GetSetsForSets(IEnumerable<Pythagoras> previousSets)
        {
            return previousSets.SelectMany(set => set.Next());
        }

        public static IEnumerable<Pythagoras> GetSet(int iteration, float baseSize)
        {
            if (Sets.Count > iteration) //Available in known
            {
                return Sets[iteration];
            }
            if (iteration == 0)
            {
                var set = new[] { Pythagoras.Generate(new Vector2(0, baseSize), new Vector2(baseSize, baseSize)) };

                Sets.Add(set);
                return set;
            }

            var sets = GetSetsForSets(GetSet(iteration - 1, baseSize));

            Sets.Add(sets);
            return sets;
        }

        public static IEnumerable<IEnumerable<Pythagoras>> GetSets(int iterations, float baseSize)
        {
            GenerateSets(iterations, baseSize);
            return Sets.Take(iterations);
        }

        public static void GenerateSets(int iterations, float baseSize )
        { 
            //Ensure Nth set was generated
            if (Sets.Count <= iterations)
                GetSet(iterations, baseSize);
        }
    }
}
