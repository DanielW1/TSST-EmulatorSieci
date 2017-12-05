namespace NewNMS
{
    partial class Application
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonSend = new System.Windows.Forms.Button();
            this.buttonListen = new System.Windows.Forms.Button();
            this.listBoxReceived = new System.Windows.Forms.ListBox();
            this.comboBoxRouters = new System.Windows.Forms.ComboBox();
            this.comboBoxActions = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxTables = new System.Windows.Forms.ComboBox();
            this.labelTable = new System.Windows.Forms.Label();
            this.textBox_IP_IN = new System.Windows.Forms.TextBox();
            this.textBox_Port_IN = new System.Windows.Forms.TextBox();
            this.textBoxBand_IN = new System.Windows.Forms.TextBox();
            this.textBoxFrequencyIN = new System.Windows.Forms.TextBox();
            this.textBoxModulation = new System.Windows.Forms.TextBox();
            this.textBoxBitrate = new System.Windows.Forms.TextBox();
            this.textBoxDestination_IP = new System.Windows.Forms.TextBox();
            this.textBoxPort_OUT = new System.Windows.Forms.TextBox();
            this.textBoxHops = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.textBoxFrequencyOUT = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxBand_OUT = new System.Windows.Forms.TextBox();
            this.Band_OUT = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.buttonHiding = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonSend
            // 
            this.buttonSend.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.buttonSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.buttonSend.Location = new System.Drawing.Point(313, 28);
            this.buttonSend.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonSend.Name = "buttonSend";
            this.buttonSend.Size = new System.Drawing.Size(180, 61);
            this.buttonSend.TabIndex = 0;
            this.buttonSend.Text = "Send";
            this.buttonSend.UseVisualStyleBackColor = false;
            this.buttonSend.Click += new System.EventHandler(this.buttonSend_Click);
            // 
            // buttonListen
            // 
            this.buttonListen.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.buttonListen.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.buttonListen.Location = new System.Drawing.Point(40, 28);
            this.buttonListen.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonListen.Name = "buttonListen";
            this.buttonListen.Size = new System.Drawing.Size(187, 61);
            this.buttonListen.TabIndex = 1;
            this.buttonListen.Text = "Run";
            this.buttonListen.UseVisualStyleBackColor = false;
            this.buttonListen.Click += new System.EventHandler(this.buttonListen_Click_1);
            // 
            // listBoxReceived
            // 
            this.listBoxReceived.FormattingEnabled = true;
            this.listBoxReceived.HorizontalScrollbar = true;
            this.listBoxReceived.ItemHeight = 16;
            this.listBoxReceived.Location = new System.Drawing.Point(596, 15);
            this.listBoxReceived.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.listBoxReceived.Name = "listBoxReceived";
            this.listBoxReceived.ScrollAlwaysVisible = true;
            this.listBoxReceived.Size = new System.Drawing.Size(479, 436);
            this.listBoxReceived.TabIndex = 2;
            // 
            // comboBoxRouters
            // 
            this.comboBoxRouters.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.comboBoxRouters.FormattingEnabled = true;
            this.comboBoxRouters.Location = new System.Drawing.Point(5, 208);
            this.comboBoxRouters.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBoxRouters.Name = "comboBoxRouters";
            this.comboBoxRouters.Size = new System.Drawing.Size(198, 26);
            this.comboBoxRouters.TabIndex = 3;
            // 
            // comboBoxActions
            // 
            this.comboBoxActions.FormattingEnabled = true;
            this.comboBoxActions.Location = new System.Drawing.Point(439, 210);
            this.comboBoxActions.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBoxActions.Name = "comboBoxActions";
            this.comboBoxActions.Size = new System.Drawing.Size(138, 24);
            this.comboBoxActions.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(4, 182);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(199, 24);
            this.label1.TabIndex = 8;
            this.label1.Text = "Agent to communicate";
            // 
            // comboBoxTables
            // 
            this.comboBoxTables.FormattingEnabled = true;
            this.comboBoxTables.Location = new System.Drawing.Point(229, 210);
            this.comboBoxTables.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxTables.Name = "comboBoxTables";
            this.comboBoxTables.Size = new System.Drawing.Size(160, 24);
            this.comboBoxTables.TabIndex = 13;
            // 
            // labelTable
            // 
            this.labelTable.AutoSize = true;
            this.labelTable.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelTable.Location = new System.Drawing.Point(246, 182);
            this.labelTable.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTable.Name = "labelTable";
            this.labelTable.Size = new System.Drawing.Size(129, 24);
            this.labelTable.TabIndex = 14;
            this.labelTable.Text = "Choose Table";
            // 
            // textBox_IP_IN
            // 
            this.textBox_IP_IN.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBox_IP_IN.Location = new System.Drawing.Point(5, 287);
            this.textBox_IP_IN.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_IP_IN.Name = "textBox_IP_IN";
            this.textBox_IP_IN.Size = new System.Drawing.Size(117, 27);
            this.textBox_IP_IN.TabIndex = 15;
            // 
            // textBox_Port_IN
            // 
            this.textBox_Port_IN.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBox_Port_IN.Location = new System.Drawing.Point(166, 362);
            this.textBox_Port_IN.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_Port_IN.Name = "textBox_Port_IN";
            this.textBox_Port_IN.Size = new System.Drawing.Size(91, 27);
            this.textBox_Port_IN.TabIndex = 16;
            // 
            // textBoxBand_IN
            // 
            this.textBoxBand_IN.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBoxBand_IN.Location = new System.Drawing.Point(295, 287);
            this.textBoxBand_IN.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxBand_IN.Name = "textBoxBand_IN";
            this.textBoxBand_IN.Size = new System.Drawing.Size(94, 27);
            this.textBoxBand_IN.TabIndex = 17;
            // 
            // textBoxFrequencyIN
            // 
            this.textBoxFrequencyIN.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBoxFrequencyIN.Location = new System.Drawing.Point(439, 362);
            this.textBoxFrequencyIN.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxFrequencyIN.Name = "textBoxFrequencyIN";
            this.textBoxFrequencyIN.Size = new System.Drawing.Size(138, 27);
            this.textBoxFrequencyIN.TabIndex = 18;
            // 
            // textBoxModulation
            // 
            this.textBoxModulation.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBoxModulation.Location = new System.Drawing.Point(5, 429);
            this.textBoxModulation.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxModulation.Name = "textBoxModulation";
            this.textBoxModulation.Size = new System.Drawing.Size(117, 27);
            this.textBoxModulation.TabIndex = 19;
            // 
            // textBoxBitrate
            // 
            this.textBoxBitrate.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBoxBitrate.Location = new System.Drawing.Point(295, 429);
            this.textBoxBitrate.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxBitrate.Name = "textBoxBitrate";
            this.textBoxBitrate.Size = new System.Drawing.Size(94, 27);
            this.textBoxBitrate.TabIndex = 20;
            // 
            // textBoxDestination_IP
            // 
            this.textBoxDestination_IP.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBoxDestination_IP.Location = new System.Drawing.Point(5, 362);
            this.textBoxDestination_IP.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxDestination_IP.Name = "textBoxDestination_IP";
            this.textBoxDestination_IP.Size = new System.Drawing.Size(119, 27);
            this.textBoxDestination_IP.TabIndex = 21;
            // 
            // textBoxPort_OUT
            // 
            this.textBoxPort_OUT.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBoxPort_OUT.Location = new System.Drawing.Point(166, 287);
            this.textBoxPort_OUT.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxPort_OUT.Name = "textBoxPort_OUT";
            this.textBoxPort_OUT.Size = new System.Drawing.Size(91, 27);
            this.textBoxPort_OUT.TabIndex = 22;
            // 
            // textBoxHops
            // 
            this.textBoxHops.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBoxHops.Location = new System.Drawing.Point(166, 429);
            this.textBoxHops.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxHops.Name = "textBoxHops";
            this.textBoxHops.Size = new System.Drawing.Size(91, 27);
            this.textBoxHops.TabIndex = 23;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label4.Location = new System.Drawing.Point(36, 266);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 20);
            this.label4.TabIndex = 24;
            this.label4.Text = "IP_IN";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label5.Location = new System.Drawing.Point(173, 338);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 20);
            this.label5.TabIndex = 25;
            this.label5.Text = "Port_IN";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label6.Location = new System.Drawing.Point(302, 266);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(73, 20);
            this.label6.TabIndex = 26;
            this.label6.Text = "Band_IN";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label7.Location = new System.Drawing.Point(435, 338);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(112, 20);
            this.label7.TabIndex = 27;
            this.label7.Text = "Frequency_IN";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label8.Location = new System.Drawing.Point(13, 405);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(90, 20);
            this.label8.TabIndex = 28;
            this.label8.Text = "Modulation";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label9.Location = new System.Drawing.Point(302, 405);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(65, 20);
            this.label9.TabIndex = 29;
            this.label9.Text = "BitRate";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label10.Location = new System.Drawing.Point(1, 338);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(118, 20);
            this.label10.TabIndex = 30;
            this.label10.Text = "Destination_IP";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label11.Location = new System.Drawing.Point(173, 266);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(84, 20);
            this.label11.TabIndex = 31;
            this.label11.Text = "Port_OUT";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label12.Location = new System.Drawing.Point(192, 405);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(49, 20);
            this.label12.TabIndex = 32;
            this.label12.Text = "Hops";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label13.Location = new System.Drawing.Point(435, 182);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(131, 24);
            this.label13.TabIndex = 33;
            this.label13.Text = "Choose action";
            // 
            // textBoxFrequencyOUT
            // 
            this.textBoxFrequencyOUT.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBoxFrequencyOUT.Location = new System.Drawing.Point(439, 289);
            this.textBoxFrequencyOUT.Name = "textBoxFrequencyOUT";
            this.textBoxFrequencyOUT.Size = new System.Drawing.Size(138, 27);
            this.textBoxFrequencyOUT.TabIndex = 34;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label2.Location = new System.Drawing.Point(435, 266);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 20);
            this.label2.TabIndex = 35;
            this.label2.Text = "Frequency_OUT";
            // 
            // textBoxBand_OUT
            // 
            this.textBoxBand_OUT.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBoxBand_OUT.Location = new System.Drawing.Point(295, 361);
            this.textBoxBand_OUT.Name = "textBoxBand_OUT";
            this.textBoxBand_OUT.Size = new System.Drawing.Size(94, 27);
            this.textBoxBand_OUT.TabIndex = 36;
            // 
            // Band_OUT
            // 
            this.Band_OUT.AutoSize = true;
            this.Band_OUT.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Band_OUT.Location = new System.Drawing.Point(291, 338);
            this.Band_OUT.Name = "Band_OUT";
            this.Band_OUT.Size = new System.Drawing.Size(92, 20);
            this.Band_OUT.TabIndex = 37;
            this.Band_OUT.Text = "Band_OUT";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.button1.Location = new System.Drawing.Point(40, 102);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(187, 55);
            this.button1.TabIndex = 38;
            this.button1.Text = "Show";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // buttonHiding
            // 
            this.buttonHiding.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.buttonHiding.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.buttonHiding.Location = new System.Drawing.Point(313, 102);
            this.buttonHiding.Name = "buttonHiding";
            this.buttonHiding.Size = new System.Drawing.Size(180, 55);
            this.buttonHiding.TabIndex = 39;
            this.buttonHiding.Text = "Hide ";
            this.buttonHiding.UseVisualStyleBackColor = false;
            this.buttonHiding.Click += new System.EventHandler(this.buttonHiding_Click_1);
            // 
            // Application
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(1071, 461);
            this.Controls.Add(this.buttonHiding);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Band_OUT);
            this.Controls.Add(this.textBoxBand_OUT);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxFrequencyOUT);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxHops);
            this.Controls.Add(this.textBoxPort_OUT);
            this.Controls.Add(this.textBoxDestination_IP);
            this.Controls.Add(this.textBoxBitrate);
            this.Controls.Add(this.textBoxModulation);
            this.Controls.Add(this.textBoxFrequencyIN);
            this.Controls.Add(this.textBoxBand_IN);
            this.Controls.Add(this.textBox_Port_IN);
            this.Controls.Add(this.textBox_IP_IN);
            this.Controls.Add(this.labelTable);
            this.Controls.Add(this.comboBoxTables);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxActions);
            this.Controls.Add(this.comboBoxRouters);
            this.Controls.Add(this.listBoxReceived);
            this.Controls.Add(this.buttonListen);
            this.Controls.Add(this.buttonSend);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Application";
            this.Text = "Application";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonSend;
        private System.Windows.Forms.Button buttonListen;
        private System.Windows.Forms.ListBox listBoxReceived;
        private System.Windows.Forms.ComboBox comboBoxRouters;
        private System.Windows.Forms.ComboBox comboBoxActions;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxTables;
        private System.Windows.Forms.Label labelTable;
        private System.Windows.Forms.TextBox textBox_IP_IN;
        private System.Windows.Forms.TextBox textBox_Port_IN;
        private System.Windows.Forms.TextBox textBoxBand_IN;
        private System.Windows.Forms.TextBox textBoxFrequencyIN;
        private System.Windows.Forms.TextBox textBoxModulation;
        private System.Windows.Forms.TextBox textBoxBitrate;
        private System.Windows.Forms.TextBox textBoxDestination_IP;
        private System.Windows.Forms.TextBox textBoxPort_OUT;
        private System.Windows.Forms.TextBox textBoxHops;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox textBoxFrequencyOUT;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxBand_OUT;
        private System.Windows.Forms.Label Band_OUT;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button buttonHiding;
        protected System.Windows.Forms.Label label4;
    }
}

