using SFML.Graphics;

namespace OpenMcDesktop.Gui;

public class SkinEditor : Control
{
    public byte[] Data
    {
        set => Display.Skin = SkinHelpers.DecodeSkin(value);
    }
    private List<Control> Children;
    public SkinDisplay Display;
    private Grid layersGrid;
    private Button armBackLayerButton; 
    private Button bodyLayerButton; 
    private Button LayerButton; 

    public SkinEditor(byte[] data, Func<int> x, Func<int> y, Func<int> width, Func<int> height) : base(x, y, width, height)
    {
        Children = new List<Control>();
        Display = new SkinDisplay(data, x, () => y() + 64, width, height);
        Children.Add(Display);

        var bodyButton = new Button("Body", BoundZero, BoundZero, BoundZero, BoundZero);
        bodyButton.OnMouseUp += (_, _) =>
        {
            Display.Layer = ((Display.Layer & SkinDisplay.Layers.Body) == SkinDisplay.Layers.Body)
                ? Display.Layer & ~SkinDisplay.Layers.Body
                : Display.Layer | SkinDisplay.Layers.Body;
        };
        var armBackButton = new Button("Back arm", BoundZero, BoundZero, BoundZero, BoundZero);
        armBackButton.OnMouseUp += (_, _) =>
        {
            Display.Layer = ((Display.Layer & SkinDisplay.Layers.ArmBack) == SkinDisplay.Layers.ArmBack)
                ? Display.Layer & ~SkinDisplay.Layers.ArmBack
                : Display.Layer | SkinDisplay.Layers.ArmBack;
        };
        var armFrontButton = new Button("Front arm", BoundZero, BoundZero, BoundZero, BoundZero);
        armFrontButton.OnMouseUp += (_, _) =>
        {
            Display.Layer = ((Display.Layer & SkinDisplay.Layers.ArmFront) == SkinDisplay.Layers.ArmFront)
                ? Display.Layer & ~SkinDisplay.Layers.ArmFront
                : Display.Layer | SkinDisplay.Layers.ArmFront;
        };
        var legBackButton = new Button("Back leg", BoundZero, BoundZero, BoundZero, BoundZero);
        legBackButton.OnMouseUp += (_, _) =>
        {
            Display.Layer = ((Display.Layer & SkinDisplay.Layers.LegBack) == SkinDisplay.Layers.LegBack)
                ? Display.Layer & ~SkinDisplay.Layers.LegBack
                : Display.Layer | SkinDisplay.Layers.LegBack;
        };
        var legFrontButton = new Button("Front leg", BoundZero, BoundZero, BoundZero, BoundZero);
        legFrontButton.OnMouseUp += (_, _) =>
        {
            Display.Layer = ((Display.Layer & SkinDisplay.Layers.LegFront) == SkinDisplay.Layers.LegFront)
                ? Display.Layer & ~SkinDisplay.Layers.LegFront
                : Display.Layer | SkinDisplay.Layers.LegFront;
        };
        var animateButton = new Button("Animate", BoundZero, BoundZero, BoundZero, BoundZero);
        animateButton.OnMouseUp += (_, _) =>
        {
            Display.Animate = !Display.Animate;
        };

        layersGrid = new Grid(3, 2, x, y, width, () => 64)
        {
            Children = new Control?[,]
            {
                { bodyButton, armBackButton },
                { armFrontButton, legBackButton },
                { legFrontButton, animateButton }
            }
        };
        Children.Add(layersGrid);
    }
    
    public override bool HitTest(int x, int y, TestType type)
    {
        for (var i = Children.Count - 1; i >= 0; i--)
        {
            if (Children[i].HitTest(x, y, type))
            {
                return true;
            }
        }

        return false;
    }
    
    public override void Render(RenderWindow window, View view)
    {
        foreach (var child in Children.ToList())
        {
            child.Render(window, view);
        }
    }

}