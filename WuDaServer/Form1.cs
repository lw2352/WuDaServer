using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WuDaServer
{
    public partial class Form1 : Form
    {
        public MainUdpClass mainUdpClass = new MainUdpClass();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //初始化界面元素

            //test
            int i = ((int) 0x10 << 8) + (int) 0xE4;


            int battery = ( i- 3000) *3 / 100;//3v--0,6v--100
        }

        //开启服务
        private void btnStartService_Click(object sender, EventArgs e)
        {
            mainUdpClass.ServerIP = "192.168.3.152";
            mainUdpClass.ServerPort = 8090;
            mainUdpClass.bufferLen = 1024;
            mainUdpClass.checkDataBaseQueueTimeInterval = 300;
            //mainUdpClass.checkRecDataQueueTimeInterval = 100;
            mainUdpClass.checkSendDataQueueTimeInterval = 100;
            mainUdpClass.strSqlCfg = "server=localhost;user id=root;pwd=123456;port=3306;pooling=False;charset=utf8;database=dblaser";
            mainUdpClass.overTime = 5000;
            mainUdpClass.maxOverTime = 3;
            mainUdpClass.startID = 1;
            mainUdpClass.stopID = 2;
            //开启服务
            mainUdpClass.Start();
        }
    }
}
