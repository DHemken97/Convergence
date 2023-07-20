using DHCP.Common.Events;
using DHCP.Common.Helpers;
using DHCP.Common.Models;
using DotNetProjects.DhcpServer;
using System.Net;
using System.Text;

namespace DHCP.Server
{
    public interface IDhcpServer
    {
        public event EventHandler<DhcpEventArgs> DhcpEvent;
        public Site Site { get; }
        public void Start();
    }
    public class DhcpServer : IDhcpServer
    {
        public event EventHandler<DhcpEventArgs> DhcpEvent;
        public Site Site => _site;
        private Site _site;
        private DhcpPool _defaultDhcpPool;

        public DhcpServer(Site site)
        {
            _site = site;

            var defaultPoolId = site.Details.DefaultPoolId;
            _defaultDhcpPool = site.Details.DhcpPools.FirstOrDefault(pool => pool.Id == defaultPoolId)??throw new Exception("Default Pool Not Found");

        }

        public void Start()
        {

            //         var lst = NetworkInterface.GetAllNetworkInterfaces();

            //            var eth0If = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(x => x.Name == "USB_ETH");
            var server = new DHCPServer();
            server.ServerName = Site.Details.NetworkName;
            server.OnDataReceived += HandleRequest;
            server.BroadcastAddress = IPAddress.Broadcast;


            //server.SendDhcpAnswerNetworkInterface = eth0If;
            server.Start();
            Console.WriteLine("Running DHCP server. Press enter to stop it.");
            Console.ReadLine();
            server.Dispose();
        }
        private void HandleRequest(DHCPRequest request)
        {
            var dhcpEvent = new DhcpEventArgs {Request = request };

            try
            {
                var macAddress = GetMacAddress(request);
                dhcpEvent.LogAction($"Request for IP from {macAddress}");

                var device = GetDeviceFromMac(macAddress);
                if (device.Id < 1)
                {
                    device.SiteId = Site.Details.Id;
                    device.Id = (Site.Devices.Any()) ? Site.Devices.Max(d => d.Id)+1 : 1;
                    dhcpEvent.LogAction("Device not found, Adding new config");
                    _site.Devices.Add(device);
                }
                else
                    dhcpEvent.LogAction($"Device found: Id-{device.Id}   Name-{device.Name}   AssetTag-{device.AssetTagNumber}   Pool-{device.DhcpPoolId}");

                var devicePool = _defaultDhcpPool;
                if (device.DhcpPoolId>0 && device.DhcpPoolId!= _defaultDhcpPool.Id)
                    devicePool = _site.Details.DhcpPools.FirstOrDefault(pool => pool.Id == device.DhcpPoolId)?? _defaultDhcpPool;


                var ip = device.IpAddress;
                dhcpEvent.LogAction($"Device Static IP: {ip}");
                if (!IpAssignmentValid(devicePool, ip))
                {
                    dhcpEvent.LogAction($"IP Invalid");
                    ip = device.LastIp;
                    dhcpEvent.LogAction($"Last Ip: {ip}");
                }
                if (!IpAssignmentValid(devicePool, ip) && devicePool.ActiveLeases.TryGetValue(macAddress, out var _activeIp))
                {
                    dhcpEvent.LogAction($"Ip Invalid");
                    ip = _activeIp;
                    dhcpEvent.LogAction($"Active Lease Ip: {ip}");

                }
                if (!IpAssignmentValid(devicePool, ip))
                {
                    dhcpEvent.LogAction($"Ip Invalid");
                    ip = devicePool.GetNextIp();
                    dhcpEvent.LogAction($"DHCP Assigned Ip: {ip}");
                }

                var options = request.GetAllOptions();
                var requestedOptions = request.GetRequestedOptionsList();
                dhcpEvent.LogAction("Responding with options:");

                foreach (DHCPOption option in options.Keys)
                    dhcpEvent.LogAction($"{option} : {options[option].ConvertToString()}");

           //     dhcpEvent.LogAction("Requested options:");
             //   foreach (DHCPOption option in requestedOptions) dhcpEvent.LogAction($"{option.ToString()??string.Empty}");

                var response = BuildDhcpResponse(ip, device, devicePool);
                device.LastIp = ip;
                device.LastOnline = DateTime.Now;
                devicePool.AddLease(macAddress, ip);
                var type = request.GetMsgType();
                var ipAddress = IPAddress.Parse(ip);
                if (type == DHCPMsgType.DHCPDISCOVER)
                    request.SendDHCPReply(DHCPMsgType.DHCPOFFER, ipAddress, response);
                if (type == DHCPMsgType.DHCPREQUEST)
                    request.SendDHCPReply(DHCPMsgType.DHCPACK, ipAddress, response);
            }
            catch (Exception ex)
            {
                dhcpEvent.LogAction($"Exception: {ex.Message}");
                dhcpEvent.LogAction(ex.StackTrace);
            }

            DhcpEvent?.Invoke(this, dhcpEvent);
        }

        private DHCPReplyOptions BuildDhcpResponse(string ip, Device device, DhcpPool devicePool)
        {
            var replyOptions = new DHCPReplyOptions();
            // Options should be filled with valid data. Only requested options will be sent.
            replyOptions.SubnetMask = NotEmpty(device.DhcpSettings.SubnetMask,devicePool.Settings.SubnetMask);
            replyOptions.DomainName = NotEmptyString(device.DhcpSettings.DomainName,devicePool.Settings.DomainName);
           // replyOptions.ServerIdentifier = NotEmpty("192.168.42.34", devicePool.Settings.DefaultGateway);
            replyOptions.ServerIdentifier = NotEmpty(device.DhcpSettings.DefaultGateway, devicePool.Settings.DefaultGateway);
            replyOptions.RouterIP = NotEmpty(device.DhcpSettings.DefaultGateway, devicePool.Settings.DefaultGateway);
            
            var dns = BuildDns(device.DhcpSettings, devicePool.Settings);
            replyOptions.DomainNameServers = dns.ToArray();

            var tftp = NotEmptyString(device.DhcpSettings.TFTP, devicePool.Settings.TFTP);
            if (!string.IsNullOrWhiteSpace(tftp))
                replyOptions.OtherRequestedOptions?.Add(DHCPOption.TFTPServerName, Encoding.ASCII.GetBytes(tftp));
            
            // Some static routes, unused
            replyOptions.StaticRoutes = new NetworkRoute[]
            {
                new NetworkRoute(IPAddress.Parse("0.0.0.0"), IPAddress.Parse("0.0.0.0"),  IPAddress.Parse("192.168.42.254")),
                new NetworkRoute(IPAddress.Parse("255.255.255.255"), IPAddress.Parse("0.0.0.0"),  IPAddress.Parse("192.168.42.254")),
                new NetworkRoute(IPAddress.Parse("255.255.255.255"), IPAddress.Parse("255.255.255.255"),  IPAddress.Parse("192.168.42.254")),
                new NetworkRoute(IPAddress.Parse("8.8.8.0"), IPAddress.Parse("255.255.255.0"),  IPAddress.Parse("192.168.42.254")),
                new NetworkRoute(IPAddress.Parse("8.8.8.8"), IPAddress.Parse("255.255.255.0"),  IPAddress.Parse("192.168.42.254")),
            };
            return replyOptions;
        }

        private List<IPAddress> BuildDns(DhcpSettings defaultSettings, DhcpSettings fallbackSettings)
        {
            if (string.IsNullOrWhiteSpace(defaultSettings.DnsServer1) && string.IsNullOrWhiteSpace(defaultSettings.DnsServer2))
                if (string.IsNullOrWhiteSpace(fallbackSettings.DnsServer1) && string.IsNullOrWhiteSpace(fallbackSettings.DnsServer2))
                    return new List<IPAddress> { NotEmpty(defaultSettings.DefaultGateway, fallbackSettings.DefaultGateway) };
                else
                {
                    var dns = new List<IPAddress>();
                    if (!string.IsNullOrWhiteSpace(fallbackSettings.DnsServer1))
                        dns.Add(IPAddress.Parse(fallbackSettings.DnsServer1));
                    if (!string.IsNullOrWhiteSpace(fallbackSettings.DnsServer2))
                        dns.Add(IPAddress.Parse(fallbackSettings.DnsServer2));
                    return dns;
                }
            else
            {
                var dns = new List<IPAddress>();
                if (!string.IsNullOrWhiteSpace(defaultSettings.DnsServer1))
                    dns.Add(IPAddress.Parse(defaultSettings.DnsServer1));
                if (!string.IsNullOrWhiteSpace(defaultSettings.DnsServer2))
                    dns.Add(IPAddress.Parse(defaultSettings.DnsServer2));
                return dns;
            }
        }

        private string NotEmptyString(string value1, string value2, string defaultValue="")
        {
            if (string.IsNullOrWhiteSpace(value2)) return value1;
            return value2 ?? defaultValue;
        }
        private IPAddress NotEmpty(string value1, string value2)
        => IPAddress.Parse(NotEmptyString(value1, value2,"255.255.255.0"));


        private Device GetDeviceFromMac(string macAddress)
        => _site.Devices.FirstOrDefault(d => d.MAC_Address == (macAddress)) ??
                             Device.Empty(macAddress, _defaultDhcpPool.Settings.DefaultGateway, _defaultDhcpPool.Settings.SubnetMask);

        private string GetMacAddress(DHCPRequest request)
        => request.GetChaddr().ConvertToString().Split(' ')[0];

        private bool IpAssignmentValid(DhcpPool pool, string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;
            if (!IPAddress.TryParse(ip, out var ipAddress))
                return false;
            if (pool.ActiveLeases.Any(lease => lease.Value == ipAddress.ToString()))
                return false;

            return true;
        }
    }
}
