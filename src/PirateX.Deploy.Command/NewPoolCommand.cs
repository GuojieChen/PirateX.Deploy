using Akka.Configuration.Hocon;
using Microsoft.Web.Administration;
using System;

namespace PirateX.Deploy.Command
{
    [CommandName("new-pool")]
    public class NewPoolCommand : CommandBase
    {
        public override string Execute(IHoconElement param)
        {
            var hobj = param as HoconObject;
            if (hobj == null)
            {
                throw new Exception("new-pool command param error");
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
            var msg = CreateAppPool(name, version, itype);
            return msg;
        }

        private string CreateAppPool(string poolname, string runtimeVersion, ProcessModelIdentityType type)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                var pools = serverManager.ApplicationPools;
                var pool = pools[poolname];
                if (pool != null)
                    return $"pool {poolname} exist! need no create.";
                ApplicationPool newPool = serverManager.ApplicationPools.Add(poolname);
                newPool.ManagedRuntimeVersion = runtimeVersion;
                newPool.Enable32BitAppOnWin64 = true;
                newPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
                newPool.ProcessModel.IdentityType = type;
                newPool.AutoStart = true;
                serverManager.CommitChanges();
            }
            return $"pool {poolname} create success!";
        }
    }
}
