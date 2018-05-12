using Akka.Configuration.Hocon;
using Microsoft.Web.Administration;
using System;

namespace PirateX.Deploy.Command
{
    [CommandName("new-site",Description ="新建站点")]
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
            var binding  = hobj.GetKey("Binding").GetString(); // http://yourdoman.com:96
            var path = hobj.GetKey("PhysicPath").GetString();
            var msg = CreateWebsite(siteName, poolName, path, binding);
            return msg;
        }

        private string CreateWebsite(string websiteName, string appPool, string phyPath, string binding)
        {
            using (ServerManager iisManager = new ServerManager())
            {
                if (iisManager.Sites[websiteName] != null)
                    return $"website {websiteName} exist! need no create.";
                if (iisManager.ApplicationPools[appPool] == null)
                    throw new Exception($"pool {appPool} not exist! can not create website {websiteName}");

                string port = "80";
                string host = binding;
                var index = binding.IndexOf(':');
                if (index >= 0)
                {
                    port = binding.Substring(index + 1, binding.Length - index - 1);
                    if (index > 0)
                        host = binding.Substring(0, index);
                    else
                        host = "";
                }

                string bindingInfo = "*:" + port + ":" + host;
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
