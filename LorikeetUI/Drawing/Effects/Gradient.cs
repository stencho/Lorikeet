using Microsoft.Xna.Framework;

namespace LorikeetUI.Effects;

public class Gradient() : ManagedEffect(State.content, "effects/dither") {
    public void configure_shader(XYPair top_left, XYPair bottom_right, Color A, Color B) {
        
    }
}