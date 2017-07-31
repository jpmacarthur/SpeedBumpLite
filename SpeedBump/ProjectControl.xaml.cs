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
using System.ComponentModel;
using SpeedBump.Versioning;
using log4net;
using SpeedBump.Deployment;

namespace SpeedBump
{
    /// <summary>
    /// Interaction logic for ProjectControl.xaml
    /// </summary>
    /// 

    public partial class ProjectControl : UserControl
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ProjectControlSource source;
        private ProjectControlSourceItem item;
        private VersionManager ver = new VersionManager();
        public ProjectControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void Reload(ProjectControlSourceItem item, ProjectControlSource source)
        {
            this.source = source;
            this.item = item;
            projectLabel.Content = item.Project;
            Timestamp = item.Timestamp;
            UpdateVersion();

            
        }

        public void UpdateVersion()
        {
            MyFile assembly = ver.OpenAssemblyInfo(source.BaseDir + item.BaseDir +@"\" + item.StageDir);
            Versioning.Version itemVersion = ver.getchildVersion(assembly);
            Version = itemVersion.getVersion();

        }

        private void run_BT_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("[User Action] " + sender.ToString());
            DeploymentManager bumper = new DeploymentManager(source);

            string actionChoice = (actionDropdown.SelectedItem as ComboBoxItem).Content.ToString();
            switch (actionChoice)
            {
                case "Prepare":
                    break;
                case "Clean":
                    break;
                case "Bump":
                    string bumpChoice = "unknown";
                    if(majorBump_RB.IsChecked == true) { bumpChoice = "Major"; }
                    if (minorBump_RB.IsChecked == true) { bumpChoice = "Minor"; }
                    if (trivialBump_RB.IsChecked == true) { bumpChoice = "Trivial"; }
                    Versioning.Version temp = bumper.Bump(item, source, bumpChoice);
                    versionLabel.Content = temp.getVersion();
                    Timestamp = DateTime.UtcNow;
                    versionLabel.ToolTip = Timestamp;
                    source.Save();
                    //TODO Timestamp binding
                    break;
                case "Build":
                    bumper.Build(item);
                    break;
                case "Deploy":
                    break;
                default:
                    break;
            }

        }
    }
}
