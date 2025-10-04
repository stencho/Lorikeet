using System;
using System.Runtime.CompilerServices;
using Gtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LorikeetUI;

public static class State {
    public static Game game;
    public static GameWindow window;
    
    public static GraphicsDeviceManager graphics;
    public static GraphicsDevice graphics_device;
    public static ContentManager content;

    private static bool focused = false;
    private static bool was_focused = false;

    public static bool Focused => focused;
    public static bool WasFocused => was_focused;
    
    public static bool JustLostFocus => !Focused && WasFocused;
    public static bool JustGainedFocus => Focused && WasFocused;


    public static MultiAction WindowSizeChanged = new MultiAction();
        
    static XYPair LastWindowBounds;
    public static XYPair WindowBounds { get; internal set; }

    public static void Initialize(Game game, GraphicsDeviceManager graphics, GraphicsDevice graphics_device, ContentManager content, GameWindow window) {
        State.graphics = graphics;
        State.graphics_device = graphics_device;
        State.content = content;
        State.window = window;
        State.game = game;
        
        WindowBounds = window.ClientBounds.Size.ToXYPair();
        LastWindowBounds = WindowBounds;
    }
    
    public static void Update(GameTime gt) {
        LastWindowBounds = WindowBounds;
        WindowBounds = window.ClientBounds.Size.ToXYPair();
    
        if (WindowSizeChanged != null && WindowBounds != LastWindowBounds)
            WindowSizeChanged.invoke_all();
        
        was_focused = Focused;
        focused = game.IsActive;
        
        Input.mouse_previous = Input.mouse;
        Input.mouse = Mouse.GetState();

        Input.keyboard_previous = Input.keyboard;
        Input.keyboard = Keyboard.GetState();

        Input.gamepad_previous = Input.gamepad;
        Input.gamepad = GamePad.GetState(PlayerIndex.One);
        
        Clock.game_time = gt;
    }
}

public static class Input {
    public static KeyboardState keyboard;
    public static KeyboardState keyboard_previous;
    
    public static MouseState mouse;
    public static MouseState mouse_previous;
    
    public static GamePadState gamepad;
    public static GamePadState gamepad_previous;
    
    public static Vector2 mouse_pos => mouse.Position.ToVector2();
    public static Vector2 mouse_pos_previous => mouse_previous.Position.ToVector2();
    
    public static bool shift => keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
    
    public static int MouseWheelDelta => mouse.ScrollWheelValue - mouse_previous.ScrollWheelValue;
    public static int MouseWheelHorizontalDelta => mouse.HorizontalScrollWheelValue - mouse_previous.HorizontalScrollWheelValue;

    public static int MouseWheelTicks => MouseWheelDelta / 120;
    public static int MouseWheelHorizontalTicks => MouseWheelHorizontalDelta / 120;

    public static Vector2 MouseDelta => mouse_pos - mouse_pos_previous;
    
    public static bool JustPressed(MouseButtons mouse_button) {
        switch (mouse_button) {
            case MouseButtons.Left: return mouse.LeftButton == ButtonState.Pressed && mouse_previous.LeftButton == ButtonState.Released;
            case MouseButtons.Right: return mouse.RightButton == ButtonState.Pressed && mouse_previous.RightButton == ButtonState.Released;
            case MouseButtons.Middle: return mouse.MiddleButton == ButtonState.Pressed && mouse_previous.MiddleButton == ButtonState.Released;
            case MouseButtons.X1: return mouse.XButton1 == ButtonState.Pressed && mouse_previous.XButton1 == ButtonState.Released;
            case MouseButtons.X2: return mouse.XButton2 == ButtonState.Pressed && mouse_previous.XButton2 == ButtonState.Released;
            default: return false;
        }
    }
    
    public static bool JustReleased(MouseButtons mouse_button) {
        switch (mouse_button) {
            case MouseButtons.Left: return mouse.LeftButton != ButtonState.Pressed && mouse_previous.LeftButton != ButtonState.Released;
            case MouseButtons.Right: return mouse.RightButton != ButtonState.Pressed && mouse_previous.RightButton != ButtonState.Released;
            case MouseButtons.Middle: return mouse.MiddleButton != ButtonState.Pressed && mouse_previous.MiddleButton != ButtonState.Released;
            case MouseButtons.X1: return mouse.XButton1 != ButtonState.Pressed && mouse_previous.XButton1 != ButtonState.Released;
            case MouseButtons.X2: return mouse.XButton2 != ButtonState.Pressed && mouse_previous.XButton2 != ButtonState.Released;
            default: return false;
        }
    }
    
    public enum MouseButtons {
        Left, Right, Middle, X1, X2
    }
}