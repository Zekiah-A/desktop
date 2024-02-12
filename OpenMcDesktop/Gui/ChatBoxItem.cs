using SFML.Graphics;

namespace OpenMcDesktop.Gui;

public record StyledText(string Text, Color Colour, Color Shadow, Text.Styles Decoration);
