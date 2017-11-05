using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using ShipButtleSimrator;
using ShipButtleSimrator.Animation;
using ShipButtleSimrator.GameObject;
using Console = System.Console;

namespace ShipButtleSimrator
{
    public class Fleet
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
        //    itemDatas.Clone();
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

            if (itemDatas[slotId] == null)
            {
                slotData.slotID = 0;
                slotData.Itemdata = null;
                slotData.caryNum = 0;
                return false;
            }

            if (_.SLOTS == null)
            {
                slotData.slotID = 0;
                slotData.Itemdata = null;
                slotData.caryNum = 0;
                return false;
            }

            if (_.SLOTS.Length <= slotId)
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

        public void SetEnterAnimation(int shipID, bool IsMyFleet)
        {
            this.ShipObjects[shipID].Animation.AddAnimation(AnimationNode.MoveLiner(0, (-200-shipID*40, 0)));

            if (IsMyFleet)
                this.ShipObjects[shipID].Animation.AddAnimation(AnimationNode.MoveLiner(300+80*shipID, (200 + shipID * 40, 0)));
            else
                this.ShipObjects[shipID].Animation.AddAnimation(AnimationNode.MoveLiner(300 + 450 * shipID, (200 + shipID * 40, 0)));
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
                SetEnterAnimation(i,true);
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
        public Kcsvm(Control c, string myFleet = "test.json")
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

        public void StartBattle()
        {
            var reslut = new Search().ApplySeen(_myFleet, _enFleet);
            _myFleet = reslut.MyFleet;
            _enFleet = reslut.EnFleet;
            reslut.ViewConsole();
        }

        private Fleet _myFleet;
        private Fleet _enFleet;

    }



    interface IGameSeen
    {
        IGameSeenResult ApplySeen(Fleet myFleet, Fleet eFleet);

    }




    /*
     *索敵成功/失敗


加重艦船索敵值 = int( 旗艦の表示索敵值 / 2 ) + int( 二番艦の表示索敵值 / 5 ) + ∑(int( その他艦船の表示索敵值 / 8 ))


航空索敵值 = ∑(1スロットの艦上偵察機、水上偵察機、水上爆擊機、大型飛行艇の機數) + ∑(1スロットの艦上偵察機、水上偵察機、水上爆擊機、大型飛行艇の熟練度補正) + 空母系の數の補正




熟練度補正 ：
艦載機の內部熟練度と関わって
(機數>0の艦上偵察機、水上偵察機、水上爆擊機、大型飛行艇の熟練度だけ計算する)
1スロットの熟練度補正 = int( 熟練ボーナス + sqrt(0.2*內部熟練度) )
sqrt(0.2*內部熟練度)
熟練ボーナス
[0,25)
+0
[25,55)
+5
[55,100)
+15
>=100
+30


空母系の數の補正：
艦隊の空母数と関わって
空母数 = 0 ： 補正 = 0
空母数 = 1 ： 補正 = 30
空母数 > 1 ： 補正 = 30 + 10*( 空母数 - 1 )

補足：
空母数 = 輕空母、空母、裝母の数


艦隊索敵防禦力 = int( 基本索敵防禦力*( (1.0 ~ 1.4) の0.1倍の一様な乱数) )

基本索敵防禦力：
S = 0            ：  基本索敵防禦力 = 0
S = (0,30]     ：  基本索敵防禦力 = int( 1 + S / 9 )
S = (30,120] ：  基本索敵防禦力 = int( 2 + S / 20 )
S >= 120      ：  基本索敵防禦力 = int( 6 + ( S - 120 ) / 40 )

S = ∑(艦隊の1スロットの艦上戦鬥機の機數)

//fin

索敵項 = 加重艦船索敵值 + 艦船の數の補正 - 20 + int( sqrt(10*航空索敵值) )

艦船の數の補正：
艦隊の艦船の數>=2：　補正值 = 艦隊の艦船の數 - 2
艦隊の艦船の數<2  ：　補正值 = 0


索敵成功判定：


段階1 = if( ( 航空索敵值 = 0 ) , 判定式A , 段階2 )

段階2 = if( (索敵項 > (0 ~ 19の一様な整数乱数) , 判定式B , 判定式C )


判定式A = if( (索敵項 > (0 ~ 19の一様な整数乱数)) , “成功(艦載機使用せず)” , “失敗(艦載機使用せず)” )

判定式B = if( ( 航空索敵值 > 艦隊索敵防禦力 ) , “成功” , “成功(未帰還機あり)” )

判定式C = if( ( 航空索敵值 > 艦隊索敵防禦力 ) , “未帰還” , “失敗” )



補足：
深海側の索敵は必ず成功


補足：

api_search
1=成功
2=成功(未帰還機あり)
3=未帰還
4=失敗
5=成功(艦載機使用せず)
6=失敗(艦載機使用せず)

https://github.com/andanteyk/ElectronicObserver/blob/master/ElectronicObserver/Other/Information/apilist.txt

索敵機の損失：

索敵結果は「成功（未帰還機あり）」、「未帰還」及び「失敗」の場合、艦隊中スロットごとの艦上偵察機、水上偵察機、水上爆擊機、大型飛行艇はランダムに０～２機が損失
-----------------------------------
     */
    class Search : IGameSeen
    {
        public IGameSeenResult ApplySeen(Fleet myFleet, Fleet eFleet)
        {
            var result = GetResult(myFleet);


            var tmp = GetResult(myFleet);
    
           
            Command c=new Command();
            c.mes = "Search";
            c.value = tmp.Result.ToString();
            tmp.Commands = new[] {c};
            

          //  throw new Exception("未完成");
            return tmp;
        }

        public SearchResult GetResult(Fleet myFleet)
        {
            var 加重艦船索敵值 = Compute加重艦船索敵值(myFleet);
            var 航空索敵值 = Compute航空索敵值(myFleet);
            var 艦隊索敵防禦力 = Compute基本索敵防禦力(myFleet);
            var 索敵項 = 加重艦船索敵值 + Compute艦船の數の補正(myFleet) - 20 + (int) (Math.Sqrt(10 * 航空索敵值));

            var result = GetResult(航空索敵值, 索敵項, 艦隊索敵防禦力);

            var tmp = new SearchResult();
            tmp.Result = result;
            tmp.加重艦船索敵值 = 加重艦船索敵值;
            tmp.索敵項 = 索敵項;
            tmp.航空索敵值 = 航空索敵值;
            tmp.艦隊索敵防禦力 = 艦隊索敵防禦力;
            return tmp;
        }

        private SearchResult.result GetResult(int 航空索敵值, int 索敵項, int 艦隊索敵防禦力)
        {
//段階1
            if (航空索敵值 == 0)
            {
                return 判定式A(索敵項);
            }
            else
            {
                return 段階2(索敵項, 航空索敵值, 艦隊索敵防禦力);
            }
        }

        SearchResult.result 段階2(int 索敵項,int 航空索敵值,int 艦隊索敵防禦力)
        {
            if (索敵項 > Random.getNext(0, 19))
            {
                return 判定式B(航空索敵值, 艦隊索敵防禦力);
            }
            else
            {
                return 判定式C(航空索敵值, 艦隊索敵防禦力);
            }
        }



        SearchResult.result 判定式A(int 索敵項)
        {
            if (索敵項 > Random.getNext(0, 19))
            {
                return SearchResult.result.成功_艦載機使用せず;
            }
            else
            {
                return SearchResult.result.失敗_艦載機使用せず;
            }
        }




        SearchResult.result 判定式B(int 航空索敵值,int 艦隊索敵防禦力)
        {
            if (航空索敵值 > 艦隊索敵防禦力)
            {
                return SearchResult.result.成功;
            }
            else
            {
                return SearchResult.result.成功_未帰還機あり;
            }
        }

        SearchResult.result 判定式C(int 航空索敵值, int 艦隊索敵防禦力)
        {
            if (航空索敵值 > 艦隊索敵防禦力)
            {
                return SearchResult.result.未帰還;
            }
            else
            {
                return SearchResult.result.失敗;
            }
        }

        int Compute艦船の數の補正(Fleet myFleet)
        {
            int shipCount = 0;
            for (int i = 0; i < 6; i++)
            {
                var isSucsess = myFleet.GetShipData(i, out var ship);
                if (!isSucsess) continue;
                shipCount++;
            }
            if (shipCount < 2) return 0;
            else return shipCount - 2;
        }

        int Compute艦隊索敵防禦力(Fleet myFleet)
        {
            var rnd = 0.1 * Random.getNext(10, 14);
            return (int) (Compute基本索敵防禦力(myFleet) * rnd);
        }

        private int Compute基本索敵防禦力(Fleet myFleet)
        {
            var 合計戦闘機機数 = Compute合計戦闘機機数(myFleet);

            if (合計戦闘機機数 == 0) return 0;
            if (合計戦闘機機数 < 30) return 1 + 合計戦闘機機数 / 9;
            if (合計戦闘機機数 < 120) return 2 + 合計戦闘機機数 / 20;
            return 6 + (合計戦闘機機数 - 120) / 40;
        }


        private int Compute合計戦闘機機数(Fleet myFleet)
        {
            var 合計戦闘機数 = 0;
            for (var i = 0; i < 6; i++)
            for (var j = 0; j < 5; j++)
            {
                var IsSucsess = myFleet.GetSlotData(i, j, out var slot);
                if (IsSucsess == false) continue;
                if (!Is戦闘機(slot.Itemdata)) continue;
                合計戦闘機数 += slot.caryNum;
            }
            return 合計戦闘機数;
        }



        private int Compute加重艦船索敵值(Fleet myFleet)
        {
            myFleet.GetShipData(0, out var 旗艦);
            myFleet.GetShipData(1, out var 二番艦);

            var 旗艦の表示索敵值 = 旗艦.LOS;
            var 二番艦の表示索敵值 = 二番艦?.LOS??0;

            var その他艦船の表示索敵值合計 = 0.0;
            for (var i = 2; i < 6; i++)
            {
                ShipData _;
                myFleet.GetShipData(i, out _);
                if (_ != null)
                    その他艦船の表示索敵值合計 += _.LOS;
            }

            return (int) (旗艦の表示索敵值 / 2) + (int) (二番艦の表示索敵值 / 5) + (int) (その他艦船の表示索敵值合計 / 8);
        }


        private int Compute航空索敵值(Fleet myFleet)
        {
            return Get合計索敵機基数(myFleet) + Compute合計熟練度補正(myFleet) + Compute空母系の數の補正(myFleet);
        }


        private bool IsCarrier(ShipData ship)
        {
            if (ship.type == "CVL") return true;
            if (ship.type == "CV") return true;
            if (ship.type == "CVB") return true;
            return false;
        }

        private int Compute空母系の數の補正(Fleet myFleet)
        {
            var 空母系の數 = 0;
            for (var i = 0; i < 6; i++)
            {
                var isSucsess = myFleet.GetShipData(i, out var shipData);
                if (!isSucsess) continue;
                if (!IsCarrier(shipData)) continue;
                空母系の數++;
            }
            if (空母系の數 == 0) return 0;
            if (空母系の數 == 1) return 30;
            return 空母系の數 - 1 + 10 + 30;
        }


        private int Compute合計熟練度補正(Fleet myFleet)
        {
            var 合計熟練度補正 = 0.0;
            for (var i = 0; i < 6; i++)
            for (var j = 0; j < 5; j++)
            {
                var IsSucsess = myFleet.GetSlotData(i, j, out var slot);
                if (!IsSucsess) continue;
                if (slot.caryNum <= 0) continue;
                if (!Is索敵機(slot.Itemdata)) continue;
                var 內部熟練度 = Skill.GetInnerSkill(slot.Itemdata.Skill);
                var 熟練度補正 = (int) (Compute熟練ボーナス(內部熟練度) + Math.Sqrt(0.2 * 內部熟練度));
                合計熟練度補正 += 熟練度補正;
            }
            return (int) 合計熟練度補正;
        }

        public int Compute熟練ボーナス(int innerSkill)
        {
            if (innerSkill < 25)
                return 0;
            else if (innerSkill < 55)
                return 5;
            else if (innerSkill < 100)
                return 15;
            return 30;
        }

        bool Is索敵機(ItemData itemData)
        {
            //艦偵
            if (itemData.type == "CARRIERSCOUT")
                return true;
            if (itemData.type == "CARRIERSCOUT2")
                return true;
            //大艇カタリナ
            if (itemData.type == "FLYINGBOAT")
                return true;
            //水偵
            if (itemData.type == "SEAPLANE")
                return true;
            //水爆
            if (itemData.type == "SEAPLANEBOMBER")
                return true;
            return false;
        }

        bool Is戦闘機(ItemData itemData)
        {
            if (itemData.type == "FIGHTER")
                return true;
            return false;
        }

        private int Get合計索敵機基数(Fleet myFleet)
        {
            var 合計索敵機数 = 0;
            for (var i = 0; i < 6; i++)
            for (var j = 0; j < 5; j++)
            {
                var IsSucsess = myFleet.GetSlotData(i, j, out var slot);
                if (IsSucsess == false) continue;

                if (!Is索敵機(slot.Itemdata)) continue;
                合計索敵機数 += slot.caryNum;
            }
            return 合計索敵機数;
        }
    }

    static class Skill
    {
        private static int[] InnnerSkills = {0, 10, 25, 40, 55, 70, 85, 120};

        public static int GetInnerSkill(int skill)
        {
            if (skill >= InnnerSkills.Length) return 0;
            if (skill < 0) return 0;
            return InnnerSkills[skill];
        }
    }

    public class SearchResult:IGameSeenResult
    {
        public void ViewConsole()
        {
            Console.WriteLine("加重艦船索敵值="+加重艦船索敵值);
            Console.WriteLine("航空索敵值=" + 航空索敵值);
            Console.WriteLine("艦隊索敵防禦力=" + 艦隊索敵防禦力);
            Console.WriteLine("索敵項=" + 索敵項);
            Console.WriteLine("コマンドを発行しました");
            Commands[0].View();
        }
       public int 加重艦船索敵值{get;set;}
       public int 航空索敵值    {get;set;} 
       public int 艦隊索敵防禦力{get;set;} 
       public int 索敵項 { get;set; } 
        public Fleet EnFleet { get; set; }
        public Fleet MyFleet { get; set; }
        public Command[] Commands { get; set; } = null;
        public result Result { get; set; }
    public    enum result
        {
           成功=1,
            成功_未帰還機あり=2,
            未帰還=3,
            失敗=4,
            成功_艦載機使用せず=5,
            失敗_艦載機使用せず=6
        }
    }


    interface IGameSeenResult
    {
        void ViewConsole();
        Command[] Commands { get; set; }
        Fleet EnFleet { get; set; }
        Fleet MyFleet { get; set; }
    }

   public class Command
    {
        public string mes;
        public string value;
        public string[] parameters;

        public void View()
        {
            Console.WriteLine("mes="+mes);
            Console.WriteLine("value="+value);
            if(parameters!=null)
            Console.WriteLine("parameters="+string.Join(",",parameters));
        }
    }

    static class Random
    {
          static System.Random rnd=null;

        static public int getNext(int min,int max)
        {
            if(rnd==null) Init();
         return   rnd.Next(min, max);
        }

        static public double GetNext()
        {
            if (rnd == null) Init();
            return rnd.NextDouble();
        }


        static  void Init()
        {
            rnd = new System.Random();
        }


    }

}
