using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using WCF_Test;
using System.ServiceModel.Description;

namespace WCF_Host
{
    class Program
    {
        static void Main(string[] args)
        {

            Uri baseAddress = new Uri("http://localhost:8000/GettingStarted/");

            ServiceHost selfHost = new ServiceHost(typeof(Calculator), baseAddress);

            try
            {
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
                selfHost.Abort();
            }
        }
    }
}
