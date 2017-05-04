using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApplication3.Util;

namespace WindowsFormsApplication3
{
    class MScan
    {
        public string LocalIp;
        public string LocalPort;
        public int StratMemory = 600;
        public int EndMemory;
        public static List<String> MemoryList = new List<string>();
        public static List<double> FreList = new List<double>();
        public static int MemoryMumber = 0;
        public static bool DelMemory = false;
        public MScan(int EndMemory, string LocalIp, string LocalPort)
        {
            //this.DelMemory = DelMemory;
            this.EndMemory = StratMemory + EndMemory;
            this.LocalIp = LocalIp;
            this.LocalPort = LocalPort;
            foreach(String i in MemoryList){
                Double temp = Util.Util.StringToDouble(i);
                FreList.Add(temp);
            }
        }
        public string MSCAN()
        {
            string mScan = "";
            mScan += "*CLS\n";
            mScan += "*RST";
            mScan += "\r\nCALC:IFPAN:AVER:TYPE OFF";
            mScan += "\r\nABOR";
            mScan += "\r\nTRAC:FEED:CONT ITRACE,ALW";
            mScan += "\r\nTRAC:FEED:CONT MTRACE,ALW";
            mScan += "\r\nSENS:FREQ:MODE CW";
            mScan += "\r\nSTAT:EXT:ENAB #B1111111111111111";
            mScan += "\r\nSTAT:EXT:PTR #B1111111111111111";
            mScan += "\r\nSTAT:EXT:NTR #B1111111111111111";
            mScan += "\r\nSTAT:OPER:ENAB #B1111111111111111";
            mScan += "\r\nSTAT:OPER:PTR #B1111111111111111";
            mScan += "\r\nSTAT:OPER:NTR #B1111111111111111";
            mScan += "\r\nSTAT:OPER:SWE:ENAB #B1111111111111111";
            mScan += "\r\nSTAT:OPER:SWE:PTR #B1111111111111111";
            mScan += "\r\nSTAT:OPER:SWE:NTR #B1111111111111111";
            mScan += "\r\nSTAT:TRAC:ENAB #B11111111111";
            mScan += "\r\nSTAT:TRAC:PTR #B11111111111";
            mScan += "\r\nSTAT:TRAC:NTR #B00000000000";
            mScan += "\r\nINIT";
            if (DelMemory)
                mScan +="\r\nMEM:CLE 0,MAX";
            for (int i = 1; i < MemoryList.Count(); i++)
            {
                mScan += "\r\nMEM:CONT " + i.ToString() + "," + MemoryList[i] + " MHZ,30,FM,300,10,OFF,OFF,OFF,OFF,ON";
                //System.Console.WriteLine("\r\nMEM:CONT " + i.ToString() + "," + MemoryList[i].ToString() + " MHZ,30,FM,300,10,OFF,OFF,OFF,OFF,ON");
            }
            mScan += "\r\nSENS:FREQ:MODE MSC";
            mScan += "\r\nSENS:MSC:LIST:STAR " + StratMemory + "";
            mScan += "\r\nSENS:MSC:LIST:STOP " + EndMemory + "";
            mScan += "\r\nSENS:MSC:COUN INF";//次数
            mScan +="\r\nSENS:MSC:DWEL 5";//悬停时间
            mScan +="\r\nSENS:MSC:HOLD:TIME 1";
            mScan +="\r\nSENS:MSC:DIR UP";
            mScan += "\r\nSENS:MSC:CONT:OFF \'STOP:SIGN\'";
            mScan += "\r\nABOR";
            mScan += "\r\nINIT";
            mScan += "\r\nTRAC:UDP:FLAG:ON '" + LocalIp + "', 9999, 'FREQ:RX','CHAN','SQU','OPT','SWAP','VOLT:AC','FREQ:OFFS','FSTR'";
            mScan += "\r\nTRAC:UDP:TAG:ON '" + LocalIp + "', 9999, IFP";
            mScan += "\r\nTRAC:UDP:TAG:ON '" + LocalIp + "', 9999, PSC";
            mScan += "\r\nTRAC:UDP:TAG:ON '" + LocalIp + "', 9999, FSC";
            mScan += "\r\nTRAC:UDP:TAG:ON '" + LocalIp + "', 9999, MSC";
            mScan += "\r\nTRAC:UDP:TAG:OFF '" + LocalIp + "', 9999, AUD";
            mScan += "\r\nTRAC:UDP:FLAG:ON '" + LocalIp + "', 9999, 'OPT'";
            mScan += "\r\nTRAC:UDP:FLAG:ON '" + LocalIp + "', 9999, 'SWAP'";
            mScan += "\r\nSTAT:EXT:ENAB 63423";
            mScan += "\r\nSTAT:OPER:ENAB 280";
            mScan += "\r\nSENS:FUNC:OFF 'FREQ:OFFS'";
            mScan += "\r\nSENS:FUNC:ON 'FSTR'";
            mScan += "\r\n*STB";
            return mScan;
        }
    }
}

