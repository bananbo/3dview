using OpenCvSharp;
using System;
using System.Windows.Forms;

namespace _3dview
{
    public partial class Control : Form
    {
        public Control()
        {
            InitializeComponent();
        }

        Viewer viewer;
        private void button1_Click(object sender, EventArgs e)
        {
            // open viewer
            viewer = new Viewer();
            viewer.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // read shape data
            var polygon = SurfaceAnalyzer.LoadData.LoadSTL(@"..\..\STL\cube.STL", true);

            // render 3Dshape
            viewer.Render(polygon);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // get viewer image
            using (Mat mat = viewer.GetMat())
            {
                // show image
                Cv2.ImShow("mat", mat);

                // save image
                Cv2.ImWrite(@"local\mat.jpg", mat * 256);
            }
        }
    }
}
