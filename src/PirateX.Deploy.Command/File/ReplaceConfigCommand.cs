using Akka.Configuration.Hocon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace PirateX.Deploy.Command
{
    [CommandName("replace-config",Description = "替换配置内容")]
    public class ReplaceConfigCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            var hobj = param as HoconObject;
            if (hobj == null)
                throw new Exception("extract command param error");

            var configPath = hobj.GetKey("Path")?.GetString();
            var searchPattern = hobj.GetKey("Patterns").GetArray().Select(item=>item.GetString());


            string response = ReplaceConfig(configPath, searchPattern);
            return response;
        }

        private string ReplaceConfig(string path, IEnumerable<string> searchPattern)
        {
            string result = "";

            foreach (var pattern in searchPattern)
            {
                var files = Directory.GetFiles(path, pattern, SearchOption.AllDirectories);

                foreach (var file in files)
                    Replace(file);
            }

            return "repalce complete!";
        }

        private void Replace(string file)
        {
            string txt = File.ReadAllText(file);
            if (base.MachineConfig != null)
            {
                foreach (var kv in base.MachineConfig)
                {
                    txt = txt.Replace(kv.Key, kv.Value);
                }

                base.Send($"replace {file} machine config complete!");
            }

            if (base.EnvironmentConfig != null)
            {
                foreach (var kv in base.EnvironmentConfig)
                {
                    txt = txt.Replace(kv.Key, kv.Value);
                }

                base.Send($"replace {file} environment config complete!");
            }

            File.WriteAllText(file, txt);
        }
    }
}
