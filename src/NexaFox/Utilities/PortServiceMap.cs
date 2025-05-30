﻿using System.Collections.Generic;

namespace NexaFox.Utilities
{
    public static class PortServiceMap
    {
        public static readonly Dictionary<int, string> PortServices = new Dictionary<int, string>
        {
            { 21, "FTP" },
            { 22, "SSH" },
            { 23, "Telnet" },
            { 25, "SMTP" },
            { 53, "DNS" },
            { 80, "HTTP" },
            { 110, "POP3" },
            { 143, "IMAP" },
            { 443, "HTTPS" },
            { 445, "SMB" },
            { 993, "IMAPS" },
            { 995, "POP3S" },
            { 3306, "MySQL" },
            { 3389, "RDP" },
            { 5432, "PostgreSQL" },
            { 5900, "VNC" },
            { 8080, "HTTP Alt" }
        };
    }
}
