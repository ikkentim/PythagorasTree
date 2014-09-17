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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace WindowsFormsApplication1
{
    public class PythagorasWindow : GameWindow
    {
        private const int DefaultIterations = 16;
        private const int MaxIteration = 35;
        private const int SquareSize = 2000;
        private Vector2 _camera;

        private int _iterations = DefaultIterations;
        private bool _move;


        private int _moveFocusX;
        private int _moveFocusY;
        private float _scale = 8000;
        private Viewport _viewport;


        public PythagorasWindow() : base(800, 600)
        {
            Title = "Pythagoras tree";
        }

        #region Events

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_calculatingVisiblePythagorasSetsThread != null && _calculatingVisiblePythagorasSetsThread.IsAlive)
                _calculatingVisiblePythagorasSetsThread.Abort();
            base.OnClosing(e);
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
                Rescale();
                CalcData();
            }
            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            _scale += e.DeltaPrecise > 0 ? _scale/10 : -_scale/10;
            if (_scale < float.Epsilon) _scale = float.Epsilon;

            Rescale();

            if (e.Mouse.MiddleButton == ButtonState.Released)
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

            _defaultDrawList = PythagorasSets.GetSets(DefaultIterations, SquareSize).SelectMany(i => i).ToList();
            PythagorasSets.GenerateSets(MaxIteration, SquareSize);

            CalcData();
        }

        protected override void OnResize(EventArgs e)
        {
            Rescale();
        }

        #endregion

        #region Methods

        private readonly List<Pythagoras> _drawList = new List<Pythagoras>();

        private readonly object _drawListLocker = new object();

        private Thread _calculatingVisiblePythagorasSetsThread;
        private List<Pythagoras> _defaultDrawList;

        private void Rescale()
        {
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            float aspectRatio = (float) Width/Height;
            GL.Ortho(-_scale*aspectRatio, _scale*aspectRatio, -_scale, _scale, 0.0, 4.0);

            _viewport = new Viewport(new Vector2(-_camera.X - _scale*aspectRatio, -_camera.Y - _scale),
                new Vector2(-_camera.X + _scale*aspectRatio, -_camera.Y + _scale));
        }

        private void CalcData()
        {
            _iterations = 13 + (int) (2000/_scale);

            if (_iterations < DefaultIterations) _iterations = DefaultIterations;
            if (_iterations > MaxIteration) _iterations = MaxIteration;

            if (_calculatingVisiblePythagorasSetsThread != null)
                _calculatingVisiblePythagorasSetsThread.Abort();

            lock (_drawListLocker)
            {
                _drawList.Clear();
            }

            _calculatingVisiblePythagorasSetsThread = new Thread(CalculateVisiblePythagorasSetsThread)
            {
                Name = "ViewPortCalc"
            };
            _calculatingVisiblePythagorasSetsThread.Start();
        }

        private void CalculateVisiblePythagorasSetsThread()
        {
            if (_iterations < DefaultIterations || PythagorasSets.AvailableIterations < DefaultIterations)
            {
                _calculatingVisiblePythagorasSetsThread = null;
                return;
            }

            List<Pythagoras> lastVisible =
                PythagorasSets.GetSet(DefaultIterations - 1, SquareSize)
                    .Where(py => py.LeftSquare.Any(_viewport.IsNear) || py.RightSquare.Any(_viewport.IsNear))
                    .ToList();

            for (int i = DefaultIterations; i < _iterations && i < PythagorasSets.AvailableIterations; i++)
            {
                IEnumerable<Pythagoras> next = lastVisible.SelectMany(p => p.Next());

                Pythagoras[] sel =
                    next.Where(p => p.LeftSquare.Any(_viewport.Contains) || p.RightSquare.Any(_viewport.Contains))
                        .ToArray();

                lock (_drawListLocker)
                {
                    _drawList.AddRange(sel);
                }

                lastVisible = next.ToList();
            }
        }

        #endregion

        #region Rendering

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (_move)
            {
                float xd = _moveFocusX - Mouse.X;
                float yd = _moveFocusY - Mouse.Y;

                _camera += new Vector2(xd/2500*_scale, -yd/2500*_scale);
            }
        }

        private void RenderSquare(Vector2[] points, Color color)
        {
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(color);
            foreach (Vector2 point in points) GL.Vertex2(point);
            GL.End();
        }

        private void RenderTriangle(Vector2[] points, Color color)
        {
            GL.Begin(PrimitiveType.Triangles);
            GL.Color3(color);
            foreach (Vector2 point in points) GL.Vertex2(point);
            GL.End();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.PushMatrix();
            {
                GL.Translate(_camera.X, _camera.Y, 0);

                GL.PushMatrix();
                {
                    //Default square
                    RenderSquare(
                        new[]
                        {
                            new Vector2(0, 0), new Vector2(0, SquareSize), new Vector2(SquareSize, SquareSize),
                            new Vector2(SquareSize, 0)
                        }, Color.FromArgb(100, 0, 0));


                    foreach (Pythagoras py in _defaultDrawList)
                    {
                        int r = 100 - 50*py.Iteration; //iteration;

                        int g = -r;
                        if (r < 0) r = 0;
                        if (g < 0) g = 0;
                        if (g > 255) g = 255;
                        Color c = Color.FromArgb(r, g, 0);

                        RenderSquare(py.LeftSquare, c);
                        RenderSquare(py.RightSquare, c);
                    }

                    if (_drawList != null)
                    {
                        lock (_drawListLocker)
                        {
                            foreach (Pythagoras py in _drawList)
                            {
                                int r = 100 - 50*py.Iteration; //iteration;

                                int g = -r;
                                if (r < 0) r = 0;
                                if (g < 0) g = 0;
                                if (g > 255) g = 255;
                                Color c = Color.FromArgb(r, g, 0);

                                RenderSquare(py.LeftSquare, c);
                                RenderSquare(py.RightSquare, c);
                            }
                        }
                    }
                }
                GL.PopMatrix();
            }
            GL.PopMatrix();

            SwapBuffers();
        }

        #endregion
    }
}