using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LCP.Common.Json;
using LCP.Common.Logging;
using log4net;
using System.Windows.Automation.Peers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SpeedBump.Deployment;
using SpeedBump.Versioning;
using System.Threading;
using System.Windows.Controls.Primitives;
using System.IO;

namespace SpeedBump
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VersionManager ver = new VersionManager();
        public bool reportsToggle = true;
        public delegate void NewReportEventHandler(object sender, NewReportEventArgs args);
        public event NewReportEventHandler StatusUpdated;
        ProjectControlSource source;
        int checkcount;
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public MainWindow()
        {
            Logger.Setup();
            InitializeComponent();
            DataContext = this;
            Reload();
        }

        public void Reload()
        {

            log.Debug("Reload called");
            checkcount = 0;
            projectRowsPanel.Children.Clear();
            source = PersistableJson.Load<ProjectControlSource>();
            log.Debug(source.Items + "items are in the project control source");
            foreach (ProjectControlSourceItem item in source.Items)
            {
                ProjectControl row = new ProjectControl();
                row.StatusUpdated += Row_StatusUpdated;
                row.Reload(item, source);
                row.trivialBump_RB.GroupName = "bumpGroup" + item.Project;
                row.minorBump_RB.GroupName = "bumpGroup" + item.Project;
                row.majorBump_RB.GroupName = "bumpGroup" + item.Project;
                projectRowsPanel.Children.Add(row);

            }
            foreach (FTPHost host in source.FTPHosts)
            {
                CheckBox ftpcheck = new CheckBox { Content = host.IPAddress, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };
                ftpcheck.DataContext = host;
                ftpcheck.SetBinding(ToggleButton.IsCheckedProperty, "Checked");
                ftpcheck.Checked += Ftpcheck_Checked;
                ftpcheck.Unchecked += Ftpcheck_Unchecked;
                if(host.Checked == true)
                {
                    checkcount++;
                }
                ServerChoices.Children.Add(ftpcheck);
            }
            if(checkcount < 1) { foreach(ProjectControl pc in projectRowsPanel.Children)
                {
                    pc.DeployButton.IsEnabled = false;
                } }
        }

        private void Ftpcheck_Unchecked(object sender, RoutedEventArgs e)
        {
            checkcount--;
            if(checkcount == 0)
            {
                foreach(ProjectControl pc in projectRowsPanel.Children)
                {
                    pc.DeployButton.IsEnabled = false;
                }
            }
        }

        private void Ftpcheck_Checked(object sender, RoutedEventArgs e)
        {
            checkcount++;
            if (checkcount > 0)
            {
                foreach (ProjectControl pc in projectRowsPanel.Children)
                {
                    pc.DeployButton.IsEnabled = true;
                }
            }
        }

        private void Row_StatusUpdated(object sender, NewReportEventArgs e)
        {
            Report = e.Report;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            source.Save();
        }

    }
 }

