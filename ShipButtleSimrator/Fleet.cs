using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using ShipButtleSimrator;
using ShipButtleSimrator.Animation;
using ShipButtleSimrator.GameObject;

namespace ShipButtleSimrator
{
    class Fleet
    {
        (ItemData[] itesmDatas, ShipData shipData)[] _shipDatas = new (ItemData[] itesDatas, ShipData shipData)[6];
        public ShipObject[] ShipObjects = new ShipObject[6];


        public bool GetShipData(int id, out ShipData shipData)
        {
            var data = _shipDatas[id].shipData;
            if (data == null)
            {
                shipData = null;
                return false;
            }
            shipData = data;
            return true;
        }

        public bool GetSlotData(int id, int slotId, out (ItemData Itemdata, int slotID, int caryNum) slotData)
        {
            ShipData _;
            if (!GetShipData(id, out _))
            {
                slotData.slotID = 0;
                slotData.Itemdata = null;
                slotData.caryNum = 0;
                return false;
            }

            var itemDatas = _shipDatas[id].itesmDatas;

            if (itemDatas == null || itemDatas.Length == 0)
            {
                slotData.slotID = 0;
                slotData.Itemdata = null;
                slotData.caryNum = 0;
                return false;
            }
            if (slotId >= itemDatas.Length)
            {
                slotData.slotID = 0;
                slotData.Itemdata = null;
                slotData.caryNum = 0;
                return false;
            }

//TODO いか取得            
            if (itemDatas[slotId] == null)
            {
                slotData.slotID = 0;
                slotData.Itemdata = null;
                slotData.caryNum = 0;
                return false;
            }
            slotData.slotID = slotId;
            slotData.caryNum = (int) _.SLOTS[slotId];
            slotData.Itemdata = itemDatas[slotId];
            return true;
        }

        public void SetAttackAnimation(int Attacker, int Defender, bool IsAttackerMyFleet)
        {
            this.ShipObjects[Attacker].Animation.AddAnimation(AnimationNode.MoveLiner(200,(50,0)) );
            this.ShipObjects[Attacker].Animation.AddAnimation(AnimationNode.MoveLiner(400,(-50,0)));
        }
        public Fleet((ItemData[] itemdata, ShipData shipData)[] jsonObject, Control c)
        {
            for (var i = 0; i < 6; i++)
            {
                var tmp = jsonObject[i];
                if (tmp.shipData == null) continue;
                _shipDatas[i].shipData = tmp.shipData;

                _shipDatas[i].itesmDatas = new ItemData[tmp.itemdata.Length];
                for (var j = 0; j < tmp.itemdata.Length; j++)
                {
                    _shipDatas[i].itesmDatas[j] = tmp.itemdata[j];
                }

            }

            for (int i = 0; i < 6; i++)
            {
                if (_shipDatas[i].shipData == null) continue;
                ShipObjects[i] = new ShipObject(0, i * 40, c, ShipData.GetShipBitmap(_shipDatas[i].shipData.image));
                SetAttackAnimation(i,0,true);
            }
        }

        public void View()
        {
            foreach (var shipData in _shipDatas)
            {
                if (shipData.shipData == null) continue;
                if (shipData.itesmDatas == null) continue;
                if (shipData.itesmDatas.Length == 0) continue;

                var ship = shipData.shipData;
                Console.WriteLine(ship.nameJP);

                var items = shipData.itesmDatas;
                var str = string.Join(",", items.Select(x => x.nameJP));
                Console.WriteLine(str);
                Console.WriteLine();
            }
        }
    }


    class Kcsvm
    {
        public Kcsvm(Control c, string myFleet = "test2.json")
        {

            var str = File.ReadAllText(myFleet);

            var jsonObject = JsonConvert.DeserializeObject<(ItemData[] itemdata, ShipData shipData)[]>(str);

            _myFleet = new Fleet(jsonObject, c);

            _myFleet.View();

            foreach (var ship in _myFleet.ShipObjects)
            {
                GameObjectManager.Add(ship);
            }
        }

        public void Draw()
        {

        }

        private Fleet _myFleet;
        private Fleet _enFleet;

    }
}
