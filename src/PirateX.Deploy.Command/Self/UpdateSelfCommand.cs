using Akka.Configuration.Hocon;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PirateX.Deploy.Command
{
    [CommandName("update-self",Description ="更新自己")]
    public class UpdateSelfCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            var hobj = param as HoconObject;
            if (hobj == null)
            {
                throw new Exception("package-get command param error");
            }
            var feedName = hobj.GetKey("FeedName").GetString();
            var groupName = hobj.GetKey("GroupName")?.GetString() ?? "null";
            var version = hobj.GetKey("Version").GetString();
            var credential = hobj.GetKey("Credential")?.GetString();
            var progetSource = hobj.GetKey("Source")?.GetString() ;

            var packageName = (hobj.GetKey("PackageName")?.GetString()) ?? "PirateX.Deploy.Agent";

            string para = $"{feedName} {groupName} {version} {credential} {progetSource}";

            var downloadTask = PackageGetCommand.DownLoadPackage(base.Session, feedName, groupName, packageName, version, credential, progetSource);

            while (!downloadTask.IsCompleted)
            {
                base.Send("downloadTask not completed");

                Thread.Sleep(1000);
            }

            if (string.IsNullOrEmpty(downloadTask.Result))
                return "download package fail!";

            Send(downloadTask.Result);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("echo off");
            stringBuilder.AppendLine("net stop PirateX.Deploy.Agent");
            stringBuilder.AppendLine($"7z x \"{downloadTask.Result}\" -y o\"{AppDomain.CurrentDomain.BaseDirectory}\"");
            stringBuilder.AppendLine($"net start PirateX.Deploy.Agent");

            var content = stringBuilder.ToString();

            base.Send(content);

            var bytes = Encoding.UTF8.GetBytes(content);

            var filename = $"temp_{DateTime.Now.Ticks}.bat";
            using (var ms = File.Create(filename))
                ms.Write(bytes, 0, bytes.Length);

            string msg = "===updating self===";
            Send(msg);
            Thread.Sleep(3000);
            Session?.Close();
            Session?.AppServer.Stop(); //这里必须加上，不然40001端口还在Listen状态（有可能不是，但是新的监听不了），更新后开启的DeployServer就不能监听了
                                       //WithHandshake("update!");
            Task.Run(()=>CommandExecutor.RunExeWithParam(filename,string.Empty));

            return "updating... please don't wait here!";
        }
    }
}
