using System;
using System.ServiceProcess;

namespace PirateX.Deploy.Command
{
    public static class ServiceManager
    {
        public static void InstallService(UserCommand cmd, string serviceName)
        {
            var service = QueryService(serviceName);
            if (service != null)
                return;
            var installCmd = new OriginCommand();
            installCmd.Name = CommandExecutor.WorkSpace + cmd.PackageName + "\\" + cmd.ProgramName;
            installCmd.Param = "install";
            CommandExecutor.RunCommand(installCmd);
        }

        public static void UpgradeService(UserCommand cmd, ServiceController service)
        {
            if (service.Status != ServiceControllerStatus.Stopped)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 30));//等待30秒服务停止    
            }
        }

        public static void UninstallService(UserCommand cmd, ServiceController service)
        {
            if (service.Status != ServiceControllerStatus.Stopped)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 30));//等待30秒服务停止    
            }
            var uninstallCmd = new OriginCommand();
            uninstallCmd.Name = CommandExecutor.WorkSpace + cmd.PackageName + "\\" + cmd.ProgramName;
            uninstallCmd.Param = "uninstall";
            CommandExecutor.RunCommand(uninstallCmd);
        }

        public static ServiceController QueryService(string name)
        {
            var services = ServiceController.GetServices();

            foreach (var service in services)
            {
                if (name == service.ServiceName)
                    return service;
            }
            return null;
        }

        public static string GetServiceName(string proName)
        {
            string name = proName;
            if (name.EndsWith(".exe"))
            {
                char[] dch = new[] { '.', 'e', 'x', 'e' };
                name = name.TrimEnd(dch);
            }
            return name;
        }
    }
}
