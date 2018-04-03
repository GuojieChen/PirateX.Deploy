using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PirateX.Deploy.Agent
{
    [ProtoContract]
    public class ComposeCommand
    {
        /// <summary>
        /// 命令
        /// </summary>
        [ProtoMember(1)]
        public string Command { get; set; }
        /// <summary>
        /// 环境变量 JSON内容，KEY-VALUE的格式
        /// </summary>
        [ProtoMember(2)]
        public string Environment { get; set; }
        /// <summary>
        /// 机器变量 JSON内容，KEY-VALUE的格式
        /// </summary>
        [ProtoMember(3)]
        public string SpecificMachine { get; set; }
    }
}
