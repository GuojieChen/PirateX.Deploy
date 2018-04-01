using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Configuration.Hocon;
using Microsoft.Web.Administration;

namespace PirateX.Deploy.Command
{
    [CommandName("stop-pool")]
    public class StopPoolCommand:CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            string poolName = param.GetString();
            using (ServerManager serverManager = new ServerManager())
            {
                var pools = serverManager.ApplicationPools;
                var pool = pools[poolName];
                if (pool == null)
                    return $"pool {poolName} not exist! can not stop";
                if (pool.State == ObjectState.Started)
                {
                    pool.Stop();
                }
                else
                {
                    return $"pool {poolName} status is {pool.State.ToString()}!";
                }
                serverManager.CommitChanges();
            }
            return $"pool {poolName} stop success!";
        }
    }
}
