namespace PirateX.Deploy.Command
{
    public class UserCommand
    {
        /// <summary>
        /// 程序类型：WindowsService，Application, Website
        /// </summary>
        public ApplicationType AppType { get; set; }

        /// <summary>
        /// 库名称
        /// </summary>
        public string FeedName { get; set; }

        /// <summary>
        /// 组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 程序包名称
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// 可执行程序名称
        /// </summary>
        public string ProgramName { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }


        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 要进行的操作：Install, InstallOrUpgrade
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// 配置是否保留
        /// </summary>
        public bool ConfigStay { get; set; }

        /// <summary>
        /// 解压目录 -- 若不指定则为默认
        /// </summary>
        public string ExtractPath { get; set; }

    }

    public enum ApplicationType
    {
        Service = 0,

        Application = 1,

        Website = 2
    }

    public enum OperationType
    {
        Install = 0,

        InstallOrUpgrade = 1,

        Upgrade = 2,

        Uninstall = 3,

        Start = 4,
        
        Stop = 5,

        Download = 6
    }
}
