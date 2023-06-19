namespace OpenMcDesktop;

/// <summary>
/// A temporary fix for edge cases where the injected gameData instance is inacessable from anywhere else
/// </summary>
public static class StaticData
{
    public static GameData GameData { get; set; }
    public static ushort ProtocolVersion = 2;
}