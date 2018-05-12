using System;
using System.Diagnostics;
using System.Threading;
using SuperSocket.SocketBase;
using SuperWebSocket;
using log4net;
using log4net.Config;
using Topshelf.Logging;
using System.Collections.Generic;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PirateX.Deploy.Command;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using ProtoBuf;
using ServiceStack.Text;

namespace PirateX.Deploy.Agent
{
    public class AgentService
    {
        private WebSocketServer wss;

        private static readonly LogWriter logger = HostLogger.Get<AgentService>();

        private string secretkey = string.Empty;

        private string WorkSpace { get; set; }

        private int port;

        public AgentService(int port,string workspace)
        {
            this.port = port;
            this.WorkSpace = secretkey;
        }

        public void Start()
        {
            if (string.IsNullOrEmpty(secretkey))
                secretkey = Guid.NewGuid().ToString("N");

            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "secretkey.txt");
            if (File.Exists(file))
                secretkey = File.ReadAllText(file);
            else 
            {
                var bytes = Encoding.UTF8.GetBytes(secretkey);
                using (var fs = File.Create(file))
                    fs.Write(bytes, 0, bytes.Length);
            }

            logger.Info($"secretkey : {secretkey}");

            if (string.IsNullOrEmpty(WorkSpace))
                WorkSpace = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Apps");

            if (!Directory.Exists(WorkSpace))
                Directory.CreateDirectory(WorkSpace);

            logger.Info($"workspace : {WorkSpace}");

            CommandExecutor.WorkSpace = WorkSpace;

            wss = new WebSocketServer();
            wss.SessionClosed += SessionClosed;
            wss.NewSessionConnected += NewSessionConnected;
            wss.NewMessageReceived += NewMessageReceived;
            wss.Setup(port);
            logger.Info($"websocekt : ws://*:{port}");

            if (!wss.Start())
                throw new ApplicationException("start,exception ");
        }

        public void Stop()
        {
            var processId = Process.GetCurrentProcess().Id;
            var threadId = Thread.CurrentThread.ManagedThreadId;
            logger.Info($"stop deploy server, process: {processId} thread:{threadId}");
            wss?.Stop();
            wss?.Dispose();
        }

        private void NewMessageReceived(WebSocketSession session, string value)
        {
            byte[] orign = null;
            try
            {
                orign = DecryptDES(value, secretkey.Substring(0, 8));
            }
            catch
            {
                session.Close();
                return;
            }

            ProcessData(session, orign);
        }

        internal static Dictionary<string, string> EnvironmentConfig;
        internal static Dictionary<string, string> MachineConfig;

        private static bool isProcessing = false;
        private const string ComamndEnd = "===command operation end===";

        public static void ProcessData(WebSocketSession session, byte[] data)
        {
            if (isProcessing)//同时只能执行一次指令
            {
                session.Send("error: The Last compose command is processing! Please wait for a while!");
                session.Send(ComamndEnd);
                return;
            }
            isProcessing = true;
            try
            {
                var composeCmd = Deserialize<ComposeCommand>(data);
                string cmd = composeCmd.Command;
                string env = composeCmd.Environment;
                string machine = composeCmd.SpecificMachine;

                EnvironmentConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(env.Replace("\\","\\\\"))??new Dictionary<string, string>();
                MachineConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(machine.Replace("\\", "\\\\")) ?? new Dictionary<string, string>();

                //TODO 需要达到机器参数优先，环境参数其次
                cmd = ReplaceVarialbe(cmd, EnvironmentConfig, MachineConfig);

                ProcessHoconCommand(cmd, session);
            }
            catch (Exception ex)
            {
                session.Send($"error before command: {ex.Message}\r\n{ex.StackTrace}");
            }
            finally
            {
                isProcessing = false;
                MachineConfig = null;
                EnvironmentConfig = null;
                session.Send(ComamndEnd);
            }
        }

        private static string ReplaceVarialbe(string cmd, Dictionary<string, string> envDict, Dictionary<string, string> machineDict)
        {
            if (cmd.Contains("${"))
            {
                foreach (var kv in machineDict)
                {
                    cmd = cmd.Replace(kv.Key, kv.Value);
                }
            }

            if (cmd.Contains("${"))
            {
                foreach (var kv in envDict)
                {
                    cmd = cmd.Replace(kv.Key, kv.Value);
                }
            }
            return cmd;
        }

        #region HOCON

        private static void ProcessHoconCommand(string cmd, WebSocketSession session)
        {
            var config = ConfigurationFactory.ParseString(cmd, IncludeAttach);
            var root = config.Root.Values[0] as HoconObject;

            if (root != null)
            {
                var commandTypes = (from type in typeof(CommandBase).Assembly.GetTypes()
                                    where typeof(CommandBase).IsAssignableFrom(type) && !type.IsAbstract
                                    select type).ToArray();
                foreach (var hitem in root.Items)
                {
                    string key = hitem.Key;
                    if (key == "var") //跳过变量定义
                        continue;
                    var v = hitem.Value.Values[0];
                    try
                    {
                        session.Send($">>>>>>>>>>>>>>>>processing [{key}] >>>>>>>>>>>>>>>>");
                        string response = DispatchHoconCommand(session, commandTypes, key, v);
                        if (session.Connected)
                        {
                            //SendResponse(session, "CommandResponse", response);
                            session.Send(response);
                            session.Send("------------------------------------");
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = GenerateErrorMessage(key, ex);
                        session.Send(msg);
                        //SendResponse(session, key, ex);
                        break;
                    }
                }
            }
        }

        private static string GenerateErrorMessage(string key, Exception ex)
        {
            string msg = $"error [{key}]: ";
            var innerEx = ex;
            while (innerEx != null)
            {
                msg += $"{innerEx.Message}\r\n{ex.StackTrace}\r\n";
                innerEx = innerEx.InnerException;
            }
            return msg;
        }

        private static HoconRoot IncludeAttach(string key)
        {
            if (key == "system")
            {
                var sysConfig = ConfigurationFactory.Load();
                var root = sysConfig.Root;
                var sub = sysConfig.Substitutions;
                return new HoconRoot(root, sub);
            }
            return null;
        }

        private static string DispatchHoconCommand(WebSocketSession session, Type[] commandTypes, string name, IHoconElement param)
        {
            foreach (var commandType in commandTypes)
            {
                var attr = Attribute.GetCustomAttribute(commandType, typeof(CommandNameAttribute));
                var ca = (CommandNameAttribute)attr;
                if (ca.CommandName == name)
                {
                    var cmd = (ICommand)Activator.CreateInstance(commandType);
                    cmd.Session = session;
                    cmd.EnvironmentConfig = EnvironmentConfig;
                    cmd.MachineConfig = MachineConfig;
                    return cmd.Execute(param);
                }
            }
            throw new Exception($"deploy server do not support command {name}");
        }

        #endregion

        private void NewSessionConnected(WebSocketSession session)
        {
            session.Send($"PirateX.Deploy.Agent Version {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
            logger.Debug($"session connected! {session.RemoteEndPoint}\t{session.SessionID}");
        }

        private void SessionClosed(WebSocketSession session, CloseReason value)
        {
            logger.Debug($"session closed! {session.RemoteEndPoint}\t{session.SessionID}");
        }

        static byte[] DecryptDES(string pToDecrypt, string sKey)
        {
            byte[] inputByteArray = Convert.FromBase64String(pToDecrypt);
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
                des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                    }
                    return ms.ToArray();
                }
            }
        }

        private static T Deserialize<T>(byte[] input)
        {
            using (var ms = new MemoryStream(input))
            {
                return Serializer.Deserialize<T>(ms);
            }
        }
    }
}
