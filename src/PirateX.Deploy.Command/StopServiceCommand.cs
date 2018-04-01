using Akka.Configuration.Hocon;
using System;
using System.ServiceProcess;

namespace PirateX.Deploy.Command
{
    [CommandName("stop-service")]
    public class StopServiceCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            string serviceName = param.GetString();
            var service = ServiceManager.QueryService(serviceName);
            if (service == null)
            {
                return $"service {serviceName} do not exist";
            }
            if (service.Status == ServiceControllerStatus.Running)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 30));
                return $"service {serviceName} stop success!";
            }
            return $"service {serviceName} current status is {service.Status.ToString()}, can not stop!";
        }
    }
}
