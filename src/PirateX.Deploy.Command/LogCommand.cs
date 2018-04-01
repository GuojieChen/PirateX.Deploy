using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Configuration.Hocon;

namespace PirateX.Deploy.Command
{
    [CommandName("log")]
    public class LogCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            string msg = param.GetString();

            base.Send(msg);
            return $"log msg!";
        }
    }
}
