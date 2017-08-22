using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LCP.Common.Json;

namespace SpeedBump
{
    public class ProjectControlSourceItem : PersistableJson
    {
        public string Project { get; set; }
        public DateTime Timestamp { get; set; }
        public string FTPPath { get; set; }
        public string StageDir { get; set; }
        public string BaseDir { get; set; }
        public string RemoteStagingDir { get; set; }
    }
    public class ProjectControlSource : PersistableJson
    {
        public string BaseDir { get; set; }
        public List<ProjectControlSourceItem> Items {get; set;}
        public List<FTPHost> FTPHosts { get; set; }
        public string CompileDir { get; set; }
    }

    public static class ProjectControlUtil
    {
        
        public static void Init()
        {
            ProjectControlSourceItem itemOne = new ProjectControlSourceItem() {Project = "Algo Service", StageDir = "Algo Windows Service", Timestamp =  DateTime.UtcNow, BaseDir = "Algo", FTPPath = "Algo Service"};
            ProjectControlSourceItem itemTwo = new ProjectControlSourceItem() { Project = "Matlab Server", StageDir = "Matlab Server Windows Service", Timestamp = DateTime.UtcNow, BaseDir = "Matlab Server", FTPPath = "Matlab Server Service" };
            ProjectControlSourceItem itemThree = new ProjectControlSourceItem() { Project = "Optimizer", StageDir = "Optimizer Windows Service", Timestamp = DateTime.UtcNow, BaseDir = "Optimizer", FTPPath = "Optimizer" };
            ProjectControlSourceItem itemFour = new ProjectControlSourceItem() { Project = "Task Scheduler", StageDir = "Task Scheduler Windows Service", Timestamp = DateTime.UtcNow, BaseDir = "Task Scheduler", FTPPath = "Task Scheduler" };
            ProjectControlSourceItem itemFive = new ProjectControlSourceItem() { Project = "Trading Monitor", StageDir = "Task Scheduler Windows Service", Timestamp = DateTime.UtcNow, BaseDir = "Trading Monitor", FTPPath = "Trading Montior" };
            ProjectControlSourceItem itemSix = new ProjectControlSourceItem() {Project = "TT Gateway", StageDir = "TT Gateway Windows Service", Timestamp =  DateTime.UtcNow, BaseDir = "TT Gateway", FTPPath = "TT Gateway"};

            ProjectControlSource source = new ProjectControlSource();
            source.Items = new List<ProjectControlSourceItem>();
            source.Items.Add(itemOne);
            source.Items.Add(itemTwo);
            source.Items.Add(itemThree);
            source.Items.Add(itemFour);
            source.Items.Add(itemFive);
            source.Items.Add(itemSix);
            source.BaseDir = @"c:\dev\projects\";
            source.FTPHosts = new List<FTPHost>();
          //  source.FTPHosts.Add(new FTPHost {IPAddress= });
           // source.FTPHosts.Add("10.10.100.35");
            source.Save();
            



        }
    }

}
