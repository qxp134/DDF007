using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication3
{
    class UdpStreamHeader
    {
        public uint HeaderMagicNumber;
        public ushort HeaderMinorVersion;
        public ushort HeaderMajorVersion;
        public ushort HeaderSequenceNumber;
        public ushort HeaderReserved;
        public uint DataSize;
        public ushort AttributeTag;
        public ushort AttributeLength;
        public ushort TraceNumberItems;
        public char TraceReserved;
        public char TraceOptionHeaderLength;
        public uint TraceSelectorFlags;
    }
    public class UdpStreamType
    {
        public UdpStreamAudio audioStream = new UdpStreamAudio();
        public UdpStreamIfpan ifpanStream = new UdpStreamIfpan();
        public UdpStreamIf ifStream = new UdpStreamIf();
        public UdpStreamPscan pscanStream = new UdpStreamPscan();
    }
    public class UdpStreamAudio
    {
        public short Mode;
        public short FrameLen;
        public uint FrequencyLow;
        public uint Bandwidth;
        public ushort DemodulationId;
        public string DemodulationMode;
        public uint FrequencyHigh;
        public string Reserved;             //6 bytes
        public ulong Timestamp;
        public int WaveLength;
        public byte[] WaveStream;
        //public byte[] WaveStream = new byte[10240];         //Trace data: n audio frames (byte, short or long, depending on audio frame length)
    }
    public class UdpStreamIfpan
    {
        public uint FrequencyLow;
        public uint Span;
        public short Reserved;
        public short AverageType;
        public uint MesureTime;
        public uint FrequencyHigh;
        public uint SelectedChannel;
        public uint DemodulationFrequencyLow;
        public uint DemodulationFrequencyHigh;
        public ulong Timestamp;
        public List<short> pwls = new List<short>();
    }
    class UdpStreamMscan
    {
        public short CycleCount;
        public short HoldTime;
        public short DwellTime;
        public short StopSignal;
        public short FftLevel;
        public ulong FreqLow;
        public short Direction;
        public short Chan;
    }
    public class UdpStreamIf
    {
        public short Mode;
        public short FrameLength;
        public int SampleRate;
        public uint FrequencyLow;
        public uint Bandwidth;
        public ushort DemodulationId;
        public short RxAttenuation;
        public short Flags;
        public string Reserved;
        public string DemodulationMode;
        public long SampleCount;
        public uint FrequencyHigh;
        public string Reserved2;
        public ulong Timestamp;
        public List<IQData> IQSampleData = new List<IQData>();
    }

    public class IQData
    {
        public short IData;
        public short QData;
    }

    public class UdpStreamPscan
    {
        public uint StartFreqLow;
        public uint StopFreqLow;
        public uint StepFreq;
        public uint StartFreqHigh;
        public uint StopFreqHigh;
        public string Reserved;
        public List<short> FftLevel = new List<short>();
        public static List<float> TrueFre = new List<float>();
        public List<uint> FreqLow = new List<uint>();
        public List<uint> FreqHigh = new List<uint>();
        public ulong TimeSwap;
        public static List<short> Level = new List<short>();
        public static List<float> TrueLevel = new List<float>();
        public static uint NowFre = 1;
        public static bool IsFrist = true;
    }
    class UdpStreamFscan
    {
        public short CycleCount;
        public short HoldTime;
        public short DwellTime;
        public short Direction;
        public short StopSignal;
        public uint StartFreqLow;
        public uint StopFreqLow;
        public uint StepFreq;
        public uint StartFreqHigh;
        public uint StopFreqHigh;
        public short Reserved;
        public ulong TimeSwap;
        public short FftLevel;
        public uint FreqLow;
        public List<short> TrueLevel = new List<short>();
        public List<float> TrueFre = new List<float>();
    }
}

