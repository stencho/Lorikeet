using System.Runtime.CompilerServices;
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

    public static void Initialize(Game game, GraphicsDeviceManager graphics, GraphicsDevice graphics_device, ContentManager content, GameWindow window) {
        State.graphics = graphics;
        State.graphics_device = graphics_device;
        State.content = content;
        State.window = window;
        State.game = game;
    }

    public static void Update() {
        Input.mouse_previous = Input.mouse;
        Input.mouse = Mouse.GetState();

        Input.keyboard_state_previous = Input.keyboard_state;
        Input.keyboard_state = Keyboard.GetState();
    }
}

public static class Input {
    public static KeyboardState keyboard_state;
    public static KeyboardState keyboard_state_previous;
    
    public static MouseState mouse;
    public static MouseState mouse_previous;

    public static Vector2 mouse_pos => mouse.Position.ToVector2();
    public static Vector2 mouse_pos_previous => mouse_previous.Position.ToVector2();
    
    public enum MouseButtons {
        Left, Right, Middle, X1, X2
    }
    
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
    
    public static int MouseWheelDelta => mouse.ScrollWheelValue - mouse_previous.ScrollWheelValue;
    public static int MouseWheelHorizontalDelta => mouse.HorizontalScrollWheelValue - mouse_previous.HorizontalScrollWheelValue;

    public static int MouseWheelTicks => MouseWheelDelta / 120;
    public static int MouseWheelHorizontalTicks => MouseWheelHorizontalDelta / 120;

    public static Vector2 MouseDelta => mouse_pos - mouse_pos_previous;
}