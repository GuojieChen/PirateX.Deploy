using Akka.Configuration.Hocon;
using PirateX.Deploy.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PirateX.Deploy.Common
{
    [CommandName("copy-file",Description ="拷贝文件")]
    public class ReNameFIleCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            var hobj = param as HoconObject;
            if (hobj == null)
            {
                throw new Exception("extract command param error");
            }

            var source = hobj.GetKey("Source").GetString();
            var destination = hobj.GetKey("Destination").GetString();

            var overwritekey = hobj.GetKey("OverWrite");

            var overwrite = overwritekey == null ? true : overwritekey.GetBoolean();

            if (!File.Exists(source))
                return $"source file not exists -> {source}";

            File.Copy(source, destination, true);

            return $"copy file ,from {source} to {destination}";
        }
    }
}
