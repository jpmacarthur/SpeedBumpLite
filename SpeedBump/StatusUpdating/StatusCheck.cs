using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpeedBump
{
    public class StatusCheck
    {
        public bool error;
        public bool warning;
        public bool success;
        private string pattern = "[1-9]+?[0-9]?[ ][W][a][r]";


        public StatusCheck(Dictionary<string, string> status)
        {
            Regex warningCheck = new Regex(pattern);
            foreach (KeyValuePair<string, string> pair in status)
            {

                if (pair.Value.Contains("Build FAILED") || pair.Value.Contains("MSBUILD : error"))
                {
                    error = true;
                }
                else if (warningCheck.IsMatch(pair.Value))
                {
                    warning = true;
                }
                else if (pair.Value.Contains("Build succeeded"))
                {
                    success = true;
                }
            }
        }
    }
}
