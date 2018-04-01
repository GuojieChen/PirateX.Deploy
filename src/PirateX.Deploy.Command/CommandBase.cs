using Akka.Configuration.Hocon;
using SuperWebSocket;
using Topshelf.Logging;

namespace PirateX.Deploy.Command
{
    public interface ICommand
    {
        WebSocketSession Session { get; set; }

        string Execute(IHoconElement param);
    }

    public abstract class CommandBase : ICommand
    {
        public WebSocketSession Session { get; set; }

        public abstract string Execute(IHoconElement param);

        private static readonly LogWriter logger = HostLogger.Get("Command");

        public virtual void Send(string msg)
        {
            Session?.Send(msg);

            if (logger.IsDebugEnabled)
                logger.Debug(msg);
        }
    }
}
