using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Collections;
using WindowsFormsApplication3.AudioModule;
using Mitov.BasicLab;
using NAudio.Wave;



namespace WindowsFormsApplication3
{
    public partial class Form1 : Form
    {
        private Tcpclient myTcpClient;//TCP客户端(PC端控制器的TCP部分)
        private NetworkStream ns;//网络数据流
        private Udpclient udp = new Udpclient();//这个先放着，以后再改
        private UdpClient ifpanUdp;
        private const int buffersize = 1024;
        private delegate void ShowpSscanMsgCallback(double[] x, double[] y);//画图用的
        private ShowpSscanMsgCallback showPscanMsgCallback;//画图用的
        private delegate void ShowIFMsgCallback(double[] x, double[] y);//画图用的
        private ShowIFMsgCallback showIFMsgCallback;
        private delegate void ShowMscanMsgCallback(double[] x, double[] y);//画图用的
        private ShowMscanMsgCallback showMscanMsgCallback;//画图用的
        private delegate void ShowFscanMsgCallback(double[] x, double[] y);//画图用的
        private ShowFscanMsgCallback showFscanMsgCallback;//画图用的
        private Thread DisplayIFPThread;//画图用的线程
        private Thread DisplayPscanThread;//画图用的线程
        private Thread DisplayMscanThread;//画图用的线程
        private Thread DisplayFscanThread;
        private ASCIIEncoding encoder = new ASCIIEncoding();
        private string PscanStartFre;
        private string PscanStopFre;
        private string PscanStepFre;
        private string FscanStartFre;
        private string FscanStopFre;
        private string FscanStepFre;
        private string FscanDwell;
        private string ifpanCmd;//发送ifpan命令
        private bool IsWork = false;//判断仪器是否工作
        private bool IFpanFlag = false;//判断仪器是否正在采集中频数据
        private FileStream MyIFFlie;
        private FileStream MyAUDIOFlie;
        private StreamWriter MyifFile;
        private BinaryWriter MyAudioFile;
        

        public Form1()
        {
            InitializeComponent();
        }

        private void EM100Connect_Click(object sender, EventArgs e)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(cbBoxEM100IP.Text), 5555);
            myTcpClient = new Tcpclient();
            try
            {
                myTcpClient.tcpClient.Connect(ipEndPoint);
                ns = myTcpClient.tcpClient.GetStream();
                MessageBox.Show("TCP/IP 连接成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show("TCP连接失败！" + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.PscanStartFre = PsStart.Text;
            this.PscanStopFre = PsStop.Text;
            this.PscanStepFre = PsStep.Text;
            MessageBox.Show("设置成功！");
        }

        private void Pscan_Click(object sender, EventArgs e)
        {
            if (IsWork)
            {
                MessageBox.Show("其他功能还未停止");
                return;
            }
            IsWork = true;
            try
            {
                PScan pscan = new PScan(PscanStartFre, PscanStopFre, PscanStepFre, "172.17.75.10", "9999");
                myTcpClient.SendMessage(pscan.PSCAN());
                udp.ControllReceiveBar = true;
                //Thread t = new Thread(udp.ReceiveData);
                DisplayPscanThread = new Thread(DisplayPScanSpectrum);
                DisplayPscanThread.IsBackground = true;
                DisplayPscanThread.Start();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.ToString());
            }
        }
        public void DisplayPScanSpectrum()
        {
            while (udp.ControllReceiveBar)
            {
                byte[] recvByte = null;
                recvByte = udp.udpClient.Receive(ref udp.remoteEP);
                int length = recvByte.Length;
                UdpStreamHeader udpHeader = new UdpStreamHeader();
                udpHeader.HeaderMagicNumber = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(recvByte, 0));//0 3
                udpHeader.HeaderMinorVersion = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 4));//4 5
                udpHeader.HeaderMajorVersion = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 6));//6 7
                udpHeader.HeaderSequenceNumber = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 8)); //8 9
                udpHeader.HeaderReserved = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 10));//10 11
                udpHeader.DataSize = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(recvByte, 12));//12 15
                udpHeader.AttributeTag = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 16));//16 17
                udpHeader.AttributeLength = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 18));//18 19
                udpHeader.TraceNumberItems = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 20));//20 21
                udpHeader.TraceReserved = BitConverter.ToChar(recvByte, 22);//22
                udpHeader.TraceOptionHeaderLength = BitConverter.ToChar(recvByte, 23);//23
                udpHeader.TraceSelectorFlags = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(recvByte, 24));//24 27
                ushort dataType = udpHeader.AttributeTag;
                switch (dataType)
                {
                    case 1201:
                        udp.udpPscanStream.StartFreqLow = BitConverter.ToUInt32(recvByte, 28);//28 31
                        udp.udpPscanStream.StopFreqLow = BitConverter.ToUInt32(recvByte, 32);//32 33 34 35
                        udp.udpPscanStream.StepFreq = BitConverter.ToUInt32(recvByte, 36);//36 37 38 39 
                        udp.udpPscanStream.StartFreqHigh = BitConverter.ToUInt32(recvByte, 40);//40 41 42 43
                        udp.udpPscanStream.StopFreqHigh = BitConverter.ToUInt32(recvByte, 44);//44 45 46 47
                        udp.udpPscanStream.Reserved = BitConverter.ToString(recvByte, 48, 4);
                        udp.udpPscanStream.TimeSwap = BitConverter.ToUInt64(recvByte, 52);
                        //System.Console.WriteLine(udpPscanStream.TimeSwap);
                        String allFre = "";//我打算第一行输出一行频率
                        long number = 0;//计数，看看有多少个电平值 这个在哪儿算，还需改进
                        float tempFreq = udp.udpPscanStream.StartFreqLow;
                        for (; tempFreq <= udp.udpPscanStream.StopFreqLow; tempFreq += udp.udpPscanStream.StepFreq)
                        {
                            float temp = tempFreq / 1000000;
                            allFre = allFre + temp + "  ";
                            number++;
                            UdpStreamPscan.TrueFre.Add(temp);
                        }
                        udp.udpPscanStream.FftLevel.Clear();
                        udp.udpPscanStream.FreqLow.Clear();
                        for (int i = 0; i < udpHeader.TraceNumberItems; i++)
                        {
                            udp.udpPscanStream.FftLevel.Add(BitConverter.ToInt16(recvByte, 60 + i * sizeof(short)));
                        }
                        int dataBegin = 60 + udpHeader.TraceNumberItems * sizeof(short);
                        for (int i = 0; i < udpHeader.TraceNumberItems; i++)
                        {
                            udp.udpPscanStream.FreqLow.Add(BitConverter.ToUInt32(recvByte, dataBegin + i * sizeof(uint)));

                        }
                        //System.Console.WriteLine("第一个udp.udpPscanStream.FftLevel--------->" + udp.udpPscanStream.FftLevel[0]);
                        //System.Console.WriteLine("最后一个udp.udpPscanStream.FftLevel--------->" + udp.udpPscanStream.FftLevel[udpHeader.TraceNumberItems-1]);
                        FileStream fs = new FileStream(@"F:\TestTcp\rec.txt", FileMode.Append, FileAccess.Write);
                        StreamWriter sw = new StreamWriter(fs);
                        if (UdpStreamPscan.IsFrist == true)
                        {
                            UdpStreamPscan.IsFrist = false;
                            sw.WriteLine(allFre);//先输出一行频率列表
                        }
                        if (udp.udpPscanStream.FreqLow.ElementAt(0) == udp.udpPscanStream.StartFreqLow && udp.Databar == false && UdpStreamPscan.NowFre == 1 || udp.udpPscanStream.FreqLow.ElementAt(0) == udp.udpPscanStream.StartFreqLow && udp.Databar == false && UdpStreamPscan.NowFre == udp.udpPscanStream.StepFreq)//为了达到拼包的目的，设计一组静态变量
                        {
                            UdpStreamPscan.Level.AddRange(udp.udpPscanStream.FftLevel);

                            UdpStreamPscan.NowFre = udp.udpPscanStream.FreqLow.ElementAt(udpHeader.TraceNumberItems - 1) + udp.udpPscanStream.StepFreq;

                        }
                        else if (UdpStreamPscan.NowFre == udp.udpPscanStream.FreqLow.ElementAt(0) && udp.Databar == false)//这个条件其实跟上一个是一致的
                        {
                            UdpStreamPscan.Level.AddRange(udp.udpPscanStream.FftLevel);
                            UdpStreamPscan.NowFre = udp.udpPscanStream.FreqLow.ElementAt(udpHeader.TraceNumberItems - 1) + udp.udpPscanStream.StepFreq;
                        }

                        if (UdpStreamPscan.NowFre == udp.udpPscanStream.StepFreq) udp.Databar = true;
                        if (udp.Databar == true)//设置一个判断是否拼完的标志
                        {
                            // UdpStreamPscan.Level.AddRange(udpPscanStream.FftLevel);
                            UdpStreamPscan.NowFre = udp.udpPscanStream.StepFreq;

                            string allLevel = "";
                            long tempCount = 0;//计数量，为了截断后面多余的数据
                            foreach (short i in UdpStreamPscan.Level)
                            {
                                allLevel = allLevel + (float)i / 10 + " ";
                                UdpStreamPscan.TrueLevel.Add(i / 10);
                                tempCount++;
                                if (tempCount >= number) break;
                            }
                            sw.WriteLine(allLevel);
                            udp.Databar = false;
                            double[] bufX = { 0 };              //X轴数据（频率）
                            double[] bufY = { 0 };              //Y轴数据（强度）
                            long Count = 0;
                            try
                            {
                                Count = Convert.ToUInt32(UdpStreamPscan.TrueLevel.Count);
                                bufX = new double[number];
                                bufY = new double[number];
                                int Yindex = 0;
                                foreach (float i in UdpStreamPscan.TrueLevel)
                                {
                                    bufY[Yindex] = i;
                                    Yindex++;
                                    if (Yindex >= number) break;
                                }
                                int Xindex = 0;
                                foreach (float i in UdpStreamPscan.TrueFre)
                                {
                                    bufX[Xindex] = i;
                                    Xindex++;
                                    if (Xindex >= number) break;
                                }

                                scope1.Invoke(showPscanMsgCallback, bufX, bufY);

                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("DisplayPsanSpectrum 错误! " + ex.Message);

                            }
                            UdpStreamPscan.Level.Clear();
                            UdpStreamPscan.TrueLevel.Clear();
                            UdpStreamPscan.TrueFre.Clear();
                        }

                        sw.Flush();
                        sw.Close();
                        break;
                }

            }

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            showPscanMsgCallback = new ShowpSscanMsgCallback(DataShowCallback1);
            showIFMsgCallback = new ShowIFMsgCallback(DataShowCallback2);
            showFscanMsgCallback = new ShowFscanMsgCallback(DataShowCallback3);
            showMscanMsgCallback = new ShowMscanMsgCallback(DataShowCallback4);
        }
        private void DataShowCallback1(double[] x, double[] y)
        {
            scope1.Channels[0].Data.SetXYData(x, y);
        }

        private void DataShowCallback2(double[] x, double[] y)
        {
            scope2.Channels[0].Data.SetXYData(x, y);
        }
        private void DataShowCallback3(double[] x, double[] y)
        {
            scope3.Channels[0].Data.SetXYData(x, y);
        }
        private void DataShowCallback4(double[] x, double[] y)
        {
            scope4.Channels[0].Data.SetXYData(x, y);
        }
        private void PscanStop_Click(object sender, EventArgs e)
        {
            IsWork = false;
            string ifpanCmd;
            string ip = cbBoxEM100IP.Text;
            ifpanCmd = "TRAC:UDP:TAG:OFF '" + ip + "', 56513, PSC\n";
            ifpanCmd += "TRAC:UDP:DEL ALL \n";
           // ifpanCmd += "MEM:CLE 0, MAX \n";
            byte[] sendData = Encoding.Default.GetBytes(ifpanCmd);
            ns.Write(sendData, 0, sendData.Length);
            udp.ControllReceiveBar = false;
            myTcpClient.Close();
            //udp.udpClient.Close();
            ns.Dispose();
            DisplayPscanThread.Abort();
            MessageBox.Show("已经停止！");
        }
        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void End1_Click(object sender, EventArgs e)
        {
            udp.udpClient.Close();
            if (DisplayPscanThread != null) DisplayPscanThread.Abort();
            this.Close();
        }

        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(cbBoxEM100IP.Text), 5555);
            myTcpClient = new Tcpclient();
            try
            {
                myTcpClient.tcpClient.Connect(ipEndPoint);
                ns = myTcpClient.tcpClient.GetStream();
                MessageBox.Show("TCP/IP 连接成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show("TCP连接失败！" + ex.Message);
            }
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            string m_vfre = cbBoxFre.Text;
            string m_vband = cbBoxBand.Text;
            string m_demode = cbBoxDemode.Text;
            string m_vspan = cbBoxSpan.Text;

            ifpanCmd = "FREQuency " + m_vfre + " MHz\n";
            ifpanCmd += "SENS:BAND " + m_vband + " kHz\n";
            ifpanCmd += "SENS:FREQ:SPAN " + m_vspan + " kHz\n";
            if (m_demode == "FM")
            {
                ifpanCmd += "SENS:DEM FM\n";
            }
            else if (m_demode == "AM")
            {
                ifpanCmd += "SENS:DEM AM\n";
            }
            ifpanCmd += "SYST:AUD:OUTP HPH;:SYSTEM:AUDIO:REM:MODE 2;:SYST:AUD:VOL 0.5\n";
            byte[] sendData = Encoding.Default.GetBytes(ifpanCmd);
            // ns = myTcpClient.GetStream();
            ns.Write(sendData, 0, sendData.Length);
          //  MessageBox.Show("设置成功！");


            DisplayIFPThread = new Thread(DisplayIFPanSpectrum);
            DisplayIFPThread.IsBackground = true;
            DisplayIFPThread.Start();


        }

        private void btnIFPanINI_Click(object sender, EventArgs e)
        {
            if (IsWork == true)
            {
                MessageBox.Show("其他扫描正在进行！");
                return;
            }
            IsWork = true;
            string m_vfre = cbBoxFre.Text;
            string m_vband = cbBoxBand.Text;
            string m_demode = cbBoxDemode.Text;
            string m_vspan = cbBoxSpan.Text;
            string ip = "172.17.75.10";

            //ifpanCmd += "FREQuency " + m_vfre + " MHz\n";
            ifpanCmd = "CALC:IFPAN:AVER:TYPE OFF\n";              //直接得到中频测量的数据
            ifpanCmd += "ABOR\n";                                  //停止扫描
            //ifpanCmd += "SENS:FREQ:MODE CW\n";
            ifpanCmd += "TRAC:FEED:CONT ITRACE,ALW\n";             //每次的测量值的相关信息都存储在itrace
            ifpanCmd += "TRAC:FEED:CONT MTRACE,ALW\n";             //每次的测量值都存储在mtrace
            //ifpanCmd += "SENS:BAND " + m_vband + " KHz\n";
            //ifpanCmd += "SENS:DEM " + m_demode + "\n";
            //ifpanCmd += "SENS:FREQ:SPAN " + m_vspan + " kHz\n";
            ifpanCmd += "SYSTem:AUDio:REMote:MODe 2\n";
            ifpanCmd += "SENS:FREQ:MODE CW\n";
            ifpanCmd += "STAT:EXT:ENAB #B1111111111111111\n";      //扩展寄存器，和pscan配置，mscan配置，audio设置有关
            ifpanCmd += "STAT:EXT:PTR #B1111111111111111\n";
            ifpanCmd += "STAT:EXT:NTR #B1111111111111111\n";
            ifpanCmd += "STAT:OPER:ENAB #B1111111111111111\n";     //状态寄存器，和oper:swe有关
            ifpanCmd += "STAT:OPER:PTR #B1111111111111111\n";
            ifpanCmd += "STAT:OPER:NTR #B1111111111111111\n";
            ifpanCmd += "STAT:OPER:SWE:ENAB #B1111111111111111\n";//状态寄存器，和FSCAN,MSCAN,PSCAN的状态有关
            ifpanCmd += "STAT:OPER:SWE:PTR #B1111111111111111\n";
            ifpanCmd += "STAT:OPER:SWE:NTR #B1111111111111111\n";
            ifpanCmd += "STAT:TRAC:ENAB #B11111111111\n";         //状态寄存器，和mtrace,itrace,ifpan等有关
            ifpanCmd += "STAT:TRAC:PTR #B11111111111\n";
            ifpanCmd += "STAT:TRAC:NTR #B00000000000\n";
            ifpanCmd += "INIT\n";                                                     //初始化，清空接收机寄存器里的数据。。
            ifpanCmd += "TRAC:UDP:FLAG:ON '" + ip + "', 56513, 'FREQ:RX','CHAN','SQU','OPT','SWAP','VOLT:AC','FREQ:OFFS','FSTR'\n";      //设置一个udp地址和端口,增加指定的标志(决定udp数据中应包含什么)
            ifpanCmd += "TRAC:UDP:TAG:ON '" + ip + "', 56513, AUD\n";
            ifpanCmd += "TRAC:UDP:TAG:ON '" + ip + "', 56513, IFP\n";                                             //设置一个udp地址和端口,传输指定的udp属性流。
            ifpanCmd += "STAT:OPER:ENAB 280\n";
            ifpanCmd += "STAT:EXT:ENAB 63423\n";
            ifpanCmd = "FREQuency 200 MHz\n";
            ifpanCmd += "SENS:BAND 200 KHz\n";
            ifpanCmd += "SENS:FREQ:SPAN 500 kHz\n";
            ifpanCmd += "SENS:DEM FM\n";
            ifpanCmd += "SYST:AUD:OUTP HPH;:SYSTEM:AUDIO:REM:MODE 2;:SYST:AUD:VOL 0.5\n";
            byte[] sendData = Encoding.Default.GetBytes(ifpanCmd);
            //ns = myTcpClient.GetStream();
            ns.Write(sendData, 0, sendData.Length);
            try
            {
                IPEndPoint LocalIpEndPoint = new IPEndPoint(IPAddress.Parse(ip), 56513);
                ifpanUdp = new UdpClient(LocalIpEndPoint);
               // MessageBox.Show("LocalIpEndPoint UDP  连接成功！");
            }
            catch (Exception ex)
            {

                MessageBox.Show("LocalIpEndPoint UDP连接失败！" + ex.Message);
            }
            MessageBox.Show("初始化成功！");
            DisplayIFPThread = new Thread(DisplayIFPanSpectrum);
            DisplayIFPThread.IsBackground = true;
            DisplayIFPThread.Start();
        }

        private void DisplayIFPanSpectrum()
        {

            UdpStreamType ifpanRecv = new UdpStreamType();
            AudioOut audio = new AudioOut();
            //StreamWriter sw = new StreamWriter(MyFile);

            double[] bufX = { 0 };              //X轴数据（频率）

            double[] bufY = { 0 };              //Y轴数据（强度）

            List<byte> tempAudioList = new List<byte>();
            byte[] tempAudio = new byte[2000000];
            double IfpanMidFreq = 0.0;
            double IfpanSpan = 0.0;
            ushort dataType = 0;
            long Count = 0;
            double frq = 0.0;
            int tempa = 930;
            while (true)
            {
                tempa--;
                try
                {

                    IFPanReceiveMessage(ref ifpanRecv, ref dataType);
                    if (dataType == 401)        //Audio Streaming
                    {
                        int modeFile = 0;
                        AudioData _Audio = new AudioData();
                        switch (ifpanRecv.audioStream.Mode)
                        {
                            case 1:
                                _Audio.wFormatTag = 0x0031;
                                _Audio.nSamplesPerSec = 32000;
                                _Audio.wBitsPerSample = 16;
                                _Audio.nChannels = 2;
                                _Audio.nAvgBytesPerSec = 128000;
                                _Audio.nBlockAlign = 4;
                                _Audio.cbSize = 0;
                                modeFile = 4;
                                _Audio.Data = ifpanRecv.audioStream.WaveStream;
                                break;
                            case 2:
                                _Audio.wFormatTag = 0x0031;
                                _Audio.nSamplesPerSec = 32000;
                                _Audio.wBitsPerSample = 16;
                                _Audio.nChannels = 1;
                                _Audio.nAvgBytesPerSec = 64000;
                                _Audio.nBlockAlign = 2;
                                _Audio.cbSize = 0;
                                modeFile = 4;
                                _Audio.Data = ifpanRecv.audioStream.WaveStream;
                                break;
                            case 3:
                                _Audio.wFormatTag = 0x0031;
                                _Audio.nSamplesPerSec = 32000;
                                _Audio.wBitsPerSample = 8;
                                _Audio.nChannels = 2;
                                _Audio.nAvgBytesPerSec = 64000;
                                _Audio.nBlockAlign = 2;
                                _Audio.cbSize = 0;
                                modeFile = 4;
                                _Audio.Data = ifpanRecv.audioStream.WaveStream;
                                break;
                            case 4:
                                _Audio.wFormatTag = 0x0031;
                                _Audio.nSamplesPerSec = 32000;
                                _Audio.wBitsPerSample = 8;
                                _Audio.nChannels = 1;
                                _Audio.nAvgBytesPerSec = 32000;
                                _Audio.nBlockAlign = 1;
                                _Audio.cbSize = 0;
                                modeFile = 4;
                                _Audio.Data = ifpanRecv.audioStream.WaveStream;
                                break;
                            case 5:
                                _Audio.wFormatTag = 0x0031;
                                _Audio.nSamplesPerSec = 16000;
                                _Audio.wBitsPerSample = 16;
                                _Audio.nChannels = 2;
                                _Audio.nAvgBytesPerSec = 64000;
                                _Audio.nBlockAlign = 4;
                                _Audio.cbSize = 0;
                                modeFile = 2;
                                _Audio.Data = ifpanRecv.audioStream.WaveStream;
                                break;
                            case 6:
                                _Audio.wFormatTag = 0x0031;
                                _Audio.nSamplesPerSec = 16000;
                                _Audio.wBitsPerSample = 16;
                                _Audio.nChannels = 1;
                                _Audio.nAvgBytesPerSec = 32000;
                                _Audio.nBlockAlign = 2;
                                _Audio.cbSize = 0;
                                modeFile = 2;
                                _Audio.Data = ifpanRecv.audioStream.WaveStream;
                                break;
                            case 7:
                                _Audio.wFormatTag = 0x0031;
                                _Audio.nSamplesPerSec = 16000;
                                _Audio.wBitsPerSample = 8;
                                _Audio.nChannels = 2;
                                _Audio.nAvgBytesPerSec = 32000;
                                _Audio.nBlockAlign = 2;
                                _Audio.cbSize = 0;
                                modeFile = 2;
                                _Audio.Data = ifpanRecv.audioStream.WaveStream;
                                break;
                            case 8:
                                _Audio.wFormatTag = 0x0031;
                                _Audio.nSamplesPerSec = 16000;
                                _Audio.wBitsPerSample = 8;
                                _Audio.nChannels = 1;
                                _Audio.nAvgBytesPerSec = 16000;
                                _Audio.nBlockAlign = 1;
                                _Audio.cbSize = 0;
                                modeFile = 2;
                                _Audio.Data = ifpanRecv.audioStream.WaveStream;
                                break;
                            case 9:
                                _Audio.wFormatTag = 0x0031;
                                _Audio.nSamplesPerSec = 8000;
                                _Audio.wBitsPerSample = 16;
                                _Audio.nChannels = 2;
                                _Audio.nAvgBytesPerSec = 32000;
                                _Audio.nBlockAlign = 4;
                                _Audio.cbSize = 0;
                                modeFile = 1;
                                _Audio.Data = ifpanRecv.audioStream.WaveStream;
                                break;
                            case 10:
                                _Audio.wFormatTag = 0x0031;
                                _Audio.nSamplesPerSec = 8000;
                                _Audio.wBitsPerSample = 16;
                                _Audio.nChannels = 1;
                                _Audio.nAvgBytesPerSec = 16000;
                                _Audio.nBlockAlign = 2;
                                _Audio.cbSize = 0;
                                modeFile = 1;
                                _Audio.Data = ifpanRecv.audioStream.WaveStream;
                                break;
                            case 11:
                                _Audio.wFormatTag = 0x0031;
                                _Audio.nSamplesPerSec = 8000;
                                _Audio.wBitsPerSample = 8;
                                _Audio.nChannels = 2;
                                _Audio.nAvgBytesPerSec = 16000;
                                _Audio.nBlockAlign = 2;
                                _Audio.cbSize = 0;
                                modeFile = 1;
                                _Audio.Data = ifpanRecv.audioStream.WaveStream;
                                break;
                            case 12:
                                _Audio.wFormatTag = 0x0031;
                                _Audio.nSamplesPerSec = 8000;
                                _Audio.wBitsPerSample = 8;
                                _Audio.nChannels = 1;
                                _Audio.nAvgBytesPerSec = 8000;
                                _Audio.nBlockAlign = 1;
                                _Audio.cbSize = 0;
                                modeFile = 1;
                                _Audio.Data = ifpanRecv.audioStream.WaveStream;
                                break;
                        }
                        if (modeFile != 0)
                        {
                            for (int i = 0; i < _Audio.Data.Length; i++)
                            {
                                tempAudioList.Add(_Audio.Data[i]);
                            }

                        }
                        if (IFpanFlag == true)
                        {
                            MyAudioFile.Write(ifpanRecv.audioStream.WaveStream);
                        }
                        audio.PlayAudio(_Audio);

                        Array.Clear(ifpanRecv.audioStream.WaveStream, 0, ifpanRecv.audioStream.WaveStream.Length);
                    }
                    if (dataType == 501)       //IFPan Streaming
                    {

                        Count = Convert.ToUInt32(ifpanRecv.ifpanStream.pwls.Count);
                        bufX = new double[Count];
                        bufY = new double[Count];


                        IfpanMidFreq = ifpanRecv.ifpanStream.FrequencyLow / (1000.0 * 1000.0);
                        IfpanSpan = ifpanRecv.ifpanStream.Span / 1000.0;
                        frq = IfpanMidFreq;
                        bufX[0] = IfpanMidFreq - IfpanSpan / (1000.0 * 2.0);
                        for (int i = 1; i < ifpanRecv.ifpanStream.pwls.Count; i++)
                        {
                            bufX[i] = bufX[i - 1] + IfpanSpan / (1000.0 * ifpanRecv.ifpanStream.pwls.Count);
                            //bufX1.Add(bufX[i]);
                            //bufX1[i] = Convert.ToSByte(bufX[i]);
                            if (IFpanFlag == true)
                            {
                                MyifFile.Write(bufX[i]);
                                MyifFile.Write(" ");

                            }


                        }
                        if (IFpanFlag == true)
                        {
                            MyifFile.Write('\n');
                        }
                        //sw.Write('\n');
                        for (int pos = 0; pos < ifpanRecv.ifpanStream.pwls.Count; pos++)
                        {
                            bufY[pos] = ifpanRecv.ifpanStream.pwls[pos] / (10.0);
                            if (IFpanFlag == true)
                            {
                                MyifFile.Write(bufY[pos]);
                                MyifFile.Write(" ");
                            }
                        }
                        scope2.Invoke(showIFMsgCallback, bufX, bufY);

                    }

                }
                catch (Exception ex)
                {

                    MessageBox.Show("DisplayIFPanSpectrum 错误! " + ex.Message);
                    //break;
                }
                if (tempa == 0)
                {
                    tempa = 930;

                    string savePath = Environment.CurrentDirectory + "\\Files" + "\\" + DateTime.Now.ToString("yyMMdd");

                    string filenamewav = savePath + "\\" + DateTime.Now.ToString("yyyyMMddHHmm") + "_" + frq + ".wav";

                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }


                    //这里应该有个解码方式
                    for (int i = 0; i < tempAudioList.Count; i++)
                    {
                        tempAudio[i] = tempAudioList[i];
                    }
                    WaveFormatEncoding tag = WaveFormatEncoding.Pcm;
                    int sampleRate;
                    int channels;
                    int averageBytesPerSecon;
                    int blockAlign;
                    int bitPersample;


                    switch (ifpanRecv.audioStream.Mode)
                    {
                        case 1:
                            sampleRate = 32000;
                            channels = 2;
                            averageBytesPerSecon = 128000;
                            blockAlign = 4;
                            bitPersample = 16;
                            break;
                        case 2:
                            sampleRate = 32000;
                            channels = 1;
                            averageBytesPerSecon = 64000;
                            blockAlign = 2;
                            bitPersample = 16;
                            break;
                        case 3:
                            sampleRate = 32000;
                            channels = 2;
                            averageBytesPerSecon = 64000;
                            blockAlign = 2;
                            bitPersample = 8;
                            break;
                        case 4:
                            sampleRate = 32000;
                            channels = 1;
                            averageBytesPerSecon = 32000;
                            blockAlign = 1;
                            bitPersample = 8;
                            break;
                        case 5:
                            sampleRate = 16000;
                            channels = 2;
                            averageBytesPerSecon = 6400;
                            blockAlign = 4;
                            bitPersample = 16;
                            break;
                        case 6:
                            sampleRate = 16000;
                            channels = 1;
                            averageBytesPerSecon = 32000;
                            blockAlign = 2;
                            bitPersample = 16;
                            break;
                        case 7:
                            sampleRate = 16000;
                            channels = 2;
                            averageBytesPerSecon = 32000;
                            blockAlign = 2;
                            bitPersample = 8;
                            break;
                        case 8:
                            sampleRate = 16000;
                            channels = 1;
                            averageBytesPerSecon = 16000;
                            blockAlign = 1;
                            bitPersample = 8;
                            break;
                        case 9:
                            sampleRate = 8000;
                            channels = 2;
                            averageBytesPerSecon = 32000;
                            blockAlign = 4;
                            bitPersample = 16;
                            break;
                        case 10:
                            sampleRate = 8000;
                            channels = 1;
                            averageBytesPerSecon = 16000;
                            blockAlign = 2;
                            bitPersample = 16;
                            break;
                        case 11:
                            sampleRate = 8000;
                            channels = 2;
                            averageBytesPerSecon = 16000;
                            blockAlign = 2;
                            bitPersample = 8;
                            break;
                        case 12:
                            sampleRate = 8000;
                            channels = 1;
                            averageBytesPerSecon = 8000;
                            blockAlign = 1;
                            bitPersample = 8;
                            break;
                        default:
                            sampleRate = 32000;
                            channels = 1;
                            averageBytesPerSecon = 64000;
                            blockAlign = 2;
                            bitPersample = 16;
                            break;
                    }
                    #region 保存音频
                    using (WaveFileWriter writer = new WaveFileWriter(filenamewav, WaveFormat.CreateCustomFormat(tag, sampleRate, channels, averageBytesPerSecon, blockAlign, bitPersample)))
                    {
                        writer.Write(tempAudio, 0, tempAudio.Length);
                    }
                    #endregion
                    Array.Clear(tempAudio, 0, tempAudio.Length);
                    tempAudioList.Clear();
                }

            }

        }

        public void IFPanReceiveMessage(ref UdpStreamType udpStream, ref  ushort dataType)
        {
            // string hostName = Dns.GetHostName();
            // IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            //  IPAddress[] addr = ipEntry.AddressList;
            //  string add = addr[0].ToString();
            IPEndPoint LocalIpEndPoint = new IPEndPoint(IPAddress.Parse("172.17.75.10"), 56513);
            try
            {
                //recvByte = udp.udpClient.Receive(ref udp.remoteEP);
                UdpStreamHeader udpHeader = new UdpStreamHeader();
                byte[] getData = ifpanUdp.Receive(ref LocalIpEndPoint);
                int len = getData.Length;
                udpHeader.HeaderMagicNumber = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(getData, 0));
                udpHeader.HeaderMinorVersion = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(getData, 4));
                udpHeader.HeaderMajorVersion = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(getData, 6));
                udpHeader.HeaderSequenceNumber = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(getData, 8));
                udpHeader.HeaderReserved = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(getData, 10));
                udpHeader.DataSize = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(getData, 12));
                udpHeader.AttributeTag = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(getData, 16));
                udpHeader.AttributeLength = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(getData, 18));
                udpHeader.TraceNumberItems = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(getData, 20));
                udpHeader.TraceReserved = BitConverter.ToChar(getData, 22);
                udpHeader.TraceOptionHeaderLength = BitConverter.ToChar(getData, 23);
                udpHeader.TraceSelectorFlags = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(getData, 24));
                //MessageBox.Show("dataType=" + Convert.ToString(dataType));
                dataType = udpHeader.AttributeTag;
                // MessageBox.Show("dataType=" + Convert.ToString(dataType));
                switch (dataType)
                {
                    case 401:       //Audio Streaming
                        udpStream.audioStream.Mode = BitConverter.ToInt16(getData, 28);
                        udpStream.audioStream.FrameLen = BitConverter.ToInt16(getData, 30);
                        udpStream.audioStream.FrequencyLow = BitConverter.ToUInt32(getData, 32);
                        udpStream.audioStream.Bandwidth = BitConverter.ToUInt32(getData, 36);
                        udpStream.audioStream.DemodulationId = BitConverter.ToUInt16(getData, 40);
                        udpStream.audioStream.DemodulationMode = BitConverter.ToString(getData, 42, 8);
                        udpStream.audioStream.FrequencyHigh = BitConverter.ToUInt32(getData, 50);
                        udpStream.audioStream.Reserved = BitConverter.ToString(getData, 54, 6);
                        udpStream.audioStream.Timestamp = BitConverter.ToUInt64(getData, 60);
                        udpStream.audioStream.WaveStream = new byte[getData.Length - 68];
                        Array.Copy(getData, 68, udpStream.audioStream.WaveStream, 0, getData.Length - 68);
                        //MessageBox.Show(Convert.ToString(udpStream.audioStream.FrameLen));
                        break;
                    case 501:       //IFPan Streaming
                        udpStream.ifpanStream.FrequencyLow = BitConverter.ToUInt32(getData, 28);
                        udpStream.ifpanStream.Span = BitConverter.ToUInt32(getData, 32);
                        udpStream.ifpanStream.Reserved = BitConverter.ToInt16(getData, 36);
                        udpStream.ifpanStream.AverageType = BitConverter.ToInt16(getData, 38);
                        udpStream.ifpanStream.MesureTime = BitConverter.ToUInt32(getData, 40);
                        udpStream.ifpanStream.FrequencyHigh = BitConverter.ToUInt32(getData, 44);
                        udpStream.ifpanStream.SelectedChannel = BitConverter.ToUInt32(getData, 48);
                        udpStream.ifpanStream.DemodulationFrequencyLow = BitConverter.ToUInt32(getData, 52);
                        udpStream.ifpanStream.DemodulationFrequencyHigh = BitConverter.ToUInt32(getData, 56);
                        udpStream.ifpanStream.Timestamp = BitConverter.ToUInt64(getData, 60);
                        udpStream.ifpanStream.pwls.Clear();
                        for (int i = 0; i < udpHeader.TraceNumberItems; i++)
                        {
                            udpStream.ifpanStream.pwls.Add(BitConverter.ToInt16(getData, 68 + i * sizeof(short)));
                        }
                        //MessageBox.Show(Convert.ToString(udpHeader.TraceNumberOfItems));
                        break;
                }
                //return true;
                //string msg = Encoding.Default.GetString(getData);
                // MessageBox.Show(msg);     
            }

            catch (Exception ex)
            {
                // return false;
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCollectData_Click(object sender, EventArgs e)
        {
            IFpanFlag = true;
            MyIFFlie = new FileStream(@"E:\testA.txt", FileMode.Create, FileAccess.Write);
            MyifFile = new StreamWriter(MyIFFlie);

            MyAUDIOFlie = new FileStream(@"E:\testB.wav", FileMode.Create, FileAccess.Write);
            MyAudioFile = new BinaryWriter(MyAUDIOFlie);
            MessageBox.Show("正在采集");
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            IsWork = false;
            string ifpanCmd;
            string ip = "172.17.75.10";
            ifpanCmd = "TRAC:UDP:TAG:OFF '" + ip + "', 56513, AUD\n";
            ifpanCmd += "TRAC:UDP:TAG:OFF '" + ip + "', 56513, IFP\n";
            ifpanCmd += "TRAC:UDP:DEL ALL \n";
           // ifpanCmd += "MEM:CLE 0, MAX \n";
            byte[] sendData = Encoding.Default.GetBytes(ifpanCmd);
            ns.Write(sendData, 0, sendData.Length);
            myTcpClient.Close();
            //myUdpClient.Close();
            //DisplayPscanThread.Abort();
            ns.Dispose();
            MessageBox.Show("已经停止！");
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            MScan.DelMemory = true;
        }

        private void MscanStart_Click(object sender, EventArgs e)
        {
            if (IsWork)
            {
                MessageBox.Show("其他功能还未停止");
                return;
            }
            IsWork = true;
            if (MScan.MemoryList.Count == 0)
            {
                MessageBox.Show("内存为空");
                return;
            }
            try
            {
                MScan mscan = new MScan(MScan.MemoryMumber, "172.17.75.10", "9999");
                myTcpClient.SendMessage(mscan.MSCAN());
                udp.ControllReceiveBar = true;
                DisplayMscanThread = new Thread(DisplayMScanSpectrum);
                DisplayMscanThread.IsBackground = true;
                //Thread.Sleep(10000);
                DisplayMscanThread.Start();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.ToString());
            }
        }
        public void DisplayMScanSpectrum()
        {
            while (udp.ControllReceiveBar)
            {
                UdpStreamMscan udpMscanStream = new UdpStreamMscan();
                byte[] recvByte = null;
                try { recvByte = udp.udpClient.Receive(ref udp.remoteEP);
                  //这个先放着，以后再改
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.ToString());
                }
                int length = recvByte.Length;
                int number = MScan.MemoryMumber;
                UdpStreamHeader udpHeader = new UdpStreamHeader();
               
                udpHeader.HeaderMagicNumber = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(recvByte, 0));//0 3
                udpHeader.HeaderMinorVersion = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 4));//4 5
                udpHeader.HeaderMajorVersion = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 6));//6 7
                udpHeader.HeaderSequenceNumber = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 8)); //8 9
                udpHeader.HeaderReserved = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 10));//10 11
                udpHeader.DataSize = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(recvByte, 12));//12 15
                udpHeader.AttributeTag = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 16));//16 17
                udpHeader.AttributeLength = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 18));//18 19
                udpHeader.TraceNumberItems = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 20));//20 21
                udpHeader.TraceReserved = BitConverter.ToChar(recvByte, 22);//22
                udpHeader.TraceOptionHeaderLength = BitConverter.ToChar(recvByte, 23);//23
                udpHeader.TraceSelectorFlags = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(recvByte, 24));//24 27
                ushort dataType = udpHeader.AttributeTag;
                switch (dataType)
                {
                    case 201:
                    string s = "";
                    udpMscanStream.CycleCount = BitConverter.ToInt16(recvByte, 28);//28 29
                    udpMscanStream.HoldTime = BitConverter.ToInt16(recvByte, 30);//30 31
                    udpMscanStream.DwellTime = BitConverter.ToInt16(recvByte, 32);//32 33
                    udpMscanStream.Direction = BitConverter.ToInt16(recvByte, 34);//34 35
                    udpMscanStream.StopSignal = BitConverter.ToInt16(recvByte, 36);//36 37
                    //udpMscanStream.Reserved1 = BitConverter.ToInt16(recvByte, 38);//说明书上有问题 38到51不知道什么鬼
                    udpMscanStream.FftLevel = BitConverter.ToInt16(recvByte, 52);//电平值
                    udpMscanStream.Chan = BitConverter.ToInt16(recvByte, 56);//哪个内存
                    udpMscanStream.FreqLow = BitConverter.ToUInt32(recvByte, 58);//频率值
                     s = s + " " + udpMscanStream.FftLevel;
                     s = s + " " + udpMscanStream.Chan;
                     s = s + " " + udpMscanStream.FreqLow;
                    FileStream fss = new FileStream(@"F:\TestTcp\recs.txt", FileMode.Append, FileAccess.Write);
                    StreamWriter sws = new StreamWriter(fss);
                    sws.WriteLine(s);
                    sws.Close();
                    double[] bufX = { 0 };              //X轴数据（频率）
                    double[] bufY = { 0 };              //Y轴数据（强度）
                    double tempFre = udpMscanStream.FreqLow / 1000000;
                    try
                    {
                        bufX = new double[1];
                        bufY = new double[1];
                        bufY[0] = udpMscanStream.FftLevel;
                        bufX[0] = tempFre;
                            scope4.Invoke(showMscanMsgCallback, bufX, bufY);
                   }
                   catch (Exception ex)
                   {
                         MessageBox.Show("DisplayIFPanSpectrum 错误! " + ex.Message);

                   }
                    break;
             }               
          }
        }

        private void StopMc_Click(object sender, EventArgs e)
        {
            IsWork = false;
            string ifpanCmd;
            string ip = cbBoxEM100IP.Text;
            ifpanCmd = "TRAC:UDP:TAG:OFF '" + ip + "', 56513, MSC\n";
            ifpanCmd += "TRAC:UDP:DEL ALL \n";
            //ifpanCmd += "MEM:CLE 0, MAX \n";
            byte[] sendData = Encoding.Default.GetBytes(ifpanCmd);
            ns.Write(sendData, 0, sendData.Length);
            udp.ControllReceiveBar = false;
            myTcpClient.Close();
            //udp.udpClient.Close();
            ns.Dispose();
           // DisplayMscanThread.Abort();
            MessageBox.Show("已经停止！");

        }

        private void MscanCon_Click(object sender, EventArgs e)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(cbBoxEM100IP.Text), 5555);
            myTcpClient = new Tcpclient();
            try
            {
                myTcpClient.tcpClient.Connect(ipEndPoint);
                ns = myTcpClient.tcpClient.GetStream();
                MessageBox.Show("TCP/IP 连接成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show("TCP连接失败！" + ex.Message);
            }
        }

        private void FsCon_Click(object sender, EventArgs e)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(cbBoxEM100IP.Text), 5555);
            myTcpClient = new Tcpclient();
            try
            {
                myTcpClient.tcpClient.Connect(ipEndPoint);
                ns = myTcpClient.tcpClient.GetStream();
                MessageBox.Show("TCP/IP 连接成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show("TCP连接失败！" + ex.Message);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            FscanStartFre = FsStart.Text;
            FscanStopFre = FsStop.Text;
            FscanStepFre = FsStep.Text;
            FscanDwell = FsDell.Text;
        }
        private void DoFs_Click(object sender, EventArgs e)
        {
            if (IsWork)
            {
                MessageBox.Show("其他功能还未停止");
                return;
            }
            IsWork = true;
            try
            {
                FScan fscan = new FScan(FscanStartFre, FscanStopFre, FscanStepFre,FscanDwell, "172.17.75.10", "9999");
                myTcpClient.SendMessage(fscan.FSCAN());
                udp.ControllReceiveBar = true;
                DisplayFscanThread = new Thread(DisplayFScanSpectrum);
                DisplayFscanThread.IsBackground = true;
                //Thread.Sleep(10000);
                DisplayFscanThread.Start();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.ToString());
            }
        }

        public void DisplayFScanSpectrum()
        {
            while (udp.ControllReceiveBar)
            {
                UdpStreamFscan udpFscanStream = new UdpStreamFscan();
                byte[] recvByte = null;
                try
                {
                    recvByte = udp.udpClient.Receive(ref udp.remoteEP);
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.ToString());
                }
                int length = recvByte.Length;
                int number = 0;
                UdpStreamHeader udpHeader = new UdpStreamHeader();
                udpHeader.HeaderMagicNumber = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(recvByte, 0));//0 3
                udpHeader.HeaderMinorVersion = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 4));//4 5
                udpHeader.HeaderMajorVersion = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 6));//6 7
                udpHeader.HeaderSequenceNumber = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 8)); //8 9
                udpHeader.HeaderReserved = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 10));//10 11
                udpHeader.DataSize = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(recvByte, 12));//12 15
                udpHeader.AttributeTag = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 16));//16 17
                udpHeader.AttributeLength = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 18));//18 19
                udpHeader.TraceNumberItems = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(recvByte, 20));//20 21
                udpHeader.TraceReserved = BitConverter.ToChar(recvByte, 22);//22
                udpHeader.TraceOptionHeaderLength = BitConverter.ToChar(recvByte, 23);//23
                udpHeader.TraceSelectorFlags = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(recvByte, 24));//24 27
                ushort dataType = udpHeader.AttributeTag;
                switch (dataType)
                {
                    case 101:
                        udpFscanStream.CycleCount = BitConverter.ToInt16(recvByte, 28);//28 29
                        udpFscanStream.HoldTime = BitConverter.ToInt16(recvByte, 30);//30 31
                        udpFscanStream.DwellTime = BitConverter.ToInt16(recvByte, 32);//32 33
                        System.Console.WriteLine("DwellTime------>" + udpFscanStream.DwellTime);
                        udpFscanStream.Direction = BitConverter.ToInt16(recvByte, 34);//34 35
                        udpFscanStream.StopSignal = BitConverter.ToInt16(recvByte, 36);//36 37
                        udpFscanStream.StartFreqLow = BitConverter.ToUInt32(recvByte, 38);//38,39,40,41
                        udpFscanStream.StopFreqLow = BitConverter.ToUInt32(recvByte, 42);//42,43,44,45
                        udpFscanStream.StepFreq = BitConverter.ToUInt32(recvByte, 46);//46,47,48,49
                        udpFscanStream.StartFreqHigh = BitConverter.ToUInt32(recvByte, 50);//50,51,52,53
                        udpFscanStream.StopFreqHigh = BitConverter.ToUInt32(recvByte, 54);//54,55,56,57
                        udpFscanStream.Reserved = BitConverter.ToInt16(recvByte, 58);//58 59
                        udpFscanStream.TimeSwap = BitConverter.ToUInt64(recvByte, 60);//60 67
                        udpFscanStream.FftLevel = BitConverter.ToInt16(recvByte,68);//68,69
                        udpFscanStream.FreqLow = BitConverter.ToUInt32(recvByte,70);                 
                        string s = ""; 
                        FileStream fss = new FileStream(@"F:\TestTcp\recFScan.txt", FileMode.Append, FileAccess.Write);
                        StreamWriter sws = new StreamWriter(fss);
                        for (int i = 0; i < udpHeader.TraceNumberItems; i++)
                        {
                            s = udpFscanStream.FftLevel + "   " + udpFscanStream.FreqLow;
                            sws.WriteLine(s);
                        }
                        sws.Close();
                        break;
                }
                for (float f = udpFscanStream.StartFreqLow; f <= udpFscanStream.StopFreqLow; f = f + udpFscanStream.StepFreq)
                {
                    udpFscanStream.TrueFre.Add(f / 1000000);
                    number++;
                }
                double[] bufX = { 0 };              //X轴数据（频率）
                double[] bufY = { 0 };              //Y轴数据（强度）
                double tempFre = udpFscanStream.FreqLow / 1000000;
                try
                {
                     bufX = new double[number];
                     bufY = new double[number];
                    for(int i = 0;i < number;++i){
                        bufX[i] = udpFscanStream.TrueFre[i];
                    }
                    int Position = (int)((udpFscanStream.FreqLow - udpFscanStream.StartFreqLow) / udpFscanStream.StepFreq);
                    if (Position >= number) Position = number - 1;
                    bufY[Position] = udpFscanStream.FftLevel/10;                   
                    scope3.Invoke(showFscanMsgCallback, bufX, bufY);
                }
                catch (Exception ex)
                {
                     MessageBox.Show("DisplayIFPanSpectrum 错误! " + ex.Message);

                }
                       
             }

         }

        private void StopFs_Click(object sender, EventArgs e)
        {
            IsWork = false;
            string ifpanCmd;
            string ip = cbBoxEM100IP.Text;
            ifpanCmd = "TRAC:UDP:TAG:OFF '" + ip + "', 56513, SWE\n";
            ifpanCmd += "TRAC:UDP:DEL ALL \n";
            //ifpanCmd += "MEM:CLE 0, MAX \n";
            byte[] sendData = Encoding.Default.GetBytes(ifpanCmd);
            ns.Write(sendData, 0, sendData.Length);
            udp.ControllReceiveBar = false;
            myTcpClient.Close();
            //udp.udpClient.Close();
            ns.Dispose();
            // DisplayFscanThread.Abort();
            MessageBox.Show("已经停止！");
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            int high = this.Height;
            int width = this.Width;

            scope1.Height = high / 2 - 5;
            groupBox1.Height = high / 3  - 5;
            groupBox1.Width = width - 5;

            groupBox1.Top = scope1.Top + high / 2 - 5;
        }



       

  


        
    }
}
