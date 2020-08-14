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
            viewer = new Viewer();
            viewer.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 形状の読み込み
            var polygon = SurfaceAnalyzer.LoadData.LoadSTL(@"C:\Users\banan\Desktop\local\cube3_とんがり2.STL", true);

            // 形状のレンダリング
            viewer.Render(polygon);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // viewerの画像の取得
            using (Mat mat = viewer.GetMat())
            {
                // 画像の表示
                Cv2.ImShow("mat", mat);

                // 画像の保存
                Cv2.ImWrite(@"C:\Users\banan\Desktop\local\mat.jpg", mat * 256);
            }
        }
    }
}
