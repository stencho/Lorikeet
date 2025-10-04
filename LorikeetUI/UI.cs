using System;
using System.Collections.Generic;
using System.Linq;
using Bumpo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LorikeetUI.UIElements;
public static class UI {
    public static Dictionary<string, UIElement> Elements = new Dictionary<string, UIElement>();

    private static string mouse_left_down_element;
    private static string mouse_right_down_element;
    private static string mouse_over_element;
    private static string mouse_over_element_previous;

    private static bool left_mouse_down = false;
    private static bool right_mouse_down = false;
    
    public static string LeftMouseDownElement => mouse_left_down_element;
    public static string RightMouseDownElement => mouse_right_down_element;
    public static string MouseOverElement => mouse_over_element;

    public static Color ForegroundColor { get; set; } = Color.FromNonPremultiplied(255, 162, 200, 255);
    public static Color TextColor { get; set; } = Color.White;
    public static Color BackgroundColor { get; set; } = Color.FromNonPremultiplied(25,25,25,255);

    public static void AddElement(string name, UIElement element) {
        Elements.Add(name, element);
        Elements[name].Visible = true;
        Elements[name].Init();
    }

    public static void AddHiddenElement(string name, UIElement element) {
        Elements.Add(name, element);
        Elements[name].Visible = false;
        Elements[name].Init();
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
            
            Elements[e].InputState.MouseOver = false;
            
            Elements[e].InputState.RightMouseDownPrevious = Elements[e].InputState.RightMouseDown;
            Elements[e].InputState.LeftMouseDownPrevious = Elements[e].InputState.LeftMouseDown;
        }

        mouse_over_element = String.Empty;
        
        foreach (var e in Elements.Keys.Reverse()) {
            if (!Elements[e].Visible) continue;
            
            UIElement element = Elements[e];

            if (Collision2D.v2_intersects_rect(Input.mouse_pos, element.Position, element.Position + element.Size)) {
                mouse_over_element = e;
                Elements[e].InputState.MouseOver = true;
                break;
            }
        }
        
        if (mouse_over_element != mouse_over_element_previous) {
            if (!String.IsNullOrEmpty(mouse_over_element) && Elements[mouse_over_element].Actions.OnMouseEnter != null)
                Elements[mouse_over_element].Actions.OnMouseEnter.Invoke();
             
            if (!String.IsNullOrEmpty(mouse_over_element_previous) && Elements[mouse_over_element_previous].Actions.OnMouseLeave != null)
                Elements[mouse_over_element_previous].Actions.OnMouseLeave.Invoke();
        }
        
        if (Input.MouseDelta != Vector2.Zero && !String.IsNullOrEmpty(mouse_over_element)) {
            if (Elements[mouse_over_element].Actions.OnMouseMove != null) Elements[mouse_over_element].Actions.OnMouseMove.Invoke();
        }
        
        //CLICKS
        if (State.game.IsActive) {
            //RIGHT CLICK
            if (Input.JustReleased(Input.MouseButtons.Right)) {
                if (!String.IsNullOrEmpty(mouse_right_down_element)) Elements[mouse_right_down_element].InputState.RightMouseDown = false;
                
                foreach (var e in Elements.Keys.Reverse()) {
                    if (!Elements[e].Visible) continue;
                    
                    if (Elements[e].Actions.OnRightMouseUp != null) Elements[e].Actions.OnRightMouseUp.Invoke();

                    if (mouse_over_element == mouse_right_down_element && mouse_right_down_element == e) {
                        if (Elements[e].Actions.OnRightClick != null) Elements[e].Actions.OnRightClick.Invoke();
                    }

                    mouse_right_down_element = String.Empty;
                    break;
                }
            }else if (Input.JustPressed(Input.MouseButtons.Right)) {
                right_mouse_down = true;
                
                foreach (var e in Elements.Keys) {
                    if (!Elements[e].Visible) continue;
                    
                    Elements[e].InputState.RightMouseDown = false;
                }
                
                if (!String.IsNullOrEmpty(mouse_over_element)) {
                    Elements[mouse_over_element].InputState.RightMouseDown = true;
                    mouse_right_down_element = mouse_over_element;
                    
                    if (Elements[mouse_over_element].Actions.OnRightMouseDown != null) Elements[mouse_over_element].Actions.OnRightMouseDown.Invoke();
                }
            }
            
            //LEFT MOUSE
            if (Input.JustReleased(Input.MouseButtons.Left)) {
                if (!String.IsNullOrEmpty(mouse_left_down_element)) Elements[mouse_left_down_element].InputState.LeftMouseDown = false;
                
                foreach (var e in Elements.Keys.Reverse()) {
                    if (!Elements[e].Visible) continue;
                    
                    if (Elements[e].Actions.OnMouseUp != null) Elements[e].Actions.OnMouseUp.Invoke();

                    if (mouse_over_element == mouse_left_down_element && mouse_left_down_element == e) {
                        if (Elements[e].Actions.OnClick != null) Elements[e].Actions.OnClick.Invoke();
                    }

                    mouse_left_down_element = String.Empty;
                    break;
                }
                
            } else if (Input.JustPressed(Input.MouseButtons.Left)) {
                left_mouse_down = true;
                
                foreach (var e in Elements.Keys) {
                    if (!Elements[e].Visible) continue;
                    
                    Elements[e].InputState.LeftMouseDown = false;
                }
                
                if (!String.IsNullOrEmpty(mouse_over_element)) {
                    Elements[mouse_over_element].InputState.LeftMouseDown = true;
                    mouse_left_down_element = mouse_over_element;
                    
                    if (Elements[mouse_over_element].Actions.OnMouseDown != null) Elements[mouse_over_element].Actions.OnMouseDown.Invoke();
                }
            }
        }

        foreach (var e in Elements.Keys) {
            Elements[e].InputState.MousePosRelative = Input.mouse_pos - Elements[e].Position;
            Elements[e].InputState.JustClicked = Elements[e].InputState.LeftMouseDown && !Elements[e].InputState.LeftMouseDownPrevious;
            Elements[e].InputState.JustReleasedLeftClick = !Elements[e].InputState.LeftMouseDown && Elements[e].InputState.LeftMouseDownPrevious;
            Elements[e].InputState.JustRightClicked = Elements[e].InputState.RightMouseDown && !Elements[e].InputState.RightMouseDownPrevious;
            Elements[e].InputState.JustReleasedRightClick = !Elements[e].InputState.RightMouseDown && Elements[e].InputState.RightMouseDownPrevious;

            if (!Elements[e].Visible) continue;
            Elements[e].Update();
        }
        
        mouse_over_element_previous = mouse_over_element;
    }
    public static void PreDraw() {
        foreach (var e in Elements.Keys) {
            if (!Elements[e].Visible) continue;
            Elements[e].PreDraw();
        }
    }

    public static void Draw() {
        foreach (var e in Elements.Keys) {
            if (!Elements[e].Visible) continue;
            Elements[e].Draw();
        }
    }
}

//define the UIElement and all its state
//give it a little kiss

public class InputState {
    public Vector2 MousePosRelative { get; internal set; } = Vector2.Zero;

    public bool MouseOver { get; internal set; } = false;
    public bool LeftMouseDown { get; internal set; } = false;
    internal bool LeftMouseDownPrevious { get; set; } = false;
    public bool RightMouseDown { get; internal set; } = false;
    internal bool RightMouseDownPrevious { get; set; } = false;
    public bool JustClicked { get; set; }
    public bool JustRightClicked { get; set; }
    public bool JustReleasedLeftClick { get; set; }
    public bool JustReleasedRightClick { get; set; }
}

public class UIActions {
    public Action OnMouseEnter { get; set; }
    public Action OnMouseLeave { get; set; }
    public Action OnMouseDown { get; set; }
    public Action OnMouseUp { get; set; }
    public Action OnRightMouseDown { get; set; }
    public Action OnRightMouseUp { get; set; }
    public Action OnClick { get; set; }
    public Action OnRightClick { get; set; }
    public Action OnMouseMove { get; set; }
}

public class RenderState {
    internal UIElement parent;
    
    public bool RenderToTarget { get; set; }
    RenderTarget2D MainTarget = null;

    public RenderState(UIElement parent) => this.parent = parent;
    
    Dictionary<string, RenderTarget2D> Targets = new Dictionary<string, RenderTarget2D>();
    public void AddTarget(string name, RenderTarget2D target) => Targets.Add(name, target);
}

public interface UIElement {
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }

    public InputState InputState { get; set; }
    public UIActions Actions { get; set; }
    public RenderState RenderState { get; set; }
    
    public bool Visible { get; set; }

    public void PreDraw();
    public void Draw();
    public void Update();

    public void Init() {
        this.Actions = new UIActions();
        this.InputState = new InputState();
        RenderState = new RenderState(this);
    }
}