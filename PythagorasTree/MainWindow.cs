using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Platform;

namespace WindowsFormsApplication1
{
    public class MainWindow : GameWindow
    {
        private bool _move;
        private int _moveFocusX;
        private int _moveFocusY;
        private float _camX;
        private float _camY;
        private float _zoom = 4000;

        private const int SIZE = 1000;

        private float _hScale = 3;
        private float _vScale = 3;

        public MainWindow()
            : base(800, 600)
        {
        }


        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Right)
            {
                _move = e.IsPressed;
                _moveFocusX = e.X;
                _moveFocusY = e.Y;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Right)
            {
                _move = e.IsPressed;
            }
            base.OnMouseUp(e);
        }

        void Rescale()
        {
            GL.Viewport(0, 0, Width, Height);

            float aspectRatio = (float)Width / Height;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-_zoom * aspectRatio, _zoom * aspectRatio, -_zoom, _zoom, 0.0, 4.0);

            _hScale = _zoom * aspectRatio;
            _vScale = _zoom;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            _zoom += e.DeltaPrecise * 5;
            if (_zoom < 0)
            {
                _zoom =0;
            }

            //Debug.WriteLine("Set scale to {0}", _zoom);


            Rescale();
            CalcData();

            base.OnMouseWheel(e);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Exit();

            if (e.Key == Key.F11)
                WindowState = WindowState == WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen;

            base.OnKeyDown(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color.Black);

            Rescale();
            CalcData();
        }

        protected override void OnResize(EventArgs e)
        {
            Rescale();
        }

        double VecAngle(Vector3 l, Vector3 r)
        {
            return Math.Atan2(r.Y - l.Y, r.X - l.X);
        }

        Vector3 AngleVector(double angle)
        {
            return new Vector3((float) Math.Cos(angle), (float) Math.Sin(angle), 0);
        }

        struct PythagorasSet
        {
            /// <summary>
            /// Indexes:
            ///   2
            ///  / \
            /// 0---1
            /// </summary>
            public Vector3[] Triangle { get; set; }

            /// <summary>
            /// Indexes:
            /// 3--4
            /// |  |
            /// 0--1
            /// </summary>
            public Vector3[] LeftSquare { get; set; }

            /// <summary>
            /// Indexes:
            /// 3--4
            /// |  |
            /// 0--1
            /// </summary>
            public Vector3[] RightSquare { get; set; }


            private static double VecAngle(Vector3 l, Vector3 r)
            {
                return Math.Atan2(r.Y - l.Y, r.X - l.X);
            }

            private static Vector3 AngleVector(double angle)
            {
                return new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0);
            }

            public static PythagorasSet Generate(Vector3 leftVector, Vector3 rightVector)
            {
                /* 
                 * Triangle
                 */

                var angle = VecAngle(leftVector, rightVector) + Math.PI / 2;
                Vector3 midVector = leftVector + (rightVector - leftVector) / 2;
                Vector3 newVector = AngleVector(angle);

                newVector *= (rightVector - leftVector).Length / 2;
                newVector += midVector;

                //Add triangle
                
                //_lines.Add(new[] { rightVector, newVector });
                //_lines.Add(new[] { leftVector, newVector });

                var newVectorAngle = VecAngle(newVector, leftVector);
                var newVectorAngle2 = VecAngle(rightVector, newVector);

                /*
                 * Squares below
                 */

                var leftTranslation = AngleVector(newVectorAngle - Math.PI / 2) * (newVector - leftVector).Length;
                var rightTranslation = AngleVector(newVectorAngle2 - Math.PI / 2) * (newVector - rightVector).Length;

                var leftTopLeft = leftVector + leftTranslation;
                var leftTopRight = newVector + leftTranslation;

                var rightTopLeft = newVector + rightTranslation;
                var rightTopRight = rightVector + rightTranslation;


                //Add left square
                //_lines.Add(new[] { leftVector, leftTopLeft });
                //_lines.Add(new[] { newVector, leftTopRight });
                //_lines.Add(new[] { leftTopLeft, leftTopRight });

                //Add right square
                //_lines.Add(new[] { newVector, rightTopLeft });
                //_lines.Add(new[] { rightVector, rightTopRight });
                //_lines.Add(new[] { rightTopLeft, rightTopRight });

                return new PythagorasSet
                {
                    Triangle = new[] {leftVector, rightVector, newVector},
                    LeftSquare = new[] {leftVector, newVector, leftTopLeft, leftTopRight},
                    RightSquare = new[] { newVector, rightVector, rightTopLeft, rightTopRight }
                };
            }

        }

        private static List<IEnumerable<PythagorasSet>> _indexedSets = new List<IEnumerable<PythagorasSet>>();

        private IEnumerable<PythagorasSet> GetSetsForSets(IEnumerable<PythagorasSet> p)
        {
            foreach (var s in p)
            {

                yield return PythagorasSet.Generate(s.LeftSquare[2], s.LeftSquare[3]);
                yield return PythagorasSet.Generate(s.RightSquare[2], s.RightSquare[3]);
            }
        }

        private IEnumerable<PythagorasSet> GetSet(int n)
        {
            if (_indexedSets.Count > n)
            {
                return _indexedSets[n];

            }
            if (n == 0)
            {
                var l = new Vector3(0, SIZE, 0);
                var r = new Vector3(SIZE, SIZE, 0);

                var set = new[] {PythagorasSet.Generate(l, r)};

                _indexedSets.Add(set);

                return set;
            }

            var sets = GetSetsForSets(GetSet(n - 1));

            _indexedSets.Add(sets);
            return sets;
        }

        void CalcData()
        {

            int depth = (SIZE - (int)_zoom) / 1000;

            if (depth < 10) depth = 10;

            //Debug.WriteLine(depth);
            _lines = new List<Vector3[]>();

            _lines.Add(new[] { new Vector3(0, 0, 0), new Vector3(0, SIZE, 0) });
            _lines.Add(new[] { new Vector3(0, SIZE, 0), new Vector3(SIZE, SIZE, 0) });
            _lines.Add(new[] { new Vector3(SIZE, SIZE, 0), new Vector3(SIZE, 0, 0) });
            _lines.Add(new[] { new Vector3(SIZE, 0, 0), new Vector3(0, 0, 0) });

            //Make(depth, new Vector3(0, SIZE,0), new Vector3(SIZE, SIZE,0));

            for (var i = 0; i < depth; i++)
            {
                var s = GetSet(i);

                foreach (var d in s)
                {
                    _lines.Add(new[] { d.LeftSquare[0], d.LeftSquare[1] });
                    _lines.Add(new[] { d.LeftSquare[1], d.LeftSquare[3] });
                    _lines.Add(new[] { d.LeftSquare[3], d.LeftSquare[2] });
                    _lines.Add(new[] { d.LeftSquare[2], d.LeftSquare[0] });

                    _lines.Add(new[] { d.RightSquare[0], d.RightSquare[1] });
                    _lines.Add(new[] { d.RightSquare[1], d.RightSquare[3] });
                    _lines.Add(new[] { d.RightSquare[3], d.RightSquare[2] });
                    _lines.Add(new[] { d.RightSquare[2], d.RightSquare[0] });

                    _lines.Add(new[] { d.Triangle[0], d.Triangle[1] });
                    _lines.Add(new[] { d.Triangle[1], d.Triangle[2] });
                    _lines.Add(new[] { d.Triangle[2], d.Triangle[0] });
                }
            }
        }

        private List<Vector3[]> _lines;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (_move)
            {
                var xd = Mouse.X - _moveFocusX;
                var yd = Mouse.Y - _moveFocusY;

                float xdf = -(float)xd / 2500;
                float ydf = -(float)yd / 2500;

                _camX += xdf * _zoom;
                _camY -= ydf * _zoom;

                //Debug.WriteLine("Move to {0},{1}", _camX, _camY);
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {

            GL.Clear(ClearBufferMask.ColorBufferBit);

            Vector2 left = new Vector2(-_camX - _hScale, -_camY - _vScale);
            Vector2 right = new Vector2(-_camX + _hScale, -_camY + _vScale);

            Debug.WriteLine(right.X - left.X);
            GL.PushMatrix();//cam
            {
                GL.Translate(_camX, _camY, 0);

                GL.PushMatrix();
                {
                    foreach (var s in _lines)
                    {
                        if(s.First().X >= left.X && s.First().X <= right.X &&
                            s.First().Y >= left.Y && s.First().Y <= right.Y
                            )
                        {
                        GL.Begin(BeginMode.Lines);
                        foreach (var v in s) GL.Vertex3(v);
                        GL.End();
}
                    }

                }
                GL.PopMatrix();
            }
            GL.PopMatrix();//cam

            SwapBuffers();
        }
    }
}
