using System;
using System.Collections.Generic;
using System.Drawing;

namespace ShipButtleSimrator.GameObject
{
    public static  class GameObjectManager
    {
        private static readonly List<IGameObject> GameObjectList = new List<IGameObject>();
        public static void Add(IGameObject gameObject)
        {
            GameObjectList.Add(gameObject);
        }
        public static void Refresh()
        {
            foreach (var gameObject in GameObjectList)
                if (gameObject != null) gameObject.MoveNext();


            foreach (var gameObject in GameObjectList)
                Draw(gameObject);
        }

        public static void Draw(IGameObject gameObject)
        {
            if (gameObject != null)
            {
                var tmp = gameObject.GetPoint();
                gameObject.PictureBox.Location = new Point(tmp.X, tmp.Y);
                Console.WriteLine(tmp.X + @"," + tmp.Y);
            }
        }
    }
}