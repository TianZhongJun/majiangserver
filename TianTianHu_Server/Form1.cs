using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine;
using Nancy;
using Nancy.Hosting.Self;
using MJ_FormsServer.DB;
using MJ_FormsServer.Logic;
using CardHelper;

namespace MJ_FormsServer
{
    public partial class Form1 : Form
    {
        public static Form1 singleton;
        IBootstrap bootstrap = BootstrapFactory.CreateBootstrap();
        NancyHost loginHost;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            singleton = this;

            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Thread tdInit = new Thread(new ThreadStart(() =>
            {
                btnStart.Enabled = false;
                btnStop.Enabled = false;
                btnRestart.Enabled = false;

                InitServer();

                btnStart.Enabled = true;
                btnStop.Enabled = true;
                btnRestart.Enabled = true;
            }));
            tdInit.IsBackground = true;
            tdInit.Start();
        }

        public void InitServer()
        {
            //启动登录服务器
            lblNacyStatus.Text = "登录服务器：启动中...";
            try
            {
                string url = ConfigurationManager.AppSettings["LoginServer"];
                loginHost = new NancyHost(new Uri(url));
                loginHost.Start();
                lblNacyStatus.Text = "登录服务器：启动成功 端口:" + url.Substring(url.LastIndexOf(":") + 1);
            }
            catch (Exception ex)
            {
                lblNacyStatus.Text = "登录服务器：启动失败\n" + ex.Message;
                return;
            }

            //启动游戏服务器
            lblMJtatus.Text = "游戏服务器：启动中...";
            try
            {
                MaJiangHelper.Init((initResult) =>
                {
                    if (initResult)
                    {
                        if (!bootstrap.Initialize())
                        {
                            lblMJtatus.Text = "游戏服务器：初始化失败";
                            return;
                        }
                    }
                    else
                    {
                        lblMJtatus.Text = "游戏服务器：初始化失败";
                        return;
                    }

                    try
                    {
                        if (bootstrap.Start() == StartResult.Success)
                            lblMJtatus.Text = "游戏服务器：启动成功 端口:" + bootstrap.GetServerByName("MJServer").Config.Port;
                        else
                        {
                            lblMJtatus.Text = "游戏服务器：启动失败";
                            return;
                        }

                    }
                    catch (Exception ex)
                    {
                        lblMJtatus.Text = "游戏服务器：启动失败\n" + ex.Message;
                        return;
                    }


                    //初始化数据库
                    lblDBStatus.Text = "数据库配置：初始化中...";
                    DBResult dbResult = DataBase.singleton.InitDB();
                    if (dbResult.code == 0)
                        lblDBStatus.Text = "数据库配置：初始化成功 类型:" + DataBase.singleton.dbType;
                    else
                        lblDBStatus.Text = "数据库配置：初始化失败\n" + dbResult.msg;
                });
            }
            catch (Exception ex)
            {
                lblMJtatus.Text = "游戏服务器：初始化失败\n" + ex.Message;
                return;
            }
        }

        public void StartServer()
        {
            //启动登录服务器
            lblNacyStatus.Text = "登录服务器：启动中...";
            try
            {
                string url = ConfigurationManager.AppSettings["LoginServer"];
                loginHost = new NancyHost(new Uri(url));
                loginHost.Start();
                lblNacyStatus.Text = "登录服务器：启动成功 端口:" + url.Substring(url.LastIndexOf(":") + 1);
            }
            catch (Exception ex)
            {
                lblNacyStatus.Text = "登录服务器：启动失败\n" + ex.Message;
                return;
            }

            //启动游戏服务器
            lblMJtatus.Text = "游戏服务器：启动中...";
            try
            {
                if (bootstrap.Start() == StartResult.Success)
                    lblMJtatus.Text = "游戏服务器：启动成功 端口:" + bootstrap.GetServerByName("MJServer").Config.Port;
                else
                {
                    lblMJtatus.Text = "游戏服务器：启动失败";
                    return;
                }

            }
            catch (Exception ex)
            {
                lblMJtatus.Text = "游戏服务器：启动失败\n" + ex.Message;
                return;
            }


            //初始化数据库
            lblDBStatus.Text = "数据库配置：初始化中...";
            DBResult dbResult = DataBase.singleton.InitDB();
            if (dbResult.code == 0)
                lblDBStatus.Text = "数据库配置：初始化成功 类型:" + DataBase.singleton.dbType;
            else
                lblDBStatus.Text = "数据库配置：初始化失败\n" + dbResult.msg;
        }

        public void StopServer()
        {
            //关闭数据库
            lblDBStatus.Text = "数据库配置：已关闭";
            //DBResult dbResult = DataBase.singleton.InitDB();
            //if (dbResult.code == 0)
            //    lblDBStatus.Text = "数据库配置：初始化成功 类型:" + DataBase.singleton.dbType;
            //else
            //    lblDBStatus.Text = "数据库配置：初始化失败\n" + dbResult.msg;

            //关闭游戏服务器
            lblMJtatus.Text = "游戏服务器：关闭中...";
            try
            {
                bootstrap.Stop();
                lblMJtatus.Text = "游戏服务器：已关闭";
            }
            catch (Exception ex)
            {
                lblMJtatus.Text = "游戏服务器：关闭失败\n" + ex.Message;
                return;
            }

            //关闭登录服务器
            lblNacyStatus.Text = "登录服务器：关闭中...";
            try
            {
                loginHost.Stop();
                lblNacyStatus.Text = "登录服务器：已关闭";
            }
            catch (Exception ex)
            {
                lblNacyStatus.Text = "登录服务器：启动失败\n" + ex.Message;
                return;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Thread tdStart = new Thread(new ThreadStart(() =>
            {
                btnStart.Enabled = false;
                btnStop.Enabled = false;
                btnRestart.Enabled = false;

                StartServer();

                btnStart.Enabled = true;
                btnStop.Enabled = true;
                btnRestart.Enabled = true;

            }));
            tdStart.IsBackground = true;
            tdStart.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Thread tdStop = new Thread(new ThreadStart(() =>
            {
                btnStart.Enabled = false;
                btnStop.Enabled = false;
                btnRestart.Enabled = false;

                StopServer();

                btnStart.Enabled = true;
                btnStop.Enabled = true;
                btnRestart.Enabled = true;
            }));
            tdStop.IsBackground = true;
            tdStop.Start();
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            Thread tdRestart = new Thread(new ThreadStart(() =>
            {
                btnStart.Enabled = false;
                btnStop.Enabled = false;
                btnRestart.Enabled = false;

                StopServer();
                StartServer();

                btnStart.Enabled = true;
                btnStop.Enabled = true;
                btnRestart.Enabled = true;
            }));
            tdRestart.IsBackground = true;
            tdRestart.Start();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
