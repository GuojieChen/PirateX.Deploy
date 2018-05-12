using Akka.Configuration.Hocon;
using Microsoft.Web.Administration;

namespace PirateX.Deploy.Command
{
    [CommandName("delete-site",Description ="删除站点")]
    public class DeleteSiteCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            string siteName = param.GetString();
            using (ServerManager serverManager = new ServerManager())
            {
                var sites = serverManager.Sites;
                var site = sites[siteName];
                if (site == null)
                    return $"site {siteName} not exist! can not delete!";
                sites.Remove(site);
                serverManager.CommitChanges();
            }
            return $"site {siteName} stop success!";
        }
    }
}
