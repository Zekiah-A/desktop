using WatsonWebsocket;
using System.Net;
using System.Threading;
using SFML.Graphics;

namespace OpenMc2D.Networking;

public class Connections
{
	private GameData gameData;
    public Connections(GameData data)
    {
	    gameData = data;
    }

    private async Task<Image> FetchImage(string uri)
    {
	    var response = await gameData.HttpClient.GetAsync(uri);
	    var data = await response.Content.ReadAsStreamAsync();
	    return new Image(data);
    }

    public async Task<DisplayListItem> PreConnect(string ip)
    {
	    var server = new WatsonWsClient(new Uri($"{ip}/{gameData.Name}/{gameData.AuthSignature}/{gameData.PublicKey}"));
	    var name = ip;
	    var motd = "Failed to connect";
	    var imageTask = new TaskCompletionSource<Image>();
	    var timeout = new Timer(_ =>
	    {
		    if (server.Connected)
		    {
			    server.Stop();
			    motd = "Server refused connection";
		    }
		    else
		    {
			    motd = "Failed to connect";
		    }
		    imageTask.SetCanceled();
	    }, null, 5000, Timeout.Infinite);

	    server.ServerConnected += (sender, args) =>
	    {
		    Console.WriteLine("Server connected");
		    timeout.Change(Timeout.Infinite, Timeout.Infinite);
	    };

	    server.MessageReceived += (sender, args) =>
	    {
		    var packet = (ReadablePacket) args.Data.ToArray();
		    name = packet.ReadString();
		    motd = packet.ReadString();
		    var fetchTask = FetchImage(packet.ReadString());
		    
		    Task.Run(async () => 
		    {
			    var image = await fetchTask;
			    imageTask.SetResult(image);
		    });
	    };

	    server.ServerDisconnected += (sender, args) =>
	    {
		    motd = "Server disconnected";
		    imageTask.SetCanceled();
	    };
	    
	    await server.StartAsync();
	    Image image;
	    try
	    {
		    image = await imageTask.Task;
	    }
	    catch (TaskCanceledException)
	    {
		    image = new Image(@"Resources/Brand/grass_icon.png");
	    }
	    return new DisplayListItem(new Texture(image), name, motd);
    }
}