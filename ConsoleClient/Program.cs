using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceReference1.Niviane niviane = new ServiceReference1.NivianeClient();

            // Wait to make sure the network has gone through discovery, and can accept commands
            while (niviane.IsAvailable() == false)
            {
                System.Threading.Thread.Sleep(2000);

            }


            Console.WriteLine("Here are the network nodes");
            foreach (ServiceReference1.NodeDetail node in niviane.Nodes()) { 
                Console.WriteLine(String.Format("{0}\t{1}\t{2}", 
                        node.Type,
                        node.Name,
                        node.Location
                        )
                    );
            }
            Console.WriteLine("");

            Console.WriteLine("Turning all nodes on");
            niviane.AllOn();

            Console.WriteLine("Press any key to turn them off");
            Console.ReadKey();

            Console.WriteLine("Turning all nodes off");
            niviane.AllOff();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
