using base_plugin_lib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LorikeetRGB {
    static class Program {
        //this is literally all just to detect if the fucking program is closing
        //thanks microsoft lol goodbye appdomain.currentdomain.processexit
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(SetConsoleCtrlEventHandler handler, bool add);
        private delegate bool SetConsoleCtrlEventHandler(CtrlType sig);
        private enum CtrlType {
            CTRL_C_EVENT = 0,
            BREAK_EVENT = 1,
            CLOSE_EVENT = 2,
            LOGOFF_EVENT = 5,
            SHUTDOWN_EVENT = 6
        }

        static SerialPort serial;

        static led_state strip_state;

        //static Dictionary<string, IRGBPlugin> plugins = new Dictionary<string, IRGBPlugin>();
        static Dictionary<string, Type> plugin_types = new Dictionary<string, Type>();

        static bool connected = false;
        static bool loaded = false;

        static int attempts = 0;

        static string connect_buffer = "";

        static Thread RC_timer;

        static byte zone_entry_index_value = 0;

        [STAThread]
        static void Main() {
            init();

            while (loaded) {
                ConsoleKey k;

                switch (state) {
                    case menu_state.main:
                        Console.CursorVisible = false;
                        k = Console.ReadKey(true).Key;

                        if (k == ConsoleKey.L) {
                            serial.Write("W/");
                        } else if (k == ConsoleKey.T) {
                            serial.Write("T/");
                        } else if (k == ConsoleKey.Z) {
                            state = menu_state.zone_config;
                        }

                        break;
                    case menu_state.zone_config:
                        Console.CursorVisible = false;
                        k = Console.ReadKey(true).Key;

                        if (k == ConsoleKey.Escape) {
                            state = menu_state.main;
                        }
                        
                        break;

                    case menu_state.zone_entry_index:                        
                        Console.CursorVisible = true;
                        Console.ReadLine();
                        k = Console.ReadKey(false).Key;

                        switch (k) {
                            case ConsoleKey.Enter:
                                break;
                            case ConsoleKey.Escape:
                                break;

                            case ConsoleKey.End:
                                break;
                            case ConsoleKey.Home:
                                break;

                            case ConsoleKey.LeftArrow:
                                break;
                            case ConsoleKey.RightArrow:
                                break;

                            case ConsoleKey.UpArrow:
                                break;
                            case ConsoleKey.DownArrow:
                                break;

                            case ConsoleKey.Delete:
                                break;
                            case ConsoleKey.Backspace:
                                break;

                            case ConsoleKey.D0:
                                break;
                            case ConsoleKey.D1:
                                break;
                            case ConsoleKey.D2:
                                break;
                            case ConsoleKey.D3:
                                break;
                            case ConsoleKey.D4:
                                break;
                            case ConsoleKey.D5:
                                break;
                            case ConsoleKey.D6:
                                break;
                            case ConsoleKey.D7:
                                break;
                            case ConsoleKey.D8:
                                break;
                            case ConsoleKey.D9:
                                break;
                        }

                        break;
                }

            }
        }


        static void init() {
            Console.CursorVisible = false;
            SetConsoleCtrlHandler(OnExit, true);
            
            RC_timer = new Thread(new ThreadStart(rctm));

            strip_state.LED_pin = 6;

            strip_state.zones = new List<zone>();

            strip_state.zones.Add(new zone());
            strip_state.zones[0].R = 255;
            strip_state.zones[0].G = 0;
            strip_state.zones[0].B = 255;
            strip_state.zones[0].brightness = 255;
            strip_state.zones[0].start = 0;
            strip_state.zones[0].length = 3;

            strip_state.zones.Add(new zone());
            strip_state.zones[1].R = 0;
            strip_state.zones[1].G = 255;
            strip_state.zones[1].B = 0;
            strip_state.zones[1].brightness = 255;
            strip_state.zones[1].start = 3;
            strip_state.zones[1].length = 3;
            
            //configure serial port
            serial = new SerialPort();

            serial.BaudRate = 115200;
            serial.WriteTimeout = 100;

            serial.Handshake = Handshake.None;
            serial.Encoding = Encoding.Default;

            //for boards with 32u4 and similar chips
            serial.DtrEnable = true;
            serial.RtsEnable = true;

            Console.WriteLine("Connecting...");

            serial.DataReceived += Serial_DataReceived;

            //this is simpler than it seems
            //we attempt each available serial port
            foreach (string pn in SerialPort.GetPortNames()) {
                Console.WriteLine("Trying " + pn + ": ");

                //first we configure the one thing we care about
                serial.PortName = pn;

                try {
                    //then we attempt to open the port
                    serial.Open();

                    if (serial.IsOpen) {

                        //if it opens, we attempt multiple times to send the string PEEPEE
                        while (connect_buffer == "" && attempts < 15) {
                            attempts++;

                            Console.WriteLine("PEEPEE...");
                            serial.Write(new byte[] { (byte)'P', (byte)'E', (byte)'E', (byte)'P', (byte)'E', (byte)'E', (byte)'/' }, 0, 7);
                            Thread.Sleep(10);

                            if (connect_buffer == "POOPOO") {
                                Console.WriteLine("...POOPOO!");
                                Console.WriteLine("YEAH PEEPEE POOPOO HANDSHAKE\n");

                                connected = true;
                                attempts = 0;
                                connect_buffer = "";

                                //we're connected, so tell the other end to enable remote control and turn on
                                serial.Write(new byte[] { (byte)'C', 0x00, (byte)'/' }, 0, 3);

                                break;
                            } else {
                                connect_buffer = "";
                            }
                        }
                        //failed to connect after 5 attempts, RIP, on to the next port
                        if (!connected) {
                            Console.WriteLine("No POOPOO!");
                        }
                    }

                    //reset everything
                    attempts = 0;
                    connect_buffer = "";

                    if (!connected)
                        serial.Close();

                    //common contingencies
                } catch (System.IO.IOException ex) {
                    Console.Write("Invalid!\n");
                    attempts = 0;
                    connect_buffer = "";
                    serial.Close();
                } catch (System.TimeoutException tex) {
                    Console.Write("Timeout!\n");
                    attempts = 0;
                    connect_buffer = "";
                    serial.Close();
                }


                Console.WriteLine("Loading Plugins...");
                //after all of that, we've found the arduino or whatever, so load plugins
                if (connected) {
                    
                    var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                    
                    //well, plugin right now
                    Assembly a = Assembly.LoadFrom(Path.Combine(appDir, "plugins\\test_plugin.dll"));
                    Type[] array = a.GetTypes();

                    //go through each of the types available in the assembly
                    for (int i = 0; i < array.Length; i++) {
                        Type yt = array[i];
                        Type t = (Type)a.GetTypes().GetValue(i);


                        try {
                            //attempt to create an instance of the type as an IRGBPlugin for the internal plugin list
                            //plugins.Add(yt.FullName, (IRGBPlugin)Activator.CreateInstance(t));
                            //plugins[yt.FullName].load();

                            Console.WriteLine("Added plugin class: " + yt.FullName);
                        } catch { }
                    }
                    

                    serial.Write(new byte[] { (byte)'C', 0x01, (byte)'/' }, 0, 3);

                    break;                    
                }
            }

            Console.WriteLine();

            loaded = true;
        }

        private static void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e) {
            string a = serial.ReadExisting();

            connect_buffer = a;

            if (!connected || !loaded) return;

            string tmp_str = "";

            if (a.Length == 3) {
                strip_state.lights_on = (byte)(a[0]) > 0;
                strip_state.lamp_mode = (byte)(a[1]) > 0;
                strip_state.rc_mode = (byte)(a[2]) > 0;


                if (strip_state.rc_mode) {
                    if (!rc_thread_started) {
                        RC_timer = new Thread(new ThreadStart(rctm));
                        RC_timer.Start();
                    }

                } else {
                    if (rc_thread_started) {
                        RC_timer.Abort();
                        rc_thread_started = false;
                    }

                    strip_state.rc_mode = false;
                }
            }
        }



        enum menu_state {
            main,
            zone_config,
            zone_entry_index,
            zone_entry_name
        }

        static menu_state state = menu_state.main;


        static void do_text_UI() {
            //DRAW MAIN SCREEN
            if (state == menu_state.main) {
                Console.Write("Zones: " + strip_state.zones.Count + "  ");

                for (int i = 0; i < strip_state.zones.Count; i++) {
                    zone z = strip_state.zones[i];
                    Console.Write("\n" + i + ": " + z.start + "-" + (z.start + z.length - 1) + " (" + z.length + ") [");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(string.Format("0x{0:x2}", z.R) + " ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(string.Format("0x{0:x2}", z.G) + " ");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(string.Format("0x{0:x2}", z.B));
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("]");
                }

                Console.Write("\n\n");

                if (strip_state.lights_on) Console.ForegroundColor = ConsoleColor.Green;
                else Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[T] Toggle LED Strip " + (strip_state.lights_on ? "(On)   " : "(Off)   "));

                if (strip_state.lamp_mode) Console.ForegroundColor = ConsoleColor.Green;
                else Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[L] Toggle Lamp Mode " + (strip_state.lamp_mode ? "(On) " : "(Off) "));

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Z] Configure Zones ");

                Console.Write("\n              \n                 \n                 ");

                Console.CursorLeft = 0;
                Console.CursorTop -= 8 + strip_state.zones.Count;

            } if (state == menu_state.zone_config) {
                Console.Write("Zones: " + strip_state.zones.Count + "  ");

                for (int i = 0; i < strip_state.zones.Count; i++) {
                    zone z = strip_state.zones[i];
                    Console.Write("\n" + i + ": " + z.start + "-" + (z.start + z.length - 1) + " (" + z.length + ") [");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(string.Format("0x{0:x2}", z.R) + " ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(string.Format("0x{0:x2}", z.G) + " ");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(string.Format("0x{0:x2}", z.B));
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("]");
                }

                Console.Write("\n\n");

                Console.WriteLine("[N] New Zone                    ");
                Console.WriteLine("[P] Change Plugin               ");
                Console.WriteLine("[D] Delete Zone                 ");
                Console.WriteLine("\n                    ");
                Console.WriteLine("[Esc] Cancel                    ");

                Console.CursorLeft = 0;
                Console.CursorTop -= 8 + strip_state.zones.Count;
            }
        }




        //REMOTE CONTROL UPDATE THREAD
        static bool rc_thread_started = false;
        static void rctm() {
            rc_thread_started = true;
            while (strip_state.rc_mode == true) {
                do_text_UI();
                
                foreach (zone z in strip_state.zones) {
                    //run plugins picked for each zone
                }

                if (strip_state.zones != null && strip_state.zones.Count > 0) {
                    var zs = strip_state.get_zone_string();

                    serial.Write(zs, 0, zs.Length);                    
                }

                Thread.Sleep(30);
            }
            rc_thread_started = false;
        }



        private static bool OnExit(CtrlType signal) {
            switch (signal) {
                case CtrlType.BREAK_EVENT:
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.LOGOFF_EVENT:
                case CtrlType.SHUTDOWN_EVENT:
                case CtrlType.CLOSE_EVENT:

                    Console.CursorVisible = true;

                    try {
                        if (connected) {
                            loaded = false;
                            SetConsoleCtrlHandler(OnExit, false);

                            strip_state.rc_mode = false;

                            serial.WriteTimeout = 10;
                            serial.Write(new byte[] { (byte)'C', 0x00, (byte)'/' }, 0, 3);

                        }
                    } catch { }

                    Environment.Exit(0);

                    return false;

                default:
                    return false;
            }
        }

    }
}
