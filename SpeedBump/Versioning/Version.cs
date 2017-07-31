using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedBump.Versioning
{
    public class Version
    {
        private string type;
        private string name;
        private int[] version;
        private string verString;

        public Version()
        {
            type = "";
            name = "";
            version = new int[4];

        }
        public Version(int[] array)
        {
            type = "";
            name = "";
            version = array;

        }
        public Version(int[] version2, string type2, string name2, string verString2)
        {
            type = type2;
            name = name2;
            version = version2;
            verString = verString2;
        }
        public string getType() {;
            return type; }
        public string getName() {
            return name; }
        public string getVersion()
        {
            return verString;
        }
        public void setType(string newtype)
        { type = newtype; }

        public void setName(string newname)
        { name = newname; }
        public void setVersion(string ver)
        {
            string charVer = ver;
            verString = charVer;

        }
        public void bumpRewrite()
        {
            toArray();
            version[0] += 1;
            version[1] = 0;
            version[2] = 0;
            version[3] = 0;
            verString = toString();
        }
        public void bumpMajor()
        {
            toArray();
            version[1] += 1;
            version[2] = 0;
            version[3] = 0;
            verString = toString();
        }
        public void bumpMinor()
        {
            toArray();
            version[2] += 1;
            version[3] = 0;
            verString = toString();
        }
        public void bumpTrivial()
        {
            toArray();
            version[3] += 1;
            verString = toString();
        }
        public string toString()
        {
            string temp = "";
            foreach (int num in version)
            {
                object o = new object();
                o = num;
                temp = temp + num + '.';
            }
            temp = temp.TrimEnd('.');
            verString = temp;
                
        
            return temp;
        }
        public int[] toArray()
        {
            char[] delimiter = { '.' };
            string ver = getVersion();
            string[] nums = ver.Split(delimiter);
            int[] nums2 = Array.ConvertAll<string, int>(nums,int.Parse);
            version = nums2;
            return version;
        }
    }
}
