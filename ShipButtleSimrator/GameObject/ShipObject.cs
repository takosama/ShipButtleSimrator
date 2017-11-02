using System.Drawing;
using System.Windows.Forms;

namespace ShipButtleSimrator.GameObject
{
    public class ShipObject : IGameObject
    {
        public ShipObject(int x, int y, Control f,string path)
        {

            PictureBox = new PictureBox
            {
                BackColor = Color.Red,
                BackgroundImage = new Bitmap(path)
            };
            PictureBox.Width = PictureBox.BackgroundImage.Width;
            PictureBox.Height = PictureBox.BackgroundImage.Height;
            f.Controls.Add(PictureBox);
            X = x;
            Y = y;
        }

        public Form Form { get; set; }
        public PictureBox PictureBox { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Animation.Animation Animation = new Animation.Animation();

        public void MoveNext()
        {
            Animation.MoveNext();
        }


        public (int X, int Y) GetPoint()
        {
            if (Animation.GetEnumerator == null)
                return (X, Y);
            var tmp = Animation.GetPoint();
            return (tmp.X + X, tmp.Y + Y);
        }
    }
}