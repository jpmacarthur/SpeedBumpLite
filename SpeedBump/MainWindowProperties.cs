using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBump
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private string report;
        public string Report
        {
            get { return this.report; }
            set
            {
                if (this.report != value)
                {
                    this.report = value;
                    this.NotifyPropertyChanged("Report");
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
