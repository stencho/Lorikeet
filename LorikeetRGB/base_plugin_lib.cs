using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace base_plugin_lib {
    public struct led_state {
        public byte LED_count;
        public byte LED_pin;

        public bool lights_on;
        public bool lamp_mode;
        public bool rc_mode;

        public int[] selected_plugins;

        public zone[] zones;
        public byte[] get_zone_string() {
            if (zones == null || zones.Length == 0) { return new byte[] { }; }
            byte[] tmp = new byte[1+(zones.Length * 6)+1];
            tmp[0] = (byte)'Z';

            byte inc = 1;
            foreach (zone z in zones) {
                tmp[inc] = z.start;
                inc++;
                tmp[inc] = z.length;
                inc++;
                tmp[inc] = z.R;
                inc++;
                tmp[inc] = z.G;
                inc++;
                tmp[inc] = z.B;
                inc++;
                tmp[inc] = z.brightness;
                inc++;
            }
            tmp[inc] = (byte)'/';
            return tmp;
        }
    }

    public struct zone {
        public byte start;
        public byte length;

        public byte R;
        public byte G;
        public byte B;
        public byte brightness;
    }

    public interface IRGBPlugin {
        bool loaded { get; set; }

        byte in_R { get; set; }
        byte in_G { get; set; }
        byte in_B { get; set; }
        byte in_brightness { get; set; }

        byte out_R { get; set; }
        byte out_G { get; set; }
        byte out_B { get; set; }
        byte out_brightness { get; set; }
        

        void load();
        void update(led_state current_status);
        void show_config_dialog();
    }
    
    public enum save_value_type {
        BOOL,
        STRING,
        BYTE,
        INT,
        FLOAT
    }

    //okay, easy rules for determining the above
    //bool will always be verbatim true or false, no "s or anything, just name = true
    //string will always begin with and end with ", name = "fart"
    //byte and all of the other numeric types will always be stored on disk as their hex values, and length will easily determine their type
    //byte stored as 0x00 - 0xFF, name = 0x69
    //int stored as 0x00000000 - 0xFFFFFFFF, you know the drill
    //float stored as n.nnnnn, detected by being only numbers and one ., and float should be kept normalized where possible and combined with secondary ints if need be

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class SaveThisValue : Attribute {
        public string name;
        public save_value_type type;

        public SaveThisValue(string plugin_name, save_value_type type) {
            name = plugin_name;
            this.type = type;
        }
    }

    public class PluginConfigFileManager {
        private object plugin = null;
        private string config_name;
        FieldInfo[] fields;

        public PluginConfigFileManager(string config_name, object plugin) {
            this.plugin = plugin;
            this.config_name = config_name + ".cfg";
            fields = plugin.GetType().GetFields();
        }

        string text_buffer = "";

        void fill_text_buffer() {
            fields = plugin.GetType().GetRuntimeFields().ToArray();

            if (fields != null) {
                foreach (FieldInfo fi in fields) {
                    SaveThisValue fa = (SaveThisValue)fi.GetCustomAttribute(typeof(SaveThisValue));
                    if (fa != null) {
                        text_buffer += string.Format("{0} = {1}\r\n", fa.name, 
                            value_memory_to_string(fa.name, fa.type, fi.GetValue(plugin)));                        
                    }
                }
            }
        }

        string value_memory_to_string(string name, save_value_type type, object value) {
            switch (type) {
                case save_value_type.BOOL:    return ((bool)(value)).ToString().ToLower();
                case save_value_type.STRING:  return (string)value.ToString();
                case save_value_type.BYTE:    return ((byte)value).ToString("X");
                case save_value_type.INT:     return ((int)value).ToString("X8");
                case save_value_type.FLOAT:   return ((float)value).ToString("F9");
                default: return value.ToString() + " ??????";
            }            
        }



        public void save() {
            fill_text_buffer();

            using (FileStream fs = File.Open("plugins\\config\\" + config_name, FileMode.Create)) {
                fs.Write(Encoding.ASCII.GetBytes(text_buffer), 0, text_buffer.Length);
            }

            Console.WriteLine("Wrote text buffer: ");
            Console.Write(text_buffer);
        }

        public void load() {

        }
    }
}
