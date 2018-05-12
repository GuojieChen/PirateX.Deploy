using Akka.Configuration.Hocon;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading;

namespace PirateX.Deploy.Command
{
    [CommandName("extract",Description = "解压压缩包")]
    public class ExtractCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            var hobj = param as HoconObject;
            if (hobj == null)
            {
                throw new Exception("extract command param error");
            }
            var timeout = hobj.GetKey("Timeout")?.GetInt();
            var packageName = hobj.GetKey("PackageName").GetString();
            var version = hobj.GetKey("Version")?.GetString() ?? "latest";
            var extractPath = hobj.GetKey("Path")?.GetString() ?? "default";
            var force = hobj.GetKey("Force")?.GetBoolean() ?? false;
            var processName = hobj.GetKey("ProcessName")?.GetString();

            if (extractPath == "default")
            {
                extractPath = Path.Combine(CommandExecutor.WorkSpace, packageName); //Runner.DefaultFilePath + packageName;
            }
            if (!Directory.Exists(extractPath))
            {
                Directory.CreateDirectory(extractPath);
            }
            string zipPath = GetZipPath(packageName, version);

            if (!File.Exists(zipPath))
            {
                throw new Exception($"{zipPath} does not existed!");
            }
            var count = 0;
            int exceptionCount = 0;
            var msg = string.Empty;
            do
            {
                try
                {
                    return ExtractZip(zipPath, extractPath);
                }
                catch (Exception ex)
                {
                    if (exceptionCount == 0)
                    {
                        msg += "error " + ex.Message + "\r\n" + ex.StackTrace;
                    }
                    else
                    {
                        Send($"try extract {zipPath} failure! TRY COUNT {exceptionCount} ...");
                    }
                    exceptionCount++;
                }
                Thread.Sleep(1000);
                count++;

            } while (count < timeout);
            if (exceptionCount >= timeout && force && processName != null)
            {
                //强制执行,先将进程kill，再进行extract
                var p = Process.GetProcessesByName(processName);
                foreach (Process t in p)
                {
                    t.Kill();
                }
                msg += ExtractZip(zipPath, extractPath);
            }
            return msg;
        }

        private string ExtractZip(string zipPath, string extractPath)
        {
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)//解压文件到目录，有子目录就自建子目录
                {
                    string filePath = Path.Combine(extractPath, entry.FullName);
                    if (entry.FullName.Contains("/"))
                    {
                        var dst = entry.FullName.Split('/');
                        string subRelativeDir = entry.FullName.TrimEnd(dst[dst.Length - 1].ToCharArray());
                        string subDir = Path.Combine(extractPath, subRelativeDir);
                        if (!Directory.Exists(subDir))
                        {
                            Directory.CreateDirectory(subDir);
                        }
                    }

                    entry.ExtractToFile(filePath, true);
                }
            }
            return $"extract {zipPath} to {extractPath} success!";
        }

        private string GetZipPath(string packageName, string version)
        {
            string dir = Path.Combine(CommandExecutor.WorkSpace, "Packages");//$"{Runner.DefaultFilePath}Packages\\";
            string zipName = $"{packageName}-{version}.zip";
            string zipPath = Path.Combine(dir, zipName);//$"{dir}{zipName}";
            if (version != "latest") return zipPath;
            var files = Directory.GetFiles(dir);
            if (files.Length == 0)
            {
                throw new Exception("none downloaded packages for extract!");
            }
            string matchStr = @"(\d+).(\d+).(\d+)";//匹配版本号
            Regex regex = new Regex($"{packageName}-{matchStr}.zip");
            int maxPv = 0;
            int maxRv = 0;
            int maxCv = 0;
            bool fileExist = false;
            foreach (var file in files)
            {
                var match = regex.Match(file);
                if (match.Success && match.Groups.Count == 4)
                {
                    int projectVersion = int.Parse(match.Groups[1].Value);
                    int requireVersion = int.Parse(match.Groups[2].Value);
                    int compileVersion = int.Parse(match.Groups[3].Value);
                    fileExist = true;
                    if (projectVersion > maxPv)
                    {
                        maxPv = projectVersion;
                        maxRv = requireVersion;
                        maxCv = compileVersion;
                    }
                    else if (projectVersion == maxPv)
                    {
                        if (requireVersion > maxRv)
                        {
                            maxRv = requireVersion;
                            maxCv = compileVersion;
                        }
                        else if (requireVersion == maxRv)
                        {
                            if (compileVersion > maxCv)
                            {
                                maxCv = compileVersion;
                            }
                        }
                    }
                }
            }
            if (fileExist)
            {
                zipName = $"{packageName}-{maxPv}.{maxRv}.{maxCv}.zip";
                zipPath = Path.Combine(dir, zipName); //$"{dir}{zipName}";
            }
            else
            {
                throw new Exception($"none package {packageName} downloaded!");
            }
            return zipPath;
        }
    }
}
