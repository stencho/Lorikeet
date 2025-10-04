using System;
using Microsoft.Xna.Framework;

namespace LorikeetUI;

public class Clock {
    public static GameTime game_time;
    public static double delta_time => game_time.ElapsedGameTime.TotalSeconds;
    public static double delta_time_ms => game_time.ElapsedGameTime.TotalMilliseconds;
    
    public static double update_thread_tick_rate = 60.0;
    public static TimeSpan update_thread_goal_time = new TimeSpan((long)(10000 * (1000.0/update_thread_tick_rate)));
    public static double update_thread_delta => 10000.0 * (1000.0 / update_thread_tick_rate);
    public static double frame_rate { get; set; } = 0;
    private static double _frame_rate_timer = 0;
    private static double _frame_counter = 0;
    
    public static double tick_rate { get; set; } = 0;
    private static double _tick_rate_timer = 0;
    private static double _tick_counter = 0;

    public static ulong frame_count = 0;
    public static ulong tick_count = 0;

    internal static void FrameRateUpdate(double milliseconds) {
        _frame_rate_timer += milliseconds;
        _frame_counter++;
    
        if (_frame_rate_timer >= 1000.0) {
            frame_rate = _frame_counter;
            _frame_rate_timer -= 1000.0;
            _frame_counter = 0;
        }

        frame_count++;
    }
    
    internal static void TickRateUpdate(double milliseconds) {
        _tick_rate_timer += milliseconds;
        _tick_counter++;
        tick_count++;
        if (_tick_rate_timer >= 1000.0) {
            tick_rate = _tick_counter;
            _tick_rate_timer -= 1000.0;
            _tick_counter = 0;
        }
    }
}