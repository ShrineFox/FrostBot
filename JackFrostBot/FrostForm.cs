using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.IO;

namespace JackFrostBot
{
    public partial class FrostForm : Form
    {
        public static List<SocketGuild> guilds;
        public static DiscordSocketClient client;
        public static int selectedServer;
        public static int selectedChannel;
        public static int selectedMember;

        public FrostForm(DiscordSocketClient botClient)
        {
            InitializeComponent();
            client = botClient;
            guilds = client.Guilds.ToList();
            RefreshForm();
        }

        private void lbl_Playing_Click(object sender, EventArgs e)
        {

        }

        private void Button_Refresh_Click(object sender, EventArgs e)
        {
            RefreshForm();
        }

        public void RefreshForm()
        {
            //Get guild list
            comboBox_Server.DataSource = guilds;

            //Get channel list
            var guild = guilds[selectedServer];
            comboBox_Channel.DataSource = guild.TextChannels.ToList();

            //Get nickname and members list
            txtBox_Name.Text = guild.CurrentUser.Nickname;
            comboBox_MemberSelect.DataSource = guild.Users.ToList();

            //Get activity name
            try
            {
                txtBox_Playing.Text = client.Activity.Name;
            }
            catch
            {
                txtBox_Playing.Text = "";
            }
            

            //Get current status
            comboBox_Status.SelectedIndex = (int)client.Status;

            //Get current activity type
            try
            {
                comboBox_ActivityType.SelectedIndex = (int)client.Activity.Type;
            }
            catch
            {

            }
            

            //Get warns
            listBox_Warns.DataSource = null;
            listBox_Warns.Items.Clear();
            listBox_Warns.DataSource = UserSettings.Warns.List(guild.Id);

            //Try to restore selected positions
            comboBox_Server.SelectedIndex = selectedServer;
            comboBox_Channel.SelectedIndex = selectedChannel;
            comboBox_MemberSelect.SelectedIndex = selectedMember;
        }

        private void ComboBox_Server_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedServer = comboBox_Server.SelectedIndex;
            var guild = guilds[selectedServer];

            //Get channel list
            comboBox_Channel.DataSource = guild.TextChannels.ToList();
            try
            {
                comboBox_Channel.SelectedIndex = selectedChannel;
            }
            catch
            {
                comboBox_Channel.SelectedIndex = 0;
            }

            //Get nickname and members list
            txtBox_Name.Text = guild.CurrentUser.Nickname;
            comboBox_MemberSelect.DataSource = guild.Users.ToList();
            try
            {
                comboBox_MemberSelect.SelectedIndex = selectedMember;
            }
            catch
            {
                comboBox_MemberSelect.SelectedIndex = 0;
            }

            //Get warns
            listBox_Warns.DataSource = UserSettings.Warns.List(guild.Id);
        }

        private void RichTextBox1_Enter(object sender, EventArgs e)
        {
            
        }

        private void Btn_Send_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void txtBox_Msg_TextChanged(object sender, EventArgs e)
        {

        }

        public void SendMessage()
        {
            IMessageChannel channel = guilds[selectedServer].TextChannels.ToList()[selectedChannel];

            try
            {
                if (File.Exists(txtBox_Attachment.Text))
                {
                    channel.SendMessageAsync(txtBox_Msg.Text);
                    channel.SendFileAsync(txtBox_Attachment.Text);
                    txtBox_Attachment.Text = "";
                }
                else
                {
                    channel.SendMessageAsync(txtBox_Msg.Text);
                }
                txtBox_Msg.Text = "";
            }
            catch
            {
            }
        }

        private void TxtBox_Msg_KeyDown(object sender, KeyEventArgs e)
        {
            if (Control.ModifierKeys != Keys.Shift && e.KeyCode == Keys.Enter) 
                SendMessage();
        }

        private void TxtBox_Name_TextChanged(object sender, EventArgs e)
        {

        }

        private void TxtBox_Name_Leave(object sender, EventArgs e)
        {
            try
            {
                guilds[comboBox_Server.SelectedIndex].CurrentUser.ModifyAsync(b => b.Nickname = txtBox_Name.Text);
            }
            catch
            {

            }
        }
        private void CheckBox_SendTypingMsg_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ComboBox_Channel_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedChannel = comboBox_Channel.SelectedIndex;
            SocketTextChannel channel = guilds[selectedServer].TextChannels.ToList()[selectedChannel];

            if (channel.PermissionOverwrites.Any(o => o.Permissions.SendMessages == PermValue.Deny && (o.Permissions.ViewChannel == PermValue.Allow || o.Permissions.ViewChannel == PermValue.Inherit) && o.TargetType == PermissionTarget.Role && channel.Guild.GetRole(o.TargetId).IsEveryone))
            {
                radioButton_Locked.Checked = true;
                radioButton_Unlocked.Checked = false;
            }
            else
            {
                radioButton_Locked.Checked = false;
                radioButton_Unlocked.Checked = true;
            }

        }

        private void RadioButton_Locked_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void RadioButton_Unlocked_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void RadioButton_Locked_Click(object sender, EventArgs e)
        {
            SocketTextChannel channel = guilds[selectedServer].TextChannels.ToList()[selectedChannel];
            Moderation.Lock(channel.Guild.CurrentUser, channel);
        }

        private void RadioButton_Unlocked_Click(object sender, EventArgs e)
        {
            SocketTextChannel channel = guilds[selectedServer].TextChannels.ToList()[selectedChannel];
            Moderation.Unlock(channel.Guild.CurrentUser, channel);
        }

        private void TxtBox_Playing_Leave(object sender, EventArgs e)
        {
            client.SetGameAsync(txtBox_Playing.Text);
        }

        private void Lbl_SetTo_Click(object sender, EventArgs e)
        {

        }

        //Doesn't seem to work
        private void ComboBox_Status_SelectedIndexChanged(object sender, EventArgs e)
        {
            client.SetStatusAsync((UserStatus)comboBox_Status.SelectedIndex);
        }

        private void ComboBox_MemberSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedMember = comboBox_MemberSelect.SelectedIndex;
        }

        private void ComboBox_Action_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Btn_Go_Click(object sender, EventArgs e)
        {
            try
            {
                SocketGuild guild = guilds[selectedServer];
                ITextChannel channel = guild.TextChannels.ToList()[selectedChannel];
                SocketGuildUser user = guild.Users.ToList()[selectedMember];

                DialogResult dialogResult = MessageBox.Show($"Are you sure you want to {comboBox_Action.Items[comboBox_Action.SelectedIndex].ToString()} {user.Username}#{user.Discriminator} ({user.Nickname}) in #{channel.Name}?", "Verify Moderation Action", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    switch (comboBox_Action.SelectedIndex)
                    {
                        case 0:
                            Moderation.Warn(client.CurrentUser.Username, channel, user, txtBox_Reason.Text);
                            break;
                        case 1:
                            Moderation.Mute(client.CurrentUser.Username, channel, user);
                            break;
                        case 2:
                            Moderation.Kick(client.CurrentUser.Username, channel, user, txtBox_Reason.Text);
                            selectedMember = 0;
                            break;
                        case 3:
                            Moderation.Ban(client.CurrentUser.Username, channel, user, txtBox_Reason.Text);
                            selectedMember = 0;
                            break;
                        case 4:
                            Moderation.Unmute(client.CurrentUser.Username, channel, user);
                            break;
                    }
                    txtBox_Reason.Text = "";
                }
            }
            catch
            {

            }
            RefreshForm();
        }

        private void Btn_ClearSelectedWarns_Click(object sender, EventArgs e)
        {
            var guild = guilds[selectedServer];
            var channel = (ITextChannel)guild.TextChannels.ToList()[selectedChannel];

            foreach (var item in listBox_Warns.SelectedItems)
            {
                Moderation.ClearWarn(guild.CurrentUser, channel, listBox_Warns.Items.IndexOf(item), null);
            }
            listBox_Warns.Update();
        }

        private void Btn_ClearMembersWarns_Click(object sender, EventArgs e)
        {

        }

        private void txtBox_Msg_Enter(object sender, EventArgs e)
        {
            SocketTextChannel channel = guilds[selectedServer].TextChannels.ToList()[selectedChannel];
            if (checkBox_SendTypingMsg.Checked)
            {
                channel.TriggerTypingAsync();
            }
        }

        private void btn_PurgeMsgs_Click(object sender, EventArgs e)
        {
            SocketGuild guild = guilds[selectedServer];
            ITextChannel channel = guild.TextChannels.ToList()[selectedChannel];
            var amount = (int)numericUpDown_Purge.Value;

            DialogResult dialogResult = MessageBox.Show($"Are you sure you want to delete the last {amount} messages in #{channel.Name}?", "Delete Messages?", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                Moderation.DeleteMessages(channel, amount);
            }
        }

        
        private void Btn_Browse_Click(object sender, EventArgs e)
        {

        }

        private void ChooseFile()
        {

        }

        private void ComboBox_ActivityType_SelectedIndexChanged(object sender, EventArgs e)
        {
            client.SetActivityAsync(new Game (client.Activity.Name, (ActivityType)comboBox_ActivityType.SelectedIndex));
        }
    }
}
