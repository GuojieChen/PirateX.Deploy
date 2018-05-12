using Akka.Configuration.Hocon;
using SuperWebSocket;
using System.Collections.Generic;
using Topshelf.Logging;

namespace PirateX.Deploy.Command
{
    public interface ICommand
    {
        WebSocketSession Session { get; set; }

        string Execute(IHoconElement param);

        Dictionary<string, string> EnvironmentConfig { get; set; }
        Dictionary<string, string> MachineConfig { get; set; }
    }

    public abstract class CommandBase : ICommand
    {
        public WebSocketSession Session { get; set; }
        public Dictionary<string, string> EnvironmentConfig { get; set; }
        public Dictionary<string, string> MachineConfig { get; set; }

        public abstract string Execute(IHoconElement param);

        private static readonly LogWriter logger = HostLogger.Get("Command");

        public virtual void Send(string msg,bool willlogger = true)
        {
            Session?.Send(msg);

            if (willlogger && logger.IsDebugEnabled)
                logger.Debug(msg);
        }
    }
}
