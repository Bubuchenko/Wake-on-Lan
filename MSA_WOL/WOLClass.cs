using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MSA_WOL
{
    public class WOLClass : UdpClient
    {
        public WOLClass() : base()
        {

        }

        public void SetClientToBroadcastMode()
        {
            if (this.Active)
            {
                this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 0);
            }
        }

        public static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = new Ping();
            try
            {
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            return pingable;
        }


        public static byte[] MacToBytes(string macAddress)
        {
            byte[] mac = new byte[6];

            for (var i = 0; i < 6; i++)
            {
                var t = macAddress.Substring((i * 2), 2);
                mac[i] = Convert.ToByte(t, 16);
            }

            return mac;
        }

        public static void WakeOnLan(string ip, string Mac)
        {
            byte[] mac = MacToBytes(Mac);

            // WOL packet is sent over UDP 255.255.255.0:40000.
            UdpClient client = new UdpClient();
            client.Connect(ip, 9);

            // WOL packet contains a 6-bytes trailer and 16 times a 6-bytes sequence containing the MAC address.
            byte[] packet = new byte[17 * 6];

            // Trailer of 6 times 0xFF.
            for (int i = 0; i < 6; i++)
                packet[i] = 0xFF;

            // Body of magic packet contains 16 times the MAC address.
            for (int i = 1; i <= 16; i++)
                for (int j = 0; j < 6; j++)
                    packet[i * 6 + j] = mac[j];

            // Send WOL packet.
            client.Send(packet, packet.Length);
        }

        public static void WakeUp(string macAddress, string ipaddress)
        {
            WOLClass client = new WOLClass();

            client.Connect(new IPEndPoint(IPAddress.Parse(ipaddress), 9));
            client.SetClientToBroadcastMode();

            int counter = 0;

            byte[] bytes = new byte[1024];

            for (int e = 0; e < 6; e++)
            {
                bytes[counter++] = 0xFF;
            }

            for (int e = 0; e < 16; e++)
            {
                int i = 0;

                for (int w = 0; w < 6; w++)
                {
                    bytes[counter++] = byte.Parse(macAddress.Substring(i, 2), NumberStyles.HexNumber);
                    i += 2;
                }
            }

            int returnedValue = client.Send(bytes, 1024);
        }

    }
}
