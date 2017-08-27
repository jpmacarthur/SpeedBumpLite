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
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using LCP.Common.UI;

namespace SpeedBump
{
    /// <summary>
    /// Interaction logic for ProjectControl.xaml
    /// </summary>
    /// 

    public partial class ProjectControl : UserControl
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ProjectControlSource source;
        public ProjectControlSourceItem item;
        private VersionManager ver = new VersionManager();
        public delegate void NewReportEventHandler(object sender, NewReportEventArgs args);
        public event NewReportEventHandler StatusUpdated;
        bool reportsToggle = true;
        public event EventHandler StartTask;
        public event EventHandler EndTask;
        public event NewReportEventHandler SendReportButtonClick;

        public Task runAll;
        public ProjectControl()
        {
            InitializeComponent();
            DataContext = this;
            WarningStatus.Status = new BitmapImage(new Uri("Images\\gray-circle.png",UriKind.Relative));
            this.WarningStatus.reports_BT.Click += Reports_BT_Click;
        }

        public void DisableUI()
        {
            runAll_BT.IsEnabled = false;
            run_BT.IsEnabled = false;
            RebuiltButton.IsEnabled = false;
            WarningStatus.Status = new BitmapImage(new Uri("Images\\yellow-circle.png", UriKind.Relative));
            if (this.StartTask != null)
            {
                this.StartTask(this, new EventArgs());
            }
        }
        public void EnableUI()
        {
            if (this.EndTask != null)
            {
                this.EndTask(this, new EventArgs());
            }
            runAll_BT.IsEnabled = true;
            run_BT.IsEnabled = true;
            RebuiltButton.IsEnabled = true;
        }
        private void Reports_BT_Click(object sender, RoutedEventArgs e)
        {

            if (this.SendReportButtonClick != null)
            {
                this.SendReportButtonClick(this, new NewReportEventArgs(Report));
            }


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
            MainWindow parentwin = Application.Current.MainWindow as MainWindow;
            log.Debug("[User Action] " + sender.ToString());
            DisableUI();
            List<string> checkedboxes = new List<string>();
            foreach (CheckBox cb in parentwin.ServerChoices.Children)
            {
                if (cb.IsChecked == true)
                {
                    checkedboxes.Add(cb.Content.ToString());
                }
            }
            DeploymentManager bumper = new DeploymentManager(source, item);
            string actionChoice = "";
            if ((actionDropdown.SelectedItem as ComboBoxItem).Content != null)
            {
                 actionChoice = (actionDropdown.SelectedItem as ComboBoxItem).Content.ToString();
            }

            switch (actionChoice)
            {

                case "Prepare":
                    DisableUI();
                    bool success = true;

                    Task prepare = Task.Factory.StartNew(() => {
                        try
                        {
                            bumper.Prepare();
                        }
                        catch (Exception) { success = false; }
                    });
                    Task prepare_ui = prepare.ContinueWith((antecedent) =>
                    {
                        setStatus(success);
                        EnableUI();
                    },TaskScheduler.FromCurrentSynchronizationContext());
                    break;

                case "Clean":
                    success = true;
                    Task clean = Task.Factory.StartNew(() => {
                        try
                        {
                            bumper.Clean();
                        }
                        catch (Exception) { success = false; }
                    });
                    Task clean_ui = clean.ContinueWith((antecedent) =>
                    {
                        EnableUI();
                        setStatus(success);
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                    break;

                case "Bump":
                    DisableUI();
                    success = true;
                    string bumpChoice = "unknown";
                    if(majorBump_RB.IsChecked == true) { bumpChoice = "Major"; }
                    else if (minorBump_RB.IsChecked == true) { bumpChoice = "Minor"; }
                    else if (trivialBump_RB.IsChecked == true) { bumpChoice = "Trivial"; }
                    Task<Versioning.Version> bump = Task.Factory.StartNew(() => {
                        return bumper.Bump(bumpChoice);
                    });
                    Task bump_ui = bump.ContinueWith((antecedent) =>
                    {
                        Version = antecedent.Result.getVersion();
                        Timestamp = DateTime.UtcNow;
                        item.Timestamp = Timestamp;
                        source.Save();
                        EnableUI();
                        setStatus(true);
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                    break;

                case "Build":
                    DisableUI();
                    Task build = Task.Factory.StartNew(() => {
                            Report = bumper.Build();

                    });
                    Task build_ui = build.ContinueWith((antecedent) =>
                    {
                        if (this.StatusUpdated != null)
                        {
                            this.StatusUpdated(this, new NewReportEventArgs(Report, projectLabel.Content.ToString()));
                        }
                        updateStatus(Report);
                        EnableUI();
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                    break;

                case "Deploy":
                    DisableUI();
                    success = true;
                    Task deploy = Task.Factory.StartNew(() => {
                    try
                    {
                        foreach (string address in checkedboxes)
                        {
                            bumper.Deploy(address);
                        }

                    }
                    catch (Exception ex)
                        {
                        MessageBox.Show(ex.ToString());
                            success = false;
                        }
                    });
                    Task deploy_ui = deploy.ContinueWith((antecedent) =>
                    {
                        setStatus(success);
                        EnableUI();
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                    break;
                default:
                    EnableUI();
                    break;
            }

        }

        private void runAll_BT_Click(object sender, RoutedEventArgs e)
        {
            MainWindow parentwin = Application.Current.MainWindow as MainWindow;
            DisableUI();
            string bumpChoice = "";
            bool success = true;
            string pjContent = projectLabel.Content.ToString();
            List<string> checkedboxes = new List<string>();
            foreach (CheckBox cb in parentwin.ServerChoices.Children)
            {
                if (cb.IsChecked == true)
                {
                    checkedboxes.Add(cb.Content.ToString());
                }
            }
            if (majorBump_RB.IsChecked == true) { bumpChoice = "Major"; }
            else if (minorBump_RB.IsChecked == true) { bumpChoice = "Minor"; }
            else if (trivialBump_RB.IsChecked == true) { bumpChoice = "Trivial"; }
            Task runAll = Task.Factory.StartNew(() =>
            {
                log.Debug("[User Action] " + sender.ToString());
                DeploymentManager bumper = new DeploymentManager(source, item);
                bumper.Prepare();
                bumper.Clean();
                Versioning.Version temp = bumper.Bump(bumpChoice);
                Version = temp.getVersion();
                Timestamp = DateTime.UtcNow;
                item.Timestamp = Timestamp;
                try
                {
                    Report = bumper.Build();
                    if (Report.Contains("Build FAILED") || Report.Contains("MSBUILD : error"))
                    {
                        success = false;
                    }
                    /*  if (this.StatusUpdated != null)
                      {
                          this.StatusUpdated(this, new NewReportEventArgs(Report, pjContent));
                      }*/
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    success = false;
                }
                if (success == true)
                {
                    try
                    {
                        foreach (string address in checkedboxes)
                        {
                            bumper.Deploy(address);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                        success = false;
                    }
                }
            });
            Task runAll_cont = runAll.ContinueWith((antecedent) =>
            {
                EnableUI();
                setStatus(success);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void RebuiltButton_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("[User Action] " + sender.ToString());
            DeploymentManager bumper = new DeploymentManager(source, item);

            DisableUI();
            WarningStatus.Status = new BitmapImage(new Uri("Images\\yellow-circle.png",UriKind.Relative));
            Task rebuild = Task.Factory.StartNew(() => {
                try { bumper.Clean(); }
                catch(Exception ex) { MessageBox.Show(ex.ToString()); }
                try { Report = bumper.Build(); }
                catch(Exception ex) { MessageBox.Show(ex.ToString()); }
            });
            Task rebuild_ui = rebuild.ContinueWith((antecedent) =>
            {
                EnableUI();
                string pattern = "[1-9]+?[0-9]?[ ][W][a][r]";
                Regex warningCheck = new Regex(pattern);
                if(Report == null)
                {
                    WarningStatus.Status = new BitmapImage(new Uri("Images\\red-circle.png", UriKind.Relative));
                    WarningStatus.Status.Freeze();
                }
                else if (Report.Contains("Build FAILED") || Report.Contains("MSBUILD : error"))
                {
                    WarningStatus.Status = new BitmapImage(new Uri("Images\\red-circle.png", UriKind.Relative));
                    WarningStatus.Status.Freeze();
                }
                else if (warningCheck.IsMatch(Report))
                {
                    WarningStatus.Status = new BitmapImage(new Uri("Images\\red-circle.png", UriKind.Relative));
                    WarningStatus.Status.Freeze();
                }
                else if (Report.Contains("Build succeeded"))
                {
                    WarningStatus.Status = new BitmapImage(new Uri("Images\\green-circle.png", UriKind.Relative));
                    WarningStatus.Status.Freeze();
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        public void setStatus(bool passed)
        {
            if(passed == true)
            {
                WarningStatus.Status = new BitmapImage(new Uri("Images\\green-circle.png", UriKind.Relative));
                WarningStatus.Status.Freeze();
            }
            else
            {
                WarningStatus.Status = new BitmapImage(new Uri("Images\\red-circle.png", UriKind.Relative));
                WarningStatus.Status.Freeze();
            }
        }
    }
}
