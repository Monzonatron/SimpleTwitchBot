# SimpleTwitchBot
This is a simple twitch bot made in C#.

The bot is multi-threaded. It spawns a thread to pull messages to the server and spawns unique threads to parse and react to messages.

There is an automatic-message function which is run on a modifiable timer. 

Be sure to go into the GLOBALS.cs and change the information to match your twitch bot username and oauth password. To get your oauth password, use the following link: https://twitchapps.com/tmi/
