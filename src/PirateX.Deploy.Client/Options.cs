using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PirateX.Deploy.Client
{
    public class Options
    {
        /// <summary>
        /// 主机
        /// </summary>
        public string Host { get; set; } = "127.0.0.1";
        /// <summary>
        /// 端口
        /// </summary>
        public string Port { get; set; } = "40001";
        /// <summary>
        /// 密钥
        /// </summary>
        public string SecretKey { get; set; } = "00000000000000";
        /// <summary>
        /// 环境配置
        /// </summary>
        public string EFile { get; set; }
        /// <summary>
        /// 机器配置
        /// </summary>
        public string MFile { get; set; }
        /// <summary>
        /// 内置命令
        /// </summary>
        public string CMD { get; set; }
        /// <summary>
        /// 命令文件
        /// </summary>
        public string InputFile { get; set; }

        public Options(string[] args)
        {
            for (var i = 0; i < args.Length; i = i + 2)
            {
                if (i >= args.Length - 1)
                    break;

                var key = args[i];
                var v = args[i + 1];

                switch (key)
                {
                    case "-host":
                    case "-h":
                        Host = v;
                        break;
                    case "-port":
                    case "-p":
                        Port = v;
                        break;
                    case "-secretkey":
                    case "-s":
                        SecretKey = v;
                        break;
                    case "-efile":
                    case "-e":
                        EFile = v;
                        break;
                    case "-mfile":
                    case "-m":
                        MFile = v;
                        break;
                    case "-cmd":
                    case "-c":
                        CMD = v;
                        break;
                    case "-inputfile":
                    case "-i":
                        InputFile = v;
                        break;
                }
            }
        }
    }
}
