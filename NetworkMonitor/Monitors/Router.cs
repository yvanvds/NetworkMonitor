using System.Net;
using SnmpSharpNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkMonitor.Monitors
{
    public class Router
    {
        AgentParameters param;
        UdpTarget target;
        Pdu pdu;

        public Router(string address, string community)
        {
            param = new AgentParameters(new OctetString(community));
            param.Version = SnmpVersion.Ver2;

            IpAddress agent = new IpAddress(address);
            target = new UdpTarget((IPAddress)agent, 161, 2000, 1);

            pdu = new Pdu(PduType.GetBulk);
            pdu.NonRepeaters = 0;
            pdu.MaxRepetitions = 5;

        

            
        }

        public void Update()
        {

            Oid rootOid = new Oid("1.3.6.1.2.1.31.1.1.1.1.2");
            Oid lastOid = (Oid)rootOid.Clone();

            while(lastOid != null)
            {
                if(pdu.RequestId != 0)
                {
                    pdu.RequestId++;
                }

                pdu.VbList.Clear();
                pdu.VbList.Add(lastOid);

                SnmpV2Packet result = (SnmpV2Packet)target.Request(pdu, param);

                if (result != null)
                {
                    // ErrorStatus other then 0 is an error returned by 
                    // the Agent - see SnmpConstants for error definitions
                    if (result.Pdu.ErrorStatus != 0)
                    {
                        // agent reported an error with the request
                        Console.WriteLine("Error in SNMP reply. Error {0} index {1}",
                            result.Pdu.ErrorStatus,
                            result.Pdu.ErrorIndex);
                        lastOid = null;
                        break;
                    }
                    else
                    {
                        // Walk through returned variable bindings
                        foreach (Vb v in result.Pdu.VbList)
                        {
                            // Check that retrieved Oid is "child" of the root OID
                            if (rootOid.IsRootOf(v.Oid))
                            {
                                Console.WriteLine("{0} ({1}): {2}",
                                    v.Oid.ToString(),
                                    SnmpConstants.GetTypeName(v.Value.Type),
                                    v.Value.ToString());
                                if (v.Value.Type == SnmpConstants.SMI_ENDOFMIBVIEW)
                                    lastOid = null;
                                else
                                    lastOid = v.Oid;
                            }
                            else
                            {
                                // we have reached the end of the requested
                                // MIB tree. Set lastOid to null and exit loop
                                lastOid = null;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No response received from SNMP agent.");
                }
            }

            target.Close();

            /*SnmpV2Packet result =  null;

            try
            {
                result = (SnmpV2Packet)target.Request(pdu, param);
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (result != null)
            {
                if (result.Pdu.ErrorStatus != 0)
                {
                    Console.WriteLine("Error in SNMP reply. Error {0} index {1}",
                        result.Pdu.ErrorStatus,
                        result.Pdu.ErrorIndex);
                }
                else
                {
                    Console.WriteLine("sysDescr({0}) ({1}): {2}",
                        result.Pdu.VbList[0].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[0].Value.Type),
                        result.Pdu.VbList[0].Value.ToString());
                    Console.WriteLine("sysObjectID({0}) ({1}): {2}",
                        result.Pdu.VbList[1].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[1].Value.Type),
                        result.Pdu.VbList[1].Value.ToString());
                    Console.WriteLine("sysUpTime({0}) ({1}): {2}",
                        result.Pdu.VbList[2].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[2].Value.Type),
                        result.Pdu.VbList[2].Value.ToString());
                    Console.WriteLine("sysContact({0}) ({1}): {2}",
                        result.Pdu.VbList[3].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[3].Value.Type),
                        result.Pdu.VbList[3].Value.ToString());
                    Console.WriteLine("sysName({0}) ({1}): {2}",
                        result.Pdu.VbList[4].Oid.ToString(),
                        SnmpConstants.GetTypeName(result.Pdu.VbList[4].Value.Type),
                        result.Pdu.VbList[4].Value.ToString());

                }
            }
            else
            {
                Console.WriteLine("No Response received from snmp agent.");
            }*/
        }
    }
}
