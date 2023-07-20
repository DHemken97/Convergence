namespace DHCP.Common.Models
{
    public class DhcpPool
    {
        public int Id { get; set; }
        public Guid SiteId { get; set; }
        public string Name { get; set; }
        public string Network { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public Dictionary<string, string> ActiveLeases { get; private set; } = new Dictionary<string, string>();

        public DhcpSettings Settings { get; set; }
        public void AddLease(string mac, string ip)
        {
            ActiveLeases.Remove(mac);
            ActiveLeases.Add(mac, ip);  
        }
        public string GetNextIp()
        {
            for (int i = Start; i <= End; i++)
            {
                var ip = BuildIp(i);
                if (ActiveLeases.All(lease => lease.Value != ip))
                    return ip;
            }
            return "255.255.255.255";
        }
        private string BuildIp(int lastOctet)
        {

            var networkSplit = Network.Split('.');
            return $"{networkSplit[0]}.{networkSplit[1]}.{networkSplit[2]}.{lastOctet}";
        }
    }
}
