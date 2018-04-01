using System;

namespace PirateX.Deploy.Command
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandNameAttribute : Attribute
    {
        public CommandNameAttribute(string name)
        {
            CommandName = name;
        }
        public string CommandName { get; set; }
    }
}
