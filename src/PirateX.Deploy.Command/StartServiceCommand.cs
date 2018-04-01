using Akka.Configuration.Hocon;
using System;
using System.ServiceProcess;

namespace PirateX.Deploy.Command
{
    [CommandName("start-service")]
    public class StartServiceCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            string serviceName = param.GetString();
            var service = ServiceManager.QueryService(serviceName);
            if (service == null)
            {
                return $"service {serviceName} do not exist";
            }
            if (service.Status == ServiceControllerStatus.Stopped)
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 30));
                return $"service {serviceName} start success!";
            }
            return $"service {serviceName} current status is {service.Status.ToString()}, can not start!";
        }
    }
}
