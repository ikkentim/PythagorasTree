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

using System;
using System.Collections.Generic;
using OpenTK;

namespace WindowsFormsApplication1
{
    public struct Pythagoras
    {
        public Vector2[] Triangle { get; set; }

        public Vector2[] LeftSquare { get; set; }

        public Vector2[] RightSquare { get; set; }


        private static double VecAngle(Vector2 l, Vector2 r)
        {
            return Math.Atan2(r.Y - l.Y, r.X - l.X);
        }

        private static Vector2 AngleVector(double angle)
        {
            return new Vector2((float) Math.Cos(angle), (float) Math.Sin(angle));
        }

        public static Pythagoras Generate(Vector2 leftVector, Vector2 rightVector)
        {
            /* 
              * Triangle
              */

            var angle = VecAngle(leftVector, rightVector) + Math.PI/2;
            Vector2 midVector = leftVector + (rightVector - leftVector)/2;
            Vector2 newVector = AngleVector(angle);

            newVector *= (rightVector - leftVector).Length/2;
            newVector += midVector;

            var newVectorAngle = VecAngle(newVector, leftVector);
            var newVectorAngle2 = VecAngle(rightVector, newVector);

            /*
             * Squares below
             */

            var leftTranslation = AngleVector(newVectorAngle - Math.PI/2)*(newVector - leftVector).Length;
            var rightTranslation = AngleVector(newVectorAngle2 - Math.PI/2)*(newVector - rightVector).Length;

            var leftTopLeft = leftVector + leftTranslation;
            var leftTopRight = newVector + leftTranslation;

            var rightTopLeft = newVector + rightTranslation;
            var rightTopRight = rightVector + rightTranslation;

            return new Pythagoras
            {
                Triangle = new[] {leftVector, rightVector, newVector},
                LeftSquare = new[] {leftVector, newVector, leftTopRight, leftTopLeft},
                RightSquare = new[] {newVector, rightVector, rightTopRight, rightTopLeft}
            };
        }

        public Pythagoras NextLeft()
        {
            return Generate(LeftSquare[3], LeftSquare[2]); //Set on top of left side
        }

        public Pythagoras NextRight()
        {
            return Generate(RightSquare[3], RightSquare[2]); //Set on top of left side
        }

        public IEnumerable<Pythagoras> Next()
        {
            yield return NextLeft();
            yield return NextRight();
        }
    }
}