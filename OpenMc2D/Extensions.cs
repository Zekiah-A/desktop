using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using SFML.Graphics;

namespace OpenMc2D;

public static class Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IntX(this Vector2 vector2)
    {
        return (int) vector2.X;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IntY(this Vector2 vector2)
    {
        return (int) vector2.Y;
    }
    
    public static string ToPascalCase(this string input)
    {
        var words = input.Split("_");
        for (var i = 0; i < words.Length; i++)
        {
            words[i] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words[i].ToLower());
        }
        
        return string.Concat(words);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Texture AtBlock(this Texture texture, int x, int y, int width = 16, int height = 16)
    {
        return new Texture(texture.CopyToImage(), new IntRect(x * 16, y * 16, width, height));
    }
}