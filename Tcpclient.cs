using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace WindowsFormsApplication3
{
    class Tcpclient
    {
        public TcpClient tcpClient = new TcpClient();
        private IPEndPoint remoteEP = null;
        IPAddress LocalIp = IPAddress.Parse("172.17.75.10");
        int LocalPort = 9999;
        private const int buffersize = 1024;
        private NetworkStream stream = null;
        private ASCIIEncoding encoder = new ASCIIEncoding();
       // FileStream fs = new FileStream(@"F:\TestTcp\query.txt", FileMode.Append, FileAccess.Write);
        public Tcpclient(IPEndPoint ipendpiont)
        {
            remoteEP = ipendpiont;
        }

        public Tcpclient()
        {
            // TODO: Complete member initialization
        }

        public void Query(string cmd)
        {
            //string cmd = init();
            try
            {
                //tcpClient.Connect(remoteEP);
                if (!tcpClient.Connected)
                {
                    tcpClient.Connect(remoteEP);
                    tcpClient.SendBufferSize = buffersize;
                    tcpClient.ReceiveBufferSize = buffersize;
                }
                stream = tcpClient.GetStream();
                byte[] data = encoder.GetBytes(cmd);
                if (stream.CanWrite)
                {
                    stream.Write(data, 0, data.Length);
                    stream.Flush();
                }
                //Console.WriteLine("发送命令成功");
                //接收的回复
                byte[] bytes = new byte[buffersize];
                string rdata = string.Empty;
                int length = stream.Read(bytes, 0, bytes.Length);
                if (length > 0)
                {
                    rdata = encoder.GetString(bytes, 0, length);
                   // fs.Write(bytes, 0, length);
                    //fs.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("发送命令失败！ " + e.ToString());
            }
        }
        public void SendMessage(string cmd)
        {
            try
            {
                if (!tcpClient.Connected)
                {
                    tcpClient.Connect(remoteEP);
                    tcpClient.SendBufferSize = buffersize;
                    tcpClient.ReceiveBufferSize = buffersize;
                }
                stream = tcpClient.GetStream();
                byte[] data = encoder.GetBytes(cmd);
                if (stream.CanWrite)
                {
                    stream.Write(data, 0, data.Length);
                    stream.Flush();
                }
            }
            catch (Exception ms)
            {
                ms.ToString();
            }
        }
        public void Close()
        {
            RemovEQudp();
            CloseSocket();
        }
        public void CloseSocket()
        {

            if (tcpClient.Connected)
            {
                tcpClient.Client.Close();
                tcpClient.Close();
            }
        }
        public void RemovEQudp()
        {
            string tmpCmd = string.Empty;
            tmpCmd += string.Format("TRAC:UDP:DEL \"{0}\", {1}\n", LocalIp, LocalPort);
            tmpCmd += string.Format("TRAC:UDP:DEL \"{0}\", {1}\n", LocalIp, LocalPort);
            SendMessage(tmpCmd);
        }
        public void QueryID()
        {
            Query(queryID());
        }
        /// <summary>
        /// 测量PSCAN
        /// </summary>
        public void M_pscan()
        {
            PScan pscan = new PScan("80 Mhz", "90 Mhz", "50000 Hz", "172.17.75.10", "9999");
            SendMessage(pscan.PSCAN());
        }

        private string queryID()
        {
            string mander = string.Empty;
            mander = string.Format("*IDN?\n");//查询仪器型号
            return mander;
        }
        private string FFM()
        {
            string cmd = string.Empty;
            return cmd;
        }
        private string MSCAN()
        {
            string cmd = string.Empty;
            return cmd;
        }
        /// <summary>
        ///  起止频率 步长 设置仪器 发送到 计算机的 IP和端口
        /// </summary>
        /// <returns></returns>
        public string PSCAN()
        {
            string pscanCmd = string.Empty;
            string startfreq = "87.5 MHz";
            string stopfreq = "108 MHz";
            string step = "50000 Hz";
            //IPAddress LocalIp = IPAddress.Parse("172.17.75.10");
            //int LocalPort = 9999;
            pscanCmd += "CALC:IFPAN:AVER:TYPE OFF\n";
            pscanCmd += "ABOR\n";
            pscanCmd += "TRAC:FEED:CONT ITRACE,ALW\n";
            pscanCmd += "TRAC:FEED:CONT MTRACE,ALW\n";
            pscanCmd += "SENS:FREQ:MODE CW\n";
            pscanCmd += "STAT:EXT:ENAB #B1111111111111111\n";
            pscanCmd += "STAT:EXT:PTR #B1111111111111111\n";
            pscanCmd += "STAT:EXT:NTR #B1111111111111111\n";
            pscanCmd += "STAT:OPER:ENAB #B1111111111111111\n";
            pscanCmd += "STAT:OPER:PTR #B1111111111111111\n";
            pscanCmd += "STAT:OPER:NTR #B1111111111111111\n";
            pscanCmd += "STAT:OPER:SWE:ENAB #B1111111111111111\n";
            pscanCmd += "STAT:OPER:SWE:PTR #B1111111111111111\n";
            pscanCmd += "STAT:OPER:SWE:NTR #B1111111111111111\n";
            pscanCmd += "STAT:TRAC:ENAB #B11111111111\n";
            pscanCmd += "STAT:TRAC:PTR #B11111111111\n";
            pscanCmd += "STAT:TRAC:NTR #B00000000000\n";
            pscanCmd += "INIT\n";
            pscanCmd += "SENS:FREQ:PSC:COUN INF\n";
            //pscanCmd += string.Format("SENS:FREQ:PSC:STOP {0};:SENS:FREQ:PSC:STAR {1};:SENS:PSC:STEP {2}\n", stopfreq, startfreq, step);
            pscanCmd += "SENS:FREQ:PSC:STOP " + stopfreq + "MHz;:SENS:FREQ:PSC:STAR " + startfreq + "MHz;:SENS:PSC:STEP " + step + "KHz\n";
            pscanCmd += "ABOR\n";
            pscanCmd += "SENS:FREQ:MODE PSC\n";
            pscanCmd += "INIT\n";
            pscanCmd += string.Format("TRAC:UDP:FLAG:ON \"{0}\", {1}, \"FREQ:RX\",\"CHAN\",\"SQU\",\"OPT\",\"SWAP\",\"VOLT:AC\",\"FREQ:OFFS\",\"FSTR\"\n", LocalIp, LocalPort);
            pscanCmd += string.Format("TRAC:UDP:TAG:ON \"{0}\", {1}, IFP\n", LocalIp, LocalPort);
            pscanCmd += string.Format("TRAC:UDP:TAG:ON \"{0}\", {1}, PSC\n", LocalIp, LocalPort);
            pscanCmd += string.Format("TRAC:UDP:TAG:ON \"{0}\", {1}, FSC\n", LocalIp, LocalPort);
            pscanCmd += string.Format("TRAC:UDP:TAG:ON \"{0}\", {1}, MSC\n", LocalIp, LocalPort);
            pscanCmd += string.Format("TRAC:UDP:TAG:ON \"{0}\", {1}, AUD\n", LocalIp, LocalPort);
            pscanCmd += string.Format("TRAC:UDP:FLAG:ON \"{0}\", {1}, \"OPT\"\n", LocalIp, LocalPort);
            pscanCmd += string.Format("TRAC:UDP:FLAG:ON \"{0}\", {1}, \"SWAP\"\n", LocalIp, LocalPort);
            pscanCmd += "STAT:EXT:ENAB 63423\n";
            pscanCmd += "STAT:OPER:ENAB 280\n";
            pscanCmd += "SENS:FUNC:OFF \"FREQ:OFFS\"\n";
            pscanCmd += "SENS:FUNC:OFF \"FSTR\"\n";
            pscanCmd += "*STB\n";
            return pscanCmd;
        }
    }
}