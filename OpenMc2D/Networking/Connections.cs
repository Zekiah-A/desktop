using WatsonWebsocket;
using System.Net;
using System.Net.WebSockets;
using System.Text;
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
	    var server = new WatsonWsClient(new Uri($"{ip}/{gameData.Name}" +
            $@"/{NetworkingHelpers.EncodeURIComponent(gameData.PublicKey)}" +
            $@"/{NetworkingHelpers.EncodeURIComponent(gameData.AuthSignature)}"));
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
		    timeout.Change(Timeout.Infinite, Timeout.Infinite);
	    };

	    server.MessageReceived += (sender, args) =>
	    {
		    var packet = (ReadablePacket) args.Data.ToArray();
		    name = packet.ReadString();
		    motd = packet.ReadString();
		    var imageUri = packet.ReadString();
		    
		    Task.Run(async () => 
		    {
			    imageTask.SetResult(await FetchImage(imageUri));
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
	    catch (Exception)
	    {
		    image = new Image(@"Resources/Brand/grass_icon.png");
	    }
	    return new DisplayListItem(new Texture(image), name, motd);
    }
}