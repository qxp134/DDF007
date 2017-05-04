using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication3
{
    class FScan
    {
        public string startfreq;
        public string stopfreq;
        public string step;
        public string LocalIp;
        public string LocalPort;
        public string DwellTime;
        public FScan(string startfreq,string stopfreq,string step,string Dwell,string LocalIP,string LocalPort)
        {
            this.startfreq=startfreq;
            this.stopfreq = stopfreq;
            this.step = step;
            this.LocalIp = LocalIP;
            this.LocalPort = LocalPort;
            this.DwellTime = Dwell;
        }
        public string FSCAN()
        {
            String FScanCmd = "";
            FScanCmd += "*IDN?";
            FScanCmd += "\r\nCALC:IFPAN:AVER:TYPE OFF";               //直接得到中频测量的数据
            FScanCmd += "\r\nABOR";                                   //停止一个对应的活动扫描
            FScanCmd += "\r\nTRAC:FEED:CONT ITRACE,ALW";              //每次的测量值的相关信息都存储在itrace
            FScanCmd += "\r\nTRAC:FEED:CONT MTRACE,ALW";              //每次的测量值都存储在mtrace 
            FScanCmd += "\r\nSENS:FREQ:MODE CW";                   //设置频率模式  
            FScanCmd += "\r\nSTAT:EXT:ENAB #B1111111111111111";       //扩展寄存器
            FScanCmd += "\r\nSTAT:EXT:PTR #B1111111111111111";
            FScanCmd += "\r\nSTAT:EXT:NTR #B1111111111111111";
            FScanCmd += "\r\nSTAT:OPER:ENAB #B1111111111111111";      //状态寄存器
            FScanCmd += "\r\nSTAT:OPER:PTR #B1111111111111111";
            FScanCmd += "\r\nSTAT:OPER:NTR #B1111111111111111";
            FScanCmd += "\r\nSTAT:OPER:SWE:ENAB #B1111111111111111";
            FScanCmd += "\r\nSTAT:OPER:SWE:PTR #B1111111111111111";
            FScanCmd += "\r\nSTAT:OPER:SWE:NTR #B1111111111111111";
            FScanCmd += "\r\nSTAT:TRAC:ENAB #B11111111111";
            FScanCmd += "\r\nSTAT:TRAC:PTR #B11111111111";
            FScanCmd += "\r\nSTAT:TRAC:NTR #B00000000000";
            FScanCmd += "\r\nINIT";
            FScanCmd += "\r\nSENS:FREQ:MODE SWE";
            FScanCmd += "\r\nSENS:FREQ:SWE:COUN INF";
            FScanCmd += "\r\nSENS:FREQ:STOP " + stopfreq + ";:SENS:FREQ:STAR " + startfreq + ";:SENS:SWE:STEP " + step;
            //FScanCmd += "\r\nSENS:FREQ:SWE:COUT ON";
            FScanCmd += "\r\nSWE:DWEL " + DwellTime;
            FScanCmd += "\r\nSWE:HOLD:TIME 1";
            FScanCmd += "\r\nSENS:SWE:CONT:OFF \'STOP:SIGN\'";
            FScanCmd += "\r\nOUTP:SQU ON";
            FScanCmd += "\r\nOUTP:SQU:THR 10 dbuV";
            FScanCmd += "\r\nSENS:BAND 9 KHz";
            FScanCmd += "\r\nSENS:DEM FM";
            FScanCmd += "\r\nMEAS:TIME DEF";
            FScanCmd += "\r\nMEAS:MODE CONT";
            FScanCmd += "\r\nINP:ATT: STAT OFF";
            FScanCmd += "\r\nSENS:FREQ:AFC OFF";
            FScanCmd += "\r\nSENS:GCON:MODE AGC";
            FScanCmd += "\r\nOUTP:TONE OFF";
            FScanCmd += "\r\nSYST:AUD:REM:NODE 2";
            FScanCmd += "\r\nABOR";
            FScanCmd += "\r\nINIT";
            FScanCmd += "\r\nTRAC:UDP:FLAG:ON '" + LocalIp + "', 9999, 'FREQ:RX','SQU','OPT','SWAP','VOLT:AC'";  //设置一个udp地址和端口,增加指定的标志(决定udp数据中应包含什么)
            FScanCmd += "\r\nTRAC:UDP:TAG:OFF '" + LocalIp + "', 9999, IFP";   //设置一个udp地址和端口,传输指定的udp属性流。
            FScanCmd += "\r\nTRAC:UDP:TAG:ON '" + LocalIp + "', 9999, PSC";
            FScanCmd += "\r\nTRAC:UDP:TAG:ON '" + LocalIp + "', 9999, FSC";
            FScanCmd += "\r\nTRAC:UDP:TAG:ON '" + LocalIp + "', 9999, MSC";
            FScanCmd += "\r\nTRAC:UDP:TAG:OFF '" + LocalIp + "', 9999, AUD";
            FScanCmd += "\r\nTRAC:UDP:FLAG:ON '" + LocalIp + "', 9999, 'OPT'";
            FScanCmd += "\r\nTRAC:UDP:FLAG:ON '" + LocalIp + "', 9999, 'SWAP'";
            FScanCmd += "\r\nSTAT:EXT:ENAB 63423";
            FScanCmd += "\r\nSTAT:OPER:ENAB 280";
            FScanCmd += "\r\nSENS:FUNC:OFF 'FREQ:OFFS'";
            FScanCmd += "\r\nSENS:FUNC:OFF 'FSTR'";
            FScanCmd += "\r\n*STB";
            return FScanCmd;
        }
    }
}
