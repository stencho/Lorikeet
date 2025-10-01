using System;
using System.Collections.Generic;
using System.Linq;
using Bumpo;
using Microsoft.Xna.Framework;

namespace LorikeetUI.UIElements;

public static class UI {
    public static Dictionary<string, UIElement> Elements = new Dictionary<string, UIElement>();

    private static string mouse_left_down_element;
    private static string mouse_over_element;
    private static string mouse_over_element_previous;

    private static bool left_mouse_down = false;
    
    public static string LeftMouseDownElement => mouse_left_down_element;
    public static string MouseOverElement => mouse_over_element;

    public static Color ForegroundColor { get; set; } = Color.HotPink;
    public static Color TextColor { get; set; } = Color.White;
    public static Color BackgroundColor { get; set; } = Color.FromNonPremultiplied(15,15,15,255);
    
    public static void AddElement(string name, UIElement element) {
        Elements.Add(name, element);
        Elements[name].Visible = true;
    }
    public static void AddHiddenElement(string name, UIElement element) {
        Elements.Add(name, element);
        Elements[name].Visible = false;
    }

    public static void ShowElement(string name) {
        if (Elements.ContainsKey(name)) Elements[name].Visible = true;
    }
    public static void HideElement(string name) {
        if (Elements.ContainsKey(name)) Elements[name].Visible = false;
    }
    public static void ToggleElement(string name) {
        if (Elements.ContainsKey(name)) Elements[name].Visible = !Elements[name].Visible;
    }

    public static void Update() {
        //MOUSE OVER
        foreach (var e in Elements.Keys.Reverse()) {
            if (!Elements[e].Visible) continue;
            
            Elements[e].MouseOver = false;
        }

        mouse_over_element = String.Empty;
        
        foreach (var e in Elements.Keys.Reverse()) {
            if (!Elements[e].Visible) continue;
            
            UIElement element = Elements[e];

            if (Collision2D.v2_intersects_rect(Input.mouse_pos, element.Position, element.Position + element.Size)) {
                mouse_over_element = e;
                Elements[e].MouseOver = true;
                break;
            }
        }
        
        if (mouse_over_element != mouse_over_element_previous) {
            if (!String.IsNullOrEmpty(mouse_over_element) && Elements[mouse_over_element].OnMouseEnter != null)
                Elements[mouse_over_element].OnMouseEnter.Invoke();
             
            if (!String.IsNullOrEmpty(mouse_over_element_previous) && Elements[mouse_over_element_previous].OnMouseLeave != null)
                Elements[mouse_over_element_previous].OnMouseLeave.Invoke();
        }
        
        if (Input.MouseDelta != Vector2.Zero && !String.IsNullOrEmpty(mouse_over_element)) {
            if (Elements[mouse_over_element].OnMouseMove != null) Elements[mouse_over_element].OnMouseMove.Invoke();
        }
        
        //CLICKS
        if (State.game.IsActive) {
            if (Input.JustReleased(Input.MouseButtons.Left)) {
                if (!String.IsNullOrEmpty(mouse_left_down_element)) Elements[mouse_left_down_element].LeftMouseDown = false;
                
                foreach (var e in Elements.Keys.Reverse()) {
                    if (!Elements[e].Visible) continue;
                    
                    if (Elements[e].OnMouseUp != null) Elements[e].OnMouseUp.Invoke();

                    if (mouse_over_element == mouse_left_down_element && mouse_left_down_element == e) {
                        if (Elements[e].OnClick != null) Elements[e].OnClick.Invoke();
                    }

                    mouse_left_down_element = String.Empty;
                    break;
                }
                
            } else if (Input.JustPressed(Input.MouseButtons.Left)) {
                left_mouse_down = true;
                
                foreach (var e in Elements.Keys) {
                    if (!Elements[e].Visible) continue;
                    
                    Elements[e].LeftMouseDown = false;
                }
                
                if (!String.IsNullOrEmpty(mouse_over_element)) {
                    Elements[mouse_over_element].LeftMouseDown = true;
                    mouse_left_down_element = mouse_over_element;
                    
                    if (Elements[mouse_over_element].OnMouseDown != null) Elements[mouse_over_element].OnMouseDown.Invoke();
                }
            }
        }

        foreach (var e in Elements.Keys) {
            Elements[e].MousePosRelative = Input.mouse_pos - Elements[e].Position;
            
            if (!Elements[e].Visible) continue;
            Elements[e].Update();
        }
        
        mouse_over_element_previous = mouse_over_element;
    }

    public static void Draw() {
        foreach (var e in Elements.Keys) {
            if (!Elements[e].Visible) continue;
                
            Elements[e].Draw();
        }
    }
}

public interface UIElement {
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }

    public Vector2 MousePosRelative { get; internal set; }
    
    public bool MouseOver { get; internal set; }
    public bool LeftMouseDown { get; internal set; }
    public bool Visible { get; set; }
    
    public void Draw();
    public void Update();

    public Action OnMouseEnter { get; set; }
    public Action OnMouseLeave { get; set; }
    public Action OnMouseDown { get; set; }
    public Action OnMouseUp { get; set; }
    public Action OnMouseMove { get; set; }
    public Action OnClick { get; set; }
}