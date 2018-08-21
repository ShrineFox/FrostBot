# JackFrostBot
A Discord bot that uses [Discord.Net](https://github.com/RogueException/Discord.Net).
# Features
## Moderation
- Moderators can mute/unmute users (automatically update their sendMessages permission in each channel)
- Moderators can lock/unlock channels (updates everyone's sendMessages permission for the channel)
- Moderators can warn users and remove warns (warns are recorded in a local text doc)
- Moderators can set the name of the game the bot is currently "playing"
## Automation
- Automatically verifies users and gives them access to other channels after proving that they read the rules
- Automatically deletes messages containing a filtered word 
- Automatically warns a user who attempts to bypass the word filter
- Automatically mutes/kicks/bans a user depending on their warning level
- Automatically mutes users who leave and rejoin to reset their permissions
- Announces warns/kicks/bans/locks etc. and their reason anonymously in the channel where the commands were used,
  but keeps track of who used them in a private #bot-logs channel
- Automatically removes messages that don't have any attachments in media-only channels
- Automatically removes messages from users that aren't in a voice channel in #voice-text
- Automatically removes messages that are too short (one-character responses)
- Automatically keeps a local log of all messages (author, timestamp and content)
## User Features
- Users can grant themselves roles to access opt-in only channels
- Users can make the bot repeat messages
- Users can check a list of all warns and their reasons, or narrow it down to a specific user
- Users can see a list of all commands they can use
- Users can look up a keyword to get a description and helpful links (references a local xml doc)
- Users can look up a list of all available keywords
## Customization
 - Filtered word list is kept as a local text doc that you can edit (filter.txt)
 - Filter bypass check is kept as a local text doc that you can edit (filterbypasscheck.txt)
 - Links.xml and Lists.xml can be edited to add/remove list entries
# Future Plans for further customization
- Local setup for channel and role IDs used by the bot (right now they're all specific to the Amicitia server)
- Ability for moderators to adjust the local filter list via commands
- Ability for moderators to adjust the levels for automatic mutes/kicks/bans
# Planned Features
- Slowmode (members can only post one message per minute when active as opposed to locking the channel)
- Improvements to the XML parsing code (it's jank)
