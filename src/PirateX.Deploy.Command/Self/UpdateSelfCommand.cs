using Akka.Configuration.Hocon;
using System;
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
            string para = $"{feedName} {groupName} {version} {credential} {progetSource}";
            string msg = "===updating self===";
            Send(msg);
            Thread.Sleep(3000);
            Session?.AppServer.Stop(); //这里必须加上，不然40001端口还在Listen状态（有可能不是，但是新的监听不了），更新后开启的DeployServer就不能监听了
            Session?.Close();//WithHandshake("update!");
            Task.Run(()=>CommandExecutor.RunExeWithParam(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, para));
            return "updating... please don't wait here!";
        }
    }
}
