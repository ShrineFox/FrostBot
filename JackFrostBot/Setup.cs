using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using System.IO;

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
                File.CreateText("token.txt");

            return token;
        }

        public static bool SetIniValue(ulong guildId, string keyName, string newKeyValue)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");
            bool success = false;

            foreach(SectionData section in data.Sections)
            {
                foreach(KeyData key in section.Keys)
                {
                    if (keyName.Trim(' ') == key.KeyName)
                    {
                        data[$"{section.SectionName}"][$"{key.KeyName}"] = newKeyValue;
                        parser.WriteFile($"{guildId}\\setup.ini", data);
                        success = true;
                    }
                }
            }

            return success;
        }

        public static string GetIniCategory(ulong guildId, string categoryName)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");
            string categoryText = "";
            List<string> categoryNames = new List<string>();

            foreach (SectionData section in data.Sections)
            {
                categoryNames.Add(section.SectionName);
                if (categoryName.Trim(' ') == section.SectionName)
                {
                    foreach(KeyData key in section.Keys)
                    {
                        categoryText = $"{categoryText}\n{key.KeyName}={key.Value}";
                    }
                }
            }
            if (categoryText == "")
            {
                categoryText = $"The following categories are available from the setup.ini:\n{string.Join("\n", categoryNames.ToArray())}";
            }

            return categoryText;
        }

        public static List<ulong> ModeratorRoleIds(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            List<ulong> roleIds = new List<ulong>();
            foreach (KeyData key in data["ModeratorRoles"])
            {
                roleIds.Add(Convert.ToUInt64(key.Value));
            }
            return roleIds;
        }

        public static List<ulong> PrivateChannelIds(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            List<ulong> channelIds = new List<ulong>();
            foreach (KeyData key in data["PrivateChannels"])
            {
                channelIds.Add(Convert.ToUInt64(key.Value));
            }
            return channelIds;
        }

        public static ulong BogLotChannelId(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            ulong channelId = 0;
            foreach (KeyData key in data["BotLogChannel"])
            {
                channelId = Convert.ToUInt64(key.Value);
            }
            return channelId;
        }

        public static ulong VoiceTextChannelId(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            ulong channelId = 0;
            foreach (KeyData key in data["VoiceTextChannel"])
            {
                channelId = Convert.ToUInt64(key.Value);
            }
            return channelId;
        }

        public static ulong MediaChannelId(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            ulong channelId = 0;
            foreach (KeyData key in data["MediaChannel"])
            {
                channelId = Convert.ToUInt64(key.Value);
            }
            return channelId;
        }

        public static ulong VerificationChannelId(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            ulong channelId = 0;
            foreach (KeyData key in data["VerificationChannel"])
            {
                channelId = Convert.ToUInt64(key.Value);
            }
            return channelId;
        }

        public static ulong WelcomeChannelId(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            return Convert.ToUInt64(data["DefaultChannels"]["welcome"]);
        }

        public static ulong DefaultChannelId(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");


            return Convert.ToUInt64(data["DefaultChannels"]["general"]);
        }

        public static ulong MemberRoleId (ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            ulong roleId = 0;
            foreach (KeyData key in data["MemberRole"])
            {
                roleId = Convert.ToUInt64(key.Value);
            }
            return roleId;
        }

        public static int MuteLevel(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");
            return Convert.ToInt32(data["WarnLevels"]["mute"]);
        }

        public static int KickLevel(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");
            return Convert.ToInt32(data["WarnLevels"]["kick"]);
        }

        public static int BanLevel(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");
            return Convert.ToInt32(data["WarnLevels"]["ban"]);
        }

        public static int DuplicateMsgLimit(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");
            return Convert.ToInt32(data["MessageLimits"]["maximumDuplicates"]);
        }

        public static int MsgLengthLimit(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");
            return Convert.ToInt32(data["MessageLimits"]["minimumLength"]);
        }

        public static ulong NsfwRoleId(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            ulong roleId = 0;
            foreach (KeyData key in data["NsfwRole"])
            {
                roleId = Convert.ToUInt64(key.Value);
            }
            return roleId;
        }

        public static ulong NsfwChannelId(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            ulong channelId = 0;
            foreach (KeyData key in data["NsfwChannel"])
            {
                channelId = Convert.ToUInt64(key.Value);
            }
            return channelId;
        }

        public static ulong ArtRoleId(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            ulong roleId = 0;
            foreach (KeyData key in data["ArtistRole"])
            {
                roleId = Convert.ToUInt64(key.Value);
            }
            return roleId;
        }

        public static ulong ArtChannelId(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            ulong channelId = 0;
            foreach (KeyData key in data["ArtChannel"])
            {
                channelId = Convert.ToUInt64(key.Value);
            }
            return channelId;
        }

        public static ulong BotSandBoxChannelId(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            ulong channelId = 0;
            foreach (KeyData key in data["BotSandBoxChannel"])
            {
                channelId = Convert.ToUInt64(key.Value);
            }
            return channelId;
        }

        public static ulong LurkerRoleId(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            ulong roleId = 0;
            foreach (KeyData key in data["LurkerRole"])
            {
                roleId = Convert.ToUInt64(key.Value);
            }
            return roleId;
        }

        //Strings

        public static string PasswordString(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            return data["Strings"]["verificationPassword"];
        }

        public static string LockMsg(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            return data["Strings"]["lockMessage"];
        }

        public static string UnlockMsg(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            return data["Strings"]["unlockMessage"];
        }

        public static string MuteMsg(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            return data["Strings"]["muteMessage"];
        }

        public static string UnmuteMsg(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            return data["Strings"]["unmuteMessage"];
        }

        public static string NoPermissionMessage(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            return data["Strings"]["noPermissionMessage"];
        }

        //Bools

        public static bool ModeratorsCanUpdateSetup(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");
            
            return Convert.ToBoolean(data["Bools"]["moderatorsCanUpdateSetup"]);
        }

        public static bool AssignLurkerRoles(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            return Convert.ToBoolean(data["Bools"]["assignLurkerRoles"]);
        }

        public static bool EnableSayCommand(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            return Convert.ToBoolean(data["Bools"]["enableSayCommand"]);
        }

        public static bool EnableWordFilter(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            IniData data = parser.ReadFile($"{guildId}\\setup.ini");

            return Convert.ToBoolean(data["Bools"]["enableWordFilter"]);
        }

        public static void CreateIni(string fileName)
        {
            File.WriteAllText(fileName, @"#
## IMPORTANT
#
[ModeratorRoles]
#IDs of roles that can use the bot's moderation commands
Moderators=215043129663815680
TGE=143152214247079936
[PrivateChannels]
#IDs of channels that most members can't post in. 
#Permissions will not be altered by the bot when muting/unmuting users in other channels.
welcome=390626625433239552
announcements=207352829881483285
tool-releases=316238229948989450
mod-showcase=410951708274065409
github=478986023536295937
staff=417170700936413224
bot-logs=424670454230417409
adachi-project=407950867988348928
p3fes-femc=419824275898236941
#
## CHANNELS
#
[DefaultChannels]
#First channels that a user will be directed to read when joining
welcome=390626625433239552
general=481372188331474944
[BotLogChannel]
#ID of the channel where the bot's moderation records will be posted
bot-logs=424670454230417409
[VoiceTextChannel]
#ID of a channel where users can only post while in a voice channel
voice-text=417690815263932416
[VerificationChannel]
#ID of a channel where users must enter the Password String to gain the Member Role
new-arrivals=473652163634003969
[MediaChannel]
#ID of a channel where any messages without attachments will be deleted
memes=474128982262939648
[NsfwChannel]
#ID of a channel where users must gain the Nsfw Role in order to access it
serious-talk=473651223363452950
[ArtChannel]
#ID of a channel where users cannot upload attachments unless they gain the Artist role 
art=380393842844893184
[BotSandBoxChannel]
#ID of a channel users will be redirected to in order to use lengthy bot commands.
bot-sandbox=473675595088134159
#
## ROLES
#
[MemberRole]
#Role that users obtain by entering the Password String in the Verification Channel
#This is the role that will be muted/unmuted & locked/unlocked. Leave this part blank
#if you want to use the everyone role instead.
Members=212631304208908288
[NsfwRole]
#Role that users must obtain in order to access the Nsfw Channel
Serious Talk=473653822992941056
[ArtistRole]
#Role that users must obtain in order to upload to the Art Channel
Artist=441517400081563661
[LurkerRole]
#Role that users are given upon joining, will be removed after one post
Lurkers=480511070717607936
#
## SETTINGS
#
[WarnLevels]
#Maximum number of warns a user can recieve before automatically being penalized
mute=2
kick=3
ban=4
[MessageLimits]
#Messages will be automatically deleted if they fail to meet the following requirements
maximumDuplicates=3
minimumLength=2
[Strings]
#Verification Password is what users must type in the Verification Channel to get the Member Role (not case sensitive)
verificationPassword=h
lockMessage=Hee-ho!I'm your pilot this evening. Only Amicitia Gold :tm: account holders can earn sky miles with us at this time!
unlockMessage=Be mindful of the rules and don't forget to have fun.
muteMessage=Shut your hee-ho!
unmuteMessage=Hee ur ho m8
noPermissionMessage=You don't have permission to hee this command, ho!
[Bools]
moderatorsCanUpdateSetup=true
assignLurkerRoles=true
enableSayCommand=true
enableWordFilter=true");
        }
    }
}
