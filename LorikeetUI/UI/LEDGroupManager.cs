using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Bumpo;
using LorikeetServer;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LorikeetUI.UIElements;

public class LEDGroupManager : UIElement {
    //Arrays of LED objects representing LED sections
    public class LEDGroup {
        public Vector2 Position { get; set; } = State.WindowBounds / 2f;
        public Vector2 Size { get; set; }
        public Vector2 Stride { get; set; } = Vector2.UnitX * 20;
        
        public Vector2 offset_from_anchor = Vector2.One * 9;
        
        public List<LED> LEDs = new List<LED>();

        public bool Reverse { get; set; } = false;
        
        public int EditorZ = 0;
            
        private byte led_count = 0;
        public byte LEDCount => led_count;
        
        public LEDGroup(byte count, LED initial_color, bool reverse = false) {
            led_count = count;
            for (int i = 0; i < count; i++) {
                LEDs.Add(initial_color);    
            }

            Size = (offset_from_anchor * 2) + (Stride * (count - 1));
            
            Reverse = reverse;
        }

        public void Draw() {
            Drawing.rect(Position, Position + (Stride * (led_count-1)) + (offset_from_anchor * 2), UI.ForegroundColor, 1f);
            for (int i = 0; i < LEDs.Count; i++) {
                Drawing.circle(Position + (Stride * i)  + (offset_from_anchor), 4f, 2f, 
                    Color.FromNonPremultiplied(LEDs[i].R, LEDs[i].G, LEDs[i].B, 255));
                Drawing.fill_circle(Position + (Stride * i) + (offset_from_anchor), 2f,
                    UI.BackgroundColor);
            }
        }
    }

    //A simple texture2D color picker
    public class PickableImage {
        private string filename;
        private Texture2D texture;
        
        public Vector2 Position = Vector2.Zero;
        public Vector2 size = Vector2.Zero;
        public float scale { get; set; } = 1f;

        private Color[] pixel_data;
        
        public PickableImage(string filename) {
            this.filename = filename;
            texture = Texture2D.FromFile(State.graphics_device, filename);
            size = new Vector2(texture.Width, texture.Height);
            pixel_data = new  Color[texture.Width * texture.Height];
            texture.GetData(pixel_data);
        }

        public Color PickColor(XYPair pixel) {
            int index = (int)((texture.Width * (pixel.Y)) + (pixel.X * scale));
            if (index > pixel_data.Length-1) {
                return Color.Transparent;
            }
            return pixel_data[index];
        }
        
        public void Draw() {
            Drawing.image(texture, Position, size * scale);
            Drawing.rect(Position, Position+(size * scale), UI.ForegroundColor, 1f);
        }
    }
    
    public Vector2 Position { get; set; } = Vector2.One;
    public Vector2 Size { get; set; }
    public InputState InputState { get; set; }
    public UIActions Actions { get; set; }
    public RenderState RenderState { get; set; }
    public Vector2 MousePosRelative { get; set; }
    
    public bool Visible { get; set; } = true;
    
    List<LEDGroup> LEDGroups = new List<LEDGroup>();
    List<PickableImage> Images = new List<PickableImage>();
    
    private int group_on_mouse = -1;
    private int image_resizing = -1;
    private int image_on_mouse = -1;

    public float HeaderHeight = 16;
    
    public LEDGroupManager(Vector2 position, Vector2 size, params LEDGroup[] groups) {
        Position = position;
        Size = size;
        
        float y = 0;
        float th = 0; //total height
        
        for (int i = 0; i < groups.Length; i++) {
            if (i > 0) th += groups[i - 1].Size.Y;
        }

        for (int i = 0; i < groups.Length; i++) {
            LEDGroups.Add(groups[i]);
            LEDGroups[i].EditorZ = i;
            
            if (i > 0) y += groups[i - 1].Size.Y; 
            LEDGroups[i].Position = (State.WindowBounds / 2f) - (Vector2.UnitY * th / 2f) + (Vector2.UnitY * y) - (Vector2.UnitX * LEDGroups[i].Size.X / 2f);
        }
        
        update_leds();
    }

    /// <summary>
    /// Add a background image for LED color picking
    /// </summary>
    /// <param name="filename"></param>
    public void AddImage(string filename) {
        Images.Add(new PickableImage(filename));
        
        update_picked_colors();
        update_leds();
    }

    public void PreDraw() {
        
    }

    public void Draw() {
        
        //images
        foreach (PickableImage image in Images) {
            image.Draw();
        }
        
        //LED groups
        for (int i = 0; i < LEDGroups.Count; i++) {
            LEDGroups[i].Draw();

            var iw = Drawing.measure_string_profont_xy(i.ToString()).X;
            var aw = Drawing.measure_string_profont_xy(">").X;
            if (!LEDGroups[i].Reverse) {
                Drawing.text("<", LEDGroups[i].Position + (LEDGroups[i].Size / 2f) + (XYPair.UnitX * iw), UI.TextColor);
                Drawing.text(i.ToString(), LEDGroups[i].Position, UI.TextColor);
                
            } else {
                Drawing.text(i.ToString(), LEDGroups[i].Position + (LEDGroups[i].Size) + (XYPair.UnitX * iw), UI.TextColor);
                Drawing.text(">", LEDGroups[i].Position - Vector2.UnitX * (aw + 2), UI.TextColor);
            
            }
        }
        
        //outline
        Drawing.rect(Position + Vector2.UnitX, Position + Size - (Vector2.UnitY * 2) - (Vector2.UnitX * 2), UI.ForegroundColor, 3f);
        
        //header
        Drawing.fill_rect(Position, Position + (State.WindowBounds.X * Vector2.UnitX) + (HeaderHeight * Vector2.UnitY), UI.ForegroundColor);
    }

    //Update color picker
    void update_picked_colors() {
        for (int i = 0; i < LEDGroups.Count; i++) {
            for (int led = 0; led < LEDGroups[i].LEDs.Count; led++) {
                var led_pos = LEDGroups[i].Position + (LEDGroups[i].Stride * led) + LEDGroups[i].offset_from_anchor;
                
                for (var index = Images.Count-1; index >= 0; index--) {
                    var img_pos = Images[index].Position;
                    
                    LEDGroups[i].LEDs[led] = new LED(0,0,0);
                    if (Collision2D.v2_intersects_rect(led_pos, img_pos, img_pos + (Images[index].size * Images[index].scale))) {
                        var relative = ((led_pos ) - img_pos)* Images[index].scale ;

                        Color c = Images[index].PickColor(relative.ToXYPair());
                        c.G = (byte)(c.G * 0.65f); // GREEN CORRECTION
                        LEDGroups[i].LEDs[led] = new LED(c.R, c.G, c.B);
                        
                        break;
                    }
                }
                
            }
        }
    }
    
    //send LED color changes
    void update_leds() {
        byte offset = 0;
        for (int i = 0; i < LEDGroups.Count; i++) {
            if (!LEDGroups[i].Reverse) {
                for (int led = 0; led < LEDGroups[i].LEDs.Count; led++) {
                    MappedLEDStrip.SetLEDColor((byte)(offset + led), LEDGroups[i].LEDs[led].R, LEDGroups[i].LEDs[led].G,
                        LEDGroups[i].LEDs[led].B);
                }
                offset += (byte)LEDGroups[i].LEDs.Count;
                
            } else {
                for (int led = 0; led < LEDGroups[i].LEDs.Count; led++) {
                    MappedLEDStrip.SetLEDColor((byte)(offset + ((byte)LEDGroups[i].LEDs.Count - led-1)), LEDGroups[i].LEDs[led].R, LEDGroups[i].LEDs[led].G,
                        LEDGroups[i].LEDs[led].B);
                }
                offset += (byte)LEDGroups[i].LEDs.Count;
            }
        }
    }
    
    public void Update() {
        update_picked_colors();

        //Mouse button releases
        if (InputState.JustReleasedRightClick) {
            image_resizing = -1;
        }
        if (InputState.JustReleasedLeftClick) {
            image_on_mouse = -1;
            group_on_mouse = -1;
        }

        //Right-click image resizing
        if (InputState.RightMouseDown && group_on_mouse == -1 && image_on_mouse == -1 && image_resizing == -1) {
            for (int i = 0; i < Images.Count; i++) {
                if (Collision2D.v2_intersects_rect(InputState.MousePosRelative, Images[i].Position,
                        Images[i].Position + (Images[i].size * Images[i].scale))) {
                    image_resizing = i;
                    break;
                }
            }
        }
        
        //group/image mouse picking
        if (InputState.LeftMouseDown && group_on_mouse == -1 && image_on_mouse == -1 && image_resizing == -1) { //LED groups
            for (int i = 0; i < LEDGroups.Count; i++) {
                if (Collision2D.v2_intersects_rect(InputState.MousePosRelative, LEDGroups[i].Position,
                        LEDGroups[i].Position + LEDGroups[i].Size)) {
                    group_on_mouse = i;
                    break;
                }
            }
            
        }
        if (InputState.LeftMouseDown && group_on_mouse == -1 && image_on_mouse == -1 && image_resizing == -1) { //Background images
            for (int i = 0; i < Images.Count; i++) {
                if (Collision2D.v2_intersects_rect(InputState.MousePosRelative, Images[i].Position,
                        Images[i].Position + (Images[i].size * Images[i].scale))) {
                    image_on_mouse = i;
                    break;
                }
            }
        } 

        //group/image movement & resizing
        if (InputState.LeftMouseDown && image_on_mouse > -1) {
            Images[image_on_mouse].Position += Input.MouseDelta;
            
            update_leds();
            
        } else if (InputState.RightMouseDown && image_resizing > -1 ) {
            float distance = Vector2.Distance(MousePosRelative, Images[image_resizing].Position);
            float rect_length = Vector2.Distance(Vector2.Zero, Images[image_resizing].size * Images[image_resizing].scale);
            
            float scale_delta = distance / rect_length;
            
            //Images[image_resizing].scale -= scale_delta;
            
            update_leds();
            
        } /*else if (InputState.LeftMouseDown && group_on_mouse > -1) {
            
            if (!Input.shift) {
                LEDGroups[group_on_mouse].Position += Input.MouseDelta;    
            } else {
                for (int i = 0; i < LEDGroups.Count; i++) {
                   LEDGroups[i].Position += Input.MouseDelta; 
                }
            }
            
            update_leds();
        }*/
        
        else if (InputState.LeftMouseDown && Input.shift) {
            
            for (int i = 0; i < Images.Count; i++) {
                Images[i].Position += Input.MouseDelta; 
            }
            
            for (int i = 0; i < LEDGroups.Count; i++) {
                LEDGroups[i].Position += Input.MouseDelta; 
            }
            
            update_leds();
        }
    }
}