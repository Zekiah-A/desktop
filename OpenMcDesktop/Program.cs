using System.Net;
using OpenMcDesktop;
using OpenMcDesktop.Gui;
using OpenMcDesktop.Networking;
using NativeFileDialogSharp;
using OpenMcDesktop.Game;
using OpenMcDesktop.Mods;
using SFML.Graphics;
using SFML.Window;

var mainPage = new Page();
var gamePage = new Page();
var serversPage = new Page();
var optionsPage = new Page();
var accountsPage = new Page();
var authPage = new Page();
var gameGuiPage = new Page();

// Window event listeners and input
var window = new RenderWindow(new VideoMode(1540, 1080), "OpenMc");
var view = new View(new FloatRect(0, 0, window.Size.X, window.Size.Y));
var storage = new Storage(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenMcDesktop"));
var gameData = new GameData
{
    Name = storage.Get<string>(nameof(GameData.Name)) ?? "",
    PublicKey = storage.Get<string>(nameof(GameData.PublicKey)) ?? "",
    PrivateKey = storage.Get<string>(nameof(GameData.PrivateKey)) ?? "",
    AuthSignature = storage.Get<string>(nameof(GameData.AuthSignature)) ?? "",
    KnownServers = storage.Get<List<string>>(nameof(GameData.KnownServers)) ?? new List<string> { "localhost" },
    Skin = SkinHelpers.SkinDataFromFile("Resources/Textures/alex.png"),
    Storage = storage,
    View = view
};
gameData.ModLoader = new ModLoader(gameData);
StaticData.GameData = gameData;

// TODO: Make configurable, perhaps a setter in GameData
window.SetFramerateLimit(60);

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
    if (gameData.CurrentPage?.HitTest(args.X, args.Y, TestType.MouseDown) is false)
    {
        // If not blocked by the UI, then we propagate the hit test to the main game
        Keybinds.MouseDown(args.X, args.Y, TestType.MouseDown);
    }
};
window.MouseButtonReleased += (_, args) =>
{
    if (gameData.CurrentPage?.HitTest(args.X, args.Y, TestType.MouseUp) is false)
    {
        // If not blocked by the UI, then we propagate the hit test to the main game
        Keybinds.MouseUp(args.X, args.Y, TestType.MouseUp);
    }
};
window.MouseMoved += (_, args) =>
{
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

window.KeyPressed += (_, args) => PropagateKeyTest(args, TestType.KeyDown);
window.KeyReleased += (_, args) => PropagateKeyTest(args, TestType.KeyUp);
window.TextEntered += (_, args) => gameData.CurrentPage?.TextTest(args.Unicode);

AppDomain.CurrentDomain.UnhandledException += (sender, exceptionEventArgs) =>
{
    Console.WriteLine($"Critical game error in module {sender} " + exceptionEventArgs.ExceptionObject);
};

var connections = new Connections(gameData);
var preConnections = new List<PreConnectData>();

// Dirt background rect used on many pages
var dirtBackgroundRect = new TextureRect(new Texture(@"Resources/Brand/dirt_background.png") { Repeated = true },
    Control.BoundsZero,
    Control.BoundsZero,
    () => (int) window.GetView().Size.X,
    () => (int) window.GetView().Size.Y)
{
    SubRect = new Bounds(Control.BoundsZero, Control.BoundsZero, () => (int) window.GetView().Size.X / 2, () => (int) window.GetView().Size.Y / 2)
};

// Game window icon
var icon = new Image("Resources/Brand/grass_icon.png");
window.SetIcon(icon.Size.X, icon.Size.Y, icon.Pixels);

async Task PlayServer(PreConnectData serverData)
{
    foreach (var data in preConnections.Where(data => data.Socket != serverData.Socket))
    {
        await data.Socket.StopAsync();
    }
    
    gameData.CurrentPage = gamePage;
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
    gameData.CurrentPage = mainPage;
};
var optionsGrid = new Grid(1, 6, () => (int) (window.GetView().Size.X / 4), () => (int) (window.GetView().Size.Y / 4),
    () => (int) (window.GetView().Size.X / 2), () => (int) (window.GetView().Size.Y / 2))
{
    Children =
    {
        [0, 0] = new Button("Camera: Follow player", Control.BoundsZero, Control.BoundsZero, Control.BoundsZero, Control.BoundsZero),
        [0, 1] = new Button("Framerate: 60FPS", Control.BoundsZero, Control.BoundsZero, Control.BoundsZero, Control.BoundsZero),
        [0, 2] = new Button("Sound: 75%", Control.BoundsZero, Control.BoundsZero, Control.BoundsZero, Control.BoundsZero),
        [0, 3] = new Button("Music: 75%", Control.BoundsZero, Control.BoundsZero, Control.BoundsZero, Control.BoundsZero),
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
var skinButton = new Button("Change skin", Control.BoundsZero, Control.BoundsZero, Control.BoundsZero, Control.BoundsZero);
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
var accountsBackButton = new Button("Back", Control.BoundsZero, Control.BoundsZero, Control.BoundsZero, Control.BoundsZero);
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
var fitFactor = 0.0f;
var backgroundRect = new TextureRect(backgroundTexture,
    Control.BoundsZero,
    Control.BoundsZero,
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

// Game start title intro video player
var titleVideoPlayer = new VideoPlayer("Resources/Brand/title_video.mkv",
    Control.BoundsZero,
    Control.BoundsZero,
    () => (int) window.GetView().Size.X,
    () => (int) window.GetView().Size.Y);
titleVideoPlayer.Source.Play();
titleVideoPlayer.Source.EndOfFileReached += (_, _) =>
{
    mainPage.Children.Remove(titleVideoPlayer);
};
mainPage.Children.Add(titleVideoPlayer);

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

gameGuiPage.Children.Add(
    new Grid(2, 2, Control.BoundsZero, Control.BoundsZero, () => (int) window.GetView().Size.X, () => (int) window.GetView().Size.Y)
    {
        Children =
        {
            [0, 0] = new Hotbar(Control.BoundsZero, Control.BoundsZero, Control.BoundsZero, Control.BoundsZero),
        },
        RowGap = 8
    }
);
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
});


// Render loop
while (window.IsOpen)
{
    window.DispatchEvents();
    window.Clear(Color.Black);
    gameData.CurrentPage?.Render(window, view);
    gameData.World?.Render(window, view);
    window.SetView(view);
    window.Display();
}