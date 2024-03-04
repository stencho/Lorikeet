using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.CoreAudioApi;
using System.Windows.Forms;

using base_plugin_lib;
using SlimDX.Direct3D11;
using SlimDX.Direct3D9;
using SlimDX;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace test_plugin {

    [Plugin]
    public class webhook : IRGBPlugin {

        public static class listener {
            static bool listening = false;

            static TcpListener tcp = null;
            static Int32 port = 8080;
            static IPAddress addr = IPAddress.Parse("127.0.0.1");
            static byte[] buffer = new byte[256];

            public static bool alert = false;
            public static DateTime last_packet = DateTime.Now;
            public static double ms_since_packet;

            static Thread listen_thread;

            public static void start_listen() {
                if (listening) return;

                Console.WriteLine("opening TCP");

                tcp = new TcpListener(addr, port);
                tcp.Start();

                listen_thread = new Thread(listen);
                listen_thread.Start();

                listening = true;
            }

            public static void stop_listen() {
                if (!listening) return;

                tcp.Stop();

                listening = false;
            }

            static void listen() {

                while (listening) {
                    ms_since_packet = (DateTime.Now - last_packet).TotalMilliseconds;

                    while (!tcp.Pending()) { Thread.Sleep(10); }

                    using (TcpClient client = tcp.AcceptTcpClient()) {

                        try {
                            Console.WriteLine("CONNECTED");

                            while (client.Connected) {
                                ms_since_packet = (DateTime.Now - last_packet).TotalMilliseconds;


                                if (ms_since_packet <= 1000)
                                    alert = true;
                                else {
                                    alert = false;
                                }


                                //Console.WriteLine($"\n[CA { client.Available }]");
                                while (client.Available > 0) {
                                    using (NetworkStream ns = client.GetStream()) {
                                        //while (ns.DataAvailable) {

                                        Console.WriteLine($"\n[DATA]");

                                        int l = 0;
                                        buffer = new byte[256];
                                        while ((l = ns.Read(buffer, 0, buffer.Length)) > 0) {
                                            string d = System.Text.Encoding.ASCII.GetString(buffer, 0, l);
                                            Console.Write(d);
                                        }
                                        Console.WriteLine("\n[DATA END]");

                                        last_packet = DateTime.Now;
                                        //}

                                    }
                                }


                            }
                        } catch {
                            client.Close();
                            client.Dispose();
                            Console.WriteLine("DISCONNECTED");
                        }
                    }

                }
            }

            public static bool get_state() {
                return listening;
            }
        }


        public bool loaded { get; set; } = false;

        public zone state_in { get; set; }
        public zone state_out { get; set; }

        public void load() {
            listener.start_listen();

            loaded = true;
        }

        public void show_config_dialog() {

        }

        public void update(led_state current_status) {
            zone state_tmp = state_in;
            if (listener.alert)
                Console.WriteLine(listener.ms_since_packet);

            if (!listener.alert) {
                state_tmp.R = 0;
                state_tmp.G = 255;
                state_tmp.B = 0;
            } else {
                state_tmp.R = 255;
                state_tmp.G = 0;
                state_tmp.B = 255;
            }
            state_out = state_tmp;
        }
    }

    [Plugin]
    public class audio : IRGBPlugin {
        PluginConfigFileManager pcfm;

        public bool loaded { get; set; } = false;

        public zone state_in { get; set; }
        public zone state_out { get; set; }

        
        MMDeviceEnumerator devenum = new MMDeviceEnumerator();
        MMDevice device;
        
        float current_volume_l = 0.5f;
        float current_volume_r = 0.5f;

        float output_level_l = 0.5f;
        float output_level_r = 0.5f;

        float lerp(float A, float B, float D) {
            float a = A;
            if (a < B) {
                if (a + D > B) {
                    a = B;
                } else {
                    a += D;
                }
            } else {
                if (a - D < B) {
                    a = B;
                } else {
                    a -= D;
                }
            }
            return a;
        }

        public void update(led_state current_status) {
            zone state_tmp = state_in;

            if (device == null) {
                device = devenum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            }

            current_volume_l = device.AudioMeterInformation.PeakValues[0];
            current_volume_r = device.AudioMeterInformation.PeakValues[1];
            
            
            if (current_volume_l > output_level_l)
                output_level_l = current_volume_l;
            else
                output_level_l *= .93f;

            if (current_volume_r > output_level_r)
                output_level_r = current_volume_r;
            else
                output_level_r *= .93f;
            
            
            output_level_l = lerp(output_level_l, current_volume_l, output_level_l * 0.3f);
            output_level_r = lerp(output_level_r, current_volume_r, output_level_r * 0.3f);

            if (output_level_l < 0) output_level_l = 0;
            if (output_level_r < 0) output_level_r = 0;
            
            output_level_l = current_volume_l;
            output_level_r = current_volume_r;


            state_tmp.brightness = (byte)(5 + (((output_level_l + output_level_r) / 2f) * 249));
            
            state_out = state_tmp;
        }

        public void load() {
            pcfm = new PluginConfigFileManager("audio", this);

            loaded = true;
        }

        private void Button1_Click(object sender, EventArgs e) {
            pcfm.save();
        }
        
        public void show_config_dialog() {
            MessageBox.Show("No config available!");
        }
    }

    public class DxScreenCapture {
        SlimDX.Direct3D9.Device d;
        public Surface S;
        PresentParameters present_params;

        public DxScreenCapture() {
            present_params = new PresentParameters();
            present_params.Windowed = true;
            present_params.SwapEffect = SwapEffect.Discard;
            d = new SlimDX.Direct3D9.Device(new Direct3D(), 0, DeviceType.Hardware, IntPtr.Zero, CreateFlags.SoftwareVertexProcessing, present_params);
        }

        public void CaptureScreen() {
            S = Surface.CreateOffscreenPlain(d, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, Format.A8R8G8B8, Pool.Scratch);
            d.GetFrontBufferData(0, S);           
        }
        
        public Color CalculateAverageColor() {
            using (DataStream gs = Surface.ToStream(S, SlimDX.Direct3D9.ImageFileFormat.Bmp)) {

                int nBytes = Screen.PrimaryScreen.Bounds.Width * Screen.PrimaryScreen.Bounds.Height * 3;
                byte[] bu = new byte[nBytes];
                int r = 0; int g = 0; int b = 0;

                int i = 0;

                gs.Position = 0;
                gs.Read(bu, 0, nBytes);
                while (i < nBytes) {
                    // Note: pixel format is BGR              
                    r += bu[i+0];
                    g += bu[i + 1];
                    b += bu[i +2];
                    i += 3;
                }
                int nPixels = i / 4;


                S.Dispose();
                return Color.FromArgb(r / nPixels, (int)((g / nPixels ) * 0.6f), b / nPixels);
            }
        }
    }

    [Plugin]
    public class screen : IRGBPlugin {
        DxScreenCapture sc;
        
        public bool loaded { get; set; } = false;

        public zone state_in { get; set; }
        public zone state_out { get; set; }

        PluginConfigFileManager pcfm;


        public void load() {
            pcfm = new PluginConfigFileManager("screen", this);
            loaded = true;
        }

        public void show_config_dialog() {
            MessageBox.Show("No config available!");
        }

        zone state_tmp; Color c;
        public void update(led_state current_status) {
            if (sc == null)
                sc = new DxScreenCapture();

            state_tmp = state_in;

            sc.CaptureScreen();

            c = sc.CalculateAverageColor();


            state_tmp.R = c.R;
            state_tmp.G = c.G;
            state_tmp.B = c.B;
            state_tmp.brightness = 127;
            state_out = state_tmp;
        }
    }
    /*
    public class throb : IRGBPlugin {
        public bool loaded { get; set; } = false;

        public byte in_R { get; set; }
        public byte in_G { get; set; }
        public byte in_B { get; set; }
        public byte in_brightness { get; set; }

        public byte out_R { get; set; }
        public byte out_G { get; set; }
        public byte out_B { get; set; }
        public byte out_brightness { get; set; }

        public bool uses_zones { get; set; } = false;
        public Color out_col_left { get; set; }
        public Color out_col_center { get; set; }
        public Color out_col_right { get; set; }
        
        float triangle = 0.5f;
        bool flipflop = false;

        public void load() {
            loaded = true;
        }

        public void update(led_state current_status) {
            if (flipflop)
                triangle += 0.002f;
            else
                triangle -= 0.002f;

            if (triangle > 1f) { triangle = 1; flipflop = !flipflop; }
            if (triangle < 0.05f) { triangle = 0.05f; flipflop = !flipflop; }

            out_R = in_R;
            out_G = in_G;
            out_B = in_B;

            out_brightness = (byte)(triangle * 255);
        }

        public void show_config_dialog() {
            MessageBox.Show("No config available!");
        }
    }*/

}
