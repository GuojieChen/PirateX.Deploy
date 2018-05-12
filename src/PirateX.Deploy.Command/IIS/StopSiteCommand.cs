using Akka.Configuration.Hocon;
using Microsoft.Web.Administration;

namespace PirateX.Deploy.Command
{
    [CommandName("stop-site",Description ="停止站点")]
    public class StopSiteCommand:CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            string siteName = param.GetString();
            using (ServerManager serverManager = new ServerManager())
            {
                var sites = serverManager.Sites;
                var site = sites[siteName];
                if (site == null)
                    return $"site {siteName} not exist! can not stop!";
                if (site.State == ObjectState.Started)
                {
                    site.Stop();
                }
                else
                {
                    return $"site {siteName} status is {site.State.ToString()}!";
                }
                serverManager.CommitChanges();
            }
            return $"site {siteName} stop success!";
        }
    }
}
