using Akka.Configuration.Hocon;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PirateX.Deploy.Command
{
    [CommandName("run-app",Description ="运行程序")]
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

    [CommandName("execute-batch",Description ="执行批处理脚本")]
    public class ExecuteBatchCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            var filename = $"temp_{DateTime.Now.Ticks}.bat";
            var content = param.GetString();
            var bytes = Encoding.UTF8.GetBytes("@echo off\r\n"+content);

            using (var ms = File.Create(filename))
                ms.Write(bytes, 0, bytes.Length);

            try
            {
                var runAppCmd = new OriginCommand();
                runAppCmd.Name = filename;
                runAppCmd.Param = "";
                CommandExecutor.RunExeWithParam(Session, runAppCmd);
            }
            catch
            {
                throw;
            }
            finally
            {
                File.Delete(filename);
            }

            return $"execute batch success!";
        }
    }
}
