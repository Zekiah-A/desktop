using SFML.Graphics;

namespace OpenMcDesktop;

public static class StaticData
{
    // A temporary fix for edge cases where the injected gameData instance is inacessable from anywhere else
    public static GameData GameData { get; set; }
    public static ushort ProtocolVersion = 5;

    // .c0 - .c15
    public static Color[] TextColours = new Color[]
    {
        new Color(0, 0, 0), // #000
        new Color(170, 0, 0), // #a00
        new Color(0, 170, 0), // #0a0
        new Color(255, 170, 0), // #fa0
        new Color(0, 0, 170), // #00a
        new Color(170, 0, 170), // #a0a
        new Color(0, 170, 170), // #0aa
        new Color(170, 170, 170), // #aaa
        new Color(85, 85, 85), // #555
        new Color(255, 85, 85), // #f55
        new Color(85, 255, 85), // #5f5
        new Color(255, 255, 85), // #ff5
        new Color(85, 85, 255), // #55f
        new Color(255, 85, 255), // #f5f
        new Color(85, 255, 255), // #5ff
        new Color(255, 255, 255) // #fff
    };
    // .c0 - .c15
    public static Color[] TextShadows = new Color[]
    {
        new Color(0, 0, 0, 4), // #0004
        new Color(42, 0, 0), // #2a0000
        new Color(0, 42, 0), // #002a00
        new Color(42, 42, 0), // #2a2a00
        new Color(0, 0, 0, 42), // #00002a
        new Color(42, 0, 42), // #2a002a
        new Color(0, 42, 42), // #002a2a
        new Color(42, 42, 42), // #2a2a2a
        new Color(21, 21, 21, 68), // #15151544
        new Color(63, 21, 21), // #3f1515
        new Color(21, 63, 21), // #153f15
        new Color(63, 63, 21), // #3f3f15
        new Color(21, 21, 63), // #15153f
        new Color(63, 21, 63), // #3f153f
        new Color(21, 63, 21), // #153f3f
        new Color(63, 63, 63) // #3f3f3f
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
}
