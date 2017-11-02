using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipButtleSimrator
{

    public class ItemData
    {
        public int Skill;
        public int Level;
        public int ID;
        public string name;
        public string nameJP;
        public string added;
        public string type;
        public string btype;
        public string atype;
        public string FP;
        public string AA;
        public string RNG;
        public string improveType;
        public string fitclass;
        public string ACC;
        public string CANBbonus;
        public string TP;
        public string ASW;
        public string LOS;
        public string DIVEBOMB;
        public string b_image;
        public string ACCshell;
        public string ACCnb;
        public string EV;
        public bool noRedT;
        public string AR;
        public bool isnightscout;
        public bool isconcentrated;
        public string Pshell;
        public string Pnb;
        public string AAfleet;
        public string AAself;
        public bool isjet;
        public bool specialCutIn;

        public ItemData Clone()
        {
            return (ItemData) this.MemberwiseClone();
        }
    }
}
