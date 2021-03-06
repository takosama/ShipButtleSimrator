﻿using System;
using System.Windows.Forms;
using ShipButtleSimrator.Animation;
using ShipButtleSimrator.GameObject;

namespace ShipButtleSimrator
{
    public partial class Form1 : Form
    {
        Kcsvm vm ;
        public Form1()
        {

            InitializeComponent();
            }

        private readonly ShipObject[] _s;

        private void Form1_Load(object sender, EventArgs e)
        {
            OpenFileDialog ofd=new OpenFileDialog();
            ofd.ShowDialog();
            var my = ofd.FileName;
            ofd.ShowDialog();
            var en = ofd.FileName;

            vm = new Kcsvm(this,my,en);
            vm.StartBattle();
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            GameObjectManager.Refresh();
        }

    }
}