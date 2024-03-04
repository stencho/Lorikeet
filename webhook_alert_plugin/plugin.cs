using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using base_plugin_lib;

namespace webhook_alert_plugin {
    [Plugin]
    public class webhook : IRGBPlugin {
        public bool loaded { get; set; } = false;

        public zone state_in { get; set; }
        public zone state_out { get; set; }

        public void load() {
            //listener.start_listen();

            loaded = true;
        }

        public void show_config_dialog() {
            
        }

        public void update(led_state current_status) {
            zone state_tmp = state_in;

            state_out = state_tmp;
        }
    }
}
