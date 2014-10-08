using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Runtime.Serialization;

namespace Niviane_Service
{

    [DataContract]
    public class Item
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string StringValue { get; set; }
    }

    [DataContract]
    public class ProcessRunning
    {   
        [DataMember]
        public Boolean IsRunning { get; set; }
        [DataMember]
        public string Process { get; set; }
        [DataMember]
        public string Arguments { get; set; }
    }

    [DataContract]
    public class NodeDetail
    {
        [DataMember]
        public byte NodeID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public byte Level { get; set; }
        [DataMember]
        public byte Basic { get; set; }

        public NodeDetail() { }
        public NodeDetail(byte NodeID, string Name, string Type, string Location, byte Level, byte Basic)
        {
            this.NodeID = NodeID;
            this.Name = Name;
            this.Type = Type;
            this.Location = Location;
            this.Level = Level;
            this.Basic = Basic;
        }
    }

    [ServiceContract(Name = "Niviane", Namespace = "http://niviane/", SessionMode = SessionMode.NotAllowed)]
    public interface INiviane
    {
        // Methods to get information about the state of the network
        [OperationContract(Name = "IsAvailable")]
        [WebGet(UriTemplate = "isavailable")]
        Boolean IsAvailable();

        [OperationContract(Name = "Nodes")]
        [WebGet(UriTemplate = "nodes")]
        List<NodeDetail> Nodes();

        [OperationContract(Name="Node")]
        [WebGet(UriTemplate = "node/{NodeID}")]
        NodeDetail Node(string NodeID);


        // Methods to set various items throughout the network
        [OperationContract(Name="NodeBasic")]
        [WebGet(UriTemplate = "node/{NodeID}/basic/{Value}")]
         Boolean NodeBasic(string NodeID, string Value);

        [OperationContract(Name = "NodePollingOn")]
        [WebGet(UriTemplate = "node/{NodeID}/pollingon/{Value}")]
        Boolean NodePollingOn(string NodeID, string Value);

        [OperationContract(Name = "NodePollingOff")]
        [WebGet(UriTemplate = "node/{NodeID}/pollingoff/{Value}")]
        Boolean NodePollingOff(string NodeID, string Value);

        [OperationContract(Name = "NodeName")]
        [WebGet(UriTemplate = "node/{NodeID}/name/{Value}")]
        Boolean NodeName(string NodeID, String Value);

        [OperationContract(Name = "NodeLocation")]
        [WebGet(UriTemplate = "node/{NodeID}/location/{Value}")]
        Boolean NodeLocation(string NodeID, String Value);


        [OperationContract(Name = "AllOn")]
        [WebGet(UriTemplate = "allon")]
        Boolean AllOn();

        [OperationContract(Name = "AllOff")]
        [WebGet(UriTemplate = "alloff")]
        Boolean AllOff();

        // Process control methods
        [OperationContract(Name = "ProcessRunning")]
        [WebGet(UriTemplate = "processrunning")]
        ProcessRunning ProcessRunning();

        [OperationContract(Name = "ProcessStart")]
        [WebGet(UriTemplate = "processstart/{Process}?arguments={Arguments}")]
        Boolean StartProcess(string Process, string Arguments);

        [OperationContract(Name = "ProcessStop")]
        [WebGet(UriTemplate = "processstop")]
        Boolean StopProcess();

        [OperationContract(Name = "VLC")]
        [WebGet(UriTemplate = "vlc?arguments={Arguments}")]
        Boolean VLC(string Arguments);
    }

    [ServiceBehavior(Name = "Niviane", Namespace = "http://niviane/", InstanceContextMode = InstanceContextMode.Single)]
    public class Niviane : INiviane
    {
        private ZWave zwaveControl;
        private ProcessControl managedProcess;


        public Niviane()
        {
            zwaveControl = new ZWave(Properties.Settings.Default.ZWaveConfigPath, Properties.Settings.Default.ZWaveSerialPort, Properties.Settings.Default.ZWavePollInterval, Properties.Settings.Default.LogFilePath);
            managedProcess = new ProcessControl(Properties.Settings.Default.LogFilePath);

        }

        public Boolean IsAvailable()
        {
            ZWave.Log("WCFService", "IsAvailable");
            return zwaveControl.IsNetworkReady;
        }

        public List<NodeDetail> Nodes()
        {
            List<NodeDetail> nodeList = new List<NodeDetail>();
            ZWaveNode n = new ZWaveNode();
            
            ZWave.Log("WCFService", "Nodes");
            foreach (Byte nodeID in zwaveControl.ListOfNodeIDs())
            {
               
                nodeList.Add(new NodeDetail() { 
                        NodeID = nodeID
                        , Name = zwaveControl.NodeName(nodeID)
                        , Type = zwaveControl.NodeType(nodeID)
                        , Location = zwaveControl.NodeLocation(nodeID)
                        , Basic = zwaveControl.NodeBasic(nodeID)
                        , Level = zwaveControl.NodeLevel(nodeID) 
                });

            }

            return nodeList;
            
        }

        public NodeDetail Node(string NodeID)
        {
            ZWave.Log("WCFService", "Node");
            byte nodeID;
            ZWaveNode n = new ZWaveNode();
            NodeDetail nodeDetail = new NodeDetail();

            if (byte.TryParse(NodeID, out nodeID))
            {
                nodeDetail = new NodeDetail() { 
                    NodeID = nodeID
                    , Name = zwaveControl.NodeName(nodeID)
                    , Type = zwaveControl.NodeType(nodeID)
                    , Location = zwaveControl.NodeLocation(nodeID)
                    , Basic = zwaveControl.NodeBasic(nodeID)
                    , Level = zwaveControl.NodeLevel(nodeID)
                };
            }
           
            return nodeDetail;
        }

        public Boolean NodeBasic(string NodeID, string Value)
        {
            byte nodeID, value;

            ZWave.Log("WCFService", "NodeBasic");
            if (byte.TryParse(NodeID, out nodeID) && byte.TryParse(Value, out value))
            {
                zwaveControl.SetNodeBasic(nodeID, value);
                return true;
            }
            
            return false;
        }


        public Boolean NodePollingOn(string NodeID, string Value)
        {
            byte nodeID;
            ZWave.Log("WCFService", "NodePollngOn");
            if (byte.TryParse(NodeID, out nodeID))
            {
                zwaveControl.EnablePoll(nodeID, Value);
                return true;
            }
            return false ;
        }

        public Boolean NodePollingOff(string NodeID, string Value)
        {
            byte nodeID;
            ZWave.Log("WCFService", "NodePollngOn");
            if (byte.TryParse(NodeID, out nodeID))
            {
                zwaveControl.DisablePoll(nodeID, Value);
                return true;
            }
            return false;
        }

        public Boolean NodeName(string NodeID, String Value)
        {
            byte nodeID;
            ZWave.Log("WCFService", "NodeName:" + NodeID + "," + Value);
            if (byte.TryParse(NodeID, out nodeID))
            {
                zwaveControl.SetNodeName(nodeID, Value);
                return true;
            }
            return false;

        }

        public Boolean NodeLocation(string NodeID, String Value)
        {
            byte nodeID;
            ZWave.Log("WCFService", "NodeLocation:" + NodeID + "," + Value);
            if (byte.TryParse(NodeID, out nodeID))
            {
                zwaveControl.SetNodeLocation(nodeID, Value);
                return true;
            }
            return false;

        }

        public Boolean AllOn()
        {
            ZWave.Log("WCFService", "All On");
            zwaveControl.AllOn();
            return true;
        }

        public Boolean AllOff()
        {
            ZWave.Log("WCFService", "All Off");
            zwaveControl.AllOff();
            return true;
        }

        public ProcessRunning ProcessRunning()
        {
            ZWave.Log("WCFService","ProcessRunning");
            return new ProcessRunning() { IsRunning = managedProcess.IsProcessRunning(), Process = managedProcess.CurrentProcessName(), Arguments = managedProcess.CurrentProcessArguments() };
        }

        public Boolean StartProcess(string Process, string Arguments)
        {
            ZWave.Log("WCFService", "StartProcess: " + Process + " with " + Arguments );
            managedProcess.StartProcess(Process, Arguments);
            return true;
        }

        public Boolean StopProcess()
        {
            ZWave.Log("WCFService", "StopProcess");
            managedProcess.StopProcess();
            return true;
        }

        
        public Boolean VLC(string Arguments)
        {
            StartProcess(Properties.Settings.Default.VLCPathToExe, Arguments);
            return true;
        }

        

    }




}
