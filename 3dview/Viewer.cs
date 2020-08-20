using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

// openTK
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

// surfaceAnalyzer
using SurfaceAnalyzer;



namespace _3dview
{
    public partial class Viewer : Form
    {
        #region Camera__Field

        bool isCameraRotating;
        Vector2 current, previous;
        float zoom = 1.0f;
        double rotateX = 1, rotateY = 0, rotateZ = 0;
        float theta = 0;
        float phi = 0;

        #endregion

        public Viewer()
        {
            InitializeComponent();

            AddglControl();
        }

        // add glControl
        GLControl glControl;
        private void AddglControl()
        {
            SuspendLayout();

            int width = this.Width;
            int height = this.Height;

            // init GLControl
            glControl = new GLControl();

            glControl.Name = "SHAPE";
            glControl.Size = new Size(width, height);
            glControl.Location = new System.Drawing.Point(0, 0);
            glControl.SendToBack();

            // add event handler
            glControl.Load += new EventHandler(glControl_Load);
            glControl.Resize += new EventHandler(glControl_Resize);
            glControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this._3DView_MouseDown);
            glControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this._3DView_MouseMove);
            glControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this._3DView_MouseUp);
            glControl.MouseWheel += new System.Windows.Forms.MouseEventHandler(this._3DView_MouseWheel);

            Controls.Add(glControl);

            ResumeLayout(false);

        }

        private void glControl_Load(object sender, EventArgs e)
        {
            GLControl s = (GLControl)sender;
            s.MakeCurrent();

            GL.ClearColor(Color4.White);
            GL.Enable(EnableCap.DepthTest);

            Update();
        }

        private void glControl_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, glControl.Size.Width, glControl.Size.Height);
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4,
                (float)glControl.Size.Width / (float)glControl.Size.Height, 1.0f, 256.0f);
            GL.LoadMatrix(ref projection);

            Update();
        }

        private void _3DView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                isCameraRotating = true;
                current = new Vector2(e.X, e.Y);
            }
            Update();
        }


        private void _3DView_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                isCameraRotating = false;
                previous = Vector2.Zero;
            }
            Update();
        }


        private void _3DView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (isCameraRotating)
            {
                previous = current;
                current = new Vector2(e.X, e.Y);
                Vector2 delta = current - previous;
                delta /= (float)Math.Sqrt(this.Width * this.Width + this.Height * this.Height);
                float length = delta.Length;

                if (length > 0.0)
                {
                    theta += delta.X * 10;
                    phi += delta.Y * 10;
                    rotateX = Math.Cos(theta) * Math.Cos(phi);
                    rotateY = Math.Sin(phi);
                    rotateZ = Math.Sin(theta) * Math.Cos(phi);
                }

                Update();
            }
        }

        private void _3DView_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            float delta = e.Delta;

            zoom *= (float)Math.Pow(1.001, delta);

            if (zoom > 4.0f)
                zoom = 4.0f;
            if (zoom < 0.03f)
                zoom = 0.03f;

            Update();
        }

        PolygonModel Polygon;
        public void Update()
        {
            if (Polygon == null) return;
            Render(Polygon);
        }
        public void Render(PolygonModel polygon)
        {
            Polygon = polygon;

            // clear buffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // set camera setting
            Vector3 vec_rotate = new Vector3((float)rotateX, (float)rotateY, (float)rotateZ);
            Vector3 center = new Vector3(N2TK(Polygon.GravityPoint()));
            Vector3 eye = center + vec_rotate * center.LengthFast / zoom;
            Matrix4 modelView = Matrix4.LookAt(eye, center, Vector3.UnitY);

            // set disp mode
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelView);

            // display shape model
            DrawPolygons(polygon);

            glControl.SwapBuffers();
        }

        private void DrawPolygons(PolygonModel polygon)
        {
            if (polygon == null) return;

            GL.Begin(PrimitiveType.Triangles);

            for (int l = 0; l < polygon.Faces.Count; l++)
            {
                var normal = polygon.Faces[l].Normal();
                GL.Color4(Math.Abs(normal.X), Math.Abs(normal.Y), Math.Abs(normal.Z), 0);
                GL.Normal3(N2TK(normal));
                GL.Vertex3(N2TK(polygon.Faces[l].Vertices[0].P));
                GL.Vertex3(N2TK(polygon.Faces[l].Vertices[2].P));
                GL.Vertex3(N2TK(polygon.Faces[l].Vertices[1].P));
            }

            GL.End();
        }

        // convert Numerics.Vector3 to OpenTK.Vector3
        private static OpenTK.Vector3 N2TK(System.Numerics.Vector3 vec3) => new Vector3(vec3.X, vec3.Y, vec3.Z);

        // get mat image
        public OpenCvSharp.Mat GetMat()
        {
            int width = glControl.Width;
            int height = glControl.Height;
            
            float[] floatArr = new float[width * height * 3];
            OpenCvSharp.Mat ret = new OpenCvSharp.Mat(height, width, OpenCvSharp.MatType.CV_32FC3);

            // read dataBuffer
            IntPtr dataBuffer = Marshal.AllocHGlobal(width * height * 12);
            GL.ReadBuffer(ReadBufferMode.Front);
            GL.ReadPixels(0, 0, width, height, PixelFormat.Bgr, PixelType.Float, dataBuffer);

            // to img array
            Marshal.Copy(dataBuffer, floatArr, 0, floatArr.Length);

            // to opencvsharp.Mat
            Marshal.Copy(floatArr, 0, ret.Data, floatArr.Length);

            // dispose
            Marshal.FreeHGlobal(dataBuffer);

            return ret;
        }
    }
}
