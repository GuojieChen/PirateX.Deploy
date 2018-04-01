namespace PirateX.Deploy.Command
{
    public class QueryCommand
    {
        public ApplicationType AppType { get; set; }

        public string Name { get; set; }

    }

    public class QueryResponse
    {
        /// <summary>
        /// 程序类型
        /// </summary>
        public ApplicationType AppType { get; set; }

        /// <summary>
        /// 程序名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 程序当前的版本号
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 程序的文件路径
        /// </summary>
        public string FilePath { get; set; }


        /// <summary>
        /// 状态-（未安装，正在运行，已停止）
        /// </summary>
        public string Status { get; set; }
    }
}
