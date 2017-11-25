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
using System.IO;
using Newtonsoft.Json;

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
            string path = source.BaseDir + item.BaseDir + @"\version.json";
            Deployment.JSONVersion jSONVersion = JsonConvert.DeserializeObject<Deployment.JSONVersion>(File.ReadAllText(path));
            Version = jSONVersion.Version;
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
                Task<Deployment.JSONVersion> bump = Task.Factory.StartNew(() =>
                {
                    return bumper.Bump(bumpChoice);
                });
                Task bump_ui = bump.ContinueWith((antecedent) =>
                {
                    Version = antecedent.Result.Version;
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
