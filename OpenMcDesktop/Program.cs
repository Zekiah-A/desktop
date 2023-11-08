using System.Net;
using System.Resources;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenMcDesktop;
using OpenMcDesktop.Gui;
using OpenMcDesktop.Networking;
using NativeFileDialogSharp;
using OpenMcDesktop.Game;
using OpenMcDesktop.Mods;
using OpenMcDesktop.Translations;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

var mainPage = new Page();
var serversPage = new Page();
var optionsPage = new Page();
var accountsPage = new Page();
var authPage = new Page();

// Window event listeners and input
var storagePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenMcDesktop");
var window = new RenderWindow(new VideoMode(1540, 1080), "open-mc");
var dirtBackgroundRect = new TextureRect(new Texture(@"Resources/Brand/dirt_background.png") { Repeated = true },
    Control.BoundsZero,
    Control.BoundsZero,
    () => (int) window.GetView().Size.X,
    () => (int) window.GetView().Size.Y)
{
    SubRect = new Bounds(Control.BoundsZero, Control.BoundsZero, () => (int) window.GetView().Size.X / 2, () => (int) window.GetView().Size.Y / 2)
};
var storage = new Storage(storagePath);
var gameData = new GameData
{
    Name = storage.Get<string>(nameof(GameData.Name)) ?? "",
    PublicKey = storage.Get<string>(nameof(GameData.PublicKey)) ?? "",
    PrivateKey = storage.Get<string>(nameof(GameData.PrivateKey)) ?? "",
    AuthSignature = storage.Get<string>(nameof(GameData.AuthSignature)) ?? "",
    KnownServers = storage.Get<List<string>>(nameof(GameData.KnownServers)) ?? new List<string> { "localhost" },
    Skin = SkinHelpers.SkinDataFromFile("Resources/Textures/alex.png"),
    Storage = storage,
    Window = window,
    WorldLayer = new View(new FloatRect(0, 0, window.Size.X, window.Size.Y)),
    BackgroundLayer = new View(new FloatRect(0, 0, window.Size.X, window.Size.Y)),
    UiLayer = new View(new FloatRect(0, 0, window.Size.X, window.Size.Y)),
    DirtBackgroundRect = dirtBackgroundRect,
    Logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(OpenMcDesktop)),
    Translations = ResourceManager.CreateFileBasedResourceManager("Translations", "Locale/", typeof(Translations)),
    ModLoader = new ModLoader()
};
StaticData.GameData = gameData;

// TODO: Make configurable, perhaps a setter in GameData
window.SetFramerateLimit(60);

window.Closed += (_, _) =>
{
    gameData.Logger.LogInformation("Quit received. Exiting.");
    window.Close();
};
window.Resized += (_, args) =>
{
    gameData.WorldLayer = new View(new FloatRect(0, 0, args.Width, args.Height));
    gameData.UiLayer = new View(new FloatRect(0, 0, args.Width, args.Height));
};
window.MouseButtonPressed += (_, args) =>
{
    if (window.HasFocus())
    {
        if (gameData.CurrentPage?.HitTest(args.X, args.Y, TestType.MouseDown) is false)
        {
            // If not blocked by the UI, then we propagate the hit test to the main game
            Keybinds.MouseDown(args.X, args.Y, TestType.MouseDown);
        }
    }
};
window.MouseButtonReleased += (_, args) =>
{
    if (window.HasFocus())
    {
        if (gameData.CurrentPage?.HitTest(args.X, args.Y, TestType.MouseUp) is false)
        {
            // If not blocked by the UI, then we propagate the hit test to the main game
            Keybinds.MouseUp(args.X, args.Y, TestType.MouseUp);
        }
    }
};
window.MouseMoved += (_, args) =>
{
    if (!window.HasFocus())
    {
        return;
    }
    
    if (gameData.CurrentPage?.HitTest(args.X, args.Y, TestType.MouseHover) is false)
    {
        // If not blocked by the UI, then we propagate the hit test to the main game
        Keybinds.MouseMove(args.X, args.Y, TestType.MouseHover);
    }
};

void PropagateKeyTest(KeyEventArgs args, TestType type)
{
    var modifiers = 0;
    modifiers |= args.Alt ? (int) ModifierFlags.Alt : 0;
    modifiers |= args.Control ? (int) ModifierFlags.Control : 0;
    modifiers |= args.Shift ? (int) ModifierFlags.Shift : 0;
    modifiers |= args.System ? (int) ModifierFlags.System : 0;

    if (gameData.CurrentPage?.KeyboardTest(args.Code, modifiers, type) is false)
    {
        // If not blocked by the UI, then we propagate the keyboard test to the main game
        Keybinds.KeyPressed(args.Code, modifiers, type);
    }
}

window.KeyPressed += (_, args) =>
{
    if (window.HasFocus())
    {
        PropagateKeyTest(args, TestType.KeyDown);
    }
};
window.KeyReleased += (_, args) =>
{
    if (window.HasFocus())
    {
        PropagateKeyTest(args, TestType.KeyUp);
    }
};
window.TextEntered += (_, args) =>
{
    if (window.HasFocus())
    {
        gameData.CurrentPage?.TextTest(args.Unicode);
    }
};

AppDomain.CurrentDomain.UnhandledException += (sender, exceptionEventArgs) =>
{
    gameData.Logger.LogCritical("Critical game error in module {sender}, {exceptionEventArgs}: ",
        sender, exceptionEventArgs.ExceptionObject);
};

var connections = new Connections(gameData);
var preConnections = new List<PreConnectData>();


gameData.DirtBackgroundRect = dirtBackgroundRect;

// Game window icon
var icon = new Image("Resources/Brand/grass_icon.png");
window.SetIcon(icon.Size.X, icon.Size.Y, icon.Pixels);

async Task PlayServer(PreConnectData serverData)
{
    foreach (var data in preConnections.Where(data => data.Socket != serverData.Socket))
    {
        try
        {
            await data.Socket.StopAsync();
        }
        catch (Exception) { /* Ignore */ }
    }
    
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

var serverListUpdating = false;

async Task ConnectToKnownServer(string serverIp)
{
    var listItem = new ServerListItem(new Texture(@"Resources/Brand/grass_icon.png"), serverIp, "Connecting...", serverIp);
    serverList.Children.Add(listItem);
        
    var connectionData = await connections.PreConnect(serverIp, listItem);
    preConnections.Add(connectionData);
    listItem.OnDoubleClick += async (_, _) =>
    {
        await PlayServer(connectionData);
    };
    listItem.OnMouseUp += (_, _) =>
    {
        if (serverList.SelectedIndex != -1)
        {
            serverList.Children[serverList.SelectedIndex].Selected = false;
        }

        serverList.SelectedIndex = serverList.Children.IndexOf(listItem);
        listItem.Selected = true;
    };
}

async Task UpdateServerList()
{
    if (serverListUpdating)
    {
        return;
    }
    serverListUpdating = true;

    var disconnectionTasks = new List<Task>();
    foreach (var connection in preConnections)
    {
        disconnectionTasks.Add(Task.Run(async () =>
        {
            try
            {
                await connection.Socket.StopAsync();
            }
            catch (Exception) { /* Ignore */ }
        }));
    }
    await Task.WhenAll(disconnectionTasks);
    serverList.SelectedIndex = -1;
    serverList.Children.Clear();
    preConnections.Clear();
    
    var connectionTasks = new List<Task>();
    foreach (var serverIp in gameData.KnownServers)
    {
        connectionTasks.Add(ConnectToKnownServer(serverIp));
    }
    
    await Task.WhenAll(connectionTasks);
    serverListUpdating = false;
}

Task.Run(UpdateServerList);
var serverDeleteButton = new Button("Delete",
    () => 16,
    () =>  (int) (window.GetView().Size.Y - 0.05 * window.GetView().Size.X - 16), 
    () => (int) (0.2 * window.GetView().Size.X), 
    () => (int) (0.05 * window.GetView().Size.X));
serverDeleteButton.OnMouseUp += (_, _) =>
{
    if (serverList.SelectedIndex == -1)
    {
        return;
    }
    
    gameData.KnownServers.RemoveAt(serverList.SelectedIndex);
    storage.Save(nameof(GameData.KnownServers), gameData.KnownServers);
    serverList.SelectedIndex = -1;
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
var optionsSoundSlider = new Slider(7, "Sound: 70%");
optionsSoundSlider.ValueChanged += (_, _) =>
{
    optionsSoundSlider.Text = $"Sound: {(int) ((float) optionsSoundSlider.Value / optionsSoundSlider.MaxValue * 100)}%";
};
var optionsMusicSlider = new Slider(7, "Music: 70%");
optionsMusicSlider.ValueChanged += (_, _) =>
{
    optionsMusicSlider.Text = $"Music: {(int) ((float) optionsMusicSlider.Value / optionsMusicSlider.MaxValue * 100)}%";
};

var optionsBackButton = new Button("Back", () => 0, () => 0, () => 0, () => 0);
optionsBackButton.OnMouseUp += (_, _) =>
{
    gameData.CurrentPage = mainPage;
};
var optionsGrid = new Grid(1, 6, () => (int) (window.GetView().Size.X / 4), () => (int) (window.GetView().Size.Y / 4),
    () => (int) (window.GetView().Size.X / 2), () => (int) (window.GetView().Size.Y / 2))
{
    Children =
    {
        [0, 0] = new Button("Camera: Follow player"),
        [0, 1] = new Button("Framerate: 60FPS"),
        [0, 2] = optionsSoundSlider,
        [0, 3] = optionsMusicSlider,
        [0, 5] = optionsBackButton
    },
    RowGap = 8
};
optionsPage.Children.Add(optionsGrid);

// Accounts and profile page UI
accountsPage.Children.Add(dirtBackgroundRect);
var skinEditor = new SkinEditor(gameData.Skin, () => (int) (window.GetView().Center.X - 164), () => 32, () => 328, () => 648)
{
    Display =
    {
        Animate = true
    }
};
accountsPage.Children.Add(skinEditor);
var skinButton = new Button("Change skin");
skinButton.OnMouseUp += (_, _) =>
{
    var file = Dialog.FileOpen("png,jpg,webp", Directory.GetCurrentDirectory());
    if (file?.Path is null)
    {
        return;
    }
    
    var skinDataFromFile = SkinHelpers.SkinDataFromFile(file.Path);
    gameData.Skin = skinDataFromFile;
    skinEditor.Data = skinDataFromFile;
};
var accountsBackButton = new Button("Back");
accountsBackButton.OnMouseUp += (_, _) => 
{
    gameData.CurrentPage = mainPage;
};
var accountsGrid = new Grid(1, 2, () => (int) (window.GetView().Size.X / 4), () => (int) (window.GetView().Size.Y * 0.75f),
    () => (int) (window.GetView().Size.X / 2), () => 136)
{
    Children =
    {
        [0, 0] = skinButton,
        [0, 1] = accountsBackButton,
    },
    RowGap = 8
};
accountsPage.Children.Add(accountsGrid);

// Title screen fallback if title screen video is unable to play, and appears when the video has finished
var backgroundTexture = new Texture(@"Resources/Textures/Background/panorama_0.png");
var backgroundRect = new TextureRect(backgroundTexture,
    Control.BoundsZero,
    Control.BoundsZero,
    () => (int) window.GetView().Size.X,
    () => (int) window.GetView().Size.Y)
{
    SubRect = new Bounds(Control.BoundsZero, Control.BoundsZero,
        () => (int) window.GetView().Size.X,
        () => (int) window.GetView().Size.Y)
};
mainPage.Children.Add(backgroundRect);

// Game start title intro video player
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    /*var titleVideoPlayer = new VideoPlayer("Resources/Brand/title_video.mkv",
        Control.BoundsZero,
        Control.BoundsZero,
        () => (int) window.GetView().Size.X,
        () => (int) window.GetView().Size.Y);
    titleVideoPlayer.Source.Play();
    titleVideoPlayer.Source.EndOfFileReached += (_, _) =>
    {
        mainPage.Children.Remove(titleVideoPlayer);
    };
    mainPage.Children.Add(titleVideoPlayer);*/
}

int LogoWidth() => (int) (window.GetView().Size.X * 0.57f);
int LogoHeight() => (int) (window.GetView().Size.X * 0.2f);
var logoRect = new TextureRect(new Texture(@"Resources/Brand/logo.png"),
    () => (int) window.GetView().Center.X - (LogoWidth() / 2),
    () => (int) (window.GetView().Size.Y * 0.1f),
    LogoWidth, LogoHeight);
mainPage.Children.Add(logoRect);

var playButton = new Button("Play", 
    () => (int) (window.GetView().Center.X - 0.5 * window.GetView().Center.X),
    () => (int) (window.GetView().Size.Y * 0.5),
    () => (int) (0.5 * window.GetView().Size.X), 
    () => (int) (0.05 * window.GetView().Size.X));
playButton.OnMouseUp += (_, _) =>
{
    gameData.CurrentPage = serversPage;
};
mainPage.Children.Add(playButton);

var accountButton = new Button("Account & Profile", 
    () => (int) (window.GetView().Center.X - 0.5 * window.GetView().Center.X),
    () => playButton.Bounds.EndY() + 16,
    () => (int) (0.5 * window.GetView().Size.X), 
    () => (int) (0.05 * window.GetView().Size.X));
accountButton.OnMouseUp += (_, _) =>
{
    gameData.CurrentPage = accountsPage;
};
mainPage.Children.Add(accountButton);

var optionsButton = new Button("Options", 
    () => (int) (window.GetView().Center.X - 0.5 * window.GetView().Center.X),
    () => accountButton.Bounds.EndY() + 48,
    () => (int) (0.25 * window.GetView().Size.X - 8), 
    () => (int) (0.05 * window.GetView().Size.X));
optionsButton.OnMouseUp += (_, _) =>
{
    gameData.CurrentPage = optionsPage;
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

var disclaimerLabel = new Label(
    "Open source project. Not affiliated with Mojang Studios",
    24,
    Color.White,
    () => 8,
    () => (int) window.GetView().Size.Y - 32);
mainPage.Children.Add(disclaimerLabel);

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
        gameData.CurrentPage = mainPage;
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

gameData.CurrentPage = mainPage;
Task.Run(async () =>
{
    if (gameData.CurrentPage != authPage && !await Authorise(storage.Get<string>("AuthKey")))
    {
        gameData.CurrentPage = authPage;
    }
    
    await gameData.Host.RunAsync();
});

// Update loop
Task.Run(async () =>
{
    var updateClock = new Clock();

    while (true)
    {
        var deltaTime = updateClock.ElapsedTime.AsSeconds();
        updateClock.Restart();

        gameData.World?.Update(deltaTime);
        await Task.Delay(16, CancellationToken.None);
    }
});

// Render loop
var renderClock = new Clock();
while (window.IsOpen)
{
    var deltaTime = renderClock.ElapsedTime.AsSeconds();
    renderClock.Restart();
    
    window.DispatchEvents();
    window.SetView(gameData.WorldLayer);
    gameData.World?.Render(window, gameData.WorldLayer, gameData.BackgroundLayer, deltaTime);
    window.SetView(gameData.UiLayer);
    gameData.CurrentPage?.Render(window, gameData.UiLayer, deltaTime);
    window.Display();
}