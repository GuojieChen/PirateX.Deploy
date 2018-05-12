using Akka.Configuration.Hocon;
using Microsoft.Web.Administration;

namespace PirateX.Deploy.Command
{
    [CommandName("delete-pool",Description ="删除应用程序池")]
    public class DeletePoolCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            string poolName = param.GetString();
            using (ServerManager serverManager = new ServerManager())
            {
                var pools = serverManager.ApplicationPools;

                var pool = pools[poolName];
                if (pool == null)
                    return $"pool {poolName} not exist! can not delete";
                pools.Remove(pool);
                serverManager.CommitChanges();
            }
            return $"pool {poolName} delete success!";
        }
    }
}
