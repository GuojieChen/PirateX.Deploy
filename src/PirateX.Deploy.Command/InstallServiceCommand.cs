using Akka.Configuration.Hocon;
using System;
using System.IO;
using System.ServiceProcess;

namespace PirateX.Deploy.Command
{
    [CommandName("install-service")]
    public class InstallServiceCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            var hobj = param as HoconObject;
            if (hobj == null)
            {
                throw new Exception("download command param error");
            }
            var packageName = hobj.GetKey("PackageName").GetString();
            var programName = hobj.GetKey("ProgramName")?.GetString()?? $"{packageName}.exe";
            var serviceName = hobj.GetKey("ServiceName").GetString();
            var servicepath = hobj.GetKey("Path")?.GetString() ?? "default";
            
            var service = QueryService(serviceName);
            if (service != null)
                return $"service {serviceName} existed, need no install operation!";

            if (servicepath == "default")
                servicepath = Path.Combine(CommandExecutor.WorkSpace, packageName); //Runner.DefaultFilePath + packageName;

            if (!Directory.Exists(servicepath))
                Directory.CreateDirectory(servicepath);

            var installCmd = new OriginCommand();

            installCmd.Name = "sc"; //Runner.DefaultFilePath + packageName + "\\" + programName;
            installCmd.Param = $"create {serviceName} binPath=\"" + Path.Combine(servicepath,programName) +"\"";
            CommandExecutor.RunCommand(installCmd);
            return $"{installCmd.Name} {installCmd.Param}";
        }

        private ServiceController QueryService(string name)
        {
            var services = ServiceController.GetServices();

            foreach (var service in services)
            {
                if (name == service.ServiceName)
                    return service;
            }
            return null;
        }
    }
}
