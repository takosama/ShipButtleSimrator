using System.Collections.Generic;

namespace ShipButtleSimrator.Animation
{
    public class Animation
    {
        public (int X, int Y) GetPoint()
        {
            if (GetEnumerator == null)
                return _offset;
            var tmp = GetEnumerator.Current;
            return (tmp.X + _offset.X, tmp.Y + _offset.Y);
        }

        private (int X, int Y) _offset = (0, 0);

        public void MoveNext()
        {
            if (GetEnumerator == null)
            {
                var anime = GetNextAnimation();
                if (anime == null)
                    return;
                GetEnumerator = anime.GetEnumerator();
            }
            var tmp = GetEnumerator.MoveNext();
            if (tmp) return;
            _offset.X += GetEnumerator.Current.X;
            _offset.Y += GetEnumerator.Current.Y;
            GetEnumerator = null;
            MoveNext();
        }

        public IEnumerator<(int X, int Y)> GetEnumerator;


        public Queue<IEnumerable<(int X, int Y)>> Animations = new Queue<IEnumerable<(int X, int Y)>>();


        public IEnumerable<(int X, int Y)> GetNextAnimation()
        {
            if (Animations == null)
                return null;


            if (Animations.Count == 0) return null;
            return Animations.Dequeue();
        }

        internal void AddAnimations(IEnumerable<(int X, int Y)>[] animations)
        {
            foreach (var animation in animations)
                AddAnimation(animation);
        }

        public void AddAnimation(IEnumerable<(int X, int Y)> animation)
        {
            Animations.Enqueue(animation);
        }
    }
}