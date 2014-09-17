// PythagorasTree
// Copyright (C) 2014 Tim Potze
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
// For more information, please refer to <http://unlicense.org>

using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace WindowsFormsApplication1
{
    public static class PythagorasSets
    {
        private static readonly List<IEnumerable<Pythagoras>> Sets = new List<IEnumerable<Pythagoras>>();

        public static int AvailableIterations
        {
            get { return Sets.Count; }
        }

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
                var set = new[] {Pythagoras.Generate(new Vector2(0, baseSize), new Vector2(baseSize, baseSize), 0)};

                Sets.Add(set);
                return set;
            }

            IEnumerable<Pythagoras> sets = GetSetsForSets(GetSet(iteration - 1, baseSize));

            Sets.Add(sets);
            return sets;
        }

        public static IEnumerable<IEnumerable<Pythagoras>> GetSets(int iterations, float baseSize)
        {
            GenerateSets(iterations, baseSize);
            return Sets.Take(iterations);
        }

        public static void GenerateSets(int iterations, float baseSize)
        {
            if (Sets.Count <= iterations)
                GetSet(iterations, baseSize);
        }
    }
}