using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpeedBump
{
    /// <summary>
    /// Interaction logic for MyScrollViewer.xaml
    /// </summary>
    public partial class MyScrollViewer : UserControl
    {
        public event EventHandler ReportViewerToggle;
        public MyScrollViewer()
        {
            InitializeComponent();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            if (this.ReportViewerToggle != null)
            {
                this.ReportViewerToggle(this, new EventArgs());
            }
        }
    }
}
