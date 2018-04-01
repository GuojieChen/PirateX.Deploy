using Akka.Configuration.Hocon;
using System;
using System.IO;

namespace PirateX.Deploy.Command
{
    [CommandName("run-app")]
    public class RunAppCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            var hobj = param as HoconObject;
            if (hobj == null)
            {
                throw new Exception("extract command param error");
            }
            var packageName = hobj.GetKey("PackageName").GetString();
            var programName = hobj.GetKey("ProgramName").GetString();
            var appParam = hobj.GetKey("Param")?.GetString();
            var appPath = Path.Combine(CommandExecutor.WorkSpace, packageName, programName);
            
            if (!File.Exists(appPath))
            {
                throw new Exception($"file {appPath} not exist!");
            }
            var runAppCmd = new OriginCommand();
            runAppCmd.Name = appPath;
            runAppCmd.Param = appParam;
            CommandExecutor.RunExeWithParam(Session, runAppCmd);
            return $"run application {appPath} success!";
        }
    }
}
