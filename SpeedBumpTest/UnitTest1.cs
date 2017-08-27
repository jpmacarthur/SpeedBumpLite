using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpeedBump.Deployment;
using LCP;
using System.Linq;
using System.IO;

namespace SpeedBumpTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            DeleteDirectory(@"C:\LCP\TEST");
        }
        public void DeleteDirectory(string path)
        {
            string[] dirs = Directory.GetDirectories(path);
            if (dirs.Count() != 0)
            {
                foreach (string dir in dirs)
                {
                    DeleteDirectory(dir);
                }
            }
            string[] files = Directory.GetFiles(path);
            if (files.Count() != 0)
            {
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }
            Directory.Delete(path);
        }
    }
}
