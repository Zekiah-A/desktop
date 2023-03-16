using WatsonWebsocket;
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
	    var image = new Image(@"Resources/Brand/grass_icon.png");
	    var imageTask = new TaskCompletionSource<Image>();
	    var descriptionColour = new Color(255, 255, 255, 200);
	    var timeout = new Timer(_ =>
	    {
		    if (server.Connected)
		    {
			    server.Stop();
			    motd = "Server refused connection";
		    }
		    else
		    {
			    descriptionColour = new Color(255, 0, 0, 200);
			    motd = "Failed to connect (server timeout)";
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
			    try
			    {
				    imageTask.SetResult(await FetchImage(imageUri));
			    }
			    catch (Exception)
			    {
				    imageTask.SetCanceled();
			    }
		    });
	    };

	    server.ServerDisconnected += (sender, args) =>
	    {
		    descriptionColour = new Color(255, 0, 0, 200);
		    motd = "Server disconnected";
		    imageTask.SetCanceled();
	    };
	    
	    await server.StartAsync();
	    try
	    {
		    image = await imageTask.Task;
	    }
	    catch (Exception)
	    {
		    // Image will use default value
	    }

	    return new DisplayListItem(new Texture(image), name, motd)
	    {
		    DescriptionColour = descriptionColour
	    };
    }
}