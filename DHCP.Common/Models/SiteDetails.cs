namespace DHCP.Common.Models
{
    public class SiteDetails
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Organization { get; set; }
        public string NetworkName { get; set; }
        public string Address { get; set; }
        public string Notes { get; set; }
        public string PublicIp { get; set; }
        public List<DhcpPool> DhcpPools { get; set; }
        public int DefaultPoolId { get; set; }
    }
}
