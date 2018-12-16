using LiveCharts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NetworkMonitor.Monitors
{
    public class Site
    {
        private string name;
        public string Name { get => name; }

        private string address;
        public string Address { get => address; }

        public string LastError { get => pingService.Error; }

        public int Avg15M => pingService.Avg15M;
        public int Avg5M => pingService.Avg5M;
        public int Avg10M => pingService.Avg10M;

        public int Error15M => pingService.Error15M;
        public int Error5M => pingService.Error5M;
        public int Error10M => pingService.Error10M;

        public double MinutesRecorded => pingService.MinutesRecorded;

        private Services.PingService pingService = null;

        public ChartValues<double> PingTimes => pingService.ResponseTimes;

        public Site(string Name, string Address)
        {
            this.name = Name;
            this.address = Address;
            pingService = new Services.PingService(address, 60000);
        }

        public async Task<bool> Update(int deltaTimeMS)
        {
            return await pingService.Update(deltaTimeMS);
        }

    }
}
