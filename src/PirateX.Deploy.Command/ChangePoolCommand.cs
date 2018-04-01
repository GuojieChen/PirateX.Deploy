using Akka.Configuration.Hocon;
using Microsoft.Web.Administration;
using System;

namespace PirateX.Deploy.Command
{
    [CommandName("change-pool")]
    public class ChangePoolCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            var hobj = param as HoconObject;
            if (hobj == null)
            {
                throw new Exception("change-pool command param error");
            }
            var name = hobj.GetKey("Name").GetString();
            var identityType = hobj.GetKey("IdentityType")?.GetString() ?? "NetworkService";
            var version = hobj.GetKey("RunTimeVersion")?.GetString() ?? "v4.0";
            ProcessModelIdentityType itype = ProcessModelIdentityType.NetworkService;
            switch (identityType)
            {
                case "NetworkService":
                    itype = ProcessModelIdentityType.NetworkService;
                    break;
                case "LocalService":
                    itype = ProcessModelIdentityType.LocalService;
                    break;
                case "LocalSystem":
                    itype = ProcessModelIdentityType.LocalSystem;
                    break;
                case "ApplicationPoolIdentity":
                    itype = ProcessModelIdentityType.ApplicationPoolIdentity;
                    break;
                case "SpecificUser":
                    itype = ProcessModelIdentityType.SpecificUser;
                    break;
                default:
                    throw new Exception($"IdentityType {identityType} set error");
            }
            var msg = ChangeAppPool(name, version, itype);
            return msg;
        }
        private string ChangeAppPool(string poolname, string runtimeVersion, ProcessModelIdentityType type)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                var pools = serverManager.ApplicationPools;
                var pool = pools[poolname];
                if (pool == null)
                    return $"pool {poolname} not exist! can not change.";
                pool.Stop();
                pool.ManagedRuntimeVersion = runtimeVersion;
                pool.Enable32BitAppOnWin64 = true;
                pool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
                pool.ProcessModel.IdentityType = type;
                pool.AutoStart = true;
                pool.Start();
                serverManager.CommitChanges();
            }
            return $"pool {poolname} change success!";
        }
    }
}
