using Microsoft.Xna.Framework.Content;

namespace Lorikeet.Effects;

public class ColorMap : ManagedEffect {
    public ColorMap() : base(State.content, "effects/color_map") {}
}