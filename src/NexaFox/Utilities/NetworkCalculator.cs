using System.Net;

namespace NexaFox.Utilities
{
    
        public class NetworkCalculator
        {
            public static uint CalculateNetworkAddress(IPAddress ip, IPAddress mask)
            {
                byte[] ipBytes = ip.GetAddressBytes();
                byte[] maskBytes = mask.GetAddressBytes();
                byte[] networkBytes = new byte[4];

                for (int i = 0; i < 4; i++)
                {
                    networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
                }

                if (BitConverter.IsLittleEndian) Array.Reverse(networkBytes);
                return BitConverter.ToUInt32(networkBytes, 0);
            }

            public static uint CalculateBroadcastAddress(IPAddress ip, IPAddress mask)
            {
                byte[] ipBytes = ip.GetAddressBytes();
                byte[] maskBytes = mask.GetAddressBytes();
                byte[] broadcastBytes = new byte[4];

                for (int i = 0; i < 4; i++)
                {
                    broadcastBytes[i] = (byte)(ipBytes[i] | (byte)~maskBytes[i]);
                }

                if (BitConverter.IsLittleEndian) Array.Reverse(broadcastBytes);
                return BitConverter.ToUInt32(broadcastBytes, 0);
            }
        }
}

