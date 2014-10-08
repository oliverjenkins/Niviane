using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenZWaveDotNet;
using System.ComponentModel;

namespace Niviane_Service
{
    class ZWave
    {

        static private ZWOptions m_options = null;
        static private ZWManager m_manager = null;
        static private Boolean m_nodesReady = false;
        static private UInt32 m_homeId = 0;
        static private BindingList<ZWaveNode> m_nodeList = new BindingList<ZWaveNode>();

        static private string logPath;

        public ZWave(string ZWaveConfigPath, string ZWaveSerialPort, string LogPath)
        {
            Initialise(ZWaveConfigPath, ZWaveSerialPort,0,LogPath);
        }

        public ZWave(string ZWaveConfigPath, string ZWaveSerialPort, int ZWavePollInterval, string LogPath)
        {
            Initialise(ZWaveConfigPath, ZWaveSerialPort, ZWavePollInterval, LogPath);

        }

        public void Initialise(string ZWaveConfigPath, string ZWaveSerialPort, int ZWavePollInterval, string LogPath)
        {
            logPath = LogPath;

            // Configuration details
            ZWaveConfigPath = BuildConfigPath(ZWaveConfigPath);
            System.IO.FileInfo logDirectory = new System.IO.FileInfo(ZWaveConfigPath);

            // Create the Options
            Log("Building ZWOptions with " + ZWaveConfigPath);
            m_options = new ZWOptions();
            m_options.Create(ZWaveConfigPath, @"", @"");

            // Lock the options
            m_options.Lock();

            // Create the OpenZWave Manager
            m_manager = new ZWManager();
            m_manager.Create();

            // Add an event handler for all the Z-Wave notifications
            m_manager.OnNotification += new ManagedNotificationsHandler(NotificationHandler);
            if (ZWavePollInterval  > 0)
            {
                m_manager.SetPollInterval(ZWavePollInterval);
            }

            // Add a driver, this will start up the z-wave network
            m_manager.AddDriver(ZWaveSerialPort);
        }

        public int NumberOfNodesInNetwork
        {
            get
            {
                return m_nodeList.Count;
            }
        }

        public Boolean IsNetworkReady
        {
            get
            {
                return m_nodesReady;
            }
        }

        public List<byte> ListOfNodeIDs()
        {
            List<byte> nodeListID = new List<byte>();
            foreach (ZWaveNode n in m_nodeList)
            {
                nodeListID.Add(n.ID);
            }

            return nodeListID;
        }


        public void EnablePoll(byte NodeID, string ValueLabel)
        {
            ZWValueID id = GetValueID(m_nodeList[3], ValueLabel);
            if (id != null)
            {
                Log("Message", "Enabled poll on " + id.GetId().ToString());
                m_manager.EnablePoll(id);
            }

        }

        public void DisablePoll(byte NodeID, String ValueLabel)
        {
            ZWValueID id = GetValueID(m_nodeList[3], ValueLabel);
            if (id != null)
            {
                Log("Message", "Enabled poll on " + id.GetId().ToString());
                m_manager.EnablePoll(id);
            }

        }

        public string NodeType(byte NodeID)
        {
            return m_manager.GetNodeType(m_homeId, NodeID).ToString();
        }

        public string NodeLocation(byte NodeID)
        {
            return m_manager.GetNodeLocation(m_homeId, NodeID).ToString();
        }

        public string NodeName(byte NodeID)
        {
            return m_manager.GetNodeName(m_homeId, NodeID).ToString();
        }

        public byte NodeLevel(byte NodeID)
        {
            byte valueByte = 0;
            ZWValueID valueID = GetValueID(NodeID, "Level");
            if (valueID == null)
            {
                return 0;
            }

            if (m_manager.GetValueAsByte(valueID, out valueByte))
            {
                Log(String.Format("Got level value for node {0}, and it's {1}", NodeID, valueByte));
            }

            return valueByte;
        }


        public byte NodeBasic(byte NodeID)
        {
            byte valueByte = 0;
            ZWValueID valueID = GetValueID(NodeID, "Basic");
            if (valueID == null)
            {
                return 0;
            }


            m_manager.GetNodeBasic(m_homeId, NodeID);

            if (m_manager.GetValueAsByte(valueID, out valueByte))
            {
                Log(String.Format("Got basic value for node {0}, and it's {1}", NodeID, valueByte));
            }

            return valueByte;

        }



        public void SetNodeBasic(byte NodeID, byte Value)
        {
            switch (NodeType(NodeID))
            {
                case "Binary Power Switch":
                    if (Value >= 125)
                    {
                        Value = 255;
                        m_manager.SetNodeOn(m_homeId, NodeID);
                        Log("Turning on binary power switch " + NodeID.ToString());
                    }
                    else
                    {
                        Value = 0;
                        m_manager.SetNodeOff(m_homeId, NodeID);
                        Log("Turning off binary power switch " + NodeID.ToString());
                    }
                    break;
                case "Multilevel Switch":
                    if (Value > 0)
                    {
                        m_manager.SetNodeLevel(m_homeId, NodeID, Value);
                        m_manager.SetNodeOn(m_homeId, NodeID);
                        Log("Setting level for Multilevel Switch " + NodeID.ToString() + " to " + Value.ToString());
                    }
                    else
                    {
                        m_manager.SetNodeOff(m_homeId, NodeID);
                        Log("Turning off Multilevel Switch " + NodeID.ToString());
                    }
                    break;

                case "Multilevel Power Switch":
                    if (Value > 0)
                    {
                        m_manager.SetNodeLevel(m_homeId, NodeID, Value);
                        m_manager.SetNodeOn(m_homeId, NodeID);
                        Log("Setting level for Multilevel power switch " + NodeID.ToString() + " to " + Value.ToString());
                    }
                    else
                    {
                        m_manager.SetNodeOff(m_homeId, NodeID);
                        Log("Turning off Multilevel Power Switch " + NodeID.ToString());
                    }
                    break;
                default:
                    if (NodeType(NodeID).Length == 0)
                    {
                        Log("Error", "Cannot set basic for node " + NodeID.ToString() + ", since node type is missing.");
                        Log("Error", "The path to the config directory is incorrect, or the directory does not have the needed details");
                    }
                    else
                    {
                        Log("Error", "Cannot set basic for node " + NodeType(NodeID));
                    }
                    break;
            }
            // Update the controller state to reflect this value
            SetValueID(NodeID, "Basic", Value);
        }


        public void SetNodeName(byte NodeID, string Name)
        {
            if (Name.Length > 16)
            {
                Name = Name.Substring(0, 16);
            }
            m_manager.SetNodeName(m_homeId, NodeID, Name);
            m_manager.WriteConfig(m_homeId);
        }

        public void SetNodeLocation(byte NodeID, string Location)
        {
            if (Location.Length > 16)
            {
                Location = Location.Substring(0, 16);
            }
            m_manager.SetNodeLocation(m_homeId, NodeID, Location);
            m_manager.WriteConfig(m_homeId);
        }

        public void AllOn()
        {
            m_manager.SwitchAllOn(m_homeId);
        }

        public void AllOff()
        {
            m_manager.SwitchAllOff(m_homeId);
        }

        public ZWaveNode Node(Byte nodeId)
        {
            return GetNode(m_homeId, nodeId);
        }

        public static void Log(string message)
        {
            Log("Message", message);
        }

        /// <summary>
        /// Simple logging method to keep track of actions
        /// </summary>
        /// <param name="type">Definition of the log enry e.g. Message, Error</param>
        /// <param name="message">The message to log</param>
        public static void Log(string type, string message)
        {
            try
            {
                string entry = String.Format("{0}\t{1}\t{2}", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), type, message);
                if (logPath.Length > 0)
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(logPath, true))
                    {
                        sw.WriteLine(entry);
                    }
                }
            }
            catch (Exception ex) { }

        }

        /// <summary>
        /// Method which handles the events raised by the ZWave network
        /// </summary>
        /// <param name="m_notification"></param>
        private void NotificationHandler(ZWNotification m_notification)
        {
            if (m_notification == null) { return; }

            switch (m_notification.GetType())
            {
                case ZWNotification.Type.ValueAdded:
                    {
                        ZWaveNode node = GetNode(m_notification.GetHomeId(), m_notification.GetNodeId());
                        if (node != null)
                        {
                            node.AddValue(m_notification.GetValueID());
                            Log("Event", "Node ValueAdded, " + m_manager.GetValueLabel(m_notification.GetValueID()));


                        }
                        break;
                    }

                case ZWNotification.Type.ValueRemoved:
                    {
                        ZWaveNode node = GetNode(m_notification.GetHomeId(), m_notification.GetNodeId());
                        if (node != null)
                        {
                            node.RemoveValue(m_notification.GetValueID());
                            Log("Event", "Node ValueRemoved, " + m_manager.GetValueLabel(m_notification.GetValueID()));
                        }
                        break;
                    }

                case ZWNotification.Type.ValueChanged:
                    {
                        ZWaveNode node = GetNode(m_notification.GetHomeId(), m_notification.GetNodeId());
                        if (node != null)
                        {
                            node.SetValue(m_notification.GetValueID());

                            Log("Event", "Node ValueChanged, " + m_manager.GetValueLabel(m_notification.GetValueID()).ToString());


                        }

                        break;
                    }

                case ZWNotification.Type.Group:
                    {
                        break;
                    }

                case ZWNotification.Type.NodeAdded:
                    {
                        // Add the new node to our list
                        ZWaveNode node = new ZWaveNode();
                        node.ID = m_notification.GetNodeId();
                        node.HomeID = m_notification.GetHomeId();
                        m_nodeList.Add(node);
                        Log("Event", "Node added, " + m_notification.GetNodeId().ToString());
                        break;
                    }

                case ZWNotification.Type.NodeRemoved:
                    {
                        foreach (ZWaveNode node in m_nodeList)
                        {
                            if (node.ID == m_notification.GetNodeId())
                            {
                                Log("Event", "Node removed, " + m_notification.GetNodeId().ToString());
                                m_nodeList.Remove(node);
                                break;
                            }
                        }
                        break;
                    }

                case ZWNotification.Type.NodeProtocolInfo:
                    {
                        ZWaveNode node = GetNode(m_notification.GetHomeId(), m_notification.GetNodeId());
                        if (node != null)
                        {

                            node.Label = m_manager.GetNodeType(m_homeId, node.ID);
                            Log("Event", "Node protocol info, " + node.Label.ToString());
                        }
                        break;
                    }

                case ZWNotification.Type.NodeNaming:
                    {
                        ZWaveNode node = GetNode(m_notification.GetHomeId(), m_notification.GetNodeId());
                        if (node != null)
                        {
                            node.Manufacturer = m_manager.GetNodeManufacturerName(m_homeId, node.ID);
                            node.Product = m_manager.GetNodeProductName(m_homeId, node.ID);
                            node.Location = m_manager.GetNodeLocation(m_homeId, node.ID);
                            node.Name = m_manager.GetNodeName(m_homeId, node.ID);
                        }
                        break;
                    }

                case ZWNotification.Type.NodeEvent:
                    {
                        Log("Event", "Node event");
                        break;
                    }

                case ZWNotification.Type.PollingDisabled:
                    {
                        Log("Event", "Polling disabled notification");
                        break;
                    }

                case ZWNotification.Type.PollingEnabled:
                    {
                        Log("Event", "Polling disabled notification");
                        break;
                    }

                case ZWNotification.Type.DriverReady:
                    {
                        m_homeId = m_notification.GetHomeId();
                        Log("Event", "Driver ready, with homeId " + m_homeId.ToString());
                        break;
                    }
                case ZWNotification.Type.NodeQueriesComplete:
                    {
                        break;
                    }
                case ZWNotification.Type.AllNodesQueried:
                    {
                        Log("Event", "All nodes queried");
                        m_nodesReady = true;
                        break;
                    }
                case ZWNotification.Type.AwakeNodesQueried:
                    {
                        Log("Event", "Awake nodes queried (but some sleeping nodes have not been queried)");
                        break;
                    }
            }

        }

        /// <summary>
        /// Gets a node based on the homeId and the nodeId
        /// </summary>
        /// <param name="homeId"></param>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        private ZWaveNode GetNode(UInt32 homeId, Byte nodeId)
        {
            foreach (ZWaveNode node in m_nodeList)
            {
                if ((node.ID == nodeId) && (node.HomeID == homeId))
                {
                    return node;
                }
            }

            return null;
        }
        private ZWValueID GetValueID(byte NodeID, string valueLabel)
        {
            return GetValueID(GetNode(m_homeId, NodeID), valueLabel);
        }


        private ZWValueID GetValueID(ZWaveNode node, string valueLabel)
        {
            foreach (ZWValueID valueID in node.Values)
            {
                if (m_manager.GetValueLabel(valueID) == valueLabel)
                {
                    return valueID;
                }
            }
            return null;
        }

        private void SetValueID(byte NodeID, string valueLabel, byte value)
        {
            SetValueID(GetNode(m_homeId, NodeID), valueLabel, value);
        }

        private void SetValueID(ZWaveNode node, string valueLabel, byte value)
        {
            try
            {
                foreach (ZWValueID valueID in node.Values)
                {
                    if (m_manager.GetValueLabel(valueID) == valueLabel)
                    {
                        m_manager.SetValue(valueID, value);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Error", ex.Message);
            }

        }

        private string BuildConfigPath(string configPath)
        {
            Log("Config path given was " + configPath);

            // If the config specifies a relative path, so get the full path
            if (System.IO.Path.IsPathRooted(configPath) == false)
            {
                // Get a reference to the location of the exe, note PAth.GetFull path will be to system32
                string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);

                configPath = System.IO.Path.Combine(
                    fileInfo.DirectoryName,
                    configPath
                    );
                Log("Config path was relative, will now assume its " + configPath);
            }

            if (configPath.EndsWith(@"\") == false)
            {
                // Open Z-Wave needs a config path that ends with \
                configPath = String.Concat(configPath, @"\");
            }

            if (System.IO.Directory.Exists(configPath) == false)
            {
                Log("Error", "Config path does not exist");
            }

            return configPath;
        }
    }
}
