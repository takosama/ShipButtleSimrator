using System.Windows.Forms;

namespace ShipButtleSimrator.GameObject
{
    public interface IGameObject
    {
        int X { get; set; }
        int Y { get; set; }
        void MoveNext();
        (int X, int Y) GetPoint();
        PictureBox PictureBox { get; set; }
    }
}