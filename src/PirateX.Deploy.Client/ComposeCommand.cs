using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PirateX.Deploy.Client
{
    public class ComposeCommand
    {
        public string Command { get; set; }

        public string Environment { get; set; }

        public string SpecificMachine { get; set; }
    }
}
