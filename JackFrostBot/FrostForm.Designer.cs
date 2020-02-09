namespace JackFrostBot
{
    partial class FrostForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrostForm));
            this.btn_Send = new System.Windows.Forms.Button();
            this.comboBox_Channel = new System.Windows.Forms.ComboBox();
            this.comboBox_Server = new System.Windows.Forms.ComboBox();
            this.txtBox_Msg = new System.Windows.Forms.RichTextBox();
            this.lbl_Attachment = new System.Windows.Forms.Label();
            this.txtBox_Attachment = new System.Windows.Forms.TextBox();
            this.btn_Browse = new System.Windows.Forms.Button();
            this.txtBox_Playing = new System.Windows.Forms.TextBox();
            this.lbl_Nickname = new System.Windows.Forms.Label();
            this.comboBox_Status = new System.Windows.Forms.ComboBox();
            this.radioButton_Locked = new System.Windows.Forms.RadioButton();
            this.radioButton_Unlocked = new System.Windows.Forms.RadioButton();
            this.checkBox_SendTypingMsg = new System.Windows.Forms.CheckBox();
            this.button_Refresh = new System.Windows.Forms.Button();
            this.txtBox_Name = new System.Windows.Forms.TextBox();
            this.lbl_Moderation = new System.Windows.Forms.Label();
            this.comboBox_MemberSelect = new System.Windows.Forms.ComboBox();
            this.btn_Go = new System.Windows.Forms.Button();
            this.comboBox_Action = new System.Windows.Forms.ComboBox();
            this.lbl_Reason = new System.Windows.Forms.Label();
            this.txtBox_Reason = new System.Windows.Forms.TextBox();
            this.lbl_WarnMgmt = new System.Windows.Forms.Label();
            this.listBox_Warns = new System.Windows.Forms.ListBox();
            this.btn_ClearSelectedWarns = new System.Windows.Forms.Button();
            this.btn_ClearMembersWarns = new System.Windows.Forms.Button();
            this.btn_ClearAllWarns = new System.Windows.Forms.Button();
            this.comboBox_ActivityType = new System.Windows.Forms.ComboBox();
            this.lbl_Purge = new System.Windows.Forms.Label();
            this.btn_PurgeMsgs = new System.Windows.Forms.Button();
            this.numericUpDown_Purge = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Purge)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_Send
            // 
            this.btn_Send.Location = new System.Drawing.Point(204, 134);
            this.btn_Send.Name = "btn_Send";
            this.btn_Send.Size = new System.Drawing.Size(117, 113);
            this.btn_Send.TabIndex = 0;
            this.btn_Send.Text = "Send Message to Channel";
            this.btn_Send.UseVisualStyleBackColor = true;
            this.btn_Send.Click += new System.EventHandler(this.Btn_Send_Click);
            // 
            // comboBox_Channel
            // 
            this.comboBox_Channel.FormattingEnabled = true;
            this.comboBox_Channel.Location = new System.Drawing.Point(12, 68);
            this.comboBox_Channel.Name = "comboBox_Channel";
            this.comboBox_Channel.Size = new System.Drawing.Size(208, 21);
            this.comboBox_Channel.TabIndex = 1;
            this.comboBox_Channel.Text = "Default Channel";
            this.comboBox_Channel.SelectedIndexChanged += new System.EventHandler(this.ComboBox_Channel_SelectedIndexChanged);
            // 
            // comboBox_Server
            // 
            this.comboBox_Server.FormattingEnabled = true;
            this.comboBox_Server.Location = new System.Drawing.Point(12, 41);
            this.comboBox_Server.Name = "comboBox_Server";
            this.comboBox_Server.Size = new System.Drawing.Size(208, 21);
            this.comboBox_Server.TabIndex = 2;
            this.comboBox_Server.Text = "Server";
            this.comboBox_Server.SelectedIndexChanged += new System.EventHandler(this.ComboBox_Server_SelectedIndexChanged);
            // 
            // txtBox_Msg
            // 
            this.txtBox_Msg.Location = new System.Drawing.Point(12, 134);
            this.txtBox_Msg.Name = "txtBox_Msg";
            this.txtBox_Msg.Size = new System.Drawing.Size(186, 113);
            this.txtBox_Msg.TabIndex = 3;
            this.txtBox_Msg.Text = "";
            this.txtBox_Msg.TextChanged += new System.EventHandler(this.txtBox_Msg_TextChanged);
            this.txtBox_Msg.Enter += new System.EventHandler(this.txtBox_Msg_Enter);
            this.txtBox_Msg.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtBox_Msg_KeyDown);
            // 
            // lbl_Attachment
            // 
            this.lbl_Attachment.AutoSize = true;
            this.lbl_Attachment.Location = new System.Drawing.Point(12, 251);
            this.lbl_Attachment.Name = "lbl_Attachment";
            this.lbl_Attachment.Size = new System.Drawing.Size(64, 13);
            this.lbl_Attachment.TabIndex = 5;
            this.lbl_Attachment.Text = "Attachment:";
            // 
            // txtBox_Attachment
            // 
            this.txtBox_Attachment.Location = new System.Drawing.Point(12, 267);
            this.txtBox_Attachment.Name = "txtBox_Attachment";
            this.txtBox_Attachment.Size = new System.Drawing.Size(269, 20);
            this.txtBox_Attachment.TabIndex = 6;
            // 
            // btn_Browse
            // 
            this.btn_Browse.Enabled = false;
            this.btn_Browse.Location = new System.Drawing.Point(287, 267);
            this.btn_Browse.Name = "btn_Browse";
            this.btn_Browse.Size = new System.Drawing.Size(34, 20);
            this.btn_Browse.TabIndex = 7;
            this.btn_Browse.Text = "...";
            this.btn_Browse.UseVisualStyleBackColor = true;
            this.btn_Browse.Click += new System.EventHandler(this.Btn_Browse_Click);
            // 
            // txtBox_Playing
            // 
            this.txtBox_Playing.Location = new System.Drawing.Point(430, 15);
            this.txtBox_Playing.Name = "txtBox_Playing";
            this.txtBox_Playing.Size = new System.Drawing.Size(152, 20);
            this.txtBox_Playing.TabIndex = 10;
            this.txtBox_Playing.Text = "Activity Name";
            this.txtBox_Playing.Leave += new System.EventHandler(this.TxtBox_Playing_Leave);
            // 
            // lbl_Nickname
            // 
            this.lbl_Nickname.AutoSize = true;
            this.lbl_Nickname.Location = new System.Drawing.Point(12, 18);
            this.lbl_Nickname.Name = "lbl_Nickname";
            this.lbl_Nickname.Size = new System.Drawing.Size(77, 13);
            this.lbl_Nickname.TabIndex = 9;
            this.lbl_Nickname.Text = "Bot Nickname:";
            // 
            // comboBox_Status
            // 
            this.comboBox_Status.FormattingEnabled = true;
            this.comboBox_Status.Items.AddRange(new object[] {
            "Offline",
            "Online",
            "Idle",
            "AFK",
            "Do Not Disturb",
            "Invisible"});
            this.comboBox_Status.Location = new System.Drawing.Point(588, 15);
            this.comboBox_Status.Name = "comboBox_Status";
            this.comboBox_Status.Size = new System.Drawing.Size(66, 21);
            this.comboBox_Status.TabIndex = 13;
            this.comboBox_Status.Text = "Status";
            this.comboBox_Status.SelectedIndexChanged += new System.EventHandler(this.ComboBox_Status_SelectedIndexChanged);
            // 
            // radioButton_Locked
            // 
            this.radioButton_Locked.AutoSize = true;
            this.radioButton_Locked.Location = new System.Drawing.Point(12, 92);
            this.radioButton_Locked.Name = "radioButton_Locked";
            this.radioButton_Locked.Size = new System.Drawing.Size(61, 17);
            this.radioButton_Locked.TabIndex = 14;
            this.radioButton_Locked.TabStop = true;
            this.radioButton_Locked.Text = "Locked";
            this.radioButton_Locked.UseVisualStyleBackColor = true;
            this.radioButton_Locked.CheckedChanged += new System.EventHandler(this.RadioButton_Locked_CheckedChanged);
            this.radioButton_Locked.Click += new System.EventHandler(this.RadioButton_Locked_Click);
            // 
            // radioButton_Unlocked
            // 
            this.radioButton_Unlocked.AutoSize = true;
            this.radioButton_Unlocked.Location = new System.Drawing.Point(72, 92);
            this.radioButton_Unlocked.Name = "radioButton_Unlocked";
            this.radioButton_Unlocked.Size = new System.Drawing.Size(71, 17);
            this.radioButton_Unlocked.TabIndex = 15;
            this.radioButton_Unlocked.TabStop = true;
            this.radioButton_Unlocked.Text = "Unlocked";
            this.radioButton_Unlocked.UseVisualStyleBackColor = true;
            this.radioButton_Unlocked.CheckedChanged += new System.EventHandler(this.RadioButton_Unlocked_CheckedChanged);
            this.radioButton_Unlocked.Click += new System.EventHandler(this.RadioButton_Unlocked_Click);
            // 
            // checkBox_SendTypingMsg
            // 
            this.checkBox_SendTypingMsg.AutoSize = true;
            this.checkBox_SendTypingMsg.Checked = true;
            this.checkBox_SendTypingMsg.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_SendTypingMsg.Location = new System.Drawing.Point(15, 115);
            this.checkBox_SendTypingMsg.Name = "checkBox_SendTypingMsg";
            this.checkBox_SendTypingMsg.Size = new System.Drawing.Size(132, 17);
            this.checkBox_SendTypingMsg.TabIndex = 16;
            this.checkBox_SendTypingMsg.Text = "Send Typing Message";
            this.checkBox_SendTypingMsg.UseVisualStyleBackColor = true;
            this.checkBox_SendTypingMsg.CheckedChanged += new System.EventHandler(this.CheckBox_SendTypingMsg_CheckedChanged);
            // 
            // button_Refresh
            // 
            this.button_Refresh.Location = new System.Drawing.Point(226, 41);
            this.button_Refresh.Name = "button_Refresh";
            this.button_Refresh.Size = new System.Drawing.Size(95, 48);
            this.button_Refresh.TabIndex = 17;
            this.button_Refresh.Text = "Refresh";
            this.button_Refresh.UseVisualStyleBackColor = true;
            this.button_Refresh.Click += new System.EventHandler(this.Button_Refresh_Click);
            // 
            // txtBox_Name
            // 
            this.txtBox_Name.Location = new System.Drawing.Point(92, 15);
            this.txtBox_Name.Name = "txtBox_Name";
            this.txtBox_Name.Size = new System.Drawing.Size(229, 20);
            this.txtBox_Name.TabIndex = 18;
            this.txtBox_Name.Leave += new System.EventHandler(this.TxtBox_Name_Leave);
            // 
            // lbl_Moderation
            // 
            this.lbl_Moderation.AutoSize = true;
            this.lbl_Moderation.Location = new System.Drawing.Point(327, 44);
            this.lbl_Moderation.Name = "lbl_Moderation";
            this.lbl_Moderation.Size = new System.Drawing.Size(102, 13);
            this.lbl_Moderation.TabIndex = 19;
            this.lbl_Moderation.Text = "Moderation Options:";
            // 
            // comboBox_MemberSelect
            // 
            this.comboBox_MemberSelect.FormattingEnabled = true;
            this.comboBox_MemberSelect.Location = new System.Drawing.Point(327, 60);
            this.comboBox_MemberSelect.Name = "comboBox_MemberSelect";
            this.comboBox_MemberSelect.Size = new System.Drawing.Size(193, 21);
            this.comboBox_MemberSelect.TabIndex = 20;
            this.comboBox_MemberSelect.Text = "Member";
            this.comboBox_MemberSelect.SelectedIndexChanged += new System.EventHandler(this.ComboBox_MemberSelect_SelectedIndexChanged);
            // 
            // btn_Go
            // 
            this.btn_Go.Location = new System.Drawing.Point(623, 60);
            this.btn_Go.Name = "btn_Go";
            this.btn_Go.Size = new System.Drawing.Size(31, 49);
            this.btn_Go.TabIndex = 21;
            this.btn_Go.Text = "Go";
            this.btn_Go.UseVisualStyleBackColor = true;
            this.btn_Go.Click += new System.EventHandler(this.Btn_Go_Click);
            // 
            // comboBox_Action
            // 
            this.comboBox_Action.FormattingEnabled = true;
            this.comboBox_Action.Items.AddRange(new object[] {
            "Warn",
            "Mute",
            "Kick",
            "Ban",
            "Unmute"});
            this.comboBox_Action.Location = new System.Drawing.Point(526, 60);
            this.comboBox_Action.Name = "comboBox_Action";
            this.comboBox_Action.Size = new System.Drawing.Size(91, 21);
            this.comboBox_Action.TabIndex = 22;
            this.comboBox_Action.Text = "Action";
            this.comboBox_Action.SelectedIndexChanged += new System.EventHandler(this.ComboBox_Action_SelectedIndexChanged);
            // 
            // lbl_Reason
            // 
            this.lbl_Reason.AutoSize = true;
            this.lbl_Reason.Location = new System.Drawing.Point(327, 90);
            this.lbl_Reason.Name = "lbl_Reason";
            this.lbl_Reason.Size = new System.Drawing.Size(47, 13);
            this.lbl_Reason.TabIndex = 24;
            this.lbl_Reason.Text = "Reason:";
            // 
            // txtBox_Reason
            // 
            this.txtBox_Reason.Location = new System.Drawing.Point(377, 87);
            this.txtBox_Reason.Name = "txtBox_Reason";
            this.txtBox_Reason.Size = new System.Drawing.Size(240, 20);
            this.txtBox_Reason.TabIndex = 23;
            // 
            // lbl_WarnMgmt
            // 
            this.lbl_WarnMgmt.AutoSize = true;
            this.lbl_WarnMgmt.Location = new System.Drawing.Point(327, 110);
            this.lbl_WarnMgmt.Name = "lbl_WarnMgmt";
            this.lbl_WarnMgmt.Size = new System.Drawing.Size(98, 13);
            this.lbl_WarnMgmt.TabIndex = 25;
            this.lbl_WarnMgmt.Text = "Warn Management";
            // 
            // listBox_Warns
            // 
            this.listBox_Warns.FormattingEnabled = true;
            this.listBox_Warns.Location = new System.Drawing.Point(330, 126);
            this.listBox_Warns.Name = "listBox_Warns";
            this.listBox_Warns.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBox_Warns.Size = new System.Drawing.Size(324, 134);
            this.listBox_Warns.TabIndex = 26;
            // 
            // btn_ClearSelectedWarns
            // 
            this.btn_ClearSelectedWarns.Location = new System.Drawing.Point(330, 267);
            this.btn_ClearSelectedWarns.Name = "btn_ClearSelectedWarns";
            this.btn_ClearSelectedWarns.Size = new System.Drawing.Size(123, 20);
            this.btn_ClearSelectedWarns.TabIndex = 27;
            this.btn_ClearSelectedWarns.Text = "Clear Selected Warns";
            this.btn_ClearSelectedWarns.UseVisualStyleBackColor = true;
            this.btn_ClearSelectedWarns.Click += new System.EventHandler(this.Btn_ClearSelectedWarns_Click);
            // 
            // btn_ClearMembersWarns
            // 
            this.btn_ClearMembersWarns.Enabled = false;
            this.btn_ClearMembersWarns.Location = new System.Drawing.Point(457, 267);
            this.btn_ClearMembersWarns.Name = "btn_ClearMembersWarns";
            this.btn_ClearMembersWarns.Size = new System.Drawing.Size(127, 20);
            this.btn_ClearMembersWarns.TabIndex = 28;
            this.btn_ClearMembersWarns.Text = "Clear Member\'s Warns";
            this.btn_ClearMembersWarns.UseVisualStyleBackColor = true;
            this.btn_ClearMembersWarns.Click += new System.EventHandler(this.Btn_ClearMembersWarns_Click);
            // 
            // btn_ClearAllWarns
            // 
            this.btn_ClearAllWarns.Enabled = false;
            this.btn_ClearAllWarns.Location = new System.Drawing.Point(588, 267);
            this.btn_ClearAllWarns.Name = "btn_ClearAllWarns";
            this.btn_ClearAllWarns.Size = new System.Drawing.Size(65, 20);
            this.btn_ClearAllWarns.TabIndex = 29;
            this.btn_ClearAllWarns.Text = "Clear All Warns";
            this.btn_ClearAllWarns.UseVisualStyleBackColor = true;
            // 
            // comboBox_ActivityType
            // 
            this.comboBox_ActivityType.FormattingEnabled = true;
            this.comboBox_ActivityType.Items.AddRange(new object[] {
            "Playing...",
            "Streaming...",
            "Listening to...",
            "Watching..."});
            this.comboBox_ActivityType.Location = new System.Drawing.Point(330, 15);
            this.comboBox_ActivityType.Name = "comboBox_ActivityType";
            this.comboBox_ActivityType.Size = new System.Drawing.Size(94, 21);
            this.comboBox_ActivityType.TabIndex = 30;
            this.comboBox_ActivityType.Text = "Activity Type";
            this.comboBox_ActivityType.SelectedIndexChanged += new System.EventHandler(this.ComboBox_ActivityType_SelectedIndexChanged);
            // 
            // lbl_Purge
            // 
            this.lbl_Purge.AutoSize = true;
            this.lbl_Purge.Location = new System.Drawing.Point(158, 94);
            this.lbl_Purge.Name = "lbl_Purge";
            this.lbl_Purge.Size = new System.Drawing.Size(129, 13);
            this.lbl_Purge.TabIndex = 31;
            this.lbl_Purge.Text = "Delete last                msgs";
            // 
            // btn_PurgeMsgs
            // 
            this.btn_PurgeMsgs.Location = new System.Drawing.Point(287, 92);
            this.btn_PurgeMsgs.Name = "btn_PurgeMsgs";
            this.btn_PurgeMsgs.Size = new System.Drawing.Size(34, 20);
            this.btn_PurgeMsgs.TabIndex = 33;
            this.btn_PurgeMsgs.Text = "Go";
            this.btn_PurgeMsgs.UseVisualStyleBackColor = true;
            this.btn_PurgeMsgs.Click += new System.EventHandler(this.btn_PurgeMsgs_Click);
            // 
            // numericUpDown_Purge
            // 
            this.numericUpDown_Purge.Location = new System.Drawing.Point(214, 92);
            this.numericUpDown_Purge.Name = "numericUpDown_Purge";
            this.numericUpDown_Purge.Size = new System.Drawing.Size(47, 20);
            this.numericUpDown_Purge.TabIndex = 34;
            // 
            // FrostForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(659, 301);
            this.Controls.Add(this.numericUpDown_Purge);
            this.Controls.Add(this.btn_PurgeMsgs);
            this.Controls.Add(this.lbl_Purge);
            this.Controls.Add(this.comboBox_ActivityType);
            this.Controls.Add(this.btn_ClearAllWarns);
            this.Controls.Add(this.btn_ClearMembersWarns);
            this.Controls.Add(this.btn_ClearSelectedWarns);
            this.Controls.Add(this.listBox_Warns);
            this.Controls.Add(this.lbl_WarnMgmt);
            this.Controls.Add(this.lbl_Reason);
            this.Controls.Add(this.txtBox_Reason);
            this.Controls.Add(this.comboBox_Action);
            this.Controls.Add(this.btn_Go);
            this.Controls.Add(this.comboBox_MemberSelect);
            this.Controls.Add(this.lbl_Moderation);
            this.Controls.Add(this.txtBox_Name);
            this.Controls.Add(this.button_Refresh);
            this.Controls.Add(this.checkBox_SendTypingMsg);
            this.Controls.Add(this.radioButton_Unlocked);
            this.Controls.Add(this.radioButton_Locked);
            this.Controls.Add(this.comboBox_Status);
            this.Controls.Add(this.txtBox_Playing);
            this.Controls.Add(this.lbl_Nickname);
            this.Controls.Add(this.btn_Browse);
            this.Controls.Add(this.txtBox_Attachment);
            this.Controls.Add(this.lbl_Attachment);
            this.Controls.Add(this.txtBox_Msg);
            this.Controls.Add(this.comboBox_Server);
            this.Controls.Add(this.comboBox_Channel);
            this.Controls.Add(this.btn_Send);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(675, 340);
            this.MinimumSize = new System.Drawing.Size(675, 340);
            this.Name = "FrostForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "JackFrostBot";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Purge)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_Send;
        private System.Windows.Forms.ComboBox comboBox_Channel;
        private System.Windows.Forms.ComboBox comboBox_Server;
        private System.Windows.Forms.RichTextBox txtBox_Msg;
        private System.Windows.Forms.Label lbl_Attachment;
        private System.Windows.Forms.TextBox txtBox_Attachment;
        private System.Windows.Forms.Button btn_Browse;
        private System.Windows.Forms.TextBox txtBox_Playing;
        private System.Windows.Forms.Label lbl_Nickname;
        private System.Windows.Forms.ComboBox comboBox_Status;
        private System.Windows.Forms.RadioButton radioButton_Locked;
        private System.Windows.Forms.RadioButton radioButton_Unlocked;
        private System.Windows.Forms.CheckBox checkBox_SendTypingMsg;
        private System.Windows.Forms.Button button_Refresh;
        private System.Windows.Forms.TextBox txtBox_Name;
        private System.Windows.Forms.Label lbl_Moderation;
        private System.Windows.Forms.ComboBox comboBox_MemberSelect;
        private System.Windows.Forms.Button btn_Go;
        private System.Windows.Forms.ComboBox comboBox_Action;
        private System.Windows.Forms.Label lbl_Reason;
        private System.Windows.Forms.TextBox txtBox_Reason;
        private System.Windows.Forms.Label lbl_WarnMgmt;
        private System.Windows.Forms.ListBox listBox_Warns;
        private System.Windows.Forms.Button btn_ClearSelectedWarns;
        private System.Windows.Forms.Button btn_ClearMembersWarns;
        private System.Windows.Forms.Button btn_ClearAllWarns;
        private System.Windows.Forms.ComboBox comboBox_ActivityType;
        private System.Windows.Forms.Label lbl_Purge;
        private System.Windows.Forms.Button btn_PurgeMsgs;
        private System.Windows.Forms.NumericUpDown numericUpDown_Purge;
    }
}