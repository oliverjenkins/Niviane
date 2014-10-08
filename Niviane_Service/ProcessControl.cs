using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Niviane_Service
{
    class ProcessControl
    {
        private Process process = null;
        static private string logPath = "";

        public ProcessControl(string LogPath)
        {
            logPath = LogPath;
        }

        ~ProcessControl()
        {
            Log("Stopping ProcessController");
            StopProcess();
        }

        public Boolean IsProcessRunning()
        {
            return (process != null);
        }
        public string CurrentProcessName()
        {
            if (IsProcessRunning())
            {
                return process.StartInfo.FileName;
            }
            else
            {
                return "";
            }
        }

        public string CurrentProcessArguments()
        {
            if (IsProcessRunning())
            {
                return process.StartInfo.Arguments;
            }
            else
            {
                return "";
            }
        }

        public void StartProcess(string path, string arguments)
        {
            StopProcess();
            process = new Process();
            process.StartInfo.FileName = path;
            process.StartInfo.Arguments = arguments;
            Log("Starting " + path);
            process.Start();
        }

        public void StopProcess()
        {
            if (IsProcessRunning())
            {
                Log("Stopping process: " + process.StartInfo.FileName);
                process.Kill();
                process = null; 
            }
        }


        public void Log(string message)
        {
            Log("Process", message);
        }

        /// <summary>
        /// Simple logging method to keep track of actions
        /// </summary>
        /// <param name="type">Definition of the log enry e.g. Message, Error</param>
        /// <param name="message">The message to log</param>
        public void Log(string type, string message)
        {

            string entry = String.Format("{0}\t{1}\t{2}", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), type, message);
            if (logPath.Length > 0)
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(logPath, true))
                {
                    sw.WriteLine(entry);
                }
            }
            Console.WriteLine(entry);

        }
    }
}
