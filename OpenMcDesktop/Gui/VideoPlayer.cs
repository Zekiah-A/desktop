using MotionNET;
using MotionNET.SFML;
using SFML.Graphics;
using SFML.System;

namespace OpenMcDesktop.Gui;

public class VideoPlayer : Control
{
    public DataSource Source;
    public SfmlVideoPlayback VideoPlayback;
    public bool LoadingSuccess { get; }

    public VideoPlayer(string path, Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base(x, y, width, height)
    {
        Source = new DataSource();
        LoadingSuccess = Source.LoadFromFile(path);
        VideoPlayback = new SfmlVideoPlayback(Source);
    }

    public override void Render(RenderWindow window, View view)
    {
        if (!LoadingSuccess)
        {
            return;
        }

        VideoPlayback.Position = new Vector2f(Bounds.StartX(), Bounds.StartY());
        VideoPlayback.Scale = new Vector2f
        (
            ((float) Bounds.EndX() - Bounds.StartX()) / Source.VideoSize.X,
            ((float) Bounds.EndY() - Bounds.StartY()) / Source.VideoSize.Y
        );
        
        Source.Update();
        window.Draw(VideoPlayback);
    }
}