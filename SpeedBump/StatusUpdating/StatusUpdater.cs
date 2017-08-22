using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SpeedBump
{
    public partial class ProjectControl
    {
        public void updateStatus(StatusCheck check)
        {
            if (check.error == true && check.success == true && check.warning == true)
            {
                WarningStatus.Status = new BitmapImage(new Uri("Images\\All Circle.png", UriKind.Relative));
                WarningStatus.Status.Freeze();
            }
            if (check.error == true && check.success == true && check.warning == false)
            {
                WarningStatus.Status = new BitmapImage(new Uri("Images\\Success Error.png", UriKind.Relative));
                WarningStatus.Status.Freeze();
            }
            if (check.error == false && check.success == true && check.warning == true)
            {
                WarningStatus.Status = new BitmapImage(new Uri("Images\\Success Warning.png", UriKind.Relative));
                WarningStatus.Status.Freeze();
            }
            if (check.error == true && check.success == false && check.warning == true)
            {
                WarningStatus.Status = new BitmapImage(new Uri("Images\\Warning Error.png", UriKind.Relative));
                WarningStatus.Status.Freeze();
            }
            if (check.error == true && check.success == false && check.warning == false)
            {
                WarningStatus.Status = new BitmapImage(new Uri("Images\\Error Circle.png", UriKind.Relative));
                WarningStatus.Status.Freeze();
            }
            if (check.error == false && check.success == true && check.warning == false)
            {
                WarningStatus.Status = new BitmapImage(new Uri("Images\\Good Circle.png", UriKind.Relative));
                WarningStatus.Status.Freeze();
            }
            if (check.error == false && check.success == false && check.warning == true)
            {
                WarningStatus.Status = new BitmapImage(new Uri("Images\\Warning Circle.png", UriKind.Relative));
                WarningStatus.Status.Freeze();
            }
        }
        public void updateStatus(string report)
        {
            string pattern = "[1-9]+?[0-9]?[ ][W][a][r]";
            Regex warningCheck = new Regex(pattern);
            if (report.Contains("Build FAILED") || report.Contains("MSBUILD : error"))
            {
                WarningStatus.Status = new BitmapImage(new Uri("Images\\red-circle.png", UriKind.Relative));
                WarningStatus.Status.Freeze();
            }
            else if (warningCheck.IsMatch(report))
            {
                WarningStatus.Status = new BitmapImage(new Uri("Images\\red-circle.png", UriKind.Relative));
                WarningStatus.Status.Freeze();
            }
            else if (report.Contains("Build succeeded"))
            {
                WarningStatus.Status = new BitmapImage(new Uri("Images\\green-circle.png", UriKind.Relative));
                WarningStatus.Status.Freeze();
            }
        }
    }
}
