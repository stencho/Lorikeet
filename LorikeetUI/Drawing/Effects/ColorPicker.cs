using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LorikeetUI.Effects;

public class ColorPicker() : ManagedEffect(State.content, "effects/picker") {
    private RenderTarget2D output;
    
    public void configure_shader(Vector2 texture, Vector2 resolution, float scale, Vector2 pick_xy_pos) {
        set_param("input_texture", texture);
        set_param("texture_resolution", resolution);
        set_param("scale", scale);
        set_param("pick_xy", pick_xy_pos);
    }

    public void pick_color_to_output() {
        var rts = State.graphics_device.GetRenderTargets();
        State.graphics_device.SetRenderTarget(output);
        State.graphics_device.SetRenderTargets(rts);
    }

    public Color Pick() {
        pick_color_to_output();
        Color[] c = { Color.Transparent };
        output.GetData<Color>(c);
        return c[0];
    } 
}