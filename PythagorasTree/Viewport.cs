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

using OpenTK;

namespace WindowsFormsApplication1
{
    public struct Viewport
    {
        public Viewport(Vector2 left, Vector2 right)
            : this()
        {
            Left = left;
            Right = right;
        }

        public Vector2 Left { get; set; }
        public Vector2 Right { get; set; }

        public bool Contains(Vector2 point)
        {
            return point.X >= Left.X && point.X <= Right.X && point.Y >= Left.Y && point.Y <= Right.Y;
        }

        public bool IsNear(Vector2 point)
        {
            Vector2 ll = Left - (Right - Left)*3;
            Vector2 rr = Right + (Right - Left)*3;

            return point.X >= ll.X && point.X <= rr.X && point.Y >= ll.Y && point.Y <= rr.Y;
        }
    }
}