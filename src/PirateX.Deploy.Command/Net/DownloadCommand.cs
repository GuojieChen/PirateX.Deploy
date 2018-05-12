using Akka.Configuration.Hocon;
using System;
using System.Net;

namespace PirateX.Deploy.Command
{
    [CommandName("file-download",Description ="下载文件")]
    public class DownloadCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            var hobj = param as HoconObject;
            if (hobj == null)
            {
                throw new Exception("download command param error");
            }

            var uri = hobj.GetKey("Uri").GetString();
            var savepath = hobj.GetKey("SavePath").GetString();
            var credential = hobj.GetKey("Credential")?.GetString();
            var userData = credential.Split(':');
            string name = userData[0];
            string password = userData[1];
            var nc = new NetworkCredential(name, password);
            var client = new WebClient { Credentials = nc };

            client.DownloadFile(uri, savepath);

            return $"download file -> {uri} ok!";
        }
    }
}
