using OpenMcDesktop.Networking;

namespace OpenMcDesktop.Game.Definitions;

public interface IDecodable
{
    public object? Decode(ref ReadablePacket data, object? target);
}