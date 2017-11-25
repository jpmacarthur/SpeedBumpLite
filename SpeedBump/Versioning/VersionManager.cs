using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SpeedBump.Versioning
{
    public class VersionManager
    {
        public Deployment.JSONVersion GetVersion(string path)
        {
            Deployment.JSONVersion jSONVersion = JsonConvert.DeserializeObject<Deployment.JSONVersion>(File.ReadAllText(path));
            return jSONVersion;
        }
    }
}

