using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenZWaveDotNet;

namespace Niviane_Service
{
    public class ZWaveNode
    {
        private Byte m_id = 0;
        public Byte ID
        {
            get { return m_id; }
            set { m_id = value; }
        }

        private UInt32 m_homeId = 0;
        public UInt32 HomeID
        {
            get { return m_homeId; }
            set { m_homeId = value; }
        }

        private String m_name = "";
        public String Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        private String m_location = "";
        public String Location
        {
            get { return m_location; }
            set { m_location = value; }
        }

        private String m_label = "";
        public String Label
        {
            get { return m_label; }
            set { m_label = value; }
        }

        private String m_manufacturer = "";
        public String Manufacturer
        {
            get { return m_manufacturer; }
            set { m_manufacturer = value; }
        }

        private String m_product = "";
        public String Product
        {
            get { return m_product; }
            set { m_product = value; }
        }

        private List<ZWValueID> m_values = new List<ZWValueID>();
        public List<ZWValueID> Values
        {
            get { return m_values; }
        }

        public ZWaveNode()
        {
        }

        public void AddValue(ZWValueID valueID)
        {
            m_values.Add(valueID);
        }

        public void RemoveValue(ZWValueID valueID)
        {
            m_values.Remove(valueID);
        }

        public void SetValue(ZWValueID valueID)
        {
            int valueIndex = -1;

            for (int index = 0; index < m_values.Count; index++)
            {
                if (m_values[index].GetId() == valueID.GetId())
                {
                    valueIndex = index;
                    break;
                }
            }

            if (valueIndex >= 0)
            {
                m_values[valueIndex] = valueID;
            }
            else
            {
                AddValue(valueID);
            }
        }
    }
}
