using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace SpeedBump.Versioning
{
    public class VersionManager
    {
        /// <summary>
        /// Gets the sln file for any certain path
        /// </summary>
        /// <param name="filename">Path to folder containing .sln</param>
        /// <returns>Returns path to .sln file</returns>
        public string getSLNFile(string filename)
        {
            int last;
            string newfile = null;
            try
            {
                last = filename.LastIndexOf("\\");
                newfile = filename + filename.Substring(last) + ".sln";
            }
            catch (NullReferenceException help)
            {
                Console.WriteLine(help);

            }
            return newfile;
        }
        /// <summary>
        /// Parses manifest.json file to get the version of it
        /// </summary>
        /// <param name="json">A myFile that contains the contents of a manifest.json file and the path to the file</param>
        /// <returns>Returns a version object that contains the version and the name of the parent</returns>
        public Version getjsonVersion(MyFile json)
        {

            Version ver = new Version();
            int count = 0;
            ver.setName(json.getFilename());

            string temp = "";
            string pattern = "[:][' ']\"[^\"]+\"";
            foreach (string line in json.getData())
            {

                if (line.Contains("version") && count < 2)
                {
                    ver = new Version();
                    ver.setType("Parent");
                    var match = Regex.Match(line, pattern);
                    temp = (match.Value);
                    temp = temp.Substring(3);
                    temp = temp.TrimEnd('\"');
                    ver.setVersion(temp);
                    return ver;

                }
                else count++;
            }
            return ver;
        }




        /// <summary>
        /// Opens a json file and puts the contents into a myFile
        /// </summary>
        /// <param name="filename">A path that contains a manifest.json file</param>
        /// <returns>Returns a myFile that contains the contents and the corresponding path</returns>
        public MyFile openJSON(string filename)
        {
            MyFile json = new MyFile();
            json.setFilename(filename);
            try
            {
                foreach (string line in File.ReadLines(filename + "\\manifest.json", Encoding.UTF8))
                {
                    json.add(line);
                }
            }
            catch (System.IO.FileNotFoundException) { Console.WriteLine(); }


            return json;
        }
        /// <summary>
        /// Gets the version of a child by parsing the assembly info file
        /// </summary>
        /// <param name="file">A myFile of a child</param>
        /// <returns>Returns a version object containing version number and name of the child</returns>
        public Version getchildVersion(MyFile file)
        {
            Version ver = new Version();

            string temp = "";
            string pattern = "\"[^\"]+\"";
            foreach (string line in file.getData())
            {

                if (line.Contains("[assembly: AssemblyVersion(") && !line.Contains("//"))
                {
                    ver = new Version();
                    ver.setType("Child");
                    ver.setName(file.getFilename());
                    var match = Regex.Match(line, pattern);
                    temp = (match.Value);
                    temp = temp.Substring(1);
                    temp = temp.TrimEnd('\"');
                    ver.setVersion(temp);

                    return ver;

                }
            }


            return ver;
        }
        /// <summary>
        /// Opens an assembly info file and puts it into a myFile
        /// </summary>
        /// <param name="filename">Path to a folder containing an assembly info file</param>
        /// <returns>Returns a myFile with the contents of the assembly info file</returns>
        public MyFile OpenAssemblyInfo(string filename)
        {
            MyFile info = new MyFile();
            info.setFilename(filename);
            string path = filename + "\\properties\\AssemblyInfo.cs";
            if (File.Exists(path))
            {
                foreach (string line in File.ReadLines(filename + "\\properties\\AssemblyInfo.cs", Encoding.UTF8))
                { info.add(line); }
            }
            return info;
        }
        /// <summary>
        /// Gets the dependencies of a selected project
        /// </summary>
        /// <param name="filename">Path to a parent folder</param>
        /// <returns>Returns a list containing the names of the dependencies</returns>
        public List<string> GetDependencies(string filename)
        {
            List<string> depen = new List<string>();
            List<string> depen2 = new List<string>();
            string newfile = getSLNFile(filename);
            int first;
            int last;
            string holder;
            try
            {
                foreach (string line in File.ReadLines(newfile, Encoding.UTF8))
                {
                    if (line.Contains("\"..\\"))
                    {
                        depen.Add(line);
                    }
                }
                foreach (string line in depen)
                {
                    first = line.IndexOf("=");
                    last = line.IndexOf(",");
                    if (last > 0)
                    {
                        holder = line.Substring(first + 3);
                        last = holder.IndexOf(",") - 1;
                        holder = holder.Substring(0, last);
                        depen2.Add(holder);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException || ex is FileNotFoundException)
                {
                    Console.WriteLine("There was a slight issue");
                }
            }
            return depen2;
        }
        /// <summary>
        /// Reads a .sln file and gets the paths of the children
        /// </summary>
        /// <param name="filename">Path to a parent directory containing a .sln file</param>
        /// <returns>Returns a list containing the children</returns>
        public List<string> GetChildrenPath(string filename)
        {
            string newfile = getSLNFile(filename);
            List<string> depen = new List<string>();
            try
            {
                foreach (string line in File.ReadLines(newfile, Encoding.UTF8))
                {
                    if (!line.Contains("\"..\\") && line.Contains(".csproj"))
                    {
                        depen.Add(line);
                    }
                }

            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException || ex is FileNotFoundException)
                {
                    Console.WriteLine("There was a slight issue");
                }
            }
            return depen;
        }
        /// <summary>
        /// Gets the children of a parent file
        /// </summary>
        /// <param name="filename">Path to a parent directory containing a .sln file</param>
        /// <returns>Returns a list containing the names of the child projects</returns>
        public List<string> GetChildren(string directory)
        {


            
            string newfile = getSLNFile(directory);
            List<string> depen = new List<string>();
            List<string> depen2 = new List<string>();
            int first;
            int last;
            string holder;
            try
            {
                foreach (string line in File.ReadLines(newfile, Encoding.UTF8))
                {
                    if (!line.Contains("\"..\\") && line.Contains(".csproj"))
                    {
                        depen.Add(line);
                    }
                }
                foreach (string line in depen)
                {
                    first = line.IndexOf("=");
                    last = line.LastIndexOf(",");
                    if (last > 0)
                    {
                        holder = line.Substring(first + 3);
                        last = holder.IndexOf(",") - 1;
                        holder = holder.Substring(0, last);
                        depen2.Add(holder);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException || ex is FileNotFoundException)
                {
                    Console.WriteLine("There was a slight issue");
                }
            }
            return depen2;
        }
        /// <summary>
        /// Gets all of the subdirectories for any directory
        /// </summary>
        /// <param name="filename">Path to main directory</param>
        /// <returns>Returns a list containing the subdirectories inside the given directory</returns>
        public List<string> GetDirectories(string filename)
        {
            string[] dirs;
            List<string> temp = new List<string>();
            try
            {
                dirs = Directory.GetDirectories(filename);
                foreach (string dirt in dirs)
                {
                    temp.Add(dirt);
                }
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
            }

            return temp;


        }
        /// <summary>
        /// Verifies that all versions in the main directory match.
        /// </summary>
        /// <param name="json">Original file which contains a path and its version</param>
        /// <param name="dir">The main directory</param>
        /// <returns>Returns false and throws exception if not matching</returns>
        public bool verify(MyFile json, string dir)
        {
            bool matches = true;
            Version jsonversion = getjsonVersion(json);
            Dictionary<string, string> kids = getAllChildrenVersions(json.getFilename());
            List<MyFile> direcs = openAllJson(dir);

            foreach (KeyValuePair<string, string> entry in kids)
            {
                if (entry.Value != jsonversion.getVersion())
                {
                    matches = false;
                    Exception MismatchedVersion = new Exception(); throw MismatchedVersion;
                }
            }

            if (verifyAllJson(json, direcs) != true) { matches = false; Exception MismatchedVersion = new Exception(); throw MismatchedVersion; };


            return matches;
        }
        /// <summary>
        /// Takes a path and creates a dictionary containing the name of each child and the corresponding version of the child.
        /// </summary>
        /// <param name="filename">The path to the parent</param>
        /// <returns></returns>
        public Dictionary<string, string> getAllChildrenVersions(string filename)
        {
            Dictionary<string, string> kids = new Dictionary<string, string>();
            List<string> filesdirec = GetDirectories(filename);
            MyFile file = new MyFile();
            foreach (string child in filesdirec)
            {
                file = OpenAssemblyInfo(child);
                {
                    if (file.getCount() != 0)
                    {
                        Version little = getchildVersion(file);
                        kids.Add(little.getName(), little.getVersion());
                    }
                }

            }

            return kids;
        }
        /// <summary>
        /// Bumps all of the children contained in a parent directory.  Does not write anything to file.
        /// </summary>
        /// <param name="filename">Path to the parent directory</param>
        /// <returns>Returns a dictionary containing the paths to the children and their corresponding bumped versions.</returns>
        public Dictionary<string, string> bumpChildrenTrivial(string filename)
        {
            Dictionary<string, string> kids = new Dictionary<string, string>();
            List<string> files = GetDirectories(filename);
            MyFile file = new MyFile();
            foreach (string child in files)
            {
                file = OpenAssemblyInfo(child);
                Version little = getchildVersion(file);
                little.bumpTrivial();
                kids.Add(little.getName(), little.getVersion());

            }

            return kids;
        }
        /// <summary>
        /// Bumps all of the children contained in a parent directory.  Does not write anything to file.
        /// </summary>
        /// <param name="filename">Path to the main directory</param>
        /// <returns>Returns a dictionary containing the paths to the children and their corresponding bumped versions.</returns>
        public Dictionary<string, string> bumpChildrenMinor(string filename)
        {
            Dictionary<string, string> kids = new Dictionary<string, string>();
            List<string> files = GetDirectories(filename);
            MyFile file = new MyFile();
            foreach (string child in files)
            {
                file = OpenAssemblyInfo(child);
                Version little = getchildVersion(file);
                little.bumpMinor();
                kids.Add(little.getName(), little.getVersion());

            }

            return kids;
        }
        /// <summary>
        /// Bumps all of the children contained in a parent directory.  Does not write anything to file.
        /// </summary>
        /// <param name="filename">Path to the main directory</param>
        /// <returns>Returns a dictionary containing the paths to the children and their corresponding bumped versions.</returns>
        public Dictionary<string, string> bumpChildrenMajor(string filename)
        {
            Dictionary<string, string> kids = new Dictionary<string, string>();
            MyFile file = new MyFile();
            List<string> files = GetDirectories(filename);
            foreach (string child in files)
            {
                file = OpenAssemblyInfo(child);
                Version little = getchildVersion(file);
                little.bumpMajor();
                kids.Add(little.getName(), little.getVersion());

            }

            return kids;
        }
        /// <summary>
        /// Bumps all of the children contained in a parent directory.  Does not write anything to file.
        /// </summary>
        /// <param name="filename">Path to the main directory</param>
        /// <returns>Returns a dictionary containing the paths to the children and their corresponding bumped versions.</returns>
        public Dictionary<string, string> bumpChildrenRewrite(string filename)
        {
            Dictionary<string, string> kids = new Dictionary<string, string>();
            List<string> files = GetDirectories(filename);
            MyFile file = new MyFile();
            foreach (string child in files)
            {
                file = OpenAssemblyInfo(child);
                Version little = getchildVersion(file);
                little.bumpRewrite();
                kids.Add(little.getName(), little.getVersion());

            }

            return kids;
        }
        /// <summary>
        /// Writes the bumped assembly info files to each child
        /// </summary>
        /// <param name="files">A dictionary where the key is the path and the value is the version</param>
        /// <returns>Returns true if the write worked, False if it did not</returns>
        public bool writechildVersion(Dictionary<string, string> files)
        {
            bool worked = false;
            string pattern = "\"[^\"]+\"";
            int count = 0;
            foreach (KeyValuePair<string, string> pair in files)
            {
                string[] temp = File.ReadAllLines(pair.Key + "\\properties\\AssemblyInfo.cs");
                foreach (string thing in temp)
                {
                    if (thing.Contains("[assembly: AssemblyVersion(") && !thing.Contains("//"))
                    {
                        temp[count] = Regex.Replace(thing, pattern, '"' + pair.Value + '"');
                        count = 0;
                        worked = true;
                        break;
                    }
                    count++;
                }

                if (worked == true) File.WriteAllLines(pair.Key + "\\properties\\AssemblyInfo.cs", temp);

            }
            return worked;
        }
        /// <summary>
        /// Writes the manifest.json file with the version of the given object
        /// </summary>
        /// <param name="file">A version object to be written</param>
        /// <returns>Returns True if the file was written to and False if it was not</returns>
        public bool writejsonVersion(Version file)
        {
            bool worked = false;
            string pattern = "[:][' ']\"[^\"]+\"";
            int count = 0;
            string[] temp = File.ReadAllLines(file.getName() + "\\manifest.json");
            foreach (string thing in temp)
            {
                if (thing.Contains("version") && count < 2)
                {
                    temp[count] = Regex.Replace(thing, pattern, ": \"" + file.getVersion() + '"');
                    worked = true;
                    break;
                }
                count++;
            }

            if (worked == true) File.WriteAllLines(file.getName() + "\\manifest.json", temp);
            return worked;
        }
        /// <summary>
        /// Gets the last used directory
        /// </summary>
        /// <returns>Returns the contents of a text file</returns>
        public string lastdirect()
        {
            string temp = "";
            string location = "C:\\Users\\Pat\\Desktop\\location.txt";


            if (File.Exists(location))
            {
                using (StreamReader file = new StreamReader(location))
                {
                    char[] badchar = { '\r', '\n' };
                    temp = file.ReadToEnd();
                    temp = temp.TrimEnd(badchar);
                    return temp;
                }
            }


            return temp;
        }
        /// <summary>
        /// Writes a filename to the location.txt file
        /// </summary>
        /// <param name="filename">Path of the selected directory</param>
        public void writeDirec(string filename)
        {
            using (StreamWriter file = new StreamWriter("C:\\Users\\Pat\\Desktop\\location.txt"))
            {
                file.WriteLine(filename);
            }
        }
        /// <summary>
        /// Searches through other directories inside the main project and rewrites the manifest.json file with the updated version
        /// </summary>
        /// <param name="json">The main json file used to write the dependents</param>
        /// <param name="dir">Directory of the main project</param>
        /// <returns></returns>
        public bool writeOtherDep(MyFile json, string dir)
        {
            Version ver = getjsonVersion(json);
            List<string> direcs = GetDirectories(dir);
            string tempstr;
            string pattern = "[:][' ']\"[^\"]+\"";
            foreach (string location in direcs)
            {
                int last = json.getFilename().LastIndexOf('\\');
                tempstr = json.getFilename().Substring(last + 1);
                try
                {
                    string[] temp = File.ReadAllLines(location + "\\manifest.json");
                    for (int i = 0; i < temp.Count() - 1; i++)
                    {
                        if (temp[i].Contains(tempstr))
                        {
                            temp[i + 1] = Regex.Replace(temp[i + 1], pattern, ": \"" + ver.getVersion() + '"');
                        }
                    }
                    File.WriteAllLines(location + "\\manifest.json", temp);
                }
                catch (FileNotFoundException e) { Console.WriteLine(e); }

            }

            return true;
        }
        /// <summary>
        /// Opens multiple json files
        /// </summary>
        /// <param name="dir">Main parent directory</param>
        /// <returns>Returns a list contaning myFiles of each manifest.json file.</returns>
        public List<MyFile> openAllJson(string dir)
        {
            List<MyFile> dic = new List<MyFile>();
            List<string> direcs = GetDirectories(dir);

            foreach (string location in direcs)
            {
                string[] temp = File.ReadAllLines(location + "\\manifest.json");
                MyFile json = new MyFile(temp);
                json.setFilename(location);
                dic.Add(json);
            }



            return dic;

        }
        /// <summary>
        /// Takes an original file and compares the version of it against the rest of the files that have it as a dependency
        /// </summary>
        /// <param name="json">Original File</param>
        /// <param name="list">List of all directories to check against</param>
        public bool verifyAllJson(MyFile json, List<MyFile> list)
        {
            bool exists = false;
            string tempstr;
            Version ver = getjsonVersion(json);
            foreach (MyFile item in list)
            {
                int last = item.getFilename().LastIndexOf('\\');
                tempstr = item.getFilename().Substring(last + 1);
                for (int i = 0; i < item.data.Count - 1; i++)
                {
                    if (item.data[i].Contains(tempstr) && item.data[i + 1].Contains(ver.getVersion()))
                    { exists = true; }
                }

            }
            return exists;
        }
        /// <summary>
        /// Backs up an assembly info file into a new folder
        /// </summary>
        /// <param name="filename">Project directory</param>
        public void backupFile(string filename)
        {
            Dictionary<string, string> file = getAllChildrenVersions(filename);
            foreach (KeyValuePair<string, string> line in file)
            {
                StringBuilder str = new StringBuilder(line.Key);
                str.Append(@"\properties\BackUp_");
                str.Append(line.Value + @"\");
                Directory.CreateDirectory(str.ToString());
                File.Copy($"{line.Key}\\properties\\AssemblyInfo.cs", $"{str.ToString()}\\AssemblyInfo.cs", true);



            }
        }
        public Version GetVersion(string path)
        {
            
            MyFile assembly = OpenAssemblyInfo(path);
            Versioning.Version itemVersion = getchildVersion(assembly);
            return itemVersion;
        }
    }
}

