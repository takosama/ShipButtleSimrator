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
using static ShipButtleSimrator.Kcsvm;

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

        public void SetSlotNum(int id, int slotID, int slotnum)
        {
            ShipData _;
           if(this._shipDatas[id].shipData==null) return;
            var ship = _shipDatas[id].shipData;
            if (ship.SLOTS == null) return;
            _shipDatas[id].shipData.SLOTS[slotID] = slotnum;
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
                if(jsonObject.Length<=i) continue;
              
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
        public Kcsvm(Control c, string myFleet ,string enFleet)
        {

            var str = File.ReadAllText(myFleet);

            var jsonObject = JsonConvert.DeserializeObject<(ItemData[] itemdata, ShipData shipData)[]>(str);

            _myFleet = new Fleet(jsonObject, c);

            _myFleet.View();

           



             str = File.ReadAllText(enFleet);

             jsonObject = JsonConvert.DeserializeObject<(ItemData[] itemdata, ShipData shipData)[]>(str);

            _enFleet = new Fleet(jsonObject, c);

            _enFleet.View();

          


        }

        public void StartBattle()
        {
            var reslut = new Search().ApplySeen(_myFleet, _enFleet);
            _myFleet = reslut.MyFleet;
            _enFleet = reslut.EnFleet;
            reslut.ViewConsole();

       var air=     new AirBattle();
            air.Init((SearchResult)reslut);
            air.ApplySeen(_myFleet, _enFleet);
        }

        private Fleet _myFleet;
        private Fleet _enFleet;

      public  enum Formation
        {
            単縦,
            複縦,
            輪形,
            梯形,
            単横
        }

    }



    interface IGameSeen
    {
        IGameSeenResult ApplySeen(Fleet myFleet, Fleet eFleet);

    }

    class AirBattle : IGameSeen
    {
        private bool IsSearchSucsessed = true;
        /// <summary>
        /// 呼ぶ前にInitすること
        /// </summary>
        /// <param name="myFleet"></param>
        /// <param name="eFleet"></param>
        /// <returns></returns>
    public IGameSeenResult  ApplySeen(Fleet myFleet, Fleet eFleet,Formation formation)
        {
            int myADV = ComputeAirDefValue(myFleet,true);
            int enADV = ComputeAirDefValue(eFleet,false);
            //制空状態計算
            var airstate = ComputeAirState(myADV, enADV, this.IsSearchSucsessed);
            //st1撃墜反映
            RefrectShootDown(true,myFleet, airstate);
            RefrectShootDown(false,eFleet, airstate);

            //触接可否判定 
            var tmpm = ComputeStartTouch(true, myFleet, airstate);
            var tmpe = ComputeStartTouch(false, eFleet, airstate);
        
            //触接機選択
            var mTouchPlaneResult = ComputeTouchPlane(true, myFleet, airstate, tmpm);
            var eTouchPlaneResult = ComputeTouchPlane(false, eFleet, airstate, tmpe);

            //触接攻撃力補正
            double mTouchCorrectionRate = ComputeTouchCorrectionPowerRate(mTouchPlaneResult);
            double eTouchCorrectionRate = ComputeTouchCorrectionPowerRate(eTouchPlaneResult);

            { }


            throw new NotImplementedException();
        }

        double ComputeFleetAirDefenceVal(Fleet f)
        {
            var itemdatas = new List<ItemData>();
            for(int i=0;i<6;i++)
            for (int j = 0; j < 5; j++)
            {
                var tmp = f.GetSlotData(i, j, out var dat);
                if (tmp == false) continue;
                itemdatas.Add(dat.Itemdata);
            }
           int FleetAirDefenceBounusVal= (int)itemdatas.Select(x =>
                ComputeWeaponCorrectionRate_AirDefence(x) * (x.AA == null ? 0 : double.Parse(x.AA)) +
                ComputeWeaponLevelCorrectionRate_AirDefence(x) * Math.Sqrt(x.Level)).Sum();

            //     各艦の艦隊対空ボーナス値 = [各装備の{ 装備倍率(艦隊防空) × 装備対空値 ＋ 改修係数(艦隊防空) × √(★改修値)}
            //     の合計]　(全装備合計後に端数切捨て)
            //   この表に載っていない装備種類の倍率は未検証です。
            //
            //   艦戦の改修効果は未検証です。
            //
            //   陣形補正
            //       陣形  倍率
            //       単縦陣、梯形陣、単横陣 1.0
            //   複縦陣 1.2
            //   輪形陣 1.6
        }

        double ComputeWeaponCorrectionRate_AirDefence(ItemData itemData)
        {
            //   装備倍率(艦隊防空)
            //   装備種類 倍率
            //   三式弾 0.6
            //   電探(大型 / 小型)   0.4
            //   高角砲 * 27、高射装置 0.35
            //   主砲(赤)、副砲(黄)、対空機銃、
            //   艦戦、艦爆、水偵    0.2
            if (itemData.type == "TYPE3SHELL") return 0.6;
            else if (itemData.type == "RADARXL") return 0.4;
            else if (itemData.type == "RADARL") return 0.4;
            else if (itemData.type == "RADARS") return 0.4;
            else if (itemData.type == "SECGUNAA") return 0.35;
            else if (itemData.type == "SECGUNSAA") return 0.35;
            else if (itemData.type == "MAINGUNS") return 0.2;
            else if (itemData.type == "MAINGUNM") return 0.2;
            else if (itemData.type == "MAINGUNL") return 0.2;
            else if (itemData.type == "MAINGUNXL") return 0.2;
            else if (itemData.type == "SECGUN") return 0.2;
            else if (itemData.type == "FIGHTER") return 0.2;
            else if (itemData.type == "DIVEBOMBER") return 0.2;
            else if (itemData.type == "SEAPLANE") return 0.2;
            else return 0;
        }

        double ComputeWeaponLevelCorrectionRate_AirDefence(ItemData itemData)
        {
            //   改修係数(艦隊防空)
            //   装備種類 倍率
            //   高角砲(高射装置有)  3
            //   高角砲(高射装置無)、高射装置 2
            //   電探(大型 / 小型)   1.5A_HAFD,
            //   対空機銃、副砲(黄)  0
            if ((itemData.type == "SECGUNAA" || itemData.type == "SECGUNSAA") && itemData.btype == "A_HAFD")
                return 3.0;
            if ((itemData.type == "SECGUNAA" || itemData.type == "SECGUNSAA"))
                return 2.0;
            if (itemData.type == "AAFD")
                return 2.0;
             if (itemData.type == "RADARXL") return 1.5;
             if (itemData.type == "RADARL") return  1.5;
             if (itemData.type == "RADARS") return  1.5;
            return 0;

        }

        //触接攻撃力補正　計算
        private double ComputeTouchCorrectionPowerRate(
            (bool isPlaneSelected, ItemData SelectedPlane) ComputeTouchPlaneResult)
        {
            if (ComputeTouchPlaneResult.isPlaneSelected == false)
            {
                Console.WriteLine("触接失敗");
                return 1;
            }
            Console.WriteLine("触接成功");
            Console.WriteLine(ComputeTouchPlaneResult.SelectedPlane.nameJP);

            var tmp = int.Parse(ComputeTouchPlaneResult.SelectedPlane.ACC);
            double rtn = 0;
            if (tmp == 0) rtn= 1.12;
            else if (tmp == 1) rtn = 1.12;
            else if (tmp == 2) rtn = 1.17;
            else rtn = 1.20;
            Console.WriteLine("補正値="+rtn);
            return rtn;
        }

        //触接機選択
        private (bool isPlaneSelected, ItemData SelectedPlane) ComputeTouchPlane(bool IsMyFleet, Fleet fleet,
            AirState airState,
            (bool isStart, double StartRate) ComputeStartTouchResult)
        {
            if (IsMyFleet == false)
                airState = ConvertToEnemy(airState);
            if (ComputeStartTouchResult.isStart == false)
                return (false, null);

            if (ComputeStartTouchResult.StartRate < Random.GetNext())
                return (false, null);

            //触接機判定開始
            var JoinTouch = new List<(ItemData itemData, int shipid, int slotid)>();
            for (var i = 0; i < 6; i++)
            for (var j = 0; j < 5; j++)
            {
                var tmp = fleet.GetSlotData(i, j, out var dat);
                if (tmp == false) continue;
                if (dat.caryNum == 0) continue;

                if (CanJoinTouch(dat.Itemdata) == false) continue;
                JoinTouch.Add((dat.Itemdata, i, j));
            }
            JoinTouch.Sort(Comparer);

            //命中の　航巡　　艦の順番で　昇順　slotのidで昇順にソート
            int Comparer((ItemData itemData, int shipid, int slotid) a, (ItemData itemData, int shipid, int slotid) b)
            {
                if (int.Parse(a.itemData.ACC) > int.Parse(b.itemData.ACC))
                    return -1;
                if (int.Parse(a.itemData.ACC) < int.Parse(b.itemData.ACC))
                    return 1;
                if (a.shipid < b.shipid)
                    return -1;
                if (a.shipid > b.shipid)
                    return 1;
                if (a.slotid < b.slotid)
                    return -1;
                if (a.slotid > b.slotid)
                    return 1;
                return 0;
            }

            foreach (var plane in JoinTouch)
            {
                var tmp = plane.itemData;
                var rate = double.Parse(tmp.LOS) * 0.07;
                //触接機の乱数判定
                if (Random.GetNext() <= rate)
                    return (true, tmp);
            }
            //すべて失敗でfalse
            return (false, null);
        }

        //触接開始判定
        (bool IsStart,double StartRate) ComputeStartTouch(bool IsMyFleet,Fleet fleet,AirState airState)
        {
            /*        A = ∑(int( (艦上偵察機、水上偵察機、大型飛行艇の索敵值)*sqrt(機數) ))
            B = 70 - (15 * 制空定數)
            制空定數：　確保 = 3空優 = 2劣勢 = 1均衡、喪失、航空戦が発生しない = 0

            触接開始判定 = if (A < (0 ~(B - 1)の一様な整数乱数) , 不發 , 触接開始 )

                → 触接開始率 = (int(A) + 1) / B
                */
            if (IsMyFleet == false)
                airState = ConvertToEnemy(airState);
            double a = 0;
            List<(ItemData ItemData, int carry)> JoinTouchStart = new List<(ItemData ItemData, int carry)>();
            for (int i = 0; i < 6; i++)
            for (int j = 0; j < 5; j++)
            {
                var tmp = fleet.GetSlotData(i, j, out var dat);
                if (tmp == false) continue;
                if (dat.caryNum == 0) continue;
                if(CanJoinTouchStart(dat.Itemdata)==false)continue;
                
                JoinTouchStart.Add((dat.Itemdata,dat.caryNum));
            }
            Console.WriteLine("触接可能は");
            foreach (var p in JoinTouchStart)
            {
                Console.WriteLine(p.ItemData.nameJP);
            }
            if (JoinTouchStart.Count != 0)
                a = JoinTouchStart.Select(x => (int) (double.Parse(x.ItemData.LOS) * Math.Sqrt(x.carry))).Sum();
            int b = (int)( 70 - (15.0 * GetAirDefValueConst(airState)));
            bool IsStart = false;
            double rate = 0;
            //触接開始判定 = if (A < (0 ~(B - 1)の一様な整数乱数) , 不發 , 触接開始 )
            if (a < Random.getNext(0, b - 1))
                IsStart = false;
            else
                IsStart = true;
            //触接開始率
            //                触接開始率 = (int(A) + 1) / B
            if (IsStart)
                rate = 1.0*((int) a + 1) / b;
            else
                rate = 0;
            return (IsStart, rate);
        }

        AirState ConvertToEnemy(AirState a)
         {
            if (a == AirState.Ensure) return AirState.Loss;
            if (a == AirState.Predominance) return AirState.Inferiority;
            if (a == AirState.Balance) return AirState.Balance;
            if (a == AirState.Inferiority) return AirState.Predominance;
            if (a == AirState.Loss) return AirState.Ensure;
            return 0;
        }
        int GetAirDefValueConst(AirState a)
        {
            if (a == AirState.Ensure) return 3;
           else if (a == AirState.Predominance) return 2;
           else if (a == AirState.Inferiority) return 1;
            else return 0;
        }
        bool CanJoinTouchStart(ItemData item)
        {
            //艦上偵察機、水上偵察機、大型飛行艇の索敵值
            //艦攻はいらない
            if (item.type == "FLYINGBOAT") return true;
            if (item.type == "SEAPLANE") return true;
            if (item.type == "CARRIERSCOUT") return true;
            return false;
        }

        bool CanJoinTouch(ItemData item)
        {
            if (item.type == "FLYINGBOAT") return true;
            if (item.type == "SEAPLANE") return true;
            if (item.type == "CARRIERSCOUT") return true;
            if (item.type == "TORPBOMBER") return true;
            return false;
        }

        private void RefrectShootDown(bool isMyFleet, Fleet fleet, AirState airstate)
        {
            for (int i = 0; i < 6; i++)
            for (int j = 0; j < 5; j++)
            {
                var tmp = fleet.GetSlotData(i, j, out var dat);
                if (tmp == false) continue;
                //カ号　三式指揮は落ちない　
                //航空戦未参加木をふるい落とす
                if (CanJoinAirButtle(dat.Itemdata) == false) continue;
                ;
                var shootDownNum = ComputeShootDownNum(dat.caryNum, isMyFleet, airstate);

                fleet.SetSlotNum(i, j, dat.caryNum - shootDownNum);
                Console.WriteLine(i+","+j+dat.Itemdata.nameJP+" slot"+dat.caryNum+"　撃墜"+ shootDownNum);
            }
        }

        //撃墜数計算
        int ComputeAirConst(AirState airState)
        {
            if (airState == AirState.Ensure) return 1;
            if (airState == AirState.Predominance) return 3;
            if (airState == AirState.Balance) return 5;
            if (airState == AirState.Inferiority) return 7;
            if (airState == AirState.Loss) return 10;
            return 10;
        }

        int ComputeShootDownNum(int slotnum, bool IsMyFleet, AirState airState)
        {

            var AirConst = ComputeAirConst(airState);

            double shootDownRand = 0;
            if (IsMyFleet)
                shootDownRand = GetRandDouble(0, (AirConst / 3.0), 0.1, 2) + AirConst / 4.0;
            else
                shootDownRand = 0.35 * GetRandDouble(0, 11 - AirConst, 1, 2) +
                                0.65 * GetRandDouble(0, 11 - AirConst, 1, 2);
            if ((int)(slotnum * shootDownRand / 10) > slotnum)
                return slotnum;
            else
                return (int) (slotnum * shootDownRand / 10);


        //            Stage1 擊墜數
            //
            //                制空定數 = 自軍制空状態 ：確保 = 1  、 劣勢 = 7  、  優勢 = 3  、  喪失 = 10  、  均衡 = 5
            //
            //            味方被擊墜乱数 = (0 ~制空定數 / 3)の0.1の倍数の一様な乱数 + 制空定數 / 4
            //
            //            相手被擊墜乱数 = 0.35 * ((0 ~(11 - 制空定數))の一様な整数乱数1) +0.65 * ((0 ~(11 - 制空定數))の一様な整数乱数2)
            //
            //            擊墜數 = if (int(機數 * 擊墜乱数 / 10) > 機數 , 機數 , int(機數 * 擊墜乱数 / 10))

            double GetRandDouble(double min, double max, double up_keisu, int scale)
            {
                List<int> list = new List<int>();
                int num = (int)Math.Pow(10.0, (double)scale);
                int num2 = (int)(min * (double)num);
                int num3 = (int)(max * (double)num);
                int num4 = (int)(up_keisu * (double)num);
                for (int i = num2; i <= num3; i += num4)
                {
                    list.Add(i);
                }
                var __AnonType = Enumerable.First(Enumerable.OrderBy(Enumerable.Select(list, (int value) => new
                {
                    value
                }), x => Guid.NewGuid()));
                return (double) __AnonType.value / (double)num;
            }

        }


        AirState ComputeAirState(int myADV, int enADV, bool isSearchSuccsess)
        {
            if (!isSearchSuccsess)
                myADV = 0;
            if (myADV == 0 && enADV == 0)
                return AirState.Balance;
            if (enADV == 0)
                return AirState.Ensure;
            if (enADV * 3 <= myADV) return AirState.Ensure;
            else if (enADV * 1.5 <= myADV) return AirState.Predominance;
            else if (enADV * 2.0 / 3.0 < myADV) return AirState.Balance;
            else if (enADV / 3.0 < myADV) return AirState.Inferiority;
            else
                return AirState.Loss;
        

        /*
        制空状態 自軍必要制空値 自軍被迎撃割合 敵機迎撃割合  触接 弾着観測射撃  夜間触接 * 20
        自軍 敵軍  自軍 敵軍  自軍 敵軍
        制空権喪失   1 / 3倍以下
            もしくは自艦隊索敵失敗 65 / 256～150 / 256  0 %～10 % 不   可 不   可 不   可
            航空劣勢（非表示）	1 / 3より大きい~2 / 3以下  45 / 256～105 / 256  0 %～40 % 可   可
            航空均衡（非表示）	2 / 3より大きい~3 / 2未満
            航空戦フェイズ未発生  30 / 256～75 / 256   0 %～60 % 不可  不可 * 21
        可 * 22
        航空優勢    3 / 2(1.5倍)以上~3倍未満    20 / 256～45 / 256   0 %～80 % 可   可 可   不 可   可
            制空権確保   3倍以上
    */
        }

        public enum AirState
        {
            Ensure,
            Predominance,
            Balance,
            Inferiority,
            Loss
        }
            

        public void Init(SearchResult res)
        {
            if (res.Result == SearchResult.result.失敗 || res.Result == SearchResult.result.失敗_艦載機使用せず)
                this.IsSearchSucsessed = false;
        }

        int ComputeAirDefValue(Fleet f,bool IsMyFleet)
        {
            var JoinPlanes = new List<(ItemData item, int slotID, int carryNum)>();
            for (int i = 0; i < 6; i++)
            for (int j = 0; j < 5; j++)
            {
                var tmp = f.GetSlotData(i, j, out var dat);
                if (tmp == false) continue;
                if (CanJoinAirButtle(dat.Itemdata) == false) continue;
                if(dat.caryNum==0) continue;
         if(dat.Itemdata.AA==null) dat.Itemdata.AA="0";      
                JoinPlanes.Add(dat);
            }

            int ComputeVal((ItemData item, int slotID, int carryNum) x)
            {
                var tmp= (int) Math.Floor(
                    (int.Parse(x.item.AA) + ComputeWeaponCorrection(x.item) * x.item.Level) * Math.Sqrt(x.carryNum) +
               (IsMyFleet?GetSkillCrrection(x.item):0));
                return tmp;
            }
            int rtn = JoinPlanes.Select(x =>ComputeVal(x)
               ).Sum();
            return rtn;
        }


        double ComputeWeaponCorrection(ItemData item)
        {
            if (item.type == "FIGHTER") return .2;
            if (item.type == "TORPBOMBER") return .25;
            if (item.type == "SEAPLANEFIGHTER") return .2;
            return 0;

        }

        double GetSkillCrrection(ItemData item)
        {
            int innerSkill = Skill.GetInnerSkill(item.Skill);
// 内部熟練ボーナス＝√(内部熟練度 / 10)
            var innerskillBounus = Math.Sqrt(innerSkill / 10);
            //制空ボーナス(艦戦・水戦・局戦/水爆)
            var AABounus = ComputeAABounus(item);


            return innerskillBounus + AABounus;
        }

        int ComputeAABounus(ItemData data)
        {
            int[] CorrectionsWithoutSeaBomber = {0, 0, 2, 5, 9, 14, 14, 22};
            int[] CorrectionSeaBomber = {0, 0, 1, 1, 1, 3, 3, 6};
            int skill = data.Skill;
            if (data.type == "FIGHTER") return CorrectionsWithoutSeaBomber[skill];
            if (data.type == "SEAPLANEFIGHTER") return CorrectionsWithoutSeaBomber[skill];
            if (data.type == "SEAPLANEBOMBER") return CorrectionSeaBomber[skill];
            return 0;
        }

        //カ号　三式指揮などは　参加できない

        bool CanJoinAirButtle(ItemData data)
        {
            if (data.type == "FIGHTER") return true;
            if (data.type == "TORPBOMBER") return true;
            if (data.type == "DIVEBOMBER") return true;
            if (data.type == "SEAPLANEFIGHTER") return true;
            if (data.type == "SEAPLANEBOMBER") return true;
            return false;
        }
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
            var result = GetResult(myFleet,eFleet);


            var tmp = GetResult(myFleet,eFleet);
    
           
            Command c=new Command();
            c.mes = "Search";
            c.value = tmp.Result.ToString();
            tmp.Commands = new[] {c};
            

          //  throw new Exception("未完成");
            return tmp;
        }

        public SearchResult GetResult(Fleet myFleet,Fleet enFleet)
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
            tmp.MyFleet = myFleet;
            tmp.EnFleet = enFleet;
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
