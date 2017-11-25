using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBump.Deployment
{
    public class JSONVersion
    {
        public string Version { get; set; }
        public int[] IntVersion { get; set; }

        public int[] toArray()
        {
            char[] delimiter = { '.' };
            string[] nums = Version.Split(delimiter);
            int[] nums2 = Array.ConvertAll<string, int>(nums, int.Parse);
            IntVersion = nums2;
            return IntVersion;
        }
        public void bumpRewrite()
        {
            toArray();
            IntVersion[0] += 1;
            IntVersion[1] = 0;
            IntVersion[2] = 0;
            IntVersion[3] = 0;
            Version = toString();
        }
        public void bumpMajor()
        {
            toArray();
            IntVersion[1] += 1;
            IntVersion[2] = 0;
            IntVersion[3] = 0;
            Version = toString();
        }
        public void bumpMinor()
        {
            toArray();
            IntVersion[2] += 1;
            IntVersion[3] = 0;
            Version = toString();
        }
        public void bumpTrivial()
        {
            toArray();
            IntVersion[3] += 1;
            Version = toString();
        }
        public string toString()
        {
            string temp = "";
            foreach (int num in IntVersion)
            {
                object o = new object();
                o = num;
                temp = temp + num + '.';
            }
            temp = temp.TrimEnd('.');
            Version = temp;


            return temp;
        }
    }
}
