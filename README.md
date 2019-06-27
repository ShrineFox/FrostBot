# JackFrostBot
![Logo](https://i.imgur.com/ohjqKCo.png)  
A flexible server moderation tool using [Discord.Net](https://github.com/RogueException/Discord.Net).
# Installation
1. [Create a bot user](https://discordapp.com/developers/applications/) and invite it to your server.
2. Extract [the zip from releases](https://github.com/Amicitia/JackFrost-Bot/releases), place your bot user's token in token.txt and run the exe.
3. Fill out the generated XML files in the Server folder to customize the bot for your server. To get IDs, use Discord's developer mode and right click channels/roles and choose Copy ID.
4. Give the bot a role with admin priveleges in order to manage permissions and post in any channel.
5. For more specific setup instructions, including how to use every feature, [check out the Wiki](https://github.com/Amicitia/JackFrost-Bot/wiki).
# Features
## Verification
 • Optionally require members to type a specific message to gain access to the rest of the server.  
 ![Verified](https://i.imgur.com/bmOZDJg.gif)
## Locking and Muting
 • Quickly deny typing privileges to a channel to end strife, or temporarily silence individual misbehaving users.   
![LockUnlock](https://i.imgur.com/VXmtZlC.gif)
## Warn Management
 • Let your moderation team hand out warns, which users can accumulate.  
 • You get to configure a custom threshold for how many warns lead to automatic mutes, kicks, or bans.  
 • Users that try to evade mutes will be instantly muted when they rejoin.  
![MuteUnmute](https://i.imgur.com/WclQQid.gif)
## Spam Prevention
 • You can filter specific phrases from incoming messages and, depending on severity, automatically issue warns.  
 • Set a cap on post length and duplicate messages. Users can check why their messages were auto-deleted.  
![Spam](https://i.imgur.com/rwJMnhP.gif)
## Role Management
 • Users can opt in and out of specific roles at will, allowing them to access optional sections of the server.  
 • Users can create their own roles using a hex value to change their username color on their own.  
![Roles](https://i.imgur.com/HjFoYas.gif)
## Moderation Logs
 • Turn a channel into an audit log to see who used certain commands.
 ![Log](https://i.imgur.com/oIvIdw1.png)
## Purge Lurkers
 • Unlike Discord's [pruning feature](https://support.discordapp.com/hc/en-us/articles/213507137-What-is-Pruning-How-do-I-use-it-), you can remove users that have never typed in your server.  
 ![Nonmembers](https://i.imgur.com/m3jTIl4.gif)
## Share Information
 • Users can look up a list of commands they can use, with auto-generated usage examples.  
 • Optionally, you can make a list of keywords users can look up to view a matching embed with a description and useful links.  
 ![Lists](https://i.imgur.com/1VtBD4Z.gif)
## Save Messages
 • The "pin" command can send an embed of any message to another channel as an alternative to Discord's default pinning system, which has a per-channel limit.  
 ![Pin](https://i.imgur.com/D9QSsy7.gif)
## Easy Usage
 • All settings are in plaintext XML files that can be edited. They are auto-generated per server when you run the bot.  
 • The host of the bot can use a GUI to manage moderation actions remotely.  
 ![GUI](https://i.imgur.com/PpJqwCb.png)
