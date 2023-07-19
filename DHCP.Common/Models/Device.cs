namespace DHCP.Common.Models
{
    public class Device
    {
        public int Id { get; set; }
        public Guid SiteId { get; set; }
        public int DhcpPoolId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MAC_Address { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string AssetTagNumber { get; set; } = string.Empty;
        public string ReferenceField1 { get; set; } = string.Empty;
        public string ReferenceField2 { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string IpAddress { get; set; }
        public DateTime LastOnline { get; set; }
        public string LastIp { get; set; }
        public DateTime Expiration { get; set; }
        public DhcpSettings DhcpSettings { get; set; }


        public static Device Empty(string mac, string gateway, string subnet)
        {
            return new Device
            {
                MAC_Address = mac,
                Id = 0,
                SiteId = Guid.Empty,
                DhcpSettings = new DhcpSettings
                {
                    DefaultGateway = gateway,
                    SubnetMask = subnet,

                }

            };
        }
    }
}
