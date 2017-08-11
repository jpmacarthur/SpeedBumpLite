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
    /// Interaction logic for Warning.xaml
    /// </summary>
    public partial class Warning : UserControl
    {
        public Warning()
        {
            InitializeComponent();
            DataContext = this;
            var yourImage = new BitmapImage(new Uri("Images\\Neutral Circle.png", UriKind.Relative));
            status = yourImage;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            onButtonClicked(e);
        }
        public event EventHandler ButtonClicked;
        protected virtual void onButtonClicked(EventArgs e)
        {
            var handler = ButtonClicked;
            if(handler != null)
            {
                handler(this, e);
            }
        }
    }
}
