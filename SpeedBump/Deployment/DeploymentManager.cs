using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeedBump.Versioning;
using System.IO;
using System.Text.RegularExpressions;
using log4net;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Threading;
using Newtonsoft.Json;

namespace SpeedBump.Deployment
{
    public class DeploymentManager
    {
        //TODO remove source and item from functions
        public DeploymentManager(ProjectControlSource source, ProjectControlSourceItem item)
        {
            this.source = source;
            this.item = item;
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private VersionManager ver = new VersionManager();
        private ProjectControlSource source;
        private ProjectControlSourceItem item;

        public Deployment.JSONVersion Bump(string choice)
        {
            string projectPath = source.BaseDir + item.BaseDir + @"\version.json";
            Deployment.JSONVersion itemVersion = ver.GetVersion(projectPath);

            switch (choice)
            {
                case "Trivial":
                    itemVersion.bumpTrivial();
                    break;
                case "Minor":
                    itemVersion.bumpMinor();
                    break;
                case "Major":
                    itemVersion.bumpMajor();
                    break;
                default:
                    throw new Exception(choice + "is not a valid option");
            }
            string[] temp = File.ReadAllLines(projectPath);
            int count = 0;
            string pattern = @"\d*[.]\d*[.]\d*[.]\d*";
            foreach (string line in temp)
            {
                if(line.Contains(@"version"))
                {
                    temp[count] = Regex.Replace(line, pattern, itemVersion.Version);
                }
                count++;
            }

            File.WriteAllLines(projectPath, temp);
            return ver.GetVersion(projectPath);
        }
        private void copyDirectory()
        {
            string path = source.BaseDir + item.BaseDir + "\\" + "version.json";
            Deployment.JSONVersion itemVersion = ver.GetVersion(path);
            string SourcePath = source.BaseDir + item.BaseDir + "\\" + item.StageDir;

            Directory.CreateDirectory(SourcePath + @"\bin\x64\copy\" + itemVersion.Version);
            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(SourcePath + @"\bin\x64\Debug", "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(SourcePath + @"\bin\x64\Debug", SourcePath + @"\bin\x64\copy\" + itemVersion.Version), true);
        }
        private void Zip()
        {
            string jsonpath = source.BaseDir + item.BaseDir + "\\" + "version.json";
            Deployment.JSONVersion itemVersion = ver.GetVersion(jsonpath);
            string path = source.BaseDir + item.BaseDir + "\\" + item.StageDir + @"\bin\x64\copy\";
            ZipFile.CreateFromDirectory(path, source.BaseDir + item.BaseDir + "\\" + item.StageDir + @"\bin\x64\" + itemVersion.Version + ".zip"); 
        }
        private void upload(string address)
        {
            string path = source.BaseDir + item.BaseDir + "\\" + "version.json";
            Deployment.JSONVersion itemVersion = ver.GetVersion(path);
            string remoteStagingDir = item.RemoteStagingDir; // get this from the algo control properties; you may have to create a new entry
            string zipFilename = itemVersion.Version + ".zip";
            var password = source.FTPHosts.Where(f => f.IPAddress == address)
                .Select(p => p.Password)
                .First();
            var username = source.FTPHosts.Where(f => f.IPAddress == address)
                .Select(u => u.Username)
                .First();
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(username, password);
                    client.UploadFile(@"ftp://" + address + "/" + remoteStagingDir + "/" + zipFilename, "STOR", source.BaseDir + item.BaseDir + "\\" + item.StageDir + @"\bin\x64\" + zipFilename);
                }
            }catch(Exception) { remove(); throw; }
        }
        private void remove()
        {
            string path = source.BaseDir + item.BaseDir + "\\" + "version.json";
            Deployment.JSONVersion itemVersion = ver.GetVersion(path);
            Directory.Delete(source.BaseDir + item.BaseDir + "\\" + item.StageDir + "\\" + @"bin\x64\copy", true);
            File.Delete(source.BaseDir + item.BaseDir + "\\" + item.StageDir + "\\" + @"bin\x64\" + itemVersion.Version + ".zip");       
        }
        public void Deploy(string address)
        {
            copyDirectory();
            Zip();
            upload(address);
            remove();
        }
        private void DeleteDirectory(string path)
        {
            string[] dirs = Directory.GetDirectories(path);
            if(dirs.Count() != 0)
            {
                foreach(string dir in dirs)
                {
                    DeleteDirectory(dir);
                }
            }
            string[] files = Directory.GetFiles(path);
            if(files.Count() != 0)
            {
                foreach(string file in files)
                {
                    File.Delete(file);
                }
            }
            Directory.Delete(path);
        }
        
    } }
