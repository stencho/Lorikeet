namespace LorikeetUI {
    partial class MainStripForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.add_zone_button = new System.Windows.Forms.Button();
            this.delete_zone_button = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.plugin_selection_box = new System.Windows.Forms.ComboBox();
            this.plugin_config_button = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.serial_status_label = new System.Windows.Forms.Label();
            this.lamp_button = new System.Windows.Forms.Button();
            this.toggle_button = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.IntegralHeight = false;
            this.listBox1.ItemHeight = 29;
            this.listBox1.Location = new System.Drawing.Point(6, 19);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(149, 223);
            this.listBox1.TabIndex = 0;
            this.listBox1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBox1_DrawItem);
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // add_zone_button
            // 
            this.add_zone_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.add_zone_button.Location = new System.Drawing.Point(6, 248);
            this.add_zone_button.Name = "add_zone_button";
            this.add_zone_button.Size = new System.Drawing.Size(67, 23);
            this.add_zone_button.TabIndex = 3;
            this.add_zone_button.Text = "Add";
            this.add_zone_button.UseVisualStyleBackColor = true;
            this.add_zone_button.Click += new System.EventHandler(this.add_zone_button_Click);
            // 
            // delete_zone_button
            // 
            this.delete_zone_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.delete_zone_button.Location = new System.Drawing.Point(88, 248);
            this.delete_zone_button.Name = "delete_zone_button";
            this.delete_zone_button.Size = new System.Drawing.Size(67, 23);
            this.delete_zone_button.TabIndex = 4;
            this.delete_zone_button.Text = "Delete";
            this.delete_zone_button.UseVisualStyleBackColor = true;
            this.delete_zone_button.Click += new System.EventHandler(this.delete_zone_button_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.plugin_selection_box);
            this.groupBox1.Controls.Add(this.plugin_config_button);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(179, 41);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(318, 277);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Plugin";
            // 
            // plugin_selection_box
            // 
            this.plugin_selection_box.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.plugin_selection_box.Enabled = false;
            this.plugin_selection_box.FormattingEnabled = true;
            this.plugin_selection_box.Location = new System.Drawing.Point(6, 19);
            this.plugin_selection_box.Name = "plugin_selection_box";
            this.plugin_selection_box.Size = new System.Drawing.Size(306, 21);
            this.plugin_selection_box.TabIndex = 2;
            this.plugin_selection_box.SelectedIndexChanged += new System.EventHandler(this.plugin_selection_box_SelectedIndexChanged);
            // 
            // plugin_config_button
            // 
            this.plugin_config_button.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.plugin_config_button.Enabled = false;
            this.plugin_config_button.Location = new System.Drawing.Point(6, 46);
            this.plugin_config_button.Name = "plugin_config_button";
            this.plugin_config_button.Size = new System.Drawing.Size(306, 23);
            this.plugin_config_button.TabIndex = 1;
            this.plugin_config_button.Text = "Configure";
            this.plugin_config_button.UseVisualStyleBackColor = true;
            this.plugin_config_button.Click += new System.EventHandler(this.plugin_config_button_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(6, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(306, 199);
            this.label1.TabIndex = 0;
            this.label1.Text = "Info String";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.listBox1);
            this.groupBox2.Controls.Add(this.delete_zone_button);
            this.groupBox2.Controls.Add(this.add_zone_button);
            this.groupBox2.Location = new System.Drawing.Point(12, 41);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(161, 277);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Zones";
            // 
            // serial_status_label
            // 
            this.serial_status_label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.serial_status_label.Location = new System.Drawing.Point(1, 320);
            this.serial_status_label.Name = "serial_status_label";
            this.serial_status_label.Size = new System.Drawing.Size(507, 21);
            this.serial_status_label.TabIndex = 7;
            this.serial_status_label.Text = "    Disconnected";
            this.serial_status_label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lamp_button
            // 
            this.lamp_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lamp_button.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.lamp_button.Location = new System.Drawing.Point(257, 12);
            this.lamp_button.Name = "lamp_button";
            this.lamp_button.Size = new System.Drawing.Size(240, 23);
            this.lamp_button.TabIndex = 8;
            this.lamp_button.Text = "Lamp Mode On";
            this.lamp_button.UseVisualStyleBackColor = true;
            this.lamp_button.Click += new System.EventHandler(this.button4_Click);
            // 
            // toggle_button
            // 
            this.toggle_button.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toggle_button.Location = new System.Drawing.Point(12, 12);
            this.toggle_button.Name = "toggle_button";
            this.toggle_button.Size = new System.Drawing.Size(240, 23);
            this.toggle_button.TabIndex = 9;
            this.toggle_button.Text = "Toggle Off";
            this.toggle_button.UseVisualStyleBackColor = true;
            this.toggle_button.Click += new System.EventHandler(this.button5_Click);
            // 
            // MainStripForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(509, 343);
            this.Controls.Add(this.toggle_button);
            this.Controls.Add(this.lamp_button);
            this.Controls.Add(this.serial_status_label);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainStripForm";
            this.Text = "Lorikeet";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainStripForm_KeyDown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button add_zone_button;
        private System.Windows.Forms.Button delete_zone_button;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox plugin_selection_box;
        private System.Windows.Forms.Button plugin_config_button;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label serial_status_label;
        private System.Windows.Forms.Button lamp_button;
        private System.Windows.Forms.Button toggle_button;
    }
}

