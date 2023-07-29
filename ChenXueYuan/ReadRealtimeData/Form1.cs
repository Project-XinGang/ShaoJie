using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using OPCAutomation;

namespace ReadData
{
    public partial class Form1 : Form
    {

        // 主机IP
        string strHostIP = "";
        // 主机名称
        string strHostName = "";
        // OPCServer Object
        OPCServer KepServer;
        OPCItem KepItem;

        int serverHandle;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //获取本地计算机IP,计算机名称
            IPHostEntry IPHost = Dns.Resolve(Environment.MachineName);
            
            strHostIP = IPHost.AddressList[0].ToString();
            this.label1.Text = strHostIP.ToString();
            
            IPHostEntry ipHostEntry = Dns.GetHostByAddress(strHostIP);
            strHostName = ipHostEntry.HostName.ToString();
            this.label2.Text = strHostName;

            
            //获取本地计算机上的OPCServerName
            try
            {
                KepServer = new OPCServer();
                object serverList = KepServer.GetOPCServers(strHostName);

                foreach (string turn in (Array)serverList)
                {
                    this.ServerList.Items.Add(turn);
                }

                ServerList.SelectedIndex = 0;
                //btnConnServer.Enabled = true;
            }
            catch (Exception err)
            {
                MessageBox.Show("枚举本地OPC服务器出错：" + err.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // 连接OPC服务器
            try
            {
                if (!ConnectRemoteServer(label2.Text, ServerList.Text))
                {
                    return;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("初始化出错：" + err.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            OPCGroups groups = KepServer.OPCGroups;
            OPCGroup group = groups.Add("ReadGroup");
            OPCItems items = group.OPCItems;
            int clientHandle = 1;


            KepItem = items.AddItem("SJ8.设备 1.O2_OUT", clientHandle);



        }
        private bool ConnectRemoteServer(string remoteServerIP, string remoteServerName)
        {
            try
            {
                KepServer.Connect(remoteServerName, remoteServerIP);
                this.label3.Text = "连接成功";
            }
            catch (Exception err)
            {
                MessageBox.Show("连接远程服务器出现错误：" + err.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            object value, quality, timestamp;
            KepItem.Read((short)OPCDataSource.OPCDevice, out value, out quality, out timestamp);
            String str = value.ToString() + "  " + quality.ToString() + "  " + timestamp.ToString();
            this.label4.Text = str;
            serverHandle = KepItem.ServerHandle;
        }
    }
}
