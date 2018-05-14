using Akka.Configuration.Hocon;
using Microsoft.Web.Administration;
using PirateX.Deploy.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PirateX.Deploy.Common.IIS
{
    [CommandName("site-newapplication", Description = "站点新增应用程序")]
    public class NewApplicationCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            var hobj = param as HoconObject;
            if (hobj == null)
                throw new Exception("site-newapplication command param error");

            var siteName = hobj.GetKey("SiteName").GetString();
            var application = $"/{hobj.GetKey("Application").GetString()}";
            var path = hobj.GetKey("PhysicPath").GetString();

            using (var iisManager = new ServerManager())
            {
                var site = iisManager.Sites[siteName];
                if(site == null)
                    throw new Exception($"site[{siteName}] not exists!");

                var app = site.Applications[application];

                if (app != null)
                    return $"application {application} exists!";

                site.Applications.Add(application,path);

                return $"application {application} create success!";
            }
        }
    }
}
