using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PirateX.Deploy.Common
{
    public class CommandKeyAttribute:Attribute
    {
        public string Key { get; private set; }

        public string Default { get; set; }

        public string Description { get; set; }

        public bool IsRequired { get; set; }
    }
}
