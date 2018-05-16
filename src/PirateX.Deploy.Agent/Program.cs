using System;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Sockets;
using Topshelf;
using ServiceStack.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PirateX.Deploy.Agent
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 40001;
            string workspace = string.Empty;

            HostFactory.Run(x =>
            {
                x.UseNLog();
                x.Service<AgentService>(s =>
                {
                    s.ConstructUsing(name => new AgentService(port,workspace) {  });
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());

                    x.AddCommandLineDefinition("port", v => port = Convert.ToInt32(v));
                    x.AddCommandLineDefinition("workspace", v => workspace = v);
                });

                x.RunAsLocalService();
            });

            Console.Read();
        }
    }
}
