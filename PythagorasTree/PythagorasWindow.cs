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
using System.Drawing;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace WindowsFormsApplication1
{
    public class PythagorasWindow : GameWindow
    {
        private const int SIZE = 1000;
        private float _camX;
        private float _camY;

        private float _horizontalScale = 3;
        private int _iterations = 10;
        private bool _move;
        private int _moveFocusX;
        private int _moveFocusY;
        private float _verticalScale = 3;
        private float _zoom = 4000;
 
        public PythagorasWindow() : base(800, 600)
        {
            Title = "Pythagoras tree";
        }

        #region Events

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

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            _zoom += e.DeltaPrecise > 0 ? _zoom/50 : -_zoom/50;
            if (_zoom < float.Epsilon)
            {
                _zoom = float.Epsilon;
            }

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
            GL.ClearColor(Color.Wheat);

            Rescale();
            CalcData();
        }

        protected override void OnResize(EventArgs e)
        {
            Rescale();
        }

        #endregion

        #region Methods

        private void Rescale()
        {
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            float aspectRatio = (float) Width/Height;
            GL.Ortho(-_zoom*aspectRatio, _zoom*aspectRatio, -_zoom, _zoom, 0.0, 4.0);

            _horizontalScale = _zoom*aspectRatio;
            _verticalScale = _zoom;
        }

        public IEnumerable<Pythagoras> _drawThis;

        private void CalcData()
        {
            _iterations = 10 + (int) (2000/_zoom);

            if (_iterations < 10) _iterations = 10;
            if (_iterations > 20) _iterations = 20;

            PythagorasSets.GenerateSets(_iterations, SIZE);

            _drawThis = PythagorasSets.GetSets(_iterations, SIZE).SelectMany(p => p);
        }

        #endregion

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (_move)
            {
                var xd = Mouse.X - _moveFocusX;
                var yd = Mouse.Y - _moveFocusY;

                float xdf = -(float) xd/2500;
                float ydf = -(float) yd/2500;

                _camX += xdf*_zoom;
                _camY -= ydf*_zoom;
            }
        }

        private bool RenderSquare(Vector2[] points, Vector2 vpLeft, Vector2 vpRight, Color color, bool check)
        {
            if (check && !points.Any(p => p.X >= vpLeft.X || p.X <= vpRight.X || p.Y >= vpLeft.Y || p.Y <= vpRight.Y))
                return false;

            GL.Begin(PrimitiveType.Quads);
            GL.Color3(color);
            foreach (var point in points) GL.Vertex2(point);
            GL.End();

            return true;
        }

        private bool RenderTriangle(Vector2[] points, Vector2 vpLeft, Vector2 vpRight, Color color, bool check)
        {
            if (check && !points.Any(p => p.X >= vpLeft.X || p.X <= vpRight.X || p.Y >= vpLeft.Y || p.Y <= vpRight.Y))
                return false;

            GL.Begin(PrimitiveType.Triangles);
            GL.Color3(color);
            foreach (var point in points) GL.Vertex2(point);
            GL.End();

            return true;
        }

        private void RenderPytha(Pythagoras p, Vector2 vpLeft, Vector2 vpRight, int iteration)
        {
            var r = 100 - 50*iteration;

            var g = -r;
            if (r < 0) r = 0;
            if (g < 0) g = 0;
            if (g > 255) g = 255;
            var c = Color.FromArgb(r, g, 0);

            bool ls = RenderSquare(p.LeftSquare, vpLeft, vpRight, c, true);
            bool rs = RenderSquare(p.RightSquare, vpLeft, vpRight, c, true);

            if (ls && iteration < _iterations) RenderPytha(p.NextLeft(), vpLeft, vpRight, iteration + 1);
            if (rs && iteration < _iterations) RenderPytha(p.NextRight(), vpLeft, vpRight, iteration + 1);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            Vector2 vpLeft = new Vector2(-_camX - _horizontalScale, -_camY - _verticalScale);
            Vector2 vpRight = new Vector2(-_camX + _horizontalScale, -_camY + _verticalScale);

            GL.PushMatrix(); //cam
            {
                GL.Translate(_camX, _camY, 0);

                GL.PushMatrix();
                {
                    RenderSquare(
                        new[]
                        {
                            new Vector2(0, 0), new Vector2(0, SIZE), new Vector2(SIZE, SIZE),
                            new Vector2(SIZE, 0)
                        }, vpLeft, vpRight, Color.FromArgb(100, 0, 0), true);


                    var r = 100 - 50*0;//iteration;

                    var g = -r;
                    if (r < 0) r = 0;
                    if (g < 0) g = 0;
                    if (g > 255) g = 255;
                    var c = Color.FromArgb(r, g, 0);

                    if(_drawThis != null)
                        foreach (var py in _drawThis)
                        {
                            RenderSquare(py.LeftSquare, vpLeft, vpRight, c, false);
                            RenderSquare(py.RightSquare, vpLeft, vpRight, c, false);
                        }
                    /*
                    var iteration = 0;
                    foreach (var set in PythagorasSets.GetSets(12, SIZE))
                    {
                        var r = 100 - 50*iteration;

                        var g = -r;
                        if (r < 0) r = 0;
                        if (g < 0) g = 0;
                        if (g > 255) g = 255;
                        var c = Color.FromArgb(r, g, 0);
                        foreach (var py in set)
                        {
                            if (iteration == 11)
                            {
                                RenderPytha(py, vpLeft, vpRight, iteration);
                            }
                            else
                            {
                                RenderSquare(py.LeftSquare, vpLeft, vpRight, c, true);
                                RenderSquare(py.RightSquare, vpLeft, vpRight, c, true);
                            }
                        }

                        iteration++;
                    }*/
                }
                GL.PopMatrix();
            }
            GL.PopMatrix(); //cam

            SwapBuffers();
        }
    }
}