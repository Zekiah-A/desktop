using WatsonWebsocket;
using System.Net;
using System.Threading;

namespace OpenMc2D.Networking;

public class Connection
{
    public Connection()
    {
        
    }

    public void PreConnect(string ip)
    {
        var server = new WatsonWsClient(new Uri(ip));
        var timeout = new Timer(_ => { server.Stop(); }, null, 0, 5000);
        
        server.ServerConnected += (sender, args) =>
        {
            timeout.Change(Timeout.Infinite, Timeout.Infinite);
        };

        server.MessageReceived += (sender, args) =>
        {
            
        };

        server.ServerConnected += (sender, args) =>
        {
        
        };
    }
}