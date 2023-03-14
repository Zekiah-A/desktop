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

var backgroundStream = File.OpenRead(@"Resources/Brand/dirt_background.png");
var backgroundTexture = new Texture(new Image(backgroundStream))
{
    Repeated = true
};
var backgroundRect = new TextureRect(backgroundTexture, () => 0, () => 0, () => (int) window.GetView().Size.X, () => (int) window.GetView().Size.Y);
mainPage.Children.Add(backgroundRect);

var button = new Button("Play game", 
    () => (int) (window.GetView().Center.X - 0.5 * window.GetView().Center.X),
    () => (int) (window.GetView().Size.Y * 0.8),
    () => (int) (0.5 * window.GetView().Size.X), 
    () => (int) (0.05 * window.GetView().Size.X));
button.OnMouseUp += (_, _) =>
{
    mainPage = null;
};

mainPage.Children.Add(button);

// Render loop
while (window.IsOpen)
{
    window.DispatchEvents();
    window.Clear(Color.Black);

    if (mainPage != null)
        mainPage.Render(window);
    
    window.Display();
    Thread.Sleep(16);
}