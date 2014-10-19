using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Configuration.Install;
using System.ServiceModel.Web;

namespace Niviane_Service
{
    public partial class Niviane_WindowsService : ServiceBase
    {
        public WebServiceHost serviceHost = null;

        public Niviane_WindowsService()
        {
            InitializeComponent();
        }

        public void Start()
        {
            this.OnStart(new string[0]);
        }
        protected override void OnStart(string[] args)
        {
            Log("Service", "Service is starting");
            if (serviceHost != null)
            {
                serviceHost.Close();
            }
            serviceHost = new WebServiceHost(typeof(Niviane));
            serviceHost.Open();
        }

        protected override void OnStop()
        {
            Log("Service", "Service is stopping");
            if (serviceHost != null)
            {
            
                serviceHost.Close();
                serviceHost = null;
            }
        }

        public static void Log(string type, string message)
        {
            string LogFilePath = Properties.Settings.Default.LogFilePath;
            string entry = String.Format("{0}\t{1}\t{2}", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), type, message);
            if (LogFilePath.Length > 0)
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(LogFilePath, true))
                {


                    sw.WriteLine(entry);
                }
            }
            Console.WriteLine(entry);

        }
    }


    // Installer related items
    // the service to be installed by the Installutil.exe tool
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private ServiceProcessInstaller process;
        private ServiceInstaller service;

        public ProjectInstaller()
        {
            process = new ServiceProcessInstaller();
            process.Account = ServiceAccount.LocalSystem;
            service = new ServiceInstaller();
            service.ServiceName = "Niviane";
            service.Description = "Manages various items throughout the home, such as Z-Wave and automation tasks";
            service.StartType = ServiceStartMode.Automatic;

            Installers.Add(process);
            Installers.Add(service);
            this.Committed += new InstallEventHandler(ServiceInstaller_Committed);
        }

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);

            string targetDirectory = Context.Parameters["targetdir"];
            string paramServiceAddress = Context.Parameters["Param1"];
            string paramLogFilePath = Context.Parameters["Param2"];
            string paramPathToVLC = Context.Parameters["Param3"];

            string paramZWaveConfigPath = Context.Parameters["Param4"];
            string paramZWaveSerialPort = Context.Parameters["Param5"];
            string paramZWavePollInterval = Context.Parameters["Param6"];


            // Make sure the baseAddress path ends with a slash
            if (paramServiceAddress.EndsWith("/") == false)
            {
                paramServiceAddress = String.Concat(paramServiceAddress, "/");
            }

            string path = System.IO.Path.Combine(targetDirectory, "Niviane_Service.exe.config");
            System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();

            xDoc.Load(path);

            System.Xml.XmlNode node;
            node = xDoc.SelectSingleNode("/configuration/system.serviceModel/services/service/host/baseAddresses");
            node.InnerXml = String.Concat("<add baseAddress=\"", paramServiceAddress, "\" />");
            node = xDoc.SelectSingleNode("/configuration/userSettings/Niviane_Service.Properties.Settings/setting[@name='LogFilePath']/value");
            node.InnerText = paramLogFilePath;
            node = xDoc.SelectSingleNode("/configuration/userSettings/Niviane_Service.Properties.Settings/setting[@name='VLCPathToExe']/value");
            node.InnerText = paramPathToVLC;

            node = xDoc.SelectSingleNode("/configuration/userSettings/Niviane_Service.Properties.Settings/setting[@name='ZWaveConfigPath']/value");
            node.InnerText = paramZWaveConfigPath;
            node = xDoc.SelectSingleNode("/configuration/userSettings/Niviane_Service.Properties.Settings/setting[@name='ZWaveSerialPort']/value");
            node.InnerText = paramZWaveSerialPort;
            node = xDoc.SelectSingleNode("/configuration/userSettings/Niviane_Service.Properties.Settings/setting[@name='ZWavePollInterval']/value");
            node.InnerText = paramZWavePollInterval;

            xDoc.Save(path); // saves the config file 
            
        }

        void ServiceInstaller_Committed(object sender, InstallEventArgs e)
        {
            // Auto Start the Service Once Installation is Finished.
            var controller = new ServiceController(service.ServiceName);
            controller.Start();
        }

    }
}
