using DHCP.Common.Events;
using DHCP.Server;

var site = new DHCP.Common.Models.Site();
site.Details = new DHCP.Common.Models.SiteDetails();
site.Details.DhcpPools = new List<DHCP.Common.Models.DhcpPool>();
site.Details.DhcpPools.Add(new DHCP.Common.Models.DhcpPool { Id = 1});
site.Details.DefaultPoolId = 1;
site.Devices = new List<DHCP.Common.Models.Device>();
var server = new DhcpServer(site);
server.DhcpEvent += LogEventMessage;
Console.WriteLine("Starting Server...");
server.Start();
while (true)
    Console.ReadLine();



void LogEventMessage(object? sender, DhcpEventArgs e)
{
    Console.WriteLine(e.EventLog);
    Console.WriteLine("--------------------------------------------");
}