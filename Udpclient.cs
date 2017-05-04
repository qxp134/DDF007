using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace WindowsFormsApplication3
{
    /// <summary>
    /// 用UDP 异步接收 数据 构造函数 没写
    /// </summary>
    class Udpclient
    {
        UdpStreamHeader udpHeader = new UdpStreamHeader();
        public UdpStreamPscan udpPscanStream = new UdpStreamPscan();
        public UdpStreamAudio udpAudioStream = new UdpStreamAudio();
        public UdpStreamIfpan udpIfStream = new UdpStreamIfpan();
        public bool ControllReceiveBar = false;
        public static IPEndPoint localEP = new IPEndPoint(IPAddress.Parse("172.17.75.10"), 9999);//本地IP和端口
        public IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("172.17.75.1"), 0);//仪器IP: 172.17.75.1 UDP端口：3010
        public UdpClient udpClient = new UdpClient(localEP);
        byte[] recvByte = null;
        public static bool messageReceived = false;
        public bool Databar = false;
        public List<uint> Frq = new List<uint>();
        public List<short> FftLevel = new List<short>();
        //FileStream fs = new FileStream(@"F:\TestTcp\TestTcp\TestTcp\rec.txt", FileMode.Append, FileAccess.Write);
        //StreamWriter sw = new StreamWriter(fs);
        public Udpclient()
        {

        }
        /// <summary>
        /// IP地址 端口号 通过SCPI命令设置 
        /// </summary>
        public void UdpReceive()
        {
            recvByte = udpClient.Receive(ref remoteEP);
            int length = recvByte.Length;

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
                    udpPscanStream.StartFreqLow = BitConverter.ToUInt32(recvByte, 28);//28 31
                    udpPscanStream.StopFreqLow = BitConverter.ToUInt32(recvByte, 32);//32 33 34 35
                    udpPscanStream.StepFreq = BitConverter.ToUInt32(recvByte, 36);//36 37 38 39 
                    udpPscanStream.StartFreqHigh = BitConverter.ToUInt32(recvByte, 40);//40 41 42 43
                    udpPscanStream.StopFreqHigh = BitConverter.ToUInt32(recvByte, 44);//44 45 46 47
                    udpPscanStream.Reserved = BitConverter.ToString(recvByte, 48, 4);
                    udpPscanStream.TimeSwap = BitConverter.ToUInt64(recvByte, 52);
                    //System.Console.WriteLine(udpPscanStream.TimeSwap);
                    String allFre = "";//我打算第一行输出一行频率
                    long number = 0;//计数，看看有多少个电平值 这个在哪儿算，还需改进
                    float tempFreq = udpPscanStream.StartFreqLow;
                    for (; tempFreq <= udpPscanStream.StopFreqLow; tempFreq += udpPscanStream.StepFreq)
                    {
                        float temp = tempFreq / 1000000;
                        allFre = allFre + temp + "  ";
                        number++;
                    }
                    udpPscanStream.FftLevel.Clear();
                    udpPscanStream.FreqLow.Clear();
                    for (int i = 0; i < udpHeader.TraceNumberItems; i++)
                    {
                        udpPscanStream.FftLevel.Add(BitConverter.ToInt16(recvByte, 60 + i * sizeof(short)));
                    }
                    int dataBegin = 60 + udpHeader.TraceNumberItems * sizeof(short);
                    for (int i = 0; i < udpHeader.TraceNumberItems; i++)
                    {
                        udpPscanStream.FreqLow.Add(BitConverter.ToUInt32(recvByte, dataBegin + i * sizeof(uint)));

                    }
                    FileStream fs = new FileStream(@"F:\TestTcp\rec.txt", FileMode.Append, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs);
                    if (UdpStreamPscan.IsFrist == true)
                    {
                        UdpStreamPscan.IsFrist = false;
                        sw.WriteLine(allFre);//先输出一行频率列表
                    }
                    if (udpPscanStream.FreqLow.ElementAt(0) == udpPscanStream.StartFreqLow && Databar == false && UdpStreamPscan.NowFre == 1 || udpPscanStream.FreqLow.ElementAt(0) == udpPscanStream.StartFreqLow && Databar == false && UdpStreamPscan.NowFre == udpPscanStream.StepFreq)//为了达到拼包的目的，设计一组静态变量
                    {
                        UdpStreamPscan.Level.AddRange(udpPscanStream.FftLevel);
                        // System.Console.WriteLine(" UdpStreamPscan.NowFre--------->" + UdpStreamPscan.NowFre);
                        UdpStreamPscan.NowFre = udpPscanStream.FreqLow.ElementAt(udpHeader.TraceNumberItems - 1) + udpPscanStream.StepFreq;

                    }
                    else if (UdpStreamPscan.NowFre == udpPscanStream.FreqLow.ElementAt(0) && Databar == false)//这个条件其实跟上一个是一致的
                    {
                        UdpStreamPscan.Level.AddRange(udpPscanStream.FftLevel);
                        UdpStreamPscan.NowFre = udpPscanStream.FreqLow.ElementAt(udpHeader.TraceNumberItems - 1) + udpPscanStream.StepFreq;
                    }

                    if (UdpStreamPscan.NowFre == udpPscanStream.StepFreq) Databar = true;
                    if (Databar == true)//设置一个判断是否拼完的标志
                    {
                        // UdpStreamPscan.Level.AddRange(udpPscanStream.FftLevel);
                        UdpStreamPscan.NowFre = udpPscanStream.StepFreq;

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
                        Databar = false;
                        UdpStreamPscan.Level.Clear();
                        UdpStreamPscan.TrueLevel.Clear();
                    }

                    sw.Flush();
                    sw.Close();
                    break;
            }
        }
        public void ReceiveData()
        {
            while (ControllReceiveBar)
            {
                UdpReceive();
                //break;
            }
        }
        /// <summary>
        /// 关闭UDP 接收
        /// </summary>
        public void StopReceive()
        {
            udpClient.Client.Shutdown(SocketShutdown.Receive);
            udpClient.Client.Close();
            udpClient.Close();
            //fs.Close();
        }


    }

}
