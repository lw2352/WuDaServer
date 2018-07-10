using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
            //test code
            //int battery = (1000) /30;
            //battery = battery < 100 ? battery : 100;
            //double la = 114.4543166;
            //string str=la.ToString("F6");

            //初始化界面元素
            try
            {
                int index = 0;
                string configIP = System.Configuration.ConfigurationManager.AppSettings["ServerIP"];
                //获取主机名
                string HostName = Dns.GetHostName();
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    //从IP地址列表中筛选出IPv4类型的IP地址
                    //AddressFamily.InterNetwork表示此IP为IPv4,
                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        string localIP = IpEntry.AddressList[i].ToString();
                        if (localIP == configIP)
                        {
                            index = i;
                        }

                        comboBoxIP.Items.Add(localIP);
                        UtilClass.writeLog("本地IP为" + localIP);
                    }
                }

                comboBoxIP.SelectedIndex = index;
                textBoxStartID.Text = System.Configuration.ConfigurationManager.AppSettings["startID"];
                textBoxStopID.Text = System.Configuration.ConfigurationManager.AppSettings["stopID"];

#if DEBUG
                btnSearch.Visible = true;
                btnGetTimeAndLevel.Visible = true;
                btnResetLevel.Visible = true;
#else
                btnSearch.Visible = false;
                btnGetTimeAndLevel.Visible = false;
                btnResetLevel.Visible = false;
#endif
            }
            catch (Exception ex)
            {
                UtilClass.writeLog(ex.ToString());
            }
        }

        //开启服务
        private void btnStartService_Click(object sender, EventArgs e)
        {
            mainUdpClass.ServerIP = comboBoxIP.SelectedItem.ToString();

            mainUdpClass.ServerPort =
                Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ServerPort"]);
            mainUdpClass.bufferLen =
                Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["bufferLen"]);
            mainUdpClass.checkDataBaseQueueTimeInterval =
                Convert.ToInt32(
                    System.Configuration.ConfigurationManager.AppSettings["checkDataBaseQueueTimeInterval"]);
            mainUdpClass.checkSendDataQueueTimeInterval =
                Convert.ToInt32(
                    System.Configuration.ConfigurationManager.AppSettings["checkSendDataQueueTimeInterval"]);
            mainUdpClass.strSqlCfg = System.Configuration.ConfigurationManager.AppSettings["strSqlCfg"];
            mainUdpClass.overTime = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["overTime"]);
            mainUdpClass.maxOverTimes =
                Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["maxOverTimes"]);

            mainUdpClass.startID = Convert.ToInt32(textBoxStartID.Text);
            mainUdpClass.stopID = Convert.ToInt32(textBoxStopID.Text);
            mainUdpClass.channel = Convert.ToByte(System.Configuration.ConfigurationManager.AppSettings["channel"]);
            //把用户更改存入config文件
            if (System.Configuration.ConfigurationManager.AppSettings["ServerIP"] != mainUdpClass.ServerIP)
            {
                SaveConfig("ServerIP", mainUdpClass.ServerIP);
            }

            if (System.Configuration.ConfigurationManager.AppSettings["startID"] != textBoxStartID.Text)
            {
                SaveConfig("startID", textBoxStartID.Text);
            }

            if (System.Configuration.ConfigurationManager.AppSettings["stopID"] != textBoxStartID.Text)
            {
                SaveConfig("stopID", textBoxStopID.Text);
            }

            //开启服务
            if (mainUdpClass.Start() == true)
            {
                btnStartService.Text = "已开启";
                //禁用控件
                btnStartService.Enabled = false;
                comboBoxIP.Enabled = false;
                textBoxStartID.Enabled = false;
                textBoxStopID.Enabled = false;
            }
            else
            {
                MessageBox.Show("开启失败，请退出程序，检查网络IP和防火墙！");
            }
        }

        //保存配置
        public static bool SaveConfig(string key, string value)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings[key].Value = value;
                config.Save();
                return true;
            }
            catch (Exception ex)
            {
                UtilClass.writeLog(ex.ToString());
                return false;
            }
        }

        //搜索设备
        private void btnSearch_Click(object sender, EventArgs e)
        {
            DbClass.UpdateCmd("0000011E", "cmdName", "search");
        }

        //获取触发时刻和能级
        private void btnGetTimeAndLevel_Click(object sender, EventArgs e)
        {
            MySQLDB.InitDb();
            //string strResult = "";
            MySqlParameter[] parmss = null;
            string strSQL = "";
            bool IsDelSuccess = false;
            strSQL =
                "Update tcommand SET cmdName = 'getTimeAndLevel'";

            IsDelSuccess = MySQLDB.ExecuteNonQry(strSQL, parmss);
        }

        //能级复位
        private void btnResetLevel_Click(object sender, EventArgs e)
        {
            MySQLDB.InitDb();
            //string strResult = "";
            MySqlParameter[] parmss = null;
            string strSQL = "";
            bool IsDelSuccess = false;
            strSQL =
                "Update tcommand SET cmdName = 'resetLevel'";

            IsDelSuccess = MySQLDB.ExecuteNonQry(strSQL, parmss);
        }
    }
}
