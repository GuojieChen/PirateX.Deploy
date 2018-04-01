using Akka.Configuration;
using Akka.Configuration.Hocon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace PirateX.Deploy.Command
{
    public static class CommandExecutor
    {
        public static string WorkSpace { get; set; }

        public static void RunCommand(OriginCommand cmd)
        {
            var cmdd = cmd;
            do
            {
                RunExeWithParam(cmdd.Name, cmdd.Param);
                cmdd = cmdd.SubCommand;
            } while (cmdd != null);
        }

        public static void RunExeWithParam(string exe, string para)
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(exe + " ", para)
            {
                ErrorDialog = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
            };
            process.Start();
            if (!process.HasExited)
            {
                process.WaitForExit(30000);
            }
            if (!process.HasExited)
            {
                process.Kill();
            }
            int exitCode = process.ExitCode;
            if (exitCode != 0)
            {
                throw new Exception($"run command:{exe} {para} \r\n exit code {exitCode}");
            }
        }

        public static void RunExeWithParam(WebSocketSession session, OriginCommand cmd)
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(cmd.Name + " ", cmd.Param)
            {
                ErrorDialog = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
            };
            process.Start();

            session.Send($"processname:{cmd.Name}");
            session.Send($"args:{cmd.Param}");

            while (!process.StandardOutput.EndOfStream)
            {
                string line = process.StandardOutput.ReadLine();
                session.Send(line);
            }

            if (!process.HasExited)
            {
                process.WaitForExit(30000);
            }
            if (!process.HasExited)
            {
                process.Kill();
            }
            int exitCode = process.ExitCode;
            if (exitCode != 0)
            {
                throw new Exception($"run command:{cmd.Name} {cmd.Param} \r\n exit code {exitCode}");
            }
        }
    }
}
