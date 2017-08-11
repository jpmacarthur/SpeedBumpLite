using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SpeedBump
{
    public partial class MainWindow
    {
        public void updateStatus(StatusCheck check)
        {
            if (check.error == true && check.success == true && check.warning == true)
            {
                status_BT.Status = new BitmapImage(new Uri("Images\\All Circle.png", UriKind.Relative));
                status_BT.Status.Freeze();
            }
            if (check.error == true && check.success == true && check.warning == false)
            {
                status_BT.Status = new BitmapImage(new Uri("Images\\Success Error.png", UriKind.Relative));
                status_BT.Status.Freeze();
            }
            if (check.error == false && check.success == true && check.warning == true)
            {
                status_BT.Status = new BitmapImage(new Uri("Images\\Success Warning.png", UriKind.Relative));
                status_BT.Status.Freeze();
            }
            if (check.error == true && check.success == false && check.warning == true)
            {
                status_BT.Status = new BitmapImage(new Uri("Images\\Warning Error.png", UriKind.Relative));
                status_BT.Status.Freeze();
            }
            if (check.error == true && check.success == false && check.warning == false)
            {
                status_BT.Status = new BitmapImage(new Uri("Images\\Error Circle.png", UriKind.Relative));
                status_BT.Status.Freeze();
            }
            if (check.error == false && check.success == true && check.warning == false)
            {
                status_BT.Status = new BitmapImage(new Uri("Images\\Good Circle.png", UriKind.Relative));
                status_BT.Status.Freeze();
            }
            if (check.error == false && check.success == false && check.warning == true)
            {
                status_BT.Status = new BitmapImage(new Uri("Images\\Warning Circle.png", UriKind.Relative));
                status_BT.Status.Freeze();
            }
        }
    }
}
