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

namespace SpeedBump
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            projectRowsPanel.Children.Clear();
            ProjectControlSource source = PersistableJson.Load<ProjectControlSource>();
            log.Debug(source.Items + "items are in the project control source");
            foreach(ProjectControlSourceItem item in source.Items)
            {

                ProjectControl row = new ProjectControl();
                row.Reload(item, source);
                row.trivialBump_RB.GroupName = "bumpGroup" + item.Project;
                row.minorBump_RB.GroupName = "bumpGroup" + item.Project;
                row.majorBump_RB.GroupName = "bumpGroup" + item.Project;

                projectRowsPanel.Children.Add(row);
                               
            }
            foreach(string host in source.FTPHosts)
            {
                ftp_Combobox.Items.Add(host);
            }
        }

        private void runAllProjects_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("[USER ACTION] Run all projects called");
            foreach(ProjectControl child in projectRowsPanel.Children)
            {
                if(child.runAll_CB.IsChecked == true)
                {
                    child.run_BT.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    
                    //TODO Add logic for running all 
                }
            }
        }
    }
}
