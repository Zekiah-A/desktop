using SFML.Graphics;

namespace OpenMcDesktop;

public static class StaticData
{
    // A temporary fix for edge cases where the injected gameData instance is inacessable from anywhere else
    public static GameData GameData { get; set; }
    public static ushort ProtocolVersion = 5;
}
