using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using WCF_Test;
using System.ServiceModel.Description;
using AppHarbor;
using NLog;

namespace WCF_Host
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            ConsoleMirror.Initialize();
            Console.WriteLine("AppHarbor background workers rock! First");
            logger.Info(ConsoleMirror.Captured);

            Uri baseAddress = new Uri("http://backgroundworkertest.apphb.com:8000/GettingStarted/");

            ServiceHost selfHost = new ServiceHost(typeof(Calculator), baseAddress);

            try
            {
                Console.WriteLine("AppHarbor background workers rock!");
                logger.Info(ConsoleMirror.Captured);

                // Step 3 Add a service endpoint.
                selfHost.AddServiceEndpoint(typeof(ICalculator), new WSHttpBinding(), "Calculator");
                
                // Step 4 Enable metadata exchange.
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                selfHost.Description.Behaviors.Add(smb);

                // Step 5 Start the service.
                selfHost.Open();
                Console.WriteLine("The Service is ready");
                Console.ReadLine();

                selfHost.Close();

            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("An exception occurred: {0}", ce.Message);
                logger.Error(ConsoleMirror.Captured);

                selfHost.Abort();
            }
        }
    }
}
