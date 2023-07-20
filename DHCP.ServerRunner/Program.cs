using DHCP.Common.Events;
using DHCP.Common.Models;
using DHCP.Server;
using DotNetProjects.DhcpServer;
using Newtonsoft.Json;

var g = Guid.NewGuid().ToString();

var RootDir = @"C:\Users\dhemken\AppData\Roaming\Convergence\DHCP\";
var json = File.ReadAllText($"{RootDir}Site.DHCP.json");
var site = JsonConvert.DeserializeObject<Site>(json);
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

    try
    {

        var RootDir = @"C:\Users\dhemken\AppData\Roaming\Convergence\DHCP\";
        if (e.Request.GetMsgType() == DHCPMsgType.DHCPREQUEST)
            File.WriteAllText($"{RootDir}Site.DHCP.json", JsonConvert.SerializeObject(site, Formatting.Indented));
    } catch(Exception ex)
    {

    }
}