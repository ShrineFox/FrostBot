using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using System.IO;
using Discord;
using Discord.WebSocket;

namespace JackFrostBot
{
    class Setup
    {
        public static string Token()
        {
            string token = "";
            if (File.Exists("token.txt"))
                token = File.ReadAllText("token.txt");
            else
                File.CreateText("token.txt").Close();

            return token;
        }

        public static ulong OptInRoleId(IGuild guild, string roleName)
        {
            ulong optInRoleId = 0;
            foreach (var role in guild.Roles.Where(r => r.Name.ToLower() == roleName.ToLower()))
                foreach (var roleId in UserSettings.Roles.OptInRoles(guild.Id))
                    if (role.Id == roleId)
                        optInRoleId = roleId;
            return optInRoleId;
        }

        public static List<ulong> PrivateChannelIds(SocketGuild guild)
        {
            List<ulong> channelIds = new List<ulong>();
            foreach (var channel in guild.Channels)
                if (channel.PermissionOverwrites.Any(p => p.Permissions.ViewChannel.Equals(false) && p.TargetType == PermissionTarget.Role))
                    channelIds.Add(guild.Id);
            return channelIds;
        }

        public static bool IsMediaOnlyChannel(IGuildChannel channel)
        {
            if (UserSettings.Channels.MediaOnlyChannels(channel.Guild.Id).Any(c => c.Equals(channel.Id)))
                return true;

            return false;
        }

        public static bool IsVerificationChannel(IGuildChannel channel)
        {
            if (UserSettings.Verification.VerificationChannelId(channel.Guild.Id) == channel.Id)
                return true;

            return false;
        }
    }
}
