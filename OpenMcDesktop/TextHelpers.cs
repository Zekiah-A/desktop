using System.Text.RegularExpressions;
using OpenMcDesktop.Gui;
using SFML.Graphics;

namespace OpenMcDesktop;

public partial class TextHelpers
{
    // .c0 - .c15
    public static Color[] TextColours = new Color[]
    {
        new Color(0, 0, 0), // #000 .c0
        new Color(170, 0, 0), // #a00 .c1
        new Color(0, 170, 0), // #0a0 .c2
        new Color(255, 170, 0), // #fa0 .c3
        new Color(0, 0, 170), // #00a .c4
        new Color(170, 0, 170), // #a0a .c5
        new Color(0, 170, 170), // #0aa .c6
        new Color(170, 170, 170), // #aaa .c7
        new Color(85, 85, 85), // #555 .c8
        new Color(255, 85, 85), // #f55 .c9
        new Color(85, 255, 85), // #5f5 .c10
        new Color(255, 255, 85), // #ff5 .c11
        new Color(85, 85, 255), // #55f .c12
        new Color(255, 85, 255), // #f5f .c13
        new Color(85, 255, 255), // #5ff .c14
        new Color(255, 255, 255) // #fff .c15
    };

    // .s0 - .s15
    public static Color[] TextShadows = new Color[]
    {
        new Color(0, 0, 0, 4), // #0004 .c0
        new Color(42, 0, 0), // #2a0000 .c1
        new Color(0, 42, 0), // #002a00 .c2
        new Color(42, 42, 0), // #2a2a00 .c3
        new Color(0, 0, 0, 42), // #00002a .c4
        new Color(42, 0, 42), // #2a002a .c5
        new Color(0, 42, 42), // #002a2a .c6
        new Color(42, 42, 42), // #2a2a2a .c7
        new Color(21, 21, 21, 68), // #15151544 .c8
        new Color(63, 21, 21), // #3f1515 .c9
        new Color(21, 63, 21), // #153f15 .c10
        new Color(63, 63, 21), // #3f3f15 .c11
        new Color(21, 21, 63), // #15153f .c12
        new Color(63, 21, 63), // #3f153f .c13
        new Color(21, 63, 21), // #153f3f .c14
        new Color(63, 63, 63) // #3f3f3f .c15
    };

    // .s0 - .s15
    public static Text.Styles[] TextDecorations = new Text.Styles[]
    {
        Text.Styles.Regular, // .s0
        Text.Styles.Bold, // .s1
        Text.Styles.Italic, // .s2
        Text.Styles.Bold | Text.Styles.Italic, // .s3
        Text.Styles.Underlined, // .s4
        Text.Styles.Bold | Text.Styles.Underlined, // .s5
        Text.Styles.Italic | Text.Styles.Underlined, // .s6
        Text.Styles.Bold | Text.Styles.Italic | Text.Styles.Underlined, // .s7
        Text.Styles.StrikeThrough, // .s8
        Text.Styles.Bold | Text.Styles.StrikeThrough, // .s9
        Text.Styles.Italic | Text.Styles.StrikeThrough, // .s10
        Text.Styles.Bold | Text.Styles.Italic | Text.Styles.StrikeThrough, // .s11
        Text.Styles.Underlined | Text.Styles.StrikeThrough, // .s12
        Text.Styles.Bold | Text.Styles.Underlined | Text.Styles.StrikeThrough, // .s13
        Text.Styles.Italic | Text.Styles.Underlined | Text.Styles.StrikeThrough, // .s14
        Text.Styles.Bold | Text.Styles.Italic | Text.Styles.Underlined | Text.Styles.StrikeThrough, // .s14
    };

    private static Dictionary<Regex, Color> ChatColourMap = new()
    {
        { DefaultMessageRegex(), TextColours[15] }, // .msg default white
        { BaseCommandRegex(), TextColours[7] }, // .c7 grey /basecommand
        { RelativeCordinateRegex(), TextColours[11] }, //  .c11 yellow tilde relative cordinate ~0 ~1 ~ 2
        { CommandStringLiteralRegex(), TextColours[13] }, // .c13 purple sub command string literal
        { SubCommandRegex(), TextColours[10] }, // .c10 green sub command (not string literal)
        { BadCommandLiteralRegex(), TextColours[9] }, // .c9 red unclosed string command literal
    };

    public static StyledText ParseCommandStyle(string text)
    {
        for (var i = 0; i < ChatColourMap.Count; i++)
        {
            var pair = ChatColourMap.ElementAt(i);
            if (pair.Key.IsMatch(text))
            {
                return new StyledText(text, TextColours[i], TextShadows[i], TextDecorations[0]);
            }
        }

        // Default if no match
        return new StyledText(text, TextColours[0], TextShadows[0], TextDecorations[0]);
    }

    [GeneratedRegex(@"^[^/][^\s]*$")]
    private static partial Regex DefaultMessageRegex();
    [GeneratedRegex(@"^\/\w*$")]
    private static partial Regex BaseCommandRegex();
    [GeneratedRegex(@"^~|~?[+-]?(\d+(\.\d*)?|\.\d+)([Ee][+-]?\d+)?$")]
    private static partial Regex RelativeCordinateRegex();
    [GeneratedRegex(@"^""(?:[^\\\""]|\\.)*""$")]
    private static partial Regex CommandStringLiteralRegex();
    [GeneratedRegex(@"^[^""\s]\S*$")]
    private static partial Regex SubCommandRegex();
    [GeneratedRegex(@"^""$")]
    private static partial Regex BadCommandLiteralRegex();
}