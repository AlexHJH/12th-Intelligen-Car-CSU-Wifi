using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using Alex_TCP;
using System.Threading;
using System.IO.Ports;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using Emgu.CV;

namespace Alex_TCP
{
    public partial class Form1 : Form
    {
        private int ConnectType = 0;                      ///连接方式
        private int ShowType = 0;                         ///界面输出方式
        private bool IsOpen = false;                      //是否开启传输
        private static IPAddress HostIPAddress;
        private static int SeverIPPort = 0;
        private static IPAddress ClientIPAddress;
        private static int ClientIPPort = 0;
        object obj = new object();
        private AlexServerSocketObject tcpServer = null;
        private AlexClientSocketObject tcpClient = null;
        private SerialPort sp = null;
        private Alex_WaveShowBmp WaveToShow = new Alex_WaveShowBmp();
        private Alex_ImageProcess PicToShow = new Alex_ImageProcess();
        private Alex_RouteShow RouteToShow = new Alex_RouteShow();
        private Alex_Text TextToShow = new Alex_Text();
        private List<byte> ListBuff = new List<byte>();           ///存储列表
        private long ListIndex = 0;                               //链表索引
        private int NowFrameCount = 0;
        private int FrameCount = 0;



        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SettingsLoad();
            WaveToShow.Wave_init(60, 20, ShowWave_Box.Width, ShowWave_Box.Height);     //////波形图控件初始化
            RouteToShow.RouteShow_init(RoutePicBox.Width, RoutePicBox.Height);
            ConnectTypeTab.SelectedIndex = 0;                           /////连接选项
            ChooseTab.SelectedIndex = 0;                                /////显示选项
            ConnectType = 0;                                            /////连接状态
            ShowType = 0;                                                /////显示状态
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            HostIPAddress = ipHostInfo.AddressList[0];                  ////得到本机IP地址
            string ipStr = HostIPAddress.ToString();
            TCPSeverIP_textbox.Text = ipStr;
            TCPSeverIP_textbox.Enabled = false;                         ////锁定本机IP
            SeverIPPort = Convert.ToInt16(TCPSeverPort_textbox.Text);
            ClientIPPort = Convert.ToInt16(TCPClientPort_textbox.Text);
            ShowWave_Box.MouseWheel += new MouseEventHandler(WavePicBox_MouseEnter);
            RoutePicBox.MouseWheel += new MouseEventHandler(WavePicBox_MouseEnter);
            ShowWave_Box.MouseMove += new MouseEventHandler(ShowwaveMouseMove);
            RoutePicBox.MouseMove += new MouseEventHandler(ShowwaveMouseMove);
            ShowWave_Box.MouseUp += new MouseEventHandler(ShowwaveMouseUP);
            RoutePicBox.MouseUp += new MouseEventHandler(ShowwaveMouseUP);
        }


        private void WavePicBox_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.Delta > 20 || e.Delta < -20)
            {
                if (ShowType == 1)
                    WaveToShow.UintyChange(e.Delta);
                else if (ShowType == 2)
                    RouteToShow.RouteUintChange(e.Delta);
            }
        }

        /// <summary>
        /// 用于UI的禁止
        /// </summary>
        private void DisplayLock()
        {
            PicTypeCombox.Enabled = false;
            Serialportbox.Enabled = false;
            SerialBaudbox.Enabled = false;
            SerialOpen_Button.Enabled = false;            ////禁止
            TCPSeverOpen_Button.Enabled = false;
            TCPSeverPort_textbox.Enabled = false;
            TCPClientOpen_Button.Enabled = false;
            TCPClientIP_textbox.Enabled = false;
            TCPClientPort_textbox.Enabled = false;
        }
        /// <summary>
        /// 用于UI的启用
        /// </summary>
        private void DisplayUnLock()
        {
            PicTypeCombox.Enabled = true;
            Serialportbox.Enabled = true;
            SerialBaudbox.Enabled = true;
            SerialOpen_Button.Enabled = true;            ///启用
            TCPSeverOpen_Button.Enabled = true;
            TCPSeverPort_textbox.Enabled = true;
            TCPClientOpen_Button.Enabled = true;
            TCPClientIP_textbox.Enabled = true;
            TCPClientPort_textbox.Enabled = true;
        }

        private Point Cursorposition;
        private bool MouseMoveflag;
        private void ShowwaveMouseMove(object sender, MouseEventArgs e)
        {
            int px = 0, py = 0;
            int detpx = 0, detpy = 0;

            if (e.Button == MouseButtons.Left)
            {
                if (MouseMoveflag)
                {
                    px = Cursor.Position.X;
                    py = Cursor.Position.Y;
                    detpx = px - Cursorposition.X;
                    detpy = py - Cursorposition.Y;
                    Cursorposition.X = px;
                    Cursorposition.Y = py;
                    if (ShowType == 1)
                    {
                        WaveToShow.YStartChange(detpy);
                        WaveToShow.XStartChange(detpx);
                    }
                    else if(ShowType == 2)
                    {
                        RouteToShow.RouteXYMove(detpx, detpy);
                    }
                }
                else
                {
                    Cursorposition.X = Cursor.Position.X;
                    Cursorposition.Y = Cursor.Position.Y;
                    MouseMoveflag = true;
                }
            }
        }

        private void ShowwaveMouseUP(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                MouseMoveflag = false;
        }

        private void ShowWaveChange(Bitmap bp)
        {
            try
            {
                {
                    if (ShowWave_Box.InvokeRequired)
                    {
                        PicInvoke _myinvoke = new PicInvoke(ShowWaveChange);
                        ShowWave_Box.BeginInvoke(_myinvoke, new object[] { bp });
                    }
                    else
                    {
                        ShowWave_Box.Image = bp;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
            }
        }

        private void ShowRouteChange(Bitmap bp)
        {
            try
            {
                {
                    if (RoutePicBox.InvokeRequired)
                    {
                        PicInvoke _myinvoke = new PicInvoke(ShowRouteChange);
                        RoutePicBox.BeginInvoke(_myinvoke, new object[] { bp });
                    }
                    else
                    {
                        RoutePicBox.Image = bp;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
            }
        }
        /// <summary>
        /// 服务器接收
        /// </summary>
        /// <param name="sks"></param>
        private void SeverRec(Sockets sks)
        {
            this.Invoke(new ThreadStart(
                delegate
                {
                    if (sks.ex != null)
                    {
                        if (sks.ClientDispose)
                        {
                        }
                    }
                    else
                    {
                        if (sks.NewClientFlag)
                        {
                            ShowNews("客户端已连接");
                        }
                        else if (sks.Offset == 0)
                        {
                            ShowNews("客户端下线");
                        }
                        else
                        {
                            int RecvNum = sks.Offset;                   ////单包收到的大小
                            byte[] buffer = new byte[RecvNum];
                            Array.Copy(sks.RecBuffer, buffer, RecvNum);
                            ListBuff.AddRange(buffer);                   ////复制到列表
                            ListLength = ListBuff.LongCount();
                        }
                    }
                }
                ));
        }        /// //TCP服务器接收
                 /// <summary>
                 /// TCP客户端启动按键
                 /// </summary>
                 /// <param name="sender"></param>
                 /// <param name="e"></param>
        private void TCPClientOpen_Button_Click(object sender, EventArgs e)
        {
            DisplayLock();
            tcpClient = new AlexClientSocketObject();
            
            try
            {
                string IPAdd = TCPClientIP_textbox.Text.Trim();
                string IPPD = TCPClientPort_textbox.Text.Trim();
                ClientIPAddress = IPAddress.Parse(IPAdd);
                ClientIPPort = int.Parse(IPPD);
                tcpClient.InitSocket(ClientIPAddress, ClientIPPort);
                tcpClient.Start();
                AlexClientSocketObject.pushSockets = new PushSockets(ClientRec);
                ShowStatus("连接成功!");
                IsOpen = true;
            }
            catch (Exception ex)
            {
                DisplayUnLock();
                ShowStatus(string.Format("连接失败！"));    ////开线程调用UI
            }
        }
        private void TCPSeverOpen_Button_Click(object sender, EventArgs e)
        {
            SeverIPPort = Convert.ToInt32(TCPSeverPort_textbox.Text);
            if (SeverIPPort < 0) return;
            tcpServer = new AlexServerSocketObject();
            DisplayLock();
            try
            {
                tcpServer.InitSocket(HostIPAddress, SeverIPPort);
                tcpServer.Start();
                ShowStatus(string.Format("服务器监听启动成功！监听：{0}:{1}", HostIPAddress, SeverIPPort.ToString()));  ////开线程调用UI
                ShowNews("无客户端连入");
                AlexServerSocketObject.pushSockets = new PushSockets(SeverRec);
                IsOpen = true;
            }
            catch (Exception ex)
            {
                DisplayUnLock();
                ShowStatus(string.Format("服务器启动失败！"));    ////开线程调用UI
            }
        }
        /// <summary>
        /// 串口开启按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialOpen_Button_Click(object sender, EventArgs e)
        {
            DisplayLock();
            try {
                if (CheckPortSetting())     ////串口检测正确
                {
                    sp = new SerialPort();
                    sp.PortName = Serialportbox.Text.Trim();
                    sp.BaudRate = Convert.ToInt32(SerialBaudbox.Text.Trim());
                    sp.StopBits = StopBits.One;          /////1位停止位
                    sp.Parity = Parity.None;             ////无奇偶校验
                                                         //sp.ReadTimeout = -1;
                                                         // sp.RtsEnable = true;
                    sp.DataBits = 8;                     ////8位数据
                    ShowStatus("串口打开");
                    sp.Open();
                    IsOpen = true;                                        /////按照默认的选项进行设置
                    sp.DataReceived += new SerialDataReceivedEventHandler(SerialPortRec);
                }
                else
                {
                    DisplayUnLock();
                    ShowStatus("串口未设置");
                    return;
                }
            }
            catch(Exception)
            {
                DisplayUnLock();
                ShowStatus("串口打开错误");
                return;
            }
        }
        /// <summary>
        /// 服务器结束按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPSeverEnd_Button_Click(object sender, EventArgs e)
        {
            tcpServer.Stop();
            DisplayUnLock();
            ShowStatus("服务器程序停止成功！");            ////开线程调用UI
            TCPSeverOpen_Button.Enabled = true;
            TCPSeverPort_textbox.Enabled = true;
            IsOpen = false;
        } 

        /// <summary>
        /// 10MS时钟
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        long ListLength = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            DatatTrans(ListLength);
            switch (ShowType)
            {
                case 0:
                    FrameCount = (int)PicToShow.ListPic.LongCount();
                    NowFrameCount = PicToShow.NowFrameCount;
                    break;
                case 1:
                    FrameCount = (int)WaveToShow.ListWave.LongCount();
                    NowFrameCount = WaveToShow.NowFrameCount;
                    break;
                case 2:
                    FrameCount = (int)RouteToShow.ListPoint.LongCount();
                    NowFrameCount = RouteToShow.NowFrameCount;
                    break;
                default:
                    FrameCount = 0;
                    break;
            }
            AllFrameCount.Text = "/ " + FrameCount.ToString();
            if (IsFrameBarClick == false)
            {
                if (FrameCount == 0)
                    FrameBar.Value = 0;
                else
                    FrameBar.Value = FrameBar.Maximum * NowFrameCount / FrameCount;
            }
        }

        /// <summary>
        /// 主机信息
        /// </summary>
        /// <param name="msg"></param>
        private void ShowStatus(string msg)      //状态线程调用UI
        {
            try
            {
                {
                    if (Status_Label.InvokeRequired)
                    {
                        StrInvoke _myinvoke = new StrInvoke(ShowStatus);
                        Status_Label.BeginInvoke(_myinvoke, new object[] { msg });
                    }
                    else
                    {
                        Status_Label.Text = msg;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
            }
        }
        /// <summary>
        /// 外部机信息
        /// </summary>
        /// <param name="msg"></param>
        private void ShowNews(string msg)
        {
            try
            {
                {
                    if (ClientStatusLabel.InvokeRequired)
                    {
                        StrInvoke _myinvoke = new StrInvoke(ShowNews);
                        ClientStatusLabel.BeginInvoke(_myinvoke, new object[] { msg });
                    }
                    else
                    {
                        ClientStatusLabel.Text = msg;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
            }
        }
        /// <summary>
        /// 连接方式选中响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectTypeTab_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (IsOpen)
            {
                ConnectTypeTab.SelectedIndex = ConnectType;
                return;
            }
            ConnectType = ConnectTypeTab.SelectedIndex;
            if (ConnectType == 0)              //////tcp服务器得到本机地址
            {
                IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                HostIPAddress = ipHostInfo.AddressList[0];                  ////得到本机IP地址
                string ipStr = HostIPAddress.ToString();
                TCPSeverIP_textbox.Text = ipStr;
                TCPSeverIP_textbox.Enabled = false;                         ////锁定本机IP
                //TCPSeverPort_textbox.Text = "5000";
                SeverIPPort = Convert.ToInt16(TCPSeverPort_textbox.Text);
            }
            else if (ConnectType == 1)     ///////tcp客户端初始换端口IP
            {
                //TCPClientIP_textbox.Text = "192.168.5.100";
                //TCPClientPort_textbox.Text = "5000";
            }
            else if (ConnectType == 2)        ///////串口得到端口号
            {
                bool COMExist = false;
                Serialportbox.Items.Clear();
                for (int i = 0; i < 15; i++)
                {
                    try
                    {
                        SerialPort sp = new SerialPort("COM" + (i + 1).ToString());
                        sp.Open();
                        sp.Close();
                        Serialportbox.Items.Add("COM" + (i + 1).ToString());
                        COMExist = true;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                if (COMExist)
                {
                    Serialportbox.SelectedIndex = 0;
                }
                SerialBaudbox.SelectedIndex = 0;
            }
        }
        /// <summary>
        /// 显示方式变化响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChooseTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowType = ChooseTab.SelectedIndex;

        }
        /// <summary>
        /// 串口检测
        /// </summary>
        /// <returns></returns>
        private bool CheckPortSetting()
        {
            if (SerialBaudbox.Text.Trim() == "") return false;
            if (Serialportbox.Text.Trim() == "") return false;
            return true;
        }
        /// <summary>
        /// 串口接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPortRec(object sender, SerialDataReceivedEventArgs e)
        {
            int RecvNumn = sp.BytesToRead;
            byte[] buffer = new byte[RecvNumn];
            sp.Read(buffer, 0, RecvNumn);
            ListBuff.AddRange(buffer);         ////复制到列表      
            ListLength = ListBuff.LongCount();
        }/// 串口接收



        private void SerialEnd_Button_Click(object sender, EventArgs e)
        {
            if(sp == null)
            {
                ShowStatus("串口未打开");
                return;
            }
            DisplayUnLock();
            sp.Close();
            IsOpen = false;
            sp.Dispose();                                     //////释放资源
            sp = null;
            Serialportbox.Enabled = true;
            SerialBaudbox.Enabled = true;
            SerialOpen_Button.Enabled = true;            ////禁止
            ShowStatus("串口关闭");
        }

        /// <summary>
        /// 文本发送按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendText_Button_Click(object sender, EventArgs e)
        {
            if(IsOpen)
            {
                switch(ConnectType)
                {
                    case 0:
                        if(SendText_Text != null)
                            tcpServer.SendToAll(TextToShow.TextSend(SendText_Text.Text.ToString()));
                        break;
                    case 1:
                        if (SendText_Text != null)
                            tcpClient.SendData(TextToShow.TextSend(SendText_Text.Text.ToString()));
                            break;
                    case 2:
                        if (SendText_Text != null)
                            sp.WriteLine(TextToShow.TextSend(SendText_Text.Text.ToString()));
                        break;
                }
            }
            else
            {
                ShowStatus("未连接");
            }
        }

        private void ShowText(string msg)
        {
            try
            {
                {
                    if (ShowText_Text.InvokeRequired)
                    {
                        StrInvoke _myinvoke = new StrInvoke(ShowText);
                        ShowText_Text.BeginInvoke(_myinvoke, new object[] { msg });
                    }
                    else
                    {
                        ShowText_Text.Text += msg;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
            }

        }
        public int TextBoxCount = 0;
        /// <summary>
        /// ////时钟2定时显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Tick(object sender, EventArgs e)
        {
            
            switch (ShowType)
            {
                case 0:
                    ShowPic(PicToShow.PicShow());
                    try {
                        ThreadTextBox.Text = PicToShow.OSTUThread.ToString();
                        if (PicToShow.MatPerspective != null)
                        {
                            string str = null;
                            for (int i = 0; i < PicToShow.MatPerspective.Length; i++)
                            {
                                if (i % 3 == 0 && i != 0) str += "\r\n";
                                str += PicToShow.MatPerspective[i].ToString("F3") + ", ";
                            }
                            str += "\r\n缩小倍数:" + PicToShow.MatDouble.ToString("F1");
                            TempTextbox.Text = str;
                        }
                    }
                    catch(Exception)
                    {

                    }
                    break;
                case 1:
                    ShowWaveChange(WaveToShow.WaveUpdate());
                    try {
                        if (NowFrameCount > 1)
                        {
                            Wavetext1.Text = WaveToShow.ListWave[NowFrameCount - 1].Wave1.ToString();
                            Wavetext2.Text = WaveToShow.ListWave[NowFrameCount - 1].Wave2.ToString();
                            Wavetext3.Text = WaveToShow.ListWave[NowFrameCount - 1].Wave3.ToString();
                            Wavetext4.Text = WaveToShow.ListWave[NowFrameCount - 1].Wave4.ToString();
                            Wavetext5.Text = WaveToShow.ListWave[NowFrameCount - 1].Wave5.ToString();
                            Wavetext6.Text = WaveToShow.ListWave[NowFrameCount - 1].Wave6.ToString();
                        }
                        else
                        {
                            Wavetext1.Text = "";
                            Wavetext2.Text = "";
                            Wavetext3.Text = "";
                            Wavetext4.Text = "";
                            Wavetext5.Text = "";
                            Wavetext6.Text = "";
                        }
                    }
                    catch(Exception)
                    {

                    }
                    break;
                case 2:
                    ShowRouteChange(RouteToShow.RouteShow(RoutePicBox.Width, RoutePicBox.Height));
                    break;
                case 3:
                    ShowText(TextToShow.TextShow(ListBuff, ListLength));
                    break;
            }
        }

        private void ClientRec(Sockets sks)
        {
            this.Invoke(new ThreadStart(delegate
            {
                if (sks.ex != null)
                {
                    if (sks.ClientDispose == true)
                    {
                        //由于未知原因引发异常.导致客户端下线.   比如网络故障.或服务器断开连接.  
                        ShowStatus(string.Format("客户端下线.!"));
                    }
                }
                else if (sks.Offset == 0)
                {
                    //客户端主动下线  
                    ShowStatus(string.Format("客户端下线.!"));
                }
                else
                {
                    int RecvNum = sks.Offset;                   ////单包收到的大小
                    byte[] buffer = new byte[RecvNum];
                    Array.Copy(sks.RecBuffer, buffer, RecvNum);
                    ListBuff.AddRange(buffer);                   ////复制到列表
                    ListLength = ListBuff.LongCount();
                }
            }));
        }


        private void TCPClientEnd_Button_Click(object sender, EventArgs e)
        {
            tcpClient.Stop();
            DisplayUnLock();
            TCPClientOpen_Button.Enabled = true;
            TCPClientIP_textbox.Enabled = true;
            TCPClientPort_textbox.Enabled = true;
            IsOpen = false;
        }

        
        private bool IsFrameConfirm = false;
        private UInt16 FrameDataType = 0;
        private UInt16 FrameDatasize = 0;
        private void DatatTrans(long size)
        {
            byte[] Headbyte = { 0xFF, 0xFA, 0xFD, 0xF6, 0x0A };
            if((size > (ListIndex + 7)) && IsFrameConfirm == false)   /////有可读数据且包含完整头帧
            {
                int ReadSize = (int)(size - ListIndex);
                int ReadStart = (int)ListIndex;
                int i = 0;
                for (i = 0; i < (ReadSize - 7); i++)
                {
                    if((ListBuff[ReadStart + i] == Headbyte[0])
                        && (ListBuff[ReadStart + 1 + i] == Headbyte[1])
                        && (ListBuff[ReadStart + 2 + i] == Headbyte[2])
                        && (ListBuff[ReadStart + 3 + i] == Headbyte[3]))
                    {
                        FrameDataType = ListBuff[ReadStart+ 4 + i];  ////得到数据类型
                        FrameDatasize = (ushort)(ListBuff[ReadStart + 5 + i] * 256 + ListBuff[ReadStart + 6 + i]);/////得到包大小
                        IsFrameConfirm = true;
                        break;
                    }
                }
                ListIndex += (i + 7);
            }
            if (IsFrameConfirm == true)
            {
                if (size < (ListIndex + FrameDatasize + 1))
                    return;
                if (ListBuff[(int)ListIndex + FrameDatasize] != Headbyte[4])   ////尾帧校验
                {
                    IsFrameConfirm = false;
                    ListIndex = size;
                    return;
                }
                byte[] FrameBuff = new byte[FrameDatasize];          /////一个完整帧
                int ReadStart = (int)ListIndex;
                for (int i = 0; i < FrameDatasize; i++)
                {
                    FrameBuff[i] = ListBuff[ReadStart + i];          /////复制到真实数据缓冲区
                }
                switch(FrameDataType)
                {
                    case 0:
                        float[] dataf = TransToFloatData(FrameBuff, FrameDatasize);//////这样为波形数据
                        Alex_WaveShowBmp.WaveDatatype datatemp = new Alex_WaveShowBmp.WaveDatatype();
                        int RecvFloatDataSize = dataf.Length;
                        if (RecvFloatDataSize >= 1)
                            datatemp.Wave1 = dataf[0];
                        if (RecvFloatDataSize >= 2)
                            datatemp.Wave2 = dataf[1];
                        if (RecvFloatDataSize >= 3)
                            datatemp.Wave3 = dataf[2];
                        if (RecvFloatDataSize >= 4)
                            datatemp.Wave4 = dataf[3];
                        if (RecvFloatDataSize >= 5)
                            datatemp.Wave5 = dataf[4];
                        if (RecvFloatDataSize >= 6)
                            datatemp.Wave6 = dataf[5];
                        WaveToShow.ListWave.Add(datatemp);
                        break;
                    case 10:
                        float[] datar = TransToFloatData(FrameBuff, FrameDatasize);
                        if(datar.Length >= 2)
                        {
                            RouteToShow.RouteDataAdd(datar[0], datar[1]);
                            Alex_RouteShow.RouteType dataroutetemp = new Alex_RouteShow.RouteType();
                            Alex_WaveShowBmp.WaveDatatype datawavetemp = new Alex_WaveShowBmp.WaveDatatype();
                            dataroutetemp.DataSize = 0;
                            if (datar.Length >= 3)
                            {
                                dataroutetemp.Data1 = datar[2];
                                datawavetemp.Wave1 = datar[2];
                                dataroutetemp.DataSize++;
                            }
                            if (datar.Length >= 4)
                            {
                                dataroutetemp.Data2 = datar[3];
                                datawavetemp.Wave2 = datar[3];
                                dataroutetemp.DataSize++;
                            }
                            if (datar.Length >= 5)
                            {
                                dataroutetemp.Data3 = datar[4];
                                datawavetemp.Wave3 = datar[4];
                                dataroutetemp.DataSize++;
                            }
                            if (datar.Length >= 6)
                            {
                                dataroutetemp.Data4 = datar[5];
                                datawavetemp.Wave4 = datar[5];
                                dataroutetemp.DataSize++;
                            }
                            if (datar.Length >= 7)
                            {
                                dataroutetemp.Data5 = datar[6];
                                datawavetemp.Wave5 = datar[6];
                                dataroutetemp.DataSize++;
                            }
                            if (datar.Length >= 8)
                            {
                                dataroutetemp.Data6 = datar[7];
                                datawavetemp.Wave6 = datar[7];
                                dataroutetemp.DataSize++;
                            }
                            WaveToShow.ListWave.Add(datawavetemp);
                            RouteToShow.ListRouteData.Add(dataroutetemp);                   
                        }
                        break;
                    case 250:
                        int width = FrameBuff[0] * 256 + FrameBuff[1];
                        int height = FrameBuff[2] * 256 + FrameBuff[3];
                        TransToPic(FrameBuff.Skip(4).ToArray(), FrameDatasize - 4, width, height);     /////图像数据
                        break;
                }
                ListIndex = size;
                IsFrameConfirm = false;                              //////读完完整一帧
            }
        }
        private void ShowPic(Bitmap bp)
        {
            try
            {
                {
                    if (PicShowbox.InvokeRequired)
                    {
                        PicInvoke _myinvoke = new PicInvoke(ShowPic);
                        PicShowbox.BeginInvoke(_myinvoke, new object[] { bp });
                    }
                    else
                    {
                        PicShowbox.Image = bp;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
            }
        }

        /// <summary>
        /// /数据到图像的转换
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        private void TransToPic(byte[] data, int length, int width, int height)
        {
            //int width = PicToShow.Width;
            //int height = PicToShow.Height;
            if (PicTypeCombox.SelectedIndex == 1)
            {
                if ((width * height) != length)  //////确认大小
                    return;
                PicToShow.BmpAdd(data, width, height, PixelFormat.Format8bppIndexed);
            }
            else if(PicTypeCombox.SelectedIndex == 0)
            {
                if ((width * height) / 8 != length)  //////确认大小
                    return;
                PicToShow.BmpAdd(data, width, height, PixelFormat.Format1bppIndexed);
            }
        }
        /// <summary>
        /// 数据到浮点的转换
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        private float[] TransToFloatData(byte[] data, int length)
        {
           float[] RecvFloatData = new float[10];
           int RecvFloatDataCount = 0;
           //int RecvFloatDataSize = 0;
           //Alex_WaveShowBmp.WaveDatatype datatemp = new Alex_WaveShowBmp.WaveDatatype();
            if (length >= 4)
            {
                byte[] temp = new byte[4];
                int FloatCount = (length) / 4;
                if (FloatCount > RecvFloatData.Length)
                    return null;
                for (int i = 0; i < FloatCount; i++)
                {
                    temp[0] = data[i * 4];
                    temp[1] = data[i * 4 + 1];
                    temp[2] = data[i * 4 + 2];
                    temp[3] = data[i * 4 + 3];
                    RecvFloatData[RecvFloatDataCount] = BitConverter.ToSingle(temp, 0);
                    if (RecvFloatData[RecvFloatDataCount] > 100000)
                        RecvFloatData[RecvFloatDataCount] = 100000;
                    else if (RecvFloatData[RecvFloatDataCount] < -100000)
                        RecvFloatData[RecvFloatDataCount] = -100000;
                    RecvFloatDataCount++;
                }
                /*  RecvFloatDataSize = RecvFloatDataCount;
                  if (RecvFloatDataSize >= 1)
                      datatemp.Wave1 = RecvFloatData[0];
                  if (RecvFloatDataSize >= 2)
                      datatemp.Wave2 = RecvFloatData[1];
                  if (RecvFloatDataSize >= 3)
                      datatemp.Wave3 = RecvFloatData[2];
                  if (RecvFloatDataSize >= 4)
                      datatemp.Wave4 = RecvFloatData[3];
                  if (RecvFloatDataSize >= 5)
                      datatemp.Wave5 = RecvFloatData[4];
                  if (RecvFloatDataSize >= 6)
                      datatemp.Wave6 = RecvFloatData[5];
                  RecvFloatDataCount = 0;
                  WaveToShow.ListWave.Add(datatemp);*/
               // RecvFloatDataCount = 0;
                return RecvFloatData;
            }
            else
                return null;
        }
        /// <summary>
        /// 波形静止按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaveStaticOr_Click(object sender, EventArgs e)
        {
            if (WaveToShow.IsWaveStatic == false)
            {
                WaveToShow.IsWaveStatic = true;
                WaveStaticOr.Text = "波形静止";
            }
            else if(WaveToShow.IsWaveStatic == true)
            {
                WaveToShow.IsWaveStatic = false;
                WaveStaticOr.Text = "自动跟踪";
            }
        }
        /// <summary>
        /// 波形X轴分辨率
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            WaveToShow.UintxChange(1);
        }
        /// <summary>
        /// 波形X分辨率
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            WaveToShow.UintxChange(-1);
        }
        /// <summary>
        /// 波形全选设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked == true)
            {
                Wavecheck2.Checked = true;
                Wavecheck1.Checked = true;
                Wavecheck3.Checked = true;
                Wavecheck4.Checked = true;
                Wavecheck5.Checked = true;
                Wavecheck6.Checked = true;
            }
        }


        private void Wavecheck2_CheckedChanged(object sender, EventArgs e)
        {
            WaveToShow.WaveEnable[1] = Wavecheck2.Checked;
        }

        private void Wavecheck1_CheckedChanged(object sender, EventArgs e)
        {
            WaveToShow.WaveEnable[0] = Wavecheck1.Checked;
        }

        private void Wavecheck3_CheckedChanged(object sender, EventArgs e)
        {
            WaveToShow.WaveEnable[2] = Wavecheck3.Checked;
        }

        private void Wavecheck4_CheckedChanged(object sender, EventArgs e)
        {
            WaveToShow.WaveEnable[3] = Wavecheck4.Checked;
        }

        private void Wavecheck5_CheckedChanged(object sender, EventArgs e)
        {
            WaveToShow.WaveEnable[4] = Wavecheck5.Checked;
        }

        private void Wavecheck6_CheckedChanged(object sender, EventArgs e)
        {
            WaveToShow.WaveEnable[5] = Wavecheck6.Checked;
        }

        private void SaveWavePic_Click(object sender, EventArgs e)
        {
            string filepath = System.Environment.CurrentDirectory + "\\WaveData\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            ShowNews(WaveToShow.SaveWaveData(filepath));
        }

        private void WaveDataLoad_Click(object sender, EventArgs e)
        {
            /* FolderBrowserDialog dialog = new FolderBrowserDialog();
             dialog.Description = "请选择文件路径";
             if (dialog.ShowDialog() == DialogResult.OK)
             {
                 string foldPath = dialog.SelectedPath;
                 MessageBox.Show("已选择文件夹:" + foldPath, "选择文件夹提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
             }*/
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择文件";
            fileDialog.Filter = "csv文件(*.csv)|*.csv";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string file = fileDialog.FileName;
              //  MessageBox.Show("已选择文件:" + file, "选择文件提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                WaveToShow.WaveDataClear();
                ShowNews(WaveToShow.LoadData(file));
                //FrameCount = (int)WaveToShow.ListWave.LongCount();
            }
        }

        private bool IsFrameBarClick = false;
        private void FrameBar_MouseDown(object sender, MouseEventArgs e)
        {
            IsFrameBarClick = true;
        }

        private void FrameBar_MouseUp(object sender, MouseEventArgs e)
        {
            IsFrameBarClick = false;
        }

        private void FrameCountBarControl(float e)
        {
            switch (ShowType)
            {
                case 0:
                    PicToShow.FramBarChange(e);
                    break;
                case 1:
                    WaveToShow.FramBarChange(e);
                    break;
                case 2:
                    RouteToShow.FramBarChange(e);
                    break;
            }
        }

        private void FrameBar_MouseMove(object sender, MouseEventArgs e)
        {
            if(IsFrameBarClick == true)
            {
                FrameCountBarControl((float)FrameBar.Value/ (float)FrameBar.Maximum);
            }
        }

        private void JumpFrameButton_Click(object sender, EventArgs e)
        {
            if(FrameCount > 0)
                FrameCountBarControl(Convert.ToSingle(JumpFramtext.Text) / (float)FrameCount);
        }

        private void ClearFrameCount_Click(object sender, EventArgs e)
        {
            timer2.Enabled = false;
            //timer2.Dispose();
            switch (ShowType)
            {
                case 0:
                    PicToShow.FrameClear();
                    break;
                case 1:
                    WaveToShow.WaveDataClear();
                    break;
                case 2:
                    RouteToShow.RouteClear();
                    break;
            }
            timer2.Enabled = true;
        }

        private void SavePictureButton_Click(object sender, EventArgs e)
        {
            string filepath = System.Environment.CurrentDirectory + "\\PicData\\" + DateTime.Now.ToString("yyyMMddHHmmss") + ".bmp";
            ShowNews(PicToShow.PicSave(filepath, false));
        }

        private void PicShowAutoButton_Click(object sender, EventArgs e)
        {
            if(PicToShow.IsPicStatic)
            {
                PicShowAutoButton.Text = "自动播放";
                PicShowAutoButton.ForeColor = Color.Green;
                PicToShow.IsPicStatic = false;
            }
            else
            {
                PicShowAutoButton.Text = "暂停播放";
                PicShowAutoButton.ForeColor = Color.Red;
                PicToShow.IsPicStatic = true;
            }
        }

        private void LoadPictureButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择文件";
            fileDialog.Filter = "bmp文件(*.bmp)|*.bmp";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                //string filelist[] = fileDialog.FileNames;
               // string file = fileDialog.FileName;
                //MessageBox.Show("已选择文件:" + file, "选择文件提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                for(int i = 0; i < fileDialog.FileNames.LongCount(); i++)
                    ShowNews(PicToShow.PicLoad(fileDialog.FileNames[i]));
            }
        }


        private void 保存数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filepath = System.Environment.CurrentDirectory + "\\RouteData\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            ShowNews(RouteToShow.RouteDataSave(filepath));
        }

        private void 导入数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择文件";
            fileDialog.Filter = "csv文件(*.csv)|*.csv";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string file = fileDialog.FileName;
                RouteToShow.RouteClear();
                ShowNews(RouteToShow.LoadData(file));
            }
        }

        private void 路径跟随ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(RouteToShow.IsRouteStatic == true)
            {
                路径跟随ToolStripMenuItem.Text = "路径跟随";
                RouteToShow.IsRouteStatic = false;
            }
            else
            {
                路径跟随ToolStripMenuItem.Text = "路径静止";
                RouteToShow.IsRouteStatic = true;
            }
        }

        private void SendNewCheck_CheckedChanged(object sender, EventArgs e)
        {
            TextToShow.IsSendNew = SendNewCheck.Checked;
        }

        private void OCShowCheck_CheckedChanged(object sender, EventArgs e)
        {
            TextToShow.IsShowOc = OCShowCheck.Checked;
        }

        private void ShowText_Text_TextChanged(object sender, EventArgs e)
        {
            ShowText_Text.SelectionStart = ShowText_Text.Text.Length;
            ShowText_Text.ScrollToCaret();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            Alex_Settings.Default.TCPClientIP = TCPClientIP_textbox.Text.ToString();
            Alex_Settings.Default.TCPClientPort = TCPClientPort_textbox.Text.ToString();
            Alex_Settings.Default.TCPSeverPort = TCPSeverPort_textbox.Text.ToString();
            Alex_Settings.Default.PicShowType = PicTypeCombox.SelectedIndex;
            Alex_Settings.Default.PerspecAngle = PicPitch.Text.ToString();
            Alex_Settings.Default.PerspecFocus = PicFocus.Text.ToString();
            Alex_Settings.Default.PerspecSensorH = PicSensorHeight.Text.ToString();
            Alex_Settings.Default.PerspecLimitF = SensorLimitDistance.Text.ToString();
            Alex_Settings.Default.PerspecHeight = SensorHeight.Text.ToString();
            Alex_Settings.Default.Order1 = OrderText1.Text.ToString();
            Alex_Settings.Default.Order2 = OrderText2.Text.ToString();
            Alex_Settings.Default.Order3 = OrderText3.Text.ToString();
            Alex_Settings.Default.Order4 = OrderText4.Text.ToString();
            Alex_Settings.Default.Order5 = OrderText5.Text.ToString();
            Alex_Settings.Default.Order6 = OrderText6.Text.ToString();
            Alex_Settings.Default.Save();
        }

        private void SettingsLoad()
        {
            TCPClientIP_textbox.Text = Alex_Settings.Default.TCPClientIP;
            TCPClientPort_textbox.Text = Alex_Settings.Default.TCPClientPort;
            TCPSeverPort_textbox.Text = Alex_Settings.Default.TCPSeverPort;
            PicTypeCombox.SelectedIndex = Alex_Settings.Default.PicShowType;
            PicPitch.Text = Alex_Settings.Default.PerspecAngle;
            PicFocus.Text = Alex_Settings.Default.PerspecFocus;
            PicSensorHeight.Text = Alex_Settings.Default.PerspecSensorH;
            SensorLimitDistance.Text = Alex_Settings.Default.PerspecLimitF;
            SensorHeight.Text = Alex_Settings.Default.PerspecHeight;
            OrderText1.Text = Alex_Settings.Default.Order1;
            OrderText2.Text = Alex_Settings.Default.Order2;
            OrderText3.Text = Alex_Settings.Default.Order3;
            OrderText4.Text = Alex_Settings.Default.Order4;
            OrderText5.Text = Alex_Settings.Default.Order5;
            OrderText6.Text = Alex_Settings.Default.Order6;
        }

        private void PicDealOpen_CheckedChanged(object sender, EventArgs e)
        {
            PicToShow.IsPicDeal = PicDealOpen.Checked;
            if (PicToShow.IsPicDeal == false) return;
        }

        private void PicDealListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void PicDealListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (PicDealListBox.SelectedIndex == 0)
            {
                PicToShow.IsBinary = !PicDealListBox.GetItemChecked(0);
            }
            if (PicDealListBox.SelectedIndex == 1)
            {
                PicToShow.IsCanny = !PicDealListBox.GetItemChecked(1);
            }
            else if(PicDealListBox.SelectedIndex == 2)
            {
                PicToShow.Isperspective = !PicDealListBox.GetItemChecked(2);
            }
            else if (PicDealListBox.SelectedIndex == 3)
            {
                PicToShow.IsDLL = !PicDealListBox.GetItemChecked(3);
            }
        }

        private void PicPerspectiveButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (PicPitch.Text != null)
                {
                    float p = Convert.ToSingle(PicPitch.Text.ToString());
                    if (p > 0 && p < 80)
                    {
                        PicToShow.PicPitch = p;
                    }
                }
                if (PicFocus.Text != null)
                {
                    float f = Convert.ToSingle(PicFocus.Text.ToString());
                    if (f > 0 && f < 4)
                    {
                        PicToShow.PicFocus = f;
                    }
                }
                if (PicSensorHeight.Text != null)
                {
                    float s = Convert.ToSingle(PicSensorHeight.Text.ToString());
                    if (s > 0 && s < 4)
                    {
                        PicToShow.PicSensorHeight = s;
                    }
                }
                if (SensorLimitDistance.Text != null)
                {
                    float l = Convert.ToSingle(SensorLimitDistance.Text.ToString());
                    if (l > 500 && l < 2000)
                    {
                        PicToShow.PicLimitFar = l;
                    }
                }
                if (SensorHeight.Text != null)
                {
                    float H = Convert.ToSingle(SensorHeight.Text.ToString());
                    if (H > 50 && H < 400)
                    {
                        PicToShow.PicHeight = H;
                    }
                }
                PicToShow.IsPerspective = true;
            }
            catch(Exception)
            {

            }
        }

        private void SerialTab_Click(object sender, EventArgs e)
        {

        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }

        private void SendButton1_Click(object sender, EventArgs e)
        {
            string str = OrderText1.Text.ToString() + "\r\n";
            if (IsOpen)
            {
                switch (ConnectType)
                {
                    case 0:
                        if (SendText_Text != null)
                            tcpServer.SendToAll(TextToShow.TextSend(str));
                        break;
                    case 1:
                        if (SendText_Text != null)
                            tcpClient.SendData(TextToShow.TextSend(str));
                        break;
                    case 2:
                        if (SendText_Text != null)
                            sp.WriteLine(TextToShow.TextSend(str));
                        break;
                }
            }
            else
            {
                ShowStatus("未连接");
            }
        }

        private void SendButton3_Click(object sender, EventArgs e)
        {
            string str = OrderText3.Text.ToString() + "\r\n";
            if (IsOpen)
            {
                switch (ConnectType)
                {
                    case 0:
                        if (SendText_Text != null)
                            tcpServer.SendToAll(TextToShow.TextSend(str));
                        break;
                    case 1:
                        if (SendText_Text != null)
                            tcpClient.SendData(TextToShow.TextSend(str));
                        break;
                    case 2:
                        if (SendText_Text != null)
                            sp.WriteLine(TextToShow.TextSend(str));
                        break;
                }
            }
            else
            {
                ShowStatus("未连接");
            }
        }

        private void SendButton2_Click(object sender, EventArgs e)
        {
            string str = OrderText2.Text.ToString() + "\r\n";
            if (IsOpen)
            {
                switch (ConnectType)
                {
                    case 0:
                        if (SendText_Text != null)
                            tcpServer.SendToAll(TextToShow.TextSend(str));
                        break;
                    case 1:
                        if (SendText_Text != null)
                            tcpClient.SendData(TextToShow.TextSend(str));
                        break;
                    case 2:
                        if (SendText_Text != null)
                            sp.WriteLine(TextToShow.TextSend(str));
                        break;
                }
            }
            else
            {
                ShowStatus("未连接");
            }
        }

        private void SendButton4_Click(object sender, EventArgs e)
        {
            string str = OrderText4.Text.ToString() + "\r\n";
            if (IsOpen)
            {
                switch (ConnectType)
                {
                    case 0:
                        if (SendText_Text != null)
                            tcpServer.SendToAll(TextToShow.TextSend(str));
                        break;
                    case 1:
                        if (SendText_Text != null)
                            tcpClient.SendData(TextToShow.TextSend(str));
                        break;
                    case 2:
                        if (SendText_Text != null)
                            sp.WriteLine(TextToShow.TextSend(str));
                        break;
                }
            }
            else
            {
                ShowStatus("未连接");
            }
        }

        private void SendButton5_Click(object sender, EventArgs e)
        {
            string str = OrderText5.Text.ToString() + "\r\n";
            if (IsOpen)
            {
                switch (ConnectType)
                {
                    case 0:
                        if (SendText_Text != null)
                            tcpServer.SendToAll(TextToShow.TextSend(str));
                        break;
                    case 1:
                        if (SendText_Text != null)
                            tcpClient.SendData(TextToShow.TextSend(str));
                        break;
                    case 2:
                        if (SendText_Text != null)
                            sp.WriteLine(TextToShow.TextSend(str));
                        break;
                }
            }
            else
            {
                ShowStatus("未连接");
            }
        }

        private void SendButton6_Click(object sender, EventArgs e)
        {
            string str = OrderText6.Text.ToString() + "\r\n";
            if (IsOpen)
            {
                switch (ConnectType)
                {
                    case 0:
                        if (SendText_Text != null)
                            tcpServer.SendToAll(TextToShow.TextSend(str));
                        break;
                    case 1:
                        if (SendText_Text != null)
                            tcpClient.SendData(TextToShow.TextSend(str));
                        break;
                    case 2:
                        if (SendText_Text != null)
                            sp.WriteLine(TextToShow.TextSend(str));
                        break;
                }
            }
            else
            {
                ShowStatus("未连接");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择文件";
            fileDialog.Filter = "dll文件(*.dll)|*.dll";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string file = fileDialog.FileName;
                PicToShow.LoadDll(file);
                //MessageBox.Show("已选择文件:" + file, "选择文件提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string filepath = System.Environment.CurrentDirectory + "\\PicData\\" + DateTime.Now.ToString("yyyMMddHHmmss");
            ShowNews(PicToShow.PicSave(filepath, true));
        }
    }
}
