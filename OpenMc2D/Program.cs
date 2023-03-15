using System.Diagnostics.Metrics;
using System.Net;
using System.Xml.Linq;
using OpenMc2D;
using OpenMc2D.Gui;
using OpenMc2D.Networking;
using SFML.Graphics;
using SFML.Window;

Page? currentPage = null;
var client = new HttpClient();
var gameData = new GameData
{
    Name = Storage.Get<string>(nameof(GameData.Name)) ?? "",
    PublicKey = Storage.Get<string>(nameof(GameData.PublicKey)) ?? "",
    PrivateKey = Storage.Get<string>(nameof(GameData.PrivateKey)) ?? "",
    AuthSignature = Storage.Get<string>(nameof(GameData.AuthSignature)) ?? ""
};

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
    if (currentPage is null || !currentPage.HitTest(args.X, args.Y, TestType.Down))
    {
        // If not blocked by the UI, then we propagate the hit test to the main game
    }
};
window.MouseButtonReleased += (_, args) =>
{
    if (currentPage is null || !currentPage.HitTest(args.X, args.Y, TestType.Up))
    {
        // If not blocked by the UI, then we propagate the hit test to the main game
    }
};
window.MouseMoved += (_, args) =>
{
    if (currentPage is null || !currentPage.HitTest(args.X, args.Y, TestType.Hover))
    {
        // If not blocked by the UI, then we propagate the hit test to the main game
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

// Servers page UI
var serversPage = new Page();
serversPage.Children.Add(dirtBackgroundRect);
var connectButton = new Button("Connect",
    () => (int) (window.GetView().Size.X - 0.2 * window.GetView().Size.X - 16),
    () =>  (int) (window.GetView().Size.Y - 0.05 * window.GetView().Size.X - 16), 
    () => (int) (0.2 * window.GetView().Size.X), 
    () => (int) (0.05 * window.GetView().Size.X));
serversPage.Children.Add(connectButton);
var addNewButton = new Button("Add new",
    () => 16,
    () =>  (int) (window.GetView().Size.Y - 0.05 * window.GetView().Size.X - 16), 
    () => (int) (0.2 * window.GetView().Size.X), 
    () => (int) (0.05 * window.GetView().Size.X));
serversPage.Children.Add(addNewButton);
var deleteServerButton = new Button("Refresh",
    () => (int) (0.2 * window.GetView().Size.X + 32),
    () =>  (int) (window.GetView().Size.Y - 0.05 * window.GetView().Size.X - 16), 
    () => (int) (0.2 * window.GetView().Size.X), 
    () => (int) (0.05 * window.GetView().Size.X));
serversPage.Children.Add(deleteServerButton);
var serversLabel = new Label("Connect to a server:", 28, Color.White)
{
    Bounds = new Bounds(() => (int) (window.GetView().Size.X / 2) - 156, () => 128, () => 0, () => 0)
};
serversPage.Children.Add(serversLabel);
var serverList = new DisplayList(() => 64, () => 192, () => (int) (window.GetView().Size.X - 128), () => (int) (window.GetView().Size.X * 0.8));
serverList.Children = new List<DisplayListItem>
{
    new(new Texture(@"Resources/Brand/grass_icon.png"), "MY SERVER", "This is a server ever"),
    new(new Texture(@"Resources/Brand/grass_icon.png"), "MY SERVER", "This is a server ever"),
};
serversPage.Children.Add(serverList);

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
mainPage.Children.Add(accountButton);

var optionsButton = new Button("Options", 
    () => (int) (window.GetView().Center.X - 0.5 * window.GetView().Center.X),
    () => accountButton.Bounds.EndY() + 48,
    () => (int) (0.25 * window.GetView().Size.X - 8), 
    () => (int) (0.05 * window.GetView().Size.X));
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
var authLabel = new Label("Game invite code:", 28, Color.Yellow)
{
    Bounds = new Bounds(() => (int) (window.GetView().Size.X / 2) - 128, () =>  (int) ((int) window.GetView().Size.Y * 0.1), () => 0, () => 0)
};
authPage.Children.Add(authLabel);
var authButton = new Button("Continue",
    () => (int) (window.GetView().Size.X - 0.2 * window.GetView().Size.X - 16),
    () =>  (int) (window.GetView().Size.Y - 0.05 * window.GetView().Size.X - 16), 
    () => (int) (0.2 * window.GetView().Size.X), 
    () => (int) (0.05 * window.GetView().Size.X));
authButton.OnMouseUp += async (_, _) =>
{
    if (await Authorise("00000000000000000000000000000000"))
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
    var response = await client.GetAsync("https://blobk.at:1024/" + key);
    var lines = (await response.Content.ReadAsStringAsync()).Split("\n");
    if (lines.Length != 4 || response.StatusCode != HttpStatusCode.OK)
    {
        return false;
    }
    
    Storage.Save("AuthKey", key);
    gameData.PublicKey = lines[0];
    Storage.Save(nameof(GameData.PublicKey), lines[0]);
    gameData.PrivateKey = lines[1];
    Storage.Save(nameof(gameData.PrivateKey), lines[1]);
    gameData.AuthSignature = lines[3];
    Storage.Save(nameof(gameData.AuthSignature), lines[2]);
    gameData.Name =  lines[3];
    Storage.Save(nameof(gameData.Name), lines[3]);
    return true;
}

currentPage = authPage;
//currentPage = await Authorise(Storage.Get<string>("AuthKey")) ? mainPage : authPage;

// Render loop
while (window.IsOpen)
{
    window.DispatchEvents();
    window.Clear(Color.Black);
    currentPage?.Render(window, uiView);
    window.Display();
    Thread.Sleep(gameData.FrameSleepMs);
}