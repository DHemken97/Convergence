using DotNetProjects.DhcpServer;

namespace DHCP.Common.Events
{
    public class DhcpEventArgs : EventArgs
    {
     public DHCPRequest Request { get; set; }
        public string EventLog { get; set; } = string.Empty;
        public void LogAction(string message)
            => EventLog += $"{message}\r\n";
    }
}
