using SFML.Graphics;

namespace OpenMcDesktop;

public record StyledTextNode(string Text, Color Colour, Color Shadow, Text.Styles Decoration) : TextStyle(Colour, Shadow, Decoration);