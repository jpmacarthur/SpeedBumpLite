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

namespace SpeedBump.Deployment
{
    public class DeploymentManager
    {
        public DeploymentManager(ProjectControlSource source)
        {
            this.source = source;
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private VersionManager ver = new VersionManager();
        private ProjectControlSource source = new ProjectControlSource();

        public Versioning.Version Bump(ProjectControlSourceItem item, ProjectControlSource source, string choice)
        {
            VersionManager ver = new VersionManager();
            Versioning.Version newVersion = new Versioning.Version();
            string projectPath = source.BaseDir + item.BaseDir + @"\";
            Versioning.Version itemVersion = ver.GetVersion(projectPath + item.StageDir);

            //List<string> childpaths = ver.GetChildren(source.BaseDir + item.BaseDir);
            string[] childpaths = Directory.GetDirectories(projectPath);

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


            string pattern = "\"[^\"]+\"";
            int count = 0;
            foreach (string child in childpaths)
            {
                string filepath = child + "\\properties\\AssemblyInfo.cs";
                if (!File.Exists(filepath))
                {
                    log.Warn(filepath +"does not exist. Skipping");
                    continue;
                }
                string[] temp = File.ReadAllLines(filepath);
                foreach (string line in temp)
                {
                    if (line.Contains("[assembly: AssemblyVersion(") && !line.Contains("//"))
                    {
                        temp[count] = Regex.Replace(line, pattern, '"' + itemVersion.getVersion() + '"');
                        count = 0;
                        break;
                    }
                    count++;
                }
                File.WriteAllLines(filepath, temp);

                newVersion = ver.GetVersion(projectPath + item.StageDir);





            }

            return newVersion;
        }
        private string run(string command, string arguments)
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo();
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            bool b = process.Start();

            StringBuilder buffer = new StringBuilder();
            buffer.Append(command + " " + arguments + "\n\n");
            while (!process.HasExited)
            {
                buffer.Append(process.StandardOutput.ReadToEnd());
            }

            var exitCode = process.ExitCode;
            process.Close();

            return buffer.ToString();
        }
        public void Build(ProjectControlSourceItem item)
        {
            string command = source.CompileDir + @"\msbuild.exe";
            string arguments = source.BaseDir + item.BaseDir +"\\" + item.Project + ".sln /p:Configuration=Debug";
            log.Debug("command="+command);
            log.Debug("arguments=" + arguments);
            string result = run(command, arguments);
            if(result.Contains("Build FAILED"))
            {
                throw new Exception("Build Failed");
            }
            log.Debug(result);
        }
        
    } }
