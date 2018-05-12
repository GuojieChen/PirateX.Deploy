using Akka.Configuration.Hocon;
using Microsoft.Web.Administration;
using System.Threading;

namespace PirateX.Deploy.Command
{
    [CommandName("start-site",Description ="启动站点")]
    public class StartSiteCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            string siteName = param.GetString();
            using (ServerManager serverManager = new ServerManager())
            {
                var sites = serverManager.Sites;
                var site = sites[siteName];
                if (site == null)
                    return $"site {siteName} not exist! can not start";
                Thread.Sleep(200);//让site有准备的时间，sites为Lazy加载,否则有可能在site.State调用出错（not a valid object）
                if (site.State == ObjectState.Stopped)
                {
                    site.Start();
                }
                else
                {
                    return $"site {siteName} status is {site.State.ToString()}!";
                }
                serverManager.CommitChanges();
            }
            return $"site {siteName} start success!";
        }
    }
    
}
