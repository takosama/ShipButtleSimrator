using System.Collections.Generic;

namespace ShipButtleSimrator.Animation
{



    public static class AnimationNode
    {
        public static IEnumerable<(int X, int Y)> MoveLiner(int time, (int DistX, int DistY)vector)
        {
            (double x, double y) pos = (0, 0);
            var speed = (x:(double) vector.DistX / time * 34, y: (double) vector.DistY / time * 34);
            for (var i = 0; i < time; i += 34)
            {
                pos.x += speed.x;
                pos.y += speed.y;
                yield return ((int) pos.x, (int) pos.y);
            }
            yield return (vector.DistX, vector.DistY);
        }

        public static IEnumerable<(int X, int Y)> MoveFoard()
        {
            for (var i = 0; i < 100; i++)
                yield return (i, 0);
        }

        public static IEnumerable<(int X, int Y)> MoveBack()
        {
            for (var i = 100; i > 0; i--)
                yield return (i, 0);
        }
    }
}