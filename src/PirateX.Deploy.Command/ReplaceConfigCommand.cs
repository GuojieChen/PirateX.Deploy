//using Akka.Configuration.Hocon;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using Newtonsoft.Json;

//namespace PirateX.Deploy.Command
//{
//    [CommandName("replace-config")]
//    public class ReplaceConfigCommand : CommandBase
//    {
//        public override string Execute(IHoconElement param)
//        {
//            var hobj = param as HoconObject;
//            if (hobj == null)
//            {
//                throw new Exception("extract command param error");
//            }
//            var packageName = hobj.GetKey("PackageName").GetString();
//            var configPath = hobj.GetKey("Path")?.GetString();
//            if (string.IsNullOrEmpty(configPath) || configPath == "default")
//            {
//                configPath = CommandExecutor.DefaultFilePath + packageName;
//            }
//            else
//            {
//                //configPath = Path.Combine(configPath, packageName);
//            }
//            string response = ReplaceConfig(configPath);
//            return response;
//        }

//        private string ReplaceConfig(string configPath)
//        {
//            string result = "";
//            string configFile = Path.Combine(configPath, "deploy_config.json");// configPath + "\\deploy_config.json";
//            if (!File.Exists(configFile))
//            {
//                return $"{configFile} not exist!";
//            }
//            string json = File.ReadAllText(configFile);
//            var fileList = JsonConvert.DeserializeObject<List<string>>(json);
//            if (CommandExecutor.MachineConfig == null)
//            {
//                result += "have no machine config!\r\n";
//            }
//            if (CommandExecutor.EnvironmentConfig == null)
//            {
//                result += "have no environment config!\r\n";
//            }

//            foreach (var relativePath in fileList)
//            {
//                string configFilePath = Path.Combine(configPath, relativePath);//configPath + "\\" + relativePath;
//                if (!File.Exists(configFilePath))
//                {
//                    result += $"config file {configFilePath} not exist!\r\n";
//                }
//                string txt = File.ReadAllText(configFilePath);
//                if (CommandExecutor.MachineConfig != null)
//                {
//                    foreach (var kv in CommandExecutor.MachineConfig)
//                    {
//                        txt = txt.Replace(kv.Key, kv.Value);
//                    }
//                    result += $"replace {configFilePath} machine config complete!\r\n";
//                }

//                if (CommandExecutor.EnvironmentConfig != null)
//                {
//                    foreach (var kv in CommandExecutor.EnvironmentConfig)
//                    {
//                        txt = txt.Replace(kv.Key, kv.Value);
//                    }
//                    result += $"replace {configFilePath} environment config complete!\r\n";
//                }

//                File.WriteAllText(configFilePath, txt);
//            }
//            result += "repalce complete!";
//            return result;
//        }
//    }
//}
