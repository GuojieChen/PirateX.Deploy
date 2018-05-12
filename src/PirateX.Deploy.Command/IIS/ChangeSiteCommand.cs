using Akka.Configuration.Hocon;
using Microsoft.Web.Administration;
using System;

namespace PirateX.Deploy.Command
{
    [CommandName("change-site",Description ="修改IIS web站点")]
    public class ChangeSiteCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            var hobj = param as HoconObject;
            if (hobj == null)
            {
                throw new Exception("change-site command param error");
            }
            var siteName = hobj.GetKey("SiteName").GetString();
            var poolName = hobj.GetKey("PoolName").GetString();
            var hostName = hobj.GetKey("PoolName")?.GetString() ?? "";
            var path = hobj.GetKey("PhysicPath").GetString();
            var port = hobj.GetKey("Port")?.GetInt() ?? 80;
            var msg = ChangeWebsite(siteName, poolName, port, path, hostName);
            return msg;
        }
        private string ChangeWebsite(string websiteName, string appPool, int port, string phyPath, string hostname)
        {
            using (ServerManager iisManager = new ServerManager())
            {
                if (iisManager.Sites[websiteName] != null)
                    return $"website {websiteName} exist! need no change.";
                if (iisManager.ApplicationPools[appPool] == null)
                    throw new Exception($"pool {appPool} not exist! can not change website {websiteName}");
                var site = iisManager.Sites[websiteName];
                if (site == null)
                {
                    throw new Exception($"site {websiteName} not exist! can not change website {websiteName}");
                }
                string bindingInfo = "*:" + port + ":" + hostname;
                site.ApplicationDefaults.ApplicationPoolName = appPool;
                foreach (var item in site.Applications)
                {
                    item.ApplicationPoolName = appPool;
                }
                iisManager.CommitChanges();
                return $"website {websiteName} bind info {bindingInfo}  physic path {phyPath} set success!";
            }
        }
    }
}
