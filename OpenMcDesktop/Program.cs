using System.Net;
using OpenMcDesktop;
using OpenMcDesktop.Gui;
using OpenMcDesktop.Networking;
using NativeFileDialogSharp;
using OpenMcDesktop.Mods;
using SFML.Graphics;
using SFML.Window;

var currentPage = (Page?) null;
var mainPage = new Page();
var gamePage = new Page();
var serversPage = new Page();
var optionsPage = new Page();
var accountsPage = new Page();
var authPage = new Page();

// Window event listeners and input
var window = new RenderWindow(new VideoMode(1540, 1080), "OpenMc");
var view = new View();
window.Closed += (_, _) =>
{
    window.Close();
};
window.Resized += (_, args) =>
{
    view = new View(new FloatRect(0, 0, args.Width, args.Height));
    window.SetView(view);
};
window.MouseButtonPressed += (_, args) =>
{
    // ReSharper disable once AccessToModifiedClosure
    if (currentPage?.HitTest(args.X, args.Y, TestType.MouseDown) is false)
    {
        // If not blocked by the UI, then we propagate the hit test to the main game
    }
};
window.MouseButtonReleased += (_, args) =>
{
    // ReSharper disable once AccessToModifiedClosure
    if (currentPage?.HitTest(args.X, args.Y, TestType.MouseUp) is false)
    {
        // If not blocked by the UI, then we propagate the hit test to the main game
    }
};
window.MouseMoved += (_, args) =>
{
    // ReSharper disable once AccessToModifiedClosure
    if (currentPage?.HitTest(args.X, args.Y, TestType.MouseHover) is false)
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

    // ReSharper disable once AccessToModifiedClosure
    if (currentPage?.KeyboardTest(args.Code, modifiers, type) is false)
    {
        // If not blocked by the UI, then we propagate the keyboard test to the main game
    }
}

window.KeyPressed += (_, args) => PropagateKeyTest(args, TestType.KeyDown);
window.KeyReleased += (_, args) => PropagateKeyTest(args, TestType.KeyUp);
window.TextEntered += (_, args) =>
{
    // ReSharper disable once AccessToModifiedClosure
    if (currentPage?.TextTest(args.Unicode) is false)
    {
        // If not blocked by the UI, then we propagate the text test to the main game
    }
};

AppDomain.CurrentDomain.UnhandledException += (sender, exceptionEventArgs) =>
{
    Console.WriteLine("Critical game error!  " + exceptionEventArgs.ExceptionObject);
    Environment.Exit(0);
};

var storage = new Storage(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenMcDesktop"));
var gameData = new GameData
{
    Name = storage.Get<string>(nameof(GameData.Name)) ?? "",
    PublicKey = storage.Get<string>(nameof(GameData.PublicKey)) ?? "",
    PrivateKey = storage.Get<string>(nameof(GameData.PrivateKey)) ?? "",
    AuthSignature = storage.Get<string>(nameof(GameData.AuthSignature)) ?? "",
    KnownServers = storage.Get<List<string>>(nameof(GameData.KnownServers)) ?? new List<string> { "localhost" },
    Storage = storage,
    View = view
};
gameData.ModLoader = new ModLoader(gameData);

var connections = new Connections(gameData);
var preConnections = new List<PreConnectData>();

var dirtBackgroundRect = new TextureRect(new Texture(@"Resources/Brand/dirt_background.png") { Repeated = true },
    () => 0,
    () => 0,
    () => (int) window.GetView().Size.X,
    () => (int) window.GetView().Size.Y)
{
    SubRect = new Bounds(() => 0, () => 0, () => (int) window.GetView().Size.X / 2, () => (int) window.GetView().Size.Y / 2)
};


async Task PlayServer(PreConnectData serverData)
{
    foreach (var data in preConnections.Where(data => data.Socket != serverData.Socket))
    {
        await data.Socket.StopAsync();
    }
    
    currentPage = gamePage;
    await connections.Connect(serverData);
}

// Servers page UI
serversPage.Children.Add(dirtBackgroundRect);
var serversLabel = new Label("Connect to a server:", 28, Color.White)
{
    Bounds = new Bounds(() => (int) (window.GetView().Size.X / 2) - 156, () => 128, () => 0, () => 0)
};
serversPage.Children.Add(serversLabel);
var serverList = new DisplayList(() => 64, () => 192,
    () => (int) (window.GetView().Size.X - 128),
    () => (int) (window.GetView().Size.X * 0.8));

void UpdateServerList()
{
    serverList.Children.Clear();
    preConnections.Clear();
    
    foreach (var serverIp in gameData.KnownServers)
    {
        _ = Task.Run(async () =>
        {
            var connectionData = await connections.PreConnect(serverIp);
            preConnections.Add(connectionData);
            connectionData.Item.OnMouseUp += async (_, _) => await PlayServer(connectionData);
            serverList.Children.Add(connectionData.Item);
        });
    }
}

UpdateServerList();
var serverDeleteButton = new Button("Delete",
    () => 16,
    () =>  (int) (window.GetView().Size.Y - 0.05 * window.GetView().Size.X - 16), 
    () => (int) (0.2 * window.GetView().Size.X), 
    () => (int) (0.05 * window.GetView().Size.X));
serverDeleteButton.OnMouseUp += (_, _) =>
{
    gameData.KnownServers.RemoveAt(gameData.KnownServers.Count - 1);
    storage.Save(nameof(GameData.KnownServers), gameData.KnownServers);
    UpdateServerList();
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
    storage.Save(nameof(GameData.KnownServers), gameData.KnownServers);
    serverInput.Text = "";
    Task.Run(UpdateServerList);
};
serversPage.Children.Add(serverInput);
var serverAddButton = new Button("Add server",
    () => (int) (0.7 * window.GetView().Size.X + 72),
    () => (int) (window.GetView().Size.Y - 0.05 * window.GetView().Size.X - 16),
    () => (int) (0.25 * window.GetView().Size.X),
    () => (int) (0.05 * window.GetView().Size.X));
serverAddButton.OnMouseUp += (_, _) =>
{
    gameData.KnownServers.Add(serverInput.Text);
    storage.Save(nameof(GameData.KnownServers), gameData.KnownServers);
    serverInput.Text = "";
    Task.Run(UpdateServerList);
};
serversPage.Children.Add(serverAddButton);

// Options page UI
optionsPage.Children.Add(dirtBackgroundRect);
var optionsBackButton = new Button("Back", () => 0, () => 0, () => 0, () => 0);
optionsBackButton.OnMouseUp += (_, _) =>
{
    currentPage = mainPage;
};
var optionsGrid = new Grid(1, 6, () => (int) (window.GetView().Size.X / 4), () => (int) (window.GetView().Size.Y / 4),
    () => (int) (window.GetView().Size.X / 2), () => (int) (window.GetView().Size.Y / 2))
{
    Children =
    {
        [0, 0] = new Button("Camera: Follow player", () => 0, () => 0, () => 0, () => 0),
        [0, 1] = new Button("Framerate: 60FPS", () => 0, () => 0, () => 0, () => 0),
        [0, 2] = new Button("Sound: 75%", () => 0, () => 0, () => 0, () => 0),
        [0, 3] = new Button("Music: 75%", () => 0, () => 0, () => 0, () => 0),
        [0, 5] = optionsBackButton
    },
    RowGap = 8
};
optionsPage.Children.Add(optionsGrid);

// Options page UI
accountsPage.Children.Add(dirtBackgroundRect);
var skinButton = new Button("Change skin", () => 0, () => 0, () => 0, () => 0);
skinButton.OnMouseUp += async (_, _) =>
{
    var file = Dialog.FileOpen("png", Directory.GetCurrentDirectory());
    if (file?.Path is null)
    {
        return;
    }

    var skinData = await File.ReadAllBytesAsync(file.Path);
    if (skinData.Length == 1008)
    {
        gameData.Skin = skinData;
    }
};
var accountsBackButton = new Button("Back", () => 0, () => 0, () => 0, () => 0);
accountsBackButton.OnMouseUp += (_, _) => 
{
    currentPage = mainPage;
};
var accountsGrid = new Grid(1, 6, () => (int) (window.GetView().Size.X / 4), () => (int) (window.GetView().Size.Y / 4),
    () => (int) (window.GetView().Size.X / 2), () => (int) (window.GetView().Size.Y / 2))
{
    Children =
    {
        [0, 3] = skinButton,
        [0, 5] = accountsBackButton,
    },
    RowGap = 8
};
accountsPage.Children.Add(accountsGrid);

var backgroundTexture = new Texture(@"Resources/Textures/Background/panorama_0.png");
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
    }, () => (int) Math.Min(window.GetView().Size.Y, backgroundTexture.Size.Y / fitFactor))
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
    
    storage.Save("AuthKey", key);
    storage.Save(nameof(gameData.Name), gameData.Name);
    storage.Save(nameof(GameData.PublicKey), gameData.PublicKey);
    storage.Save(nameof(gameData.PrivateKey), gameData.PrivateKey);
    storage.Save(nameof(gameData.AuthSignature), gameData.AuthSignature);
    return true;
}

currentPage = mainPage;
Task.Run(async () =>
{
    if (currentPage != authPage && !await Authorise(storage.Get<string>("AuthKey")))
    {
        currentPage = authPage;
    }
});

// Render loop
while (window.IsOpen)
{
    window.DispatchEvents();
    window.Clear(Color.Black);
    currentPage?.Render(window, view);
    gameData.World?.Render(window, view);
    window.SetView(view);
    window.Display();
    
    Thread.Sleep(gameData.FrameSleepMs);
}