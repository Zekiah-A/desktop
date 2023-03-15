using OpenMc2D.Gui;
using SFML.Graphics;
using SFML.Window;

Page? currentPage = null;

// Window event listeners and input
var window = new RenderWindow(new VideoMode(1540, 1080), "OpenMc2d");
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

// Servers page UI
var serversPage = new Page();
var serversBackgroundRect = new TextureRect(new Texture(@"Resources/Brand/dirt_background.png") { Repeated = true },
    () => 0,
    () => 0,
    () => (int) window.GetView().Size.X,
    () => (int) window.GetView().Size.Y)
{
    SubRect = new Bounds(() => 0, () => 0, () => (int) window.GetView().Size.X / 2, () => (int) window.GetView().Size.Y / 2)
};
serversPage.Children.Add(serversBackgroundRect);
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
    Bounds = new Bounds(() => (int) (window.GetView().Size.X / 2), () => 0, () => 0, () => 0)
};

serversPage.Children.Add(serversLabel);

// Main page game UI
var mainPage = new Page();
var backgroundRect = new TextureRect(new Texture(@"Resources/Textures/Background/panorama_2.png"),
    () => 0,
    () => 0,
    () => (int) window.GetView().Size.X,
    () => (int) window.GetView().Size.Y);
mainPage.Children.Add(backgroundRect);

var logoRect = new TextureRect(new Texture(@"Resources/Brand/logo.png"), () => (int) ((int) window.GetView().Center.X - (window.GetView().Size.X * 0.4f) / 2), () => (int) (window.GetView().Size.Y * 0.1f), () => (int) (window.GetView().Size.X * 0.4f), () => (int) (window.GetView().Size.X * 0.24f));
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

currentPage = mainPage;

// Render loop
while (window.IsOpen)
{
    window.DispatchEvents();
    window.Clear(Color.Black);

    currentPage?.Render(window);

    window.Display();
    Thread.Sleep(16);
}