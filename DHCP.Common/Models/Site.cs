namespace DHCP.Common.Models
{
    public class Site
    {
        public SiteDetails Details { get; set; }
        public List<Device> Devices { get; set; }
    }
}
