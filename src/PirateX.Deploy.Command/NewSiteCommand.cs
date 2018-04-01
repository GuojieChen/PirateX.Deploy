using Akka.Configuration.Hocon;
using Microsoft.Web.Administration;
using System;

namespace PirateX.Deploy.Command
{
    [CommandName("new-site")]
    public class NewSiteCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            var hobj = param as HoconObject;
            if (hobj == null)
            {
                throw new Exception("new-site command param error");
            }
            var siteName = hobj.GetKey("SiteName").GetString();
            var poolName = hobj.GetKey("PoolName").GetString();
            var hostName = hobj.GetKey("HostName")?.GetString() ?? "";
            var path = hobj.GetKey("PhysicPath").GetString();
            var port = hobj.GetKey("Port")?.GetInt() ?? 80;
            var msg = CreateWebsite(siteName, poolName, port, path, hostName);
            return msg;
        }

        private string CreateWebsite(string websiteName, string appPool, int port, string phyPath, string hostname)
        {
            using (ServerManager iisManager = new ServerManager())
            {
                if (iisManager.Sites[websiteName] != null)
                    return $"website {websiteName} exist! need no create.";
                if (iisManager.ApplicationPools[appPool] == null)
                    throw new Exception($"pool {appPool} not exist! can not create website {websiteName}");
                string bindingInfo = "*:" + port + ":" + hostname;
                iisManager.Sites.Add(websiteName, "http", bindingInfo, phyPath);
                iisManager.Sites[websiteName].ApplicationDefaults.ApplicationPoolName = appPool;
                foreach (var item in iisManager.Sites[websiteName].Applications)
                {
                    item.ApplicationPoolName = appPool;
                }
                iisManager.CommitChanges();
                return $"website {websiteName} bind info {bindingInfo}  physic path {phyPath} set success!";
            }
        }
    }
}
