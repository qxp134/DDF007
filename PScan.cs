using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication3
{
    class PScan
    {
        public string startfreq;
        public string stopfreq;
        public string step;
        public string LocalIp;
        public string LocalPort;
        public PScan(string startfreq = "87 Mhz", string stopfreq = "108 Mhz", string step = "25000 Hz", string LocalIP = "172.17.75.10", string LocalPort = "9999")
        {//默认值是"87 Mhz", "108 Mhz", "25000 Hz", "172.17.75.10", "9999"
            this.startfreq = startfreq;
            this.stopfreq = stopfreq;
            this.step = step;
            this.LocalIp = LocalIP;
            this.LocalPort = LocalPort;
        }
        public string PSCAN()
        {
            string pscanCmd = string.Empty;
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
            // pscanCmd += "SENS:FREQ:PSC:STOP " + stopfreq + "MHz;:SENS:FREQ:PSC:STAR " + startfreq + "MHz;:SENS:PSC:STEP " + step + "KHz\n";
            pscanCmd += string.Format("SENS:FREQ:PSC:STOP {0};:SENS:FREQ:PSC:STAR {1};:SENS:PSC:STEP {2}\n", stopfreq, startfreq, step);
            pscanCmd += "ABOR\n";
            pscanCmd += "SENS:FREQ:MODE PSC\n";
            pscanCmd += "INIT\n";
            pscanCmd += string.Format("TRAC:UDP:FLAG:ON \"{0}\", {1}, \"FREQ:RX\",\"CHAN\",\"SQU\",\"OPT\",\"SWAP\",\"VOLT:AC\",\"FREQ:OFFS\",\"FSTR\"\n", LocalIp, LocalPort);
            //pscanCmd += string.Format("TRAC:UDP:TAG:ON \"{0}\", {1}, IFP\n", LocalIp, LocalPort);
            pscanCmd += string.Format("TRAC:UDP:TAG:ON \"{0}\", {1}, PSC\n", LocalIp, LocalPort);
           // pscanCmd += string.Format("TRAC:UDP:TAG:ON \"{0}\", {1}, FSC\n", LocalIp, LocalPort);
           // pscanCmd += string.Format("TRAC:UDP:TAG:ON \"{0}\", {1}, MSC\n", LocalIp, LocalPort);
          // pscanCmd += string.Format("TRAC:UDP:TAG:ON \"{0}\", {1}, AUD\n", LocalIp, LocalPort);
           // pscanCmd += string.Format("TRAC:UDP:FLAG:ON \"{0}\", {1}, \"OPT\"\n", LocalIp, LocalPort);
           // pscanCmd += string.Format("TRAC:UDP:FLAG:ON \"{0}\", {1}, \"SWAP\"\n", LocalIp, LocalPort);
            pscanCmd += "STAT:EXT:ENAB 63423\n";
            pscanCmd += "STAT:OPER:ENAB 280\n";
            pscanCmd += "SENS:FUNC:OFF \"FREQ:OFFS\"\n";
            pscanCmd += "SENS:FUNC:OFF \"FSTR\"\n";
            pscanCmd += "*STB\n";
            return pscanCmd;
        }
    }
}

