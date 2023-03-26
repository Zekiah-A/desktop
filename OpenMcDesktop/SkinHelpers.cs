using SFML.Graphics;

namespace OpenMcDesktop;

public static class SkinHelpers
{
    public static byte[] AlexSkin;
    public static byte[] SteveSkin;

    public static byte[] SkinDataFromFile(string path)
    {
        var rawData = File.ReadAllBytes(path);
        var skinImage = new Image(rawData);
        var skinI = 0;
        var skinData = new byte[1008];
        for (var i = 0; i < 1008; i += 3)
        {
            var pixel = skinImage.GetPixel((uint) (skinI % 28), (uint) (skinI / 28));
            skinData[i] = pixel.R;
            skinData[i + 1] = pixel.G;
            skinData[i + 2] = pixel.B;
        
            if (i % 3 == 0)
            {
                skinI++;
            }
        }

        return skinData;
    }
    
    /// <summary>
    /// Decodes a skin into it's respective textures, if the skin happens to be in the minecraft format, it will also
    /// perform a beforehand conversion to the correct format.
    /// </summary>
    public static DecodedSkin DecodeSkin(byte[] data)
    {
        var source = new Span<byte>(data.Length switch
        {
            4096 => ConvertSkin(data),
            1008 => data,
            _ => throw new Exception("Invalid skin size could not be successfully decoded!")
        });

        var result = new DecodedSkin();
        var head = new Image(8, 8);
        var armFront = new Image(4, 12);
        var armBack = new Image(4, 12);
        var body = new Image(4, 12);
        var legFront = new Image(4, 12);
        var legBack = new Image(4, 12);

        WriteSpanToImageRegion(source, 28, 20, 4, 28, 12, head, 0, 0);
        WriteSpanToImageRegion(source, 28, 4, 0, 8, 12, armFront, 0, 0);
        WriteSpanToImageRegion(source, 28, 8, 0, 12, 12, armBack, 0, 0);
        WriteSpanToImageRegion(source, 28, 0, 0, 4, 12, body, 0, 0);
        WriteSpanToImageRegion(source, 28, 12, 0, 16, 12, legFront, 0, 0);
        WriteSpanToImageRegion(source, 28, 16, 0, 20, 12, legBack, 0, 0);
        
        result.Head = head;
        result.ArmFront = armFront;
        result.ArmBack = armBack;
        result.Body = body;
        result.LegFront = legFront;
        result.LegBack = legFront;

        return result;
    }

    /// <summary>
    /// Converts a minecraft skin into the 2d format that is used by OpenMcDesktop.
    /// </summary>
    public static byte[]? ConvertSkin(byte[] data)
    {
        var result = new Span<byte>(new byte[1008]);
        var image = new Image(data);
        
        if (image.Size.X != 64 || image.Size.Y != 64)
        {
            return null;
        }
        
        // Head, Arm Front, Arm Back, Body, Leg Front, Leg Back
        WriteImageRegionToSpan(image, 0, 8, 8, 16, result, 28, 20, 4);
        WriteImageRegionToSpan(image, 44, 20, 48, 32, result, 28, 4, 0);
        WriteImageRegionToSpan(image, 36, 52, 40, 64, result, 28, 8, 0);
        WriteImageRegionToSpan(image, 16, 20, 20, 32, result, 28, 0, 0);
        WriteImageRegionToSpan(image, 0, 20, 4, 32, result, 28, 12, 0);
        WriteImageRegionToSpan(image, 16, 52, 20, 64, result, 28, 16, 0);

        return result.ToArray();
    }

    /// <summary>
    /// Writes a rectangular region of an sfml image to a span as if it were 2D.
    /// "destinationWidth" must be the FULL 2D width of the destination, as if it were a 2d image.
    /// </summary>
    public static void WriteImageRegionToSpan(
        Image source, int startX, int startY, int endX, int endY, Span<byte> destination,
        int destinationWidth, int destinationStartX, int destinationStartY)
    {
        var destinationY = destinationStartY;
        for (var imageY = (uint) startY; imageY < endY; imageY++)
        {
            var destinationX = destinationStartX;
            var destinationIndex = (destinationWidth * destinationY + destinationX) * 3;

            for (var imageX = (uint) startX; imageX < endX; imageX++)
            {
                var pixel = source.GetPixel(imageX, imageY);
                destination[destinationIndex] = pixel.R;
                destination[destinationIndex + 1] = pixel.G;
                destination[destinationIndex + 2] = pixel.B;
                
                destinationIndex += 3;
                destinationX++;
            }
            destinationY++;
        }
    }
    
    /// <summary>
    /// Writes a rectangular region of a span to a SFML image as if it were 2D.
    /// "destinationWidth" must be the FULL 2D width of the destination, as if it were a 2d image.
    /// </summary>
    public static void WriteSpanToImageRegion(
        Span<byte> source, int sourceWidth, int startX, int startY, int endX, int endY,
        Image destination, int destinationStartX, int destinationStartY)
    {
        var destinationY = (uint) destinationStartY;
        for (var sourceY = (uint) startY; sourceY < endY; sourceY++)
        {
            var destinationX = (uint) destinationStartX;
            var sourceIndex = (int) (sourceWidth * sourceY + startX) * 3;

            for (var sourceX = (uint) startX; sourceX < endX; sourceX++)
            {
                var pixel = new Color(source[sourceIndex], source[sourceIndex + 1], source[sourceIndex + 2]);
                destination.SetPixel(destinationX, destinationY, pixel);

                sourceIndex += 3;
                destinationX++;
            }
            destinationY++;
        }
    }
}