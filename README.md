# JackFrostBot
![Logo](https://i.imgur.com/revniHd.png)
A Discord bot that uses [Discord.Net](https://github.com/RogueException/Discord.Net).
# Installation
1. Create a bot user and invite it to your server.
2. Extract the zip, place your bot user's token in Token.txt and run the exe.
3. Fill out the generated Setup.ini to customize the bot for your server.
Note: for channel IDs, use Discord's developer mode and right click each channel and Copy ID.
For role IDs, use the command "?get id rolename"
4. Give the bot a role with admin priveleges in order to manage permissions and post in any channel.

# Features
## Moderation
- Moderators can mute/unmute users (automatically update their sendMessages permission in each channel)
![Mutes](https://i.imgur.com/tDLt3Wx.gif)
- Moderators can lock/unlock channels (updates everyone's sendMessages permission for the channel)
![Locking](https://i.imgur.com/lofuILg.gif)
- Moderators can warn users and remove warns (warns are recorded in a local text doc)
![Warns](https://i.imgur.com/Y5pzpXP.gif)
- Moderators can set the name of the game the bot is currently "playing"
## Automation
- Automatically verifies users and gives them access to other channels after proving that they read the rules
- Automatically deletes messages containing a filtered word 
- Automatically warns a user who attempts to bypass the word filter
- Automatically mutes/kicks/bans a user depending on their warning level
- Automatically mutes users who leave and rejoin to reset their permissions
- Announces warns/kicks/bans/locks etc. and their reason anonymously in the channel where the commands were used,
  but keeps track of who used them in a private #bot-logs channel
  ![Log](https://i.imgur.com/oIvIdw1.png)
- Automatically removes messages that don't have any attachments in media-only channels
- Automatically removes messages from users that aren't in a voice channel in #voice-text
- Automatically removes messages that are too short (one-character responses)
- Automatically keeps a local log of all messages (author, timestamp and content)
## User Features
- Users can grant themselves roles to access opt-in only channels
- Users can make the bot repeat messages
- Users can check a list of all warns and their reasons, or narrow it down to a specific user
![Warns2](https://i.imgur.com/cWCV0JA.gif)
- Users can see a list of all commands they can use
- Users can look up a keyword to get a description and helpful links (references a local ini file)
![Lists](https://i.imgur.com/1VtBD4Z.gif)
- Users can look up a list of all available keywords
- Users can create and assign themselves roles with a color value of their choice
- Users can update existing color roles by changing the color value (if it's not already in use by someone else)
## Customization
 - Filtered word list is kept as a local text doc that you can edit (filter.txt)
 - Filter bypass check is kept as a local text doc that you can edit (filterbypasscheck.txt)
 - Info.ini can be edited to add/remove list entries
