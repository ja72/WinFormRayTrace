using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using System.Threading;
using System.Diagnostics;

using static System.Math;

namespace JA
{
    using JA.Geometry;
    using static JA.Geometry.Helpers;

    public delegate Vector3 DrawPixel(int i, int j);

    public partial class MainForm : Form
    {

        public MainForm()
        {
            InitializeComponent();

            this.Scene = new Scene();
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Scene.Add(new Light(new Vector3(-20, 20, 20), 1.5f));
            Scene.Add(new Light(new Vector3( 30, 50,-25), 1.8f));
            Scene.Add(new Light(new Vector3( 30, 20, 30), 1.7f));

            var ivory = new Material(RGB(0.4f, 0.4f, 0.3f), new Albedo(0.6f, 0.3f, 0.1f, 0.0f), 1.0f, 50);
            var glass = new Material(RGB(0.6f, 0.7f, 0.8f), new Albedo(0.0f, 0.5f, 0.1f, 0.8f), 1.1f, 125);
            var red = new Material(RGB(0.3f, 0.1f, 0.1f), new Albedo(0.9f, 0.1f, 0.0f, 0.0f), 1.0f, 20);
            var mirror = new Material(RGB(1.0f, 1.0f, 1.0f), new Albedo(0.0f, 10.0f, 0.8f, 0.0f), 1.0f, 1425);

            Scene.Add(new Sphere(new Vector3(  -3,     0,   -16), 2, ivory));
            Scene.Add(new Sphere(new Vector3(  -1, -1.5f,   -12), 2, glass));
            Scene.Add(new Sphere(new Vector3(1.5f, -0.5f,   -18), 3, red));
            Scene.Add(new Sphere(new Vector3(   7,     5,   -18), 4, mirror));

            Render(Scene);
        }

        public Scene Scene { get; }

        void Render(Scene scene)
        {
            Task.Run(() =>
            {
                var sw = Stopwatch.StartNew();
                var img = scene.Render(pictureBox1.ClientSize);
                double t_draw = sw.Elapsed.TotalSeconds;
                this.MainThread(() => 
                {
                    pictureBox1.Image = img;
                    this.Text = $"Ray Trace ({img.Width},{img.Height}), scene={t_draw*1e3:F1} ms";
                });
            });
        }

        private void pictureBox1_Resize(object sender, EventArgs e) => Render(Scene);
    }

    public static class Extensions
    {
        public static void MainThread(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }

        public static void MainThread<T>(this Control control, Action<T> action, T argument)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action, argument);
            }
            else
            {
                action(argument);
            }
        }
    }

}
