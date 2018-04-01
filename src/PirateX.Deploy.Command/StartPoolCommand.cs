using Akka.Configuration.Hocon;
using Microsoft.Web.Administration;

namespace PirateX.Deploy.Command
{
    [CommandName("start-pool")]
    public class StartPoolCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            string poolName = param.GetString();
            using (ServerManager serverManager = new ServerManager())
            {
                var pools = serverManager.ApplicationPools;
                var pool = pools[poolName];
                if (pool == null)
                    return $"pool {poolName} not exist! can not start";
                if (pool.State == ObjectState.Stopped)
                {
                    pool.Start();
                }
                else
                {
                    return $"pool {poolName} status is {pool.State.ToString()}!";
                }
                serverManager.CommitChanges();
            }
            return $"pool {poolName} start success!";
        }
    }
}
