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
using System.Threading;

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
        public event EventHandler StartTask;
        public event EventHandler EndTask;
        DeploymentManager bumper;
        int taskcount = 0;
        CancellationTokenSource cancelsource = new CancellationTokenSource();
        CancellationToken canceltoken;
        public Task runAll;
        public ProjectControl()
        {
            InitializeComponent();
            DataContext = this;
            WarningStatus.Status = new BitmapImage(new Uri("Images\\gray-circle.png",UriKind.Relative));
        }

        public void DisableUI()
        {
            RunOptions.Visibility = Visibility.Collapsed;
            WorkingBar.Visibility = Visibility.Visible;

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
            RunOptions.Visibility = Visibility.Visible;
            WorkingBar.Visibility = Visibility.Collapsed;
        }

        public void Reload(ProjectControlSourceItem item, ProjectControlSource source)
        {
            this.source = source;
            this.item = item;
            projectLabel.Content = item.Project;
            Timestamp = item.Timestamp;
            bumper = new DeploymentManager(source, item);
            UpdateVersion();
        }

        public void UpdateVersion()
        {
            MyFile assembly = ver.OpenAssemblyInfo(source.BaseDir + item.BaseDir +@"\" + item.StageDir);
            Versioning.Version itemVersion = ver.getchildVersion(assembly);
            Version = itemVersion.getVersion();
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

        private void CleanButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.StatusUpdated != null)
            {
                this.StatusUpdated(this, new NewReportEventArgs(""));
            }
            bool success = true;
            DeploymentManager bumper = new DeploymentManager(source,item);
            DisableUI();
            Task clean = Task.Factory.StartNew(() => {
                try
                {
                    bumper.Prepare();
                    bumper.Clean();
                }
                catch (Exception ex)
                {
                    success = false;
                    if (this.StatusUpdated != null)
                    {
                        this.StatusUpdated(this, new NewReportEventArgs(ex.ToString()));
                    }
                }
            });
            Task clean_ui = clean.ContinueWith((antecedent) =>
            {
                EnableUI();
                setStatus(success);

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.StatusUpdated != null)
            {
                this.StatusUpdated(this, new NewReportEventArgs(""));
            }
            bool success = true;
            if (taskcount > 0)
            {
                cancelsource.Cancel();
                taskcount = 0;
            }
            cancelsource = new CancellationTokenSource();
            canceltoken = cancelsource.Token;
            DeploymentManager bumper = new DeploymentManager(source, item);
            DisableUI();
            string pattern = "[1-9]+?[0-9]?[ ][W][a][r]";
            Regex warningCheck = new Regex(pattern);
            Task build = Task.Factory.StartNew(() =>
            {
                try
                {
                    taskcount++;
                    bumper.Prepare();
                    bumper.Clean();
                    string temp = bumper.Build();
                    if (temp.Contains("Build FAILED") || temp.Contains("MSBUILD : error"))
                    {
                        success = false;
                        if (this.StatusUpdated != null)
                        {
                            this.StatusUpdated(this, new NewReportEventArgs(temp));
                        }
                    }
                    else if (warningCheck.IsMatch(temp))
                    {
                        success = false;
                        if (this.StatusUpdated != null)
                        {
                            this.StatusUpdated(this, new NewReportEventArgs(temp));
                        }
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    if (this.StatusUpdated != null)
                    {
                        this.StatusUpdated(this, new NewReportEventArgs(ex.ToString()));
                    }
                }
            });
            Task clean_ui = build.ContinueWith((antecedent) =>
            {
                EnableUI();
                setStatus(success);

            }, TaskScheduler.FromCurrentSynchronizationContext());
         
                Task<int> waiting = clean_ui.ContinueWith((antecedent) =>
                {
                    if (success)
                    {
                        Thread.Sleep(5000);
                    }
                        return 1;
                }, TaskScheduler.Default);
                Task waitingcontinue = waiting.ContinueWith((antecedent) =>
                {
                    if (success)
                    {
                        if (canceltoken.IsCancellationRequested) { return; }
                        WarningStatus.Status = new BitmapImage(new Uri("Images\\gray-circle.png", UriKind.Relative));
                        WarningStatus.Status.Freeze();
                    }
                    taskcount--;
                }, canceltoken, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void BumpButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.StatusUpdated != null)
            {
                this.StatusUpdated(this, new NewReportEventArgs(""));
            }
            bool success = true;
            string bumpChoice = "unknown";
            if (majorBump_RB.IsChecked == true) { bumpChoice = "Major"; }
            else if (minorBump_RB.IsChecked == true) { bumpChoice = "Minor"; }
            else if (trivialBump_RB.IsChecked == true) { bumpChoice = "Trivial"; }
            try
            {
                Task<Versioning.Version> bump = Task.Factory.StartNew(() =>
                {
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
                if (success)
                {
                    Task<int> waiting = bump_ui.ContinueWith((antecedent) =>
                    {
                        Thread.Sleep(3000);
                        return 1;
                    }, TaskScheduler.Default);
                    Task waitingcontinue = waiting.ContinueWith((antecedent) =>
                    {
                        if (canceltoken.IsCancellationRequested) { return; }
                        WarningStatus.Status = new BitmapImage(new Uri("Images\\gray-circle.png", UriKind.Relative));
                        WarningStatus.Status.Freeze();
                        taskcount--;
                    }, canceltoken, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
            catch (AggregateException ex)
            {
                success = false;
                if (this.StatusUpdated != null)
                {
                    this.StatusUpdated(this, new NewReportEventArgs(ex.Flatten().ToString()));
                }
            }
        }

        private void DeployButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.StatusUpdated != null)
            {
                this.StatusUpdated(this, new NewReportEventArgs(""));
            }
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
            bool success = true;
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
                    success = false;
                    if (this.StatusUpdated != null)
                    {
                        this.StatusUpdated(this, new NewReportEventArgs(ex.ToString()));
                    }
                }
            });
            Task deploy_ui = deploy.ContinueWith((antecedent) =>
            {
                setStatus(success);
                EnableUI();
            }, TaskScheduler.FromCurrentSynchronizationContext());
            Task<int> waiting = deploy_ui.ContinueWith((antecedent) => {
                Thread.Sleep(5000);
                return 1;
            }, TaskScheduler.Default);
            Task waitingcontinue = waiting.ContinueWith((antecedent) =>
            {
                if (canceltoken.IsCancellationRequested) { return; }
                WarningStatus.Status = new BitmapImage(new Uri("Images\\gray-circle.png", UriKind.Relative));
                WarningStatus.Status.Freeze();
                taskcount--;
            }, canceltoken, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
