using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ShipButtleSimrator
{
        public class ShipData
        {
            public int LV = 0;
            public double ID;
            public string name;
            public string image;
            public string nameJP;
            public string type;
            public double nid;
            public string added;
            public double HP;
            public double HPmax;
            public double FP;
            public double FPbase;
            public double TP;
            public double TPbase;
            public double AA;
            public double AAbase;
            public double AR;
            public double ARbase;
            public double EV;
            public double EVbase;
        /// <summary>
        /// 運
        /// </summary>
            public double ASW;
            public double ASWbase;
        /// <summary>
        /// 索敵
        /// </summary>
            public double LOS;
            public double LOSbase;
            public double LUK;
            public double LUKmax;
            public double RNG;
            public double SPD;
            public double[] SLOTS;
            public double fuel;
            public double ammo;
            public double next;
            public double prev;
            public double nextlvl;
            public double fitclass;
            public bool hasBuiltInFD;
            public bool alwaysOASW;
            public string canTorp;
            public string canASW;
            public double nightattack;
            public double OASWstat;
            public bool unknownstats;
            public bool iscarrier;
            public bool isASWlast;
            public double[] EQUIPS;
            public double TACC;
            public string canShell;
            public string canOpTorp;
            public double divebombWeak;
            public bool isPT;
            public string imageDam;
            public double installtype;
            public double LBWeak;
            public bool nightgun;
            public bool canlaser;
            public string shellPower;
            public int Cond = 49;
            public int FuelNow = 100;
            public int AmoNow = 100;
        
            internal ShipData Clone()
            {
                var rtn = (ShipData) this.MemberwiseClone();
                rtn.SLOTS = this.SLOTS == null ? null : (double[]) this.SLOTS.Clone();
                rtn.EQUIPS = this.EQUIPS == null ? null : (double[]) this.EQUIPS.Clone();
                return rtn;
            }

            public static string GetShipBitmap(string name)
            {
                if (!File.Exists(name))
                {
                    WebClient wc = new WebClient();
                    wc.DownloadFile("https://kc3kai.github.io/kancolle-replay/assets/icons/" + name, name);
                    wc.Dispose();
                    Console.WriteLine(name);
                }
                return name;
            }
    }
}
