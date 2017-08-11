using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBump
{
    public class NewReportEventArgs : EventArgs
    {
        private readonly string report;
        private readonly Image img;
        private readonly string name;
        private readonly Task task;
        public NewReportEventArgs(string test)
        {
            this.report = test;
        }
        public NewReportEventArgs(Image img)
        {
            this.img = img;
        }
        public NewReportEventArgs(Task task)
        {
            this.task = task;
        }
        public NewReportEventArgs(string report, string name)
        {
            this.name = name;
            this.report = report;
        }
        public NewReportEventArgs(string report, string name, Task task)
        {
            this.name = name;
            this.report = report;
            this.task = task;
        }
        public string Report
        {
            get { return this.report; }
        }
        public Image Img
        {
            get { return this.img; }
        }
        public string Name
        {
            get { return this.name; }
        }
        public Task Task
        {
            get { return this.task; }
        }
    }
}
