using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace WindowsFormsApplication1
{
    public class PythagorasWindow : GameWindow
    {
        private bool _move;
        private int _moveFocusX;
        private int _moveFocusY;
        private float _camX;
        private float _camY;
        private float _zoom = 4000;
        private int _iterations = 10;
        private const int SIZE = 1000;

        private float _horizontalScale = 3;
        private float _verticalScale = 3;


        public PythagorasWindow() : base(800, 600)
        {
            
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
            _zoom += e.DeltaPrecise * 20;
            if (_zoom < 0)
            {
                _zoom = 0;
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
            GL.ClearColor(Color.Black);

            Rescale();
            CalcData();
        }

        protected override void OnResize(EventArgs e)
        {
            Rescale();
        }


        #endregion

        #region Methods

        void Rescale()
        {
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            float aspectRatio = (float)Width / Height;
            GL.Ortho(-_zoom * aspectRatio, _zoom * aspectRatio, -_zoom, _zoom, 0.0, 4.0);

            Debug.WriteLine("ReS: {0}, {1}", -_zoom * aspectRatio, _zoom * aspectRatio);
            _horizontalScale = _zoom * aspectRatio;
            _verticalScale = _zoom;
        }

        void CalcData()
        {
            //_iterations = (SIZE - (int)_zoom) / 1000;

            if (_iterations < 0) _iterations = 0;

            PythagorasSets.GenerateSets(_iterations, SIZE);
        }

        #endregion


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
            }
        }

        private void RenderSquare(Vector2[] points, Vector2 vpLeft, Vector2 vpRight)
        {
            if (!points.Any(p => p.X >= vpLeft.X || p.X <= vpRight.X || p.Y >= vpLeft.Y || p.Y <= vpRight.Y))
                return;

            GL.Begin(PrimitiveType.Quads);
            GL.Color3(Color.Blue);
            foreach (var
                point in points) GL.Vertex2(point);
            GL.End();
        }
        private void RenderTriangle(Vector2[] points, Vector2 vpLeft, Vector2 vpRight)
        {
            if (!points.Any(p => p.X >= vpLeft.X || p.X <= vpRight.X || p.Y >= vpLeft.Y || p.Y <= vpRight.Y))
                return;

            GL.Begin(PrimitiveType.Triangles);

                GL.Color3(Color.Red);
            foreach (var point in points)
            {
                GL.Vertex2(point);
            }
            GL.End();


        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {

            GL.Clear(ClearBufferMask.ColorBufferBit);

            Vector2 vpLeft = new Vector2(-_camX - _horizontalScale, -_camY - _verticalScale);
            Vector2 vpRight = new Vector2(-_camX + _horizontalScale, -_camY + _verticalScale);

            GL.PushMatrix();//cam
            {
                GL.Translate(_camX, _camY, 0);

                GL.PushMatrix();
                {
                    RenderSquare(
                        new[]
                        {
                            new Vector2(0, 0), new Vector2(0, SIZE), new Vector2(SIZE, SIZE),
                            new Vector2(SIZE, 0)
                        }, vpLeft, vpRight);

                    foreach (var set in PythagorasSets.GetSets(_iterations, SIZE))
                    {
                        foreach (var py in set)
                        {
                            RenderSquare(py.LeftSquare, vpLeft, vpRight);
                            RenderSquare(py.RightSquare, vpLeft, vpRight);
                            RenderTriangle(py.Triangle, vpLeft, vpRight);
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
