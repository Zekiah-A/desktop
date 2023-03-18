using System.Net;
using OpenMc2D;
using OpenMc2D.Game;
using OpenMc2D.Gui;
using OpenMc2D.Networking;
using SFML.Graphics;
using SFML.Window;

Page? currentPage = null;
var gameData = new GameData
{
    Name = Storage.Get<string>(nameof(GameData.Name)) ?? "",
    PublicKey = Storage.Get<string>(nameof(GameData.PublicKey)) ?? "",
    PrivateKey = Storage.Get<string>(nameof(GameData.PrivateKey)) ?? "",
    AuthSignature = Storage.Get<string>(nameof(GameData.AuthSignature)) ?? "",
    KnownServers = Storage.Get<List<string>>(nameof(GameData.KnownServers)) ?? new List<string> { "localhost" }
};
World.GameData = gameData;
var connections = new Connections(gameData);
var preConnectionsDatas = new List<PreConnectData>();

// Window event listeners and input
var window = new RenderWindow(new VideoMode(1540, 1080), "OpenMc2d");
var uiView = new View();
window.Closed += (_, _) =>
{
    window.Close();
};
window.Resized += (_, args) =>
{
    var view = new View(new FloatRect(0, 0, args.Width, args.Height));
    window.SetView(view);
};
window.MouseButtonPressed += (_, args) =>
{
    if (currentPage is null || !currentPage.HitTest(args.X, args.Y, TestType.MouseDown))
    {
        // If not blocked by the UI, then we propagate the hit test to the main game
    }
};
window.MouseButtonReleased += (_, args) =>
{
    if (currentPage is null || !currentPage.HitTest(args.X, args.Y, TestType.MouseUp))
    {
        // If not blocked by the UI, then we propagate the hit test to the main game
    }
};
window.MouseMoved += (_, args) =>
{
    if (currentPage is null || !currentPage.HitTest(args.X, args.Y, TestType.MouseHover))
    {
        // If not blocked by the UI, then we propagate the hit test to the main game
    }
};

void PropagateKeyTest(KeyEventArgs args, TestType type)
{
    var modifiers = 0;
    modifiers |= args.Alt ? (int) ModifierFlags.Alt : 0;
    modifiers |= args.Control ? (int) ModifierFlags.Control : 0;
    modifiers |= args.Shift ? (int) ModifierFlags.Shift : 0;
    modifiers |= args.System ? (int) ModifierFlags.System : 0;
    
    
    if (currentPage is null || !currentPage.KeyboardTest(args.Code, modifiers, type))
    {
        // If not blocked by the UI, then we propagate the keyboard test to the main game
    }
}
window.KeyPressed += (_, args) => PropagateKeyTest(args, TestType.KeyDown);
window.KeyReleased += (_, args) => PropagateKeyTest(args, TestType.KeyUp);
window.TextEntered += (_, args) =>
{
    if (currentPage is null || !currentPage.TextTest(args.Unicode))
    {
        // If not blocked by the UI, then we propagate the text test to the main game
    }
};

var dirtBackgroundRect = new TextureRect(new Texture(@"Resources/Brand/dirt_background.png") { Repeated = true },
    () => 0,
    () => 0,
    () => (int) window.GetView().Size.X,
    () => (int) window.GetView().Size.Y)
{
    SubRect = new Bounds(() => 0, () => 0, () => (int) window.GetView().Size.X / 2, () => (int) window.GetView().Size.Y / 2)
};

var gamePage = new Page();

async Task PlayServer(PreConnectData serverData)
{
    foreach (var data in preConnectionsDatas.Where(data => data.Socket != serverData.Socket))
    {
        await data.Socket.StopAsync();
    }
    
    currentPage = gamePage;
    await connections.Connect(serverData);
}



// Servers page UI
var serversPage = new Page();
serversPage.Children.Add(dirtBackgroundRect);
var serversLabel = new Label("Connect to a server:", 28, Color.White)
{
    Bounds = new Bounds(() => (int) (window.GetView().Size.X / 2) - 156, () => 128, () => 0, () => 0)
};
serversPage.Children.Add(serversLabel);
var serverList = new DisplayList(() => 64, () => 192,
    () => (int) (window.GetView().Size.X - 128),
    () => (int) (window.GetView().Size.X * 0.8));
async Task UpdateServerList()
{
    serverList.Children.Clear();
    
    foreach (var serverIp in gameData.KnownServers)
    {
        var connectionData = await connections.PreConnect(serverIp);
        preConnectionsDatas.Add(connectionData);
        connectionData.Item.OnMouseUp += async (_, _) => await PlayServer(connectionData);
        serverList.Children.Add(connectionData.Item);
    }
}
Task.Run(UpdateServerList);
var serverDeleteButton = new Button("Delete",
    () => 16,
    () =>  (int) (window.GetView().Size.Y - 0.05 * window.GetView().Size.X - 16), 
    () => (int) (0.2 * window.GetView().Size.X), 
    () => (int) (0.05 * window.GetView().Size.X));
serverDeleteButton.OnMouseUp += (_, _) =>
{
    gameData.KnownServers.RemoveAt(gameData.KnownServers.Count - 1);
    Storage.Save(nameof(GameData.KnownServers), gameData.KnownServers);
    Task.Run(UpdateServerList);
};
serversPage.Children.Add(serverDeleteButton);
serversPage.Children.Add(serverList);
var serverRefreshButton = new Button("Refresh",
    () => (int) (0.2 * window.GetView().Size.X + 32),
    () =>  (int) (window.GetView().Size.Y - 0.05 * window.GetView().Size.X - 16), 
    () => (int) (0.2 * window.GetView().Size.X), 
    () => (int) (0.05 * window.GetView().Size.X));
serverRefreshButton.OnMouseUp += (_, _) =>
{
    Task.Run(UpdateServerList);
};
serversPage.Children.Add(serverRefreshButton);
var serverInput = new TextInput("server ip", 
    () => (int) (0.4 * window.GetView().Size.X + 52),
    () => (int) (window.GetView().Size.Y - 0.05 * window.GetView().Size.X - 16),
    () => (int) (0.3 * window.GetView().Size.X),
    () => (int) (0.05 * window.GetView().Size.X));
serverInput.OnSubmit += (_, _) =>
{
    gameData.KnownServers.Add(serverInput.Text);
    Storage.Save(nameof(GameData.KnownServers), gameData.KnownServers);
    serverInput.Text = "";
    Task.Run(UpdateServerList);
};
serversPage.Children.Add(serverInput);
var connectButton = new Button("Connect",
    () => (int) (window.GetView().Size.X - 0.2 * window.GetView().Size.X - 16),
    () =>  (int) (window.GetView().Size.Y - 0.05 * window.GetView().Size.X - 16), 
    () => (int) (0.2 * window.GetView().Size.X), 
    () => (int) (0.05 * window.GetView().Size.X));
connectButton.OnMouseUp += (_, _) =>
{
    gameData.KnownServers.Add(serverInput.Text);
    Storage.Save(nameof(GameData.KnownServers), gameData.KnownServers);
    serverInput.Text = "";
    Task.Run(UpdateServerList);
};
serversPage.Children.Add(connectButton);


// Options page UI
var optionsPage = new Page();
optionsPage.Children.Add(dirtBackgroundRect);
var grid = new Grid(1, 6, () => (int) (window.GetView().Size.X / 4), () => (int) (window.GetView().Size.Y / 4),
    () => (int) (window.GetView().Size.X / 2), () => (int) (window.GetView().Size.Y / 2))
{
    Children =
    {
        [0, 0] = new Button("Camera: Follow player", () => 0, () => 0, () => 0, () => 0),
        [0, 1] = new Button("Framerate: 60FPS", () => 0, () => 0, () => 0, () => 0),
        [0, 2] = new Button("Sound: 75%", () => 0, () => 0, () => 0, () => 0),
        [0, 3] = new Button("Music: 75%", () => 0, () => 0, () => 0, () => 0),
        [0, 5] = new Button("Back", () => 0, () => 0, () => 0, () => 0),
    },
    RowGap = 8
};
optionsPage.Children.Add(grid);

// Options page UI
var accountsPage = new Page();
accountsPage.Children.Add(dirtBackgroundRect);

// Main page game UI
var mainPage = new Page();
var backgroundTexture = new Texture(@"Resources/Textures/Background/panorama_2.png");
var fitFactor = 0.0f;
var backgroundRect = new TextureRect(backgroundTexture,
    () => 0,
    () => 0,
    () => (int) window.GetView().Size.X,
    () => (int) window.GetView().Size.Y)
{
    SubRect = new Bounds(() => 0, () => 0, () =>
    {
        fitFactor = window.GetView().Size.X / backgroundTexture.Size.X;
        return (int) backgroundTexture.Size.X;
    }, () => (int) (backgroundTexture.Size.Y / fitFactor))
};
mainPage.Children.Add(backgroundRect);
var logoRect = new TextureRect(new Texture(@"Resources/Brand/logo.png"),
    () => (int) ((int) window.GetView().Center.X - (window.GetView().Size.X * 0.4f) / 2),
    () => (int) (window.GetView().Size.Y * 0.1f), () => (int) (window.GetView().Size.X * 0.4f),
    () => (int) (window.GetView().Size.X * 0.24f));
mainPage.Children.Add(logoRect);

var playButton = new Button("Play", 
    () => (int) (window.GetView().Center.X - 0.5 * window.GetView().Center.X),
    () => (int) (window.GetView().Size.Y * 0.5),
    () => (int) (0.5 * window.GetView().Size.X), 
    () => (int) (0.05 * window.GetView().Size.X));
playButton.OnMouseUp += (_, _) =>
{
    currentPage = serversPage;
};
mainPage.Children.Add(playButton);

var accountButton = new Button("Account & Profile", 
    () => (int) (window.GetView().Center.X - 0.5 * window.GetView().Center.X),
    () => playButton.Bounds.EndY() + 16,
    () => (int) (0.5 * window.GetView().Size.X), 
    () => (int) (0.05 * window.GetView().Size.X));
accountButton.OnMouseUp += (_, _) =>
{
    currentPage = accountsPage;
};
mainPage.Children.Add(accountButton);

var optionsButton = new Button("Options", 
    () => (int) (window.GetView().Center.X - 0.5 * window.GetView().Center.X),
    () => accountButton.Bounds.EndY() + 48,
    () => (int) (0.25 * window.GetView().Size.X - 8), 
    () => (int) (0.05 * window.GetView().Size.X));
optionsButton.OnMouseUp += (_, _) =>
{
    currentPage = optionsPage;
};
mainPage.Children.Add(optionsButton);

var quitButton = new Button("Quit", 
    () => (int) (window.GetView().Center.X + 8),
    () => accountButton.Bounds.EndY() + 48,
    () => (int) (0.25 * window.GetView().Size.X - 8), 
    () => (int) (0.05 * window.GetView().Size.X));
quitButton.OnMouseUp += (_, _) =>
{
    Environment.Exit(0);
};
mainPage.Children.Add(quitButton);

// Central server auth key page
var authPage = new Page();
authPage.Children.Add(dirtBackgroundRect);
authPage.Children.Add(dirtBackgroundRect);
var authLabel = new Label("Please enter your game invite code:", 28, Color.Yellow)
{
    Bounds = new Bounds(() => (int) (window.GetView().Size.X / 2) - 256,
        () => (int) ((int) window.GetView().Size.Y * 0.4), () => 0, () => 0)
};
authPage.Children.Add(authLabel);
var authInput = new TextInput("invite code",
    () => (int) (window.GetView().Center.X - 256),
    () => (int) window.GetView().Center.Y,
    () => 512,
    () => 64);
authPage.Children.Add(authInput);
var authButton = new Button("Continue",
    () => (int) (window.GetView().Size.X - 0.2 * window.GetView().Size.X - 16),
    () =>  (int) (window.GetView().Size.Y - 0.05 * window.GetView().Size.X - 16), 
    () => (int) (0.2 * window.GetView().Size.X), 
    () => (int) (0.05 * window.GetView().Size.X));
authButton.OnMouseUp += async (_, _) =>
{
    if (await Authorise(authInput.Text))
    {
        currentPage = mainPage;
    }
};
authPage.Children.Add(authButton);

async Task<bool> Authorise(string? key = null)
{
    if (key is null)
    {
        return false;
    }
    
    // Check key validity
    var response = await gameData.HttpClient.GetAsync("https://blobk.at:1024/" + key);
    var lines = (await response.Content.ReadAsStringAsync()).Split("\n");
    if (lines.Length != 4 || response.StatusCode != HttpStatusCode.OK)
    {
        return false;
    }
    
    gameData.Name = lines[0];
    gameData.PublicKey = lines[1];
    gameData.PrivateKey = lines[2];
    gameData.AuthSignature = lines[3];
    
    Storage.Save("AuthKey", key);
    Storage.Save(nameof(gameData.Name), gameData.Name);
    Storage.Save(nameof(GameData.PublicKey), gameData.PublicKey);
    Storage.Save(nameof(gameData.PrivateKey), gameData.PrivateKey);
    Storage.Save(nameof(gameData.AuthSignature), gameData.AuthSignature);
    return true;
}

currentPage = mainPage;
// We must use Task.Run, because SFML hates async being used on the same thread as it's onw drawing code
Task.Run(async () =>
{
    if (currentPage != authPage && !await Authorise(Storage.Get<string>("AuthKey")))
    {
        currentPage = authPage;
    }
});

// Render loop
while (window.IsOpen)
{
    window.DispatchEvents();
    window.Clear(Color.Black);
    currentPage?.Render(window, uiView);
    window.Display();
    
    Thread.Sleep(gameData.FrameSleepMs);
}