using Akka.Configuration.Hocon;
using System;
using System.IO;

namespace PirateX.Deploy.Command
{
    [CommandName("delete-download")]
    public class DeleteDownloadCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            var hobj = param as HoconObject;
            if (hobj == null)
            {
                throw new Exception("extract command param error");
            }

            var packageName = hobj.GetKey("PackageName").GetString();
            var version = hobj.GetKey("Version").GetString();
            var path = hobj.GetKey("Path")?.GetString() ?? "null";
            if (path != "null")
            {
                if (File.Exists(path))
                {
                    File.Delete(path);

                    return $"delete file -> {path} success!";
                }
                return $"file not exists -> {path}";
            }
            string zipName = $"{packageName}-{version}.zip";
            var filename = Path.Combine(CommandExecutor.WorkSpace, "Packages", zipName);//$"{dir}{zipName}";

            if (File.Exists(filename))
            {
                File.Delete(filename);

                return $"delete file -> {filename} success!";
            }
            return $"file not exists -> {filename}";
        }
    }
}
