namespace Recorder
{
    partial class Recorder
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Recorder));
            this.lbl_Timer = new System.Windows.Forms.Label();
            this.btn_record = new System.Windows.Forms.Button();
            this.btn_stop = new System.Windows.Forms.Button();
            this.Timer = new System.Windows.Forms.Timer(this.components);
            this.voice_radio = new System.Windows.Forms.RadioButton();
            this.video_radio = new System.Windows.Forms.RadioButton();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.SuspendLayout();
            // 
            // lbl_Timer
            // 
            this.lbl_Timer.AutoSize = true;
            this.lbl_Timer.Font = new System.Drawing.Font("Digital", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_Timer.Location = new System.Drawing.Point(39, 58);
            this.lbl_Timer.Name = "lbl_Timer";
            this.lbl_Timer.Size = new System.Drawing.Size(253, 62);
            this.lbl_Timer.TabIndex = 0;
            this.lbl_Timer.Text = "00:00:00";
            // 
            // btn_record
            // 
            this.btn_record.Location = new System.Drawing.Point(48, 142);
            this.btn_record.Name = "btn_record";
            this.btn_record.Size = new System.Drawing.Size(80, 30);
            this.btn_record.TabIndex = 1;
            this.btn_record.Text = "Record";
            this.btn_record.UseVisualStyleBackColor = true;
            this.btn_record.Click += new System.EventHandler(this.Btn_record_Click);
            // 
            // btn_stop
            // 
            this.btn_stop.Enabled = false;
            this.btn_stop.Location = new System.Drawing.Point(196, 142);
            this.btn_stop.Name = "btn_stop";
            this.btn_stop.Size = new System.Drawing.Size(80, 30);
            this.btn_stop.TabIndex = 1;
            this.btn_stop.Text = "Stop";
            this.btn_stop.UseVisualStyleBackColor = true;
            this.btn_stop.Click += new System.EventHandler(this.Btn_stop_Click);
            // 
            // Timer
            // 
            this.Timer.Enabled = true;
            this.Timer.Tick += new System.EventHandler(this.TimerTick);
            // 
            // voice_radio
            // 
            this.voice_radio.AutoSize = true;
            this.voice_radio.Checked = true;
            this.voice_radio.Location = new System.Drawing.Point(26, 25);
            this.voice_radio.Name = "voice_radio";
            this.voice_radio.Size = new System.Drawing.Size(52, 17);
            this.voice_radio.TabIndex = 2;
            this.voice_radio.TabStop = true;
            this.voice_radio.Text = "Voice";
            this.voice_radio.UseVisualStyleBackColor = true;
            this.voice_radio.CheckedChanged += new System.EventHandler(this.Voice_radio_CheckedChanged);
            // 
            // video_radio
            // 
            this.video_radio.AutoSize = true;
            this.video_radio.Location = new System.Drawing.Point(95, 25);
            this.video_radio.Name = "video_radio";
            this.video_radio.Size = new System.Drawing.Size(52, 17);
            this.video_radio.TabIndex = 2;
            this.video_radio.Text = "Video";
            this.video_radio.UseVisualStyleBackColor = true;
            this.video_radio.CheckedChanged += new System.EventHandler(this.Video_radio_CheckedChanged);
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "AVRecorder";
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon_MouseDoubleClick);
            // 
            // Recorder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 211);
            this.Controls.Add(this.video_radio);
            this.Controls.Add(this.voice_radio);
            this.Controls.Add(this.btn_stop);
            this.Controls.Add(this.btn_record);
            this.Controls.Add(this.lbl_Timer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Recorder";
            this.Text = "AVRecorder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Recorder_FormClosing);
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.Resize += new System.EventHandler(this.Recorder_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_Timer;
        private System.Windows.Forms.Button btn_record;
        private System.Windows.Forms.Button btn_stop;
        private System.Windows.Forms.Timer Timer;
        private System.Windows.Forms.RadioButton voice_radio;
        private System.Windows.Forms.RadioButton video_radio;
        private System.Windows.Forms.NotifyIcon notifyIcon;
    }
}

