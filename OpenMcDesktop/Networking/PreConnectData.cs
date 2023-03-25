using OpenMcDesktop.Gui;
using WatsonWebsocket;

namespace OpenMcDesktop.Networking;

/// <summary>
/// The data obtained from a server on initial MOTD/prefetch connection
/// </summary>
/// <param name="Socket">The websocket instance attached to the server</param>
/// <param name="DataPacks">Custom game data sent from the server to the client and applied at runtime, including [blockindex, itemindex, entityindex, ...JS mod URLs]</param>
/// <param name="Challenge">Used to verify the authenticity of the client without exposing their private key.</param>
/// <param name="Item">A display list item representing the icon, MOTD and name of this server instance</param>
public record struct PreConnectData(WatsonWsClient Socket, string[] DataPacks, byte[] Challenge, DisplayListItem Item);