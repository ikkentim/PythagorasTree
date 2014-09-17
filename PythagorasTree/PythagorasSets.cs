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
using System.Diagnostics;
using System.Linq;
using OpenTK;

namespace PythagorasTree
{
    public static class PythagorasSets
    {
        private static readonly List<IEnumerable<Pythagoras>> Sets = new List<IEnumerable<Pythagoras>>();

        public static int AvailableIterations
        {
            get { return Sets.Count; }
        }

        public static IEnumerable<Pythagoras> GetSet(int iteration, float baseSize)
        {
            /*
             * Set was already generated, parse from list.
             */
            if (Sets.Count > iteration)
            {
                return Sets[iteration];
            }

            /*
             * First iterations need to be handed the base plate.
             */
            if (iteration == 0)
            {
                var set = new[] {Pythagoras.Generate(new Vector2(0, baseSize), new Vector2(baseSize, baseSize), 0)};

                Sets.Add(set);
                return set;
            }

            /*
             * Other iterations can simply ge generated using the Pythagoras.Next function.
             */
            IEnumerable<Pythagoras> sets = GetSet(iteration - 1, baseSize).SelectMany(set => set.Next()).ToList();

            Sets.Add(sets);
            return sets;
        }

        public static IEnumerable<IEnumerable<Pythagoras>> GetSets(int iterations, float baseSize)
        {
            for (var i = 0; i < iterations; i++)
            {
                yield return GetSet(i, baseSize).ToArray();
            }
        }
    }
}