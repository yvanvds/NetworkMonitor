using LiveCharts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace NetworkMonitor.Services
{
    public class PingService
    {
        private string target;
        private int interval;
        private int currentPeriod = 0;

        private string error = string.Empty;
        public string Error => error;

        private int avg15M;
        public int Avg15M => avg15M;

        private int avg5M;
        public int Avg5M => avg5M;

        private int avg10M;
        public int Avg10M => avg10M;

        private int err15M;
        public int Error15M => err15M;

        private int err5M;
        public int Error5M => err5M;

        private int err10M;
        public int Error10M => err10M;

        private int valuesIn15Minutes;
        private int valuesIn10Minutes;
        private int valuesIn5Minutes;

        private double valuesIn1Minute;
        private double recordedTime;
        public double MinutesRecorded => recordedTime;

        public ChartValues<double> ResponseTimes = new ChartValues<double>();

        Ping ping = null;

        public PingService(string target, int intervalMS)
        {
            this.target = target;
            this.interval = intervalMS;
            this.currentPeriod = interval;
            valuesIn15Minutes = 900000 / intervalMS; // keep for 15 minutes
            valuesIn10Minutes = 600000 / intervalMS;
            valuesIn5Minutes = 300000 / intervalMS;
            valuesIn1Minute = 60000 / intervalMS;

            ping = new Ping();

        }

        public async Task<bool> Update(int deltatimeMS)
        {
            currentPeriod -= deltatimeMS;
            if (currentPeriod > 0) return false;
            currentPeriod = interval + currentPeriod; 

            bool success = false;
            try
            {
                PingReply reply = await ping.SendPingAsync(target);
                success = reply.Status == IPStatus.Success;
                if(success)
                {
                    error = string.Empty;
                    ResponseTimes.Add(reply.RoundtripTime);
                } else
                {
                    error = GetErrorText(reply.Status);
                    ResponseTimes.Add(double.NaN);
                }

            } catch (PingException e)
            {
                error = e.Message;
            }

            while (ResponseTimes.Count > valuesIn15Minutes)
            {
                ResponseTimes.RemoveAt(0);
            }

            CalculateValues();

            return true;
        }

        private void CalculateValues()
        {
            int index5Min = ResponseTimes.Count - 1 - valuesIn5Minutes;
            int index10Min = ResponseTimes.Count - 1 - valuesIn10Minutes;

            int Count10Min = 0; double Value10Min = 0; err10M = 0;
            int Count5Min = 0; double Value5Min = 0; err5M = 0;
            int Count15Min = 0; double Value15Min = 0; err15M = 0;

            for(int i = 0; i < ResponseTimes.Count; i++)
            {
                if(i >= index5Min)
                {
                    if(ResponseTimes[i] == double.NaN)
                    {
                        err5M++;
                    } else
                    {
                        Count5Min++;
                        Value5Min += ResponseTimes[i];
                    }
                    
                }
                if(i >= index10Min)
                {
                    if(ResponseTimes[i] == double.NaN)
                    {
                        err10M++;
                    } else
                    {
                        Count10Min++;
                        Value10Min += ResponseTimes[i];
                    }
                }

                if (ResponseTimes[i] == double.NaN)
                {
                    err15M++;
                }
                else
                {
                    Count15Min++;
                    Value15Min += ResponseTimes[i];
                }
            }

            avg10M =  Count10Min != 0 ? (int)Value10Min / Count10Min : 0;
            avg5M = Count5Min != 0 ? (int)Value5Min / Count5Min : 0;
            avg15M = Count15Min != 0 ? (int)Value15Min / Count15Min : 0;

            recordedTime = ResponseTimes.Count / valuesIn1Minute;
        }

        private static string GetErrorText(IPStatus status)
        {
            switch(status)
            {
                case IPStatus.BadDestination: return "Bad destination";
                case IPStatus.BadHeader: return "Bad header";
                case IPStatus.BadOption: return "Bad option";
                case IPStatus.BadRoute: return "Bad route";
                case IPStatus.DestinationHostUnreachable: return "Destination Host Unreachable";
                case IPStatus.DestinationNetworkUnreachable: return "Destination Network Unreachable";
                case IPStatus.DestinationPortUnreachable: return "Destination Port Unreachable";
                case IPStatus.DestinationProtocolUnreachable: return "Destination Protocol Unreachable";
                case IPStatus.DestinationUnreachable: return "Destination Unreachable";
                case IPStatus.DestinationScopeMismatch: return "Destination Scope Mismatch";
                case IPStatus.HardwareError: return "Hardware Error";
                case IPStatus.IcmpError: return "ICMP Protocol Error";
                case IPStatus.NoResources: return "Insufficient Network Resources";
                case IPStatus.PacketTooBig: return "Packet Too Big";
                case IPStatus.ParameterProblem: return "Parameter Problem";
                case IPStatus.SourceQuench: return "Source Quench";
                case IPStatus.TimedOut: return "Timeout";
                case IPStatus.TimeExceeded: return "Time Exceeded";
                case IPStatus.TtlExpired: return "Time to live expired";
                case IPStatus.TtlReassemblyTimeExceeded: return "Time to live Reassembly Time Exceeded";
                case IPStatus.Unknown: return "Unknown Error";
                case IPStatus.UnrecognizedNextHeader: return "Unrecognised Next Header";
            }
            return "Unknown Error";
        }
    }
}
