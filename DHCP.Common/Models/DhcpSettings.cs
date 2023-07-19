namespace DHCP.Common.Models
{
    public class DhcpSettings
    {


        public string SubnetMask { get; set; }
        public string DefaultGateway { get; set; }
        public string TimeServer { get; set; }
        public string DnsServer1 { get; set; }
        public string DnsServer2 { get; set; }
        public string LogServer { get; set; }
        public string DomainName { get; set; } = string.Empty;
        public string BroadcastAddress { get; set; }
        public string NtpServer { get; set; }
        public int RenewalTime { get; set; }
        public int RebindingTime { get; set; }
        public string TftpName { get; set; } = string.Empty;
        public string BootFileName { get; set; } = string.Empty;
        public string SmtpServer { get; set; } = string.Empty;
        public string Pop3Server { get; set; } = string.Empty;
        public string WwwServer { get; set; } = string.Empty;
        public string FQDN { get; set; } = string.Empty;
        public string CaptivePortal { get; set; } = string.Empty;
        public string SipServer { get; set; }
        public string PhoneTFTP { get; set; }
        public string TFTP { get; set; }
    }
}
