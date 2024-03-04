using LorikeetLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LorikeetUI {
    public partial class MainStripForm : Form {
        static SerialPort serial;

        static bool connected = false;

        static int attempts = 0;

        static string connect_buffer = "";

        static Thread RC_timer;
        

        static led_state strip_state;

        static System.Windows.Forms.Timer reconnect_timer = new System.Windows.Forms.Timer();
        int reconnect_interval = 1000; //retry reconnect every 20 seconds after DC

        static Type[] plugins;

        private void load_plugins() {
            var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


            //well, plugin right now
            Assembly a = Assembly.LoadFrom(Path.Combine(appDir, "plugins\\test_plugin.dll"));

            plugins = a.GetTypes();

            //go through each of the types available in the assembly
            for (int i = 0; i < plugins.Length; i++) {
                Type yt = plugins[i];

                try {
                    plugin_selection_box.Items.Add(yt.FullName);
                    Console.WriteLine("Added plugin class: " + yt.FullName);
                } catch { }
            }
        }

        private bool try_connect() {
            
            RC_timer = new Thread(new ThreadStart(rctm));

            strip_state.LED_pin = 6;

            strip_state.zones = new List<zone>();

            strip_state.zones.Add(new zone());
            strip_state.zones[0].R = 0;
            strip_state.zones[0].G = 150;
            strip_state.zones[0].B = 0;
            strip_state.zones[0].brightness = 255;
            strip_state.zones[0].start = 0;
            strip_state.zones[0].length = 6;
            strip_state.zones[0].zone_name = "left";

            strip_state.zones.Add(new zone());
            strip_state.zones[1].R = 255;
            strip_state.zones[1].G = 0;
            strip_state.zones[1].B = 90;
            strip_state.zones[1].brightness = 255;
            strip_state.zones[1].start = 6;
            strip_state.zones[1].length = 6;
            strip_state.zones[1].zone_name = "middle";

            strip_state.zones.Add(new zone());
            strip_state.zones[2].R = 0;
            strip_state.zones[2].G = 150;
            strip_state.zones[2].B = 0;
            strip_state.zones[2].brightness = 255;
            strip_state.zones[2].start = 12;
            strip_state.zones[2].length = 6;
            strip_state.zones[2].zone_name = "right";

            strip_state.zones.Add(new zone());
            strip_state.zones[3].R = 255;
            strip_state.zones[3].G = 0;
            strip_state.zones[3].B = 200;
            strip_state.zones[3].brightness = 255;
            strip_state.zones[3].start = 18;
            strip_state.zones[3].length = 6;
            strip_state.zones[3].zone_name = "top";

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
                
                //after all of that, we've found the arduino or whatever, so load plugins
                if (connected) {
                    serial.Write(new byte[] { (byte)'C', 0x01, (byte)'/' }, 0, 3);
                    SetStatusBarText("Connected ");
                    return true;
                }
            }

            return false;
        }

        delegate void SetTextCallback(string text);
        delegate void inv();


        private void SetToggleButtonText(string text) {
            if (this.toggle_button.InvokeRequired) {
                SetTextCallback d = new SetTextCallback(SetToggleButtonText);
                try {
                    this.Invoke(d, new object[] { text });
                } catch { }
            } else {
                this.toggle_button.Text = text;
            }
        }
        private void SetToggleLampButtonText(string text) {
            if (this.lamp_button.InvokeRequired) {
                SetTextCallback d = new SetTextCallback(SetToggleLampButtonText);
                try {
                    this.Invoke(d, new object[] { text });
                } catch { }
            } else {
                this.lamp_button.Text = text;
            }
        }
        private void SetStatusBarText(string text) {
            if (this.serial_status_label.InvokeRequired) {
                SetTextCallback d = new SetTextCallback(SetStatusBarText);
                try {
                    this.Invoke(d, new object[] { text });
                } catch { }
            } else {
                this.serial_status_label.Text = text;
            }
        }
        

        //REMOTE CONTROL UPDATE THREAD
        bool rc_thread_started = false;
        void rctm() {
            rc_thread_started = true;
            while (strip_state.rc_mode == true) {


                for(int i = 0; i < strip_state.zones.Count; i++) {
                    //run plugins picked for each zone
                    if (strip_state.zones[i].plugin_instance == null) continue;

                    strip_state.zones[i].plugin_instance.state_in = strip_state.zones[i];

                    strip_state.zones[i].plugin_instance.update(strip_state);

                    strip_state.zones[i].R = strip_state.zones[i].plugin_instance.state_out.R;
                    strip_state.zones[i].G = strip_state.zones[i].plugin_instance.state_out.G;
                    strip_state.zones[i].B = strip_state.zones[i].plugin_instance.state_out.B;

                    strip_state.zones[i].brightness = strip_state.zones[i].plugin_instance.state_out.brightness;

                    //Console.WriteLine("testing zone " + i); 
                }


                try {
                    if (strip_state.zones != null && strip_state.zones.Count > 0) {
                        var zs = strip_state.get_zone_string();

                        serial.Write(zs, 0, zs.Length);
                    }
                } catch (InvalidOperationException ex) {
                    connected = false;
                    strip_state.rc_mode = false;
                    rc_thread_started = false;
                    SetStatusBarText("Disconnected");
                    
                }

                Thread.Sleep(30);
            }
            rc_thread_started = false;
        }

        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e) {
            string a = serial.ReadExisting();

            connect_buffer = a;

            if (!connected) return;
            
            if (a.Length == 3) {
                strip_state.lights_on = (byte)(a[0]) > 0;
                strip_state.lamp_mode = (byte)(a[1]) > 0;
                strip_state.rc_mode = (byte)(a[2]) > 0;

                SetToggleButtonText("Toggle " + (strip_state.lights_on ? "Off" : "On"));
                SetToggleLampButtonText("Lamp Mode " + (strip_state.lamp_mode ? "Off" : "On"));

                SetStatusBarText("Connected");

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

        void refresh_zone_list() {
            listBox1.Items.Clear();
            foreach (zone z in strip_state.zones) {
                listBox1.Items.Add(z);
            }
        }

        public MainStripForm() {
            InitializeComponent();

            load_plugins();

            try_connect();
            
        }

        private void Form1_Load(object sender, EventArgs e) {
            //attempt connect thread goes here
            //will also need a timer or sth for throwing out the occasional peepee and checking for poopoo or whatever, some sort of ping
            //if that finds it's DC'd or the plugin system sending data does the same, need to set connected to false, reset the serial 
            //and basically start over from what I just said above
            //ez
            refresh_zone_list();
            reconnect_timer.Interval = reconnect_interval;
            reconnect_timer.Tick += Reconnect_timer_Tick;
            reconnect_timer.Start();
        }

        private void Reconnect_timer_Tick(object sender, EventArgs e) {
            if (!connected) {
                if (try_connect()) {
                    reconnect_timer.Stop();
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            if (connected) {
                serial.Write(new byte[] { (byte)'C', 0x00, (byte)'/' }, 0, 3);
                connected = false;
                strip_state.rc_mode = false;
                serial.WriteTimeout = 10;
            }
        }

        private void button5_Click(object sender, EventArgs e) {
            if (connected) {
                if (strip_state.lights_on)
                    strip_state.lights_on = false;
                else
                    strip_state.lights_on = true;

                serial.Write(new byte[] { (byte)'T', (byte)'/' }, 0, 2);
            }
        }

        private void button4_Click(object sender, EventArgs e) {
            if (connected) {
                if (strip_state.lamp_mode)
                    strip_state.lamp_mode = false;
                else
                    strip_state.lamp_mode = true;

                serial.Write(new byte[] { (byte)'W', (byte)'/' }, 0, 2);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            if (listBox1.SelectedIndex >= 0) {
                plugin_selection_box.Enabled = true;
                plugin_config_button.Enabled = true;
                plugin_selection_box.Text = strip_state.zones[listBox1.SelectedIndex].plugin_name;                
            }            
        }

        private void add_zone_button_Click(object sender, EventArgs e) {
            strip_state.zones.Add(new zone("test", Color.Red, 127, 4, 2));
            refresh_zone_list();
        }

        private void delete_zone_button_Click(object sender, EventArgs e) {
            if (listBox1.SelectedIndex >= 0) {
                strip_state.zones.RemoveAt(listBox1.SelectedIndex);
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            }
        }
        private void plugin_selection_box_SelectedIndexChanged(object sender, EventArgs e) {
            if (plugin_selection_box.SelectedIndex > -1 && listBox1.SelectedIndex > -1) {

                zone z = ((zone)strip_state.zones[listBox1.SelectedIndex]);

                if (!string.IsNullOrEmpty(z.plugin_name)) {
                    z.plugin_instance = null;
                }

                z.plugin_name = plugin_selection_box.Items[plugin_selection_box.SelectedIndex].ToString();

                Type t = null;
                foreach(Type p in plugins) {
                    if (p.FullName == z.plugin_name) {
                        t = p;
                        break;
                    }
                }

                if (t != null) {
                    z.plugin_instance = (IRGBPlugin)Activator.CreateInstance(t);
                    z.plugin_instance.load();
                }


                listBox1.Items[listBox1.SelectedIndex] = z;
                strip_state.zones[listBox1.SelectedIndex] = z;

                plugin_selection_box.Text = strip_state.zones[listBox1.SelectedIndex].plugin_name;

                listBox1.Refresh();
            }
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e) {
            e.DrawBackground();
            if (e.Index >= 0) {
                e.Graphics.DrawString(listBox1.Items[e.Index].ToString(), new Font(e.Font.FontFamily, e.Font.Size, FontStyle.Bold), SystemBrushes.ControlText, 2, e.Bounds.Top + 3);
                e.Graphics.DrawString(string.Format("{0}-{1} {2}",
                        ((zone)listBox1.Items[e.Index]).start + 1,
                        ((zone)listBox1.Items[e.Index]).start + ((zone)listBox1.Items[e.Index]).length,
                        (string.IsNullOrWhiteSpace(((zone)listBox1.Items[e.Index]).plugin_name) ? "" : "[" + ((zone)listBox1.Items[e.Index]).plugin_name + "]")
                    ), e.Font, SystemBrushes.ControlText, 2, e.Bounds.Top + 15);
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(strip_state.zones[e.Index].R, strip_state.zones[e.Index].G, strip_state.zones[e.Index].B)), e.Bounds.Right - 2 - (listBox1.ItemHeight - 4), e.Bounds.Top + 2, listBox1.ItemHeight - 4, listBox1.ItemHeight - 4);
            }
        }

        private void plugin_config_button_Click(object sender, EventArgs e) {
            strip_state.zones[listBox1.SelectedIndex].plugin_instance.show_config_dialog();
        }

        private void MainStripForm_KeyDown(object sender, KeyEventArgs e) {
           // if (e.KeyCode == Keys.Escape) Application.Exit();
        }
    }
}
