using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetworkMonitor.Controls
{
    /// <summary>
    /// Interaction logic for SiteControl.xaml
    /// </summary>
    public partial class SiteControl : UserControl
    {
        private Monitors.Site site;

        public SeriesCollection pingTimes { get; set; }
        public Func<double, string> XFormatter { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public int LatencyTrigger = 100;

        public SiteControl()
        {
            InitializeComponent();
        }

        public void Init(string name, string address)
        {
            site = new Monitors.Site(name, address);
            SiteName.Content = name;

            pingTimes = new SeriesCollection
            {
                new LineSeries
                {
                    Values = site.PingTimes,
                    PointGeometry = null
                }
            };

            XFormatter = value => (site.MinutesRecorded - value).ToString();
            YFormatter = value => value.ToString();
            DataContext = this;
        }

        public async Task Update(int deltaTimeMS)
        {
            bool updated = await site.Update(deltaTimeMS);

            if(updated)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    UpdateInterface();
                });
            }
        }

        public void UpdateInterface()
        {
            SiteError.Content = site.LastError;
            Average5M.Content = "A 5: " + site.Avg5M;
            Average10M.Content = "A10: " + site.Avg10M;
            Average15M.Content = "A15: " + site.Avg15M;

            if(site.Error5M > 0)
            {
                Timeout5M.Content = "E 5: " + site.Error5M;
            } else
            {
                Timeout5M.Content = string.Empty;
            }

            if (site.Error10M > 0)
            {
                Timeout10M.Content = "E10: " + site.Error10M;
            }
            else
            {
                Timeout10M.Content = string.Empty;
            }

            if (site.Error15M > 0)
            {
                Timeout15M.Content = "E15: " + site.Error15M;
            }
            else
            {
                Timeout15M.Content = string.Empty;
            }

            Color background = Colors.White;

            if(site.Error5M > 3 || site.Error10M > 7 || site.Error15M > 10)
            {
                background = Color.FromRgb(255, 200, 200);
            } else if(site.Error5M > 1)
            {
                background = Color.FromRgb(255, 200, 200);
            } else if(site.Avg5M > LatencyTrigger)
            {
                background = Color.FromRgb(255, 233, 170);
            }

            this.Background = new SolidColorBrush(background);
        }


    }
}
