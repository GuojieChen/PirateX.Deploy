using System;
using System.Collections.Generic;
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
            var options = new Options(args);

            string uri = $"{options.Host}:{options.Port}";
            
            Log.Info("PirateX.Deploy.Client Started! Press Ctrl + C to exit");
            Console.CancelKeyPress += Console_CancelKeyPress;

            try
            {
                cp = new CommandProcessor(options);
                cp.Start();
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
