using System;
using System.Threading;
using NLog;

namespace PirateX.Deploy.Client
{
    class Program
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private static CommandProcessor cp = null;

        //127.0.0.1 {secretkey} {template} {}
        static void Main(string[] args)
        {
            int port = 40001;
            string uri = $"127.0.0.1:{port}";

            string cmdFileName = string.Empty;
            string environmentFileName = string.Empty;
            string machineName = string.Empty;
            string secretkey = string.Empty;
            if (args.Length >= 1)
            {
                uri = args[0];
                if (uri.IndexOf(':') < 0)
                    uri += $":{port}";
            }

            if (args.Length >= 2)
            {
                secretkey = args[1];
            }
            if (args.Length >= 3)
            {
                cmdFileName = args[2];
            }
            if (args.Length >= 4)
            {
                environmentFileName = args[3];
            }
            if (args.Length >= 5)
            {
                machineName = args[4];
            }
            Log.Info("PirateX.Deploy.Client Started! Press Ctrl + C to exit");
            Console.CancelKeyPress += Console_CancelKeyPress;

            try
            {
                cp = new CommandProcessor($"ws://{uri}",secretkey);
                cp.Start(cmdFileName, environmentFileName, machineName);
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            cp.Stop();
        }
    }
}
