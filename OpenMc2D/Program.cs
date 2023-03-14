using System.Runtime.InteropServices.ComTypes;
using OpenMc2D.Gui;
using SFML.Graphics;
using SFML.Window;

var window = new RenderWindow(new VideoMode(800, 600), "OpenMc2d");
window.Closed += (_, _) =>
{
    window.Close();
};
window.Resized += (_, args) =>
{
    var view = new View(new FloatRect(0, 0, args.Width, args.Height));
    window.SetView(view);
};

// Main page game UI
var mainPage = new Page(() => 0, () => 0, () => 0, () => 0);
window.MouseButtonPressed += (_, args) =>
{
    mainPage.HitTest(args.X, args.Y, TestType.Down);
};
window.MouseButtonReleased += (_, args) =>
{
    mainPage.HitTest(args.X, args.Y, TestType.Up);
};
window.MouseMoved += (_, args) =>
{
    mainPage.HitTest(args.X, args.Y, TestType.Hover);
};

var backgroundStream = File.OpenRead(@"Resources/Textures/Background/panorama_2.png");
var backgroundTexture = new Texture(new Image(backgroundStream))
{
    Repeated = true
};
var backgroundRect = new TextureRect(backgroundTexture, () => 0, () => 0, () => (int) window.GetView().Size.X, () => (int) window.GetView().Size.Y);
mainPage.Children.Add(backgroundRect);

var playButton = new Button("Play", 
    () => (int) (window.GetView().Center.X - 0.5 * window.GetView().Center.X),
    () => (int) (window.GetView().Size.Y * 0.5),
    () => (int) (0.5 * window.GetView().Size.X), 
    () => (int) (0.05 * window.GetView().Size.X));
playButton.OnMouseUp += (_, _) =>
{
    mainPage = null;
};
mainPage.Children.Add(playButton);

var accountButton = new Button("Account & Profile", 
    () => (int) (window.GetView().Center.X - 0.5 * window.GetView().Center.X),
    () => (int) (window.GetView().Size.Y * 0.58),
    () => (int) (0.5 * window.GetView().Size.X), 
    () => (int) (0.05 * window.GetView().Size.X));
mainPage.Children.Add(accountButton);

var optionsButton = new Button("Options", 
    () => (int) (window.GetView().Center.X - (0.5 * window.GetView().Center.X)),
    () => (int) (window.GetView().Size.Y * 0.7),
    () => (int) (0.25 * window.GetView().Size.X - 8), 
    () => (int) (0.05 * window.GetView().Size.X));
mainPage.Children.Add(optionsButton);

var quitButton = new Button("Quit", 
    () => (int) (window.GetView().Center.X + 8),
    () => (int) (window.GetView().Size.Y * 0.7),
    () => (int) (0.25 * window.GetView().Size.X - 8), 
    () => (int) (0.05 * window.GetView().Size.X));
quitButton.OnMouseUp += (_, _) =>
{
    Environment.Exit(0);
};
mainPage.Children.Add(quitButton);


// Render loop
while (window.IsOpen)
{
    window.DispatchEvents();
    window.Clear(Color.Black);

    mainPage?.Render(window);

    window.Display();
    Thread.Sleep(16);
}