using NLog;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using WebSocketSharp;

namespace PirateX.Deploy.Client
{
    public class CommandProcessor
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private int commandCount = 0;
        private int commandIndex = 0;
        private bool progressBarFlag = false;
        private ProgressBar progressBar = null;
        private WebSocket ws = null;
        private string[] commands = null;
        private string environment = string.Empty;
        private string machine;
        private string secretkey = string.Empty;
        public CommandProcessor(string url,string secretkey)
        {
            this.secretkey = secretkey;
            ws = new WebSocket(url);
            ws.OnMessage += Ws_OnMessage;
            ws.OnClose += Ws_OnClose;
            ws.OnError += Ws_OnError;
        }

        private void Ws_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Console.WriteLine("session error:{0}", e.Message);
        }

        private void Ws_OnClose(object sender, CloseEventArgs e)
        {
            Console.WriteLine("session closed!");
        }

        public void Start(string cmdFileName, string environmentFileName, string machineName)
        {
            try
            {
                string command = LoadFileContent(cmdFileName);
                commands = command.Split(new[] { "##=##" }, StringSplitOptions.RemoveEmptyEntries);
                commandCount = commands.Length;
                if (commandCount <= 0)
                {
                    Logger.Warn("No valid command!");
                    return;
                }

                environment = LoadFileContent(environmentFileName);
                machine = LoadFileContent(machineName);
                if (!ws.IsAlive)
                {
                    ws.Connect();
                }
                SendCommand();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            while (commandCount > 0)
            {
                Thread.Sleep(500);
            }
        }

        public void Stop()
        {
            commandCount = 0;
        }

        public string LoadFileContent(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return "";

            return File.ReadAllText(filePath);
        }

        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Data == "===command operation end===")
            {
                commandCount--;
                if (commandCount > 0)
                {
                    SendCommand();
                }
            }
            else if (e.Data == "===updating self===") //update-self命令需要客户端自己退出
            {
                Logger.Warn("Deploy Server is updating... \r\n Try again after a while.");
                commandCount = 0;
            }
            else if (e.Data == "$Prepare Progress Bar$")
            {
                progressBarFlag = true;
                progressBar = new ProgressBar();
            }
            else if (e.Data == "$Finish Progress Bar$")
            {
                progressBarFlag = false;
            }
            else
            {
                if (e.Data.StartsWith("error"))
                {
                    Logger.Fatal(e.Data);
                    commandCount = 0;
                }
                else if (progressBarFlag)
                {
                    if (commandCount > 0)
                    {
                        progressBar.Report(double.Parse(e.Data));
                    }
                    else
                    {
                        //此时为使用 Ctrl + C 主动退出
                        Console.Write("Stoping..." + new string('\b', 10));
                    }
                }
                else
                {
                    Logger.Debug(e.Data);
                }
            }
        }

        private void SendCommand()
        {
            try
            {
                var composeCmd = new ComposeCommand()
                {
                    Command = commands[commandIndex],
                    Environment = environment,
                    SpecificMachine = machine
                };
                string cmdStr = EncryptDES(Serialize(composeCmd),secretkey.Substring(0,8));

                ws.Send(cmdStr);
                commandIndex++;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// 进行DES加密
        /// </summary>
        /// <param name="inputByteArray">字节数组</param>
        /// <param name="key">密钥，必须为8位</param>
        /// <returns>以Base64格式返回的加密字符串</returns>
        static string EncryptDES(byte[] inputByteArray, string sKey)
        {
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider()) 
            {
                des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
                des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
                using (MemoryStream ms = new System.IO.MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                    }

                    string str = Convert.ToBase64String(ms.ToArray());
                    return str;
                }
            }
        }

        static byte[] Serialize<T>(T t)
        {
            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, t);

                return ms.ToArray();
            }
        }
    }
}
