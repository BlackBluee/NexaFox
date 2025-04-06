using System.Net;

namespace NexaFox.Utilities
{
    

    public class NetworkCalculator
    {
        /// <summary>
        /// Oblicza adres sieci na podstawie adresu IP i maski podsieci.
        /// </summary>
        /// <param name="ip">Adres IP w formacie IPv4</param>
        /// <param name="mask">Maska podsieci w formacie IPv4</param>
        /// <returns>Adres sieci jako wartość uint</returns>
        /// <exception cref="ArgumentNullException">Rzucany gdy ip lub mask są null</exception>
        /// <exception cref="ArgumentException">Rzucany gdy adres IP lub maska nie są typu IPv4</exception>
        public static uint CalculateNetworkAddress(IPAddress ip, IPAddress mask)
        {
            // Walidacja wejścia
            if (ip == null) throw new ArgumentNullException(nameof(ip), "Adres IP nie może być null");
            if (mask == null) throw new ArgumentNullException(nameof(mask), "Maska podsieci nie może być null");

            byte[] ipBytes = ip.GetAddressBytes();
            byte[] maskBytes = mask.GetAddressBytes();

            // Sprawdź, czy to IPv4 (długość tablicy = 4 bajty)
            if (ipBytes.Length != 4 || maskBytes.Length != 4)
                throw new ArgumentException("Metoda obsługuje tylko adresy IPv4");

            byte[] networkBytes = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
            }

            // Konwersja tablicy bajtów na uint
            if (BitConverter.IsLittleEndian)
                Array.Reverse(networkBytes);

            return BitConverter.ToUInt32(networkBytes, 0);
        }

        /// <summary>
        /// Oblicza adres rozgłoszeniowy na podstawie adresu IP i maski podsieci.
        /// </summary>
        /// <param name="ip">Adres IP w formacie IPv4</param>
        /// <param name="mask">Maska podsieci w formacie IPv4</param>
        /// <returns>Adres rozgłoszeniowy jako wartość uint</returns>
        /// <exception cref="ArgumentNullException">Rzucany gdy ip lub mask są null</exception>
        /// <exception cref="ArgumentException">Rzucany gdy adres IP lub maska nie są typu IPv4</exception>
        public static uint CalculateBroadcastAddress(IPAddress ip, IPAddress mask)
        {
            // Walidacja wejścia
            if (ip == null) throw new ArgumentNullException(nameof(ip), "Adres IP nie może być null");
            if (mask == null) throw new ArgumentNullException(nameof(mask), "Maska podsieci nie może być null");

            byte[] ipBytes = ip.GetAddressBytes();
            byte[] maskBytes = mask.GetAddressBytes();

            // Sprawdź, czy to IPv4 (długość tablicy = 4 bajty)
            if (ipBytes.Length != 4 || maskBytes.Length != 4)
                throw new ArgumentException("Metoda obsługuje tylko adresy IPv4");

            byte[] broadcastBytes = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                broadcastBytes[i] = (byte)(ipBytes[i] | (byte)~maskBytes[i]);
            }

            // Konwersja tablicy bajtów na uint
            if (BitConverter.IsLittleEndian)
                Array.Reverse(broadcastBytes);

            return BitConverter.ToUInt32(broadcastBytes, 0);
        }

        /// <summary>
        /// Konwertuje uint na IPAddress
        /// </summary>
        /// <param name="address">Adres jako wartość uint</param>
        /// <returns>Obiekt IPAddress</returns>
        public static IPAddress UIntToIPAddress(uint address)
        {
            byte[] bytes = BitConverter.GetBytes(address);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return new IPAddress(bytes);
        }

        /// <summary>
        /// Sprawdza, czy podany adres IP należy do wskazanej podsieci
        /// </summary>
        /// <param name="ip">Adres IP do sprawdzenia</param>
        /// <param name="networkIP">Adres IP w podsieci</param>
        /// <param name="mask">Maska podsieci</param>
        /// <returns>True jeśli adres IP należy do podsieci</returns>
        public static bool IsInSameSubnet(IPAddress ip, IPAddress networkIP, IPAddress mask)
        {
            uint ipNetwork = CalculateNetworkAddress(ip, mask);
            uint networkAddress = CalculateNetworkAddress(networkIP, mask);

            return ipNetwork == networkAddress;
        }
    }
}

