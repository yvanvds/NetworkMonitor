using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetworkMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Timer timer = new Timer();
        DateTime lastTime;

        public MainWindow()
        {
            InitializeComponent();
            Google.Init("Google", "www.google.be");
            Smartschool.Init("Smartschool", "sanctamaria-aarschot.smartschool.be");
            Site.Init("Website", "www.sanctamaria-aarschot.be");

            lastTime = DateTime.Now;
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;

            var router = new Monitors.Router("127.0.0.1", "public");
            router.Update();
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DateTime current = DateTime.Now;
            var elapsed = (current - lastTime);
            lastTime = current;

            await Google.Update((int)elapsed.TotalMilliseconds);
            await Smartschool.Update((int)elapsed.TotalMilliseconds);
            await Site.Update((int)elapsed.TotalMilliseconds);
        }
    }
}
