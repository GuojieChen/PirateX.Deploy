using Akka.Configuration.Hocon;

namespace PirateX.Deploy.Command
{
    [CommandName("uninstall-service")]
    public class UninstallServiceCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            string serviceName = param.GetString();
            var service = ServiceManager.QueryService(serviceName);
            if (service == null)
            {
                return $"service {serviceName} do not exist";
            }
            var uninstallCmd = new OriginCommand
            {
                Name = "sc",
                Param = "delete " + serviceName
            };
            CommandExecutor.RunCommand(uninstallCmd);
            return $"uninstall service {serviceName} success!";
        }
    }
}
