using Akka.Configuration.Hocon;
using PirateX.Deploy.Command;
using System.Linq;
using System.Reflection;

namespace PirateX.Deploy.Common.Self
{
    [CommandName("help", Description = "查看帮助")]
    public class HelpCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            var cmds = typeof(HelpCommand).Assembly.GetTypes().Where(item => item.IsClass && !item.IsAbstract && typeof(CommandBase).IsAssignableFrom(item));

            foreach (var cmd in cmds)
            {
                var attr = cmd.GetCustomAttribute<CommandNameAttribute>();
                if (attr == null)
                    continue;

                base.Send($"{attr.CommandName,-20}\t\t{attr.Description}");
            }

            return "";
        }
    }
}
