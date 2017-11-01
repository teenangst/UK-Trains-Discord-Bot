using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UK_Trains_Discord
{
    ///ToDo:
    ///• ~~Check if inputted Discord token is valid~~ Just don't be a dingus
    ///• ~~Auto-update polls~~
    ///• !addexception NUM "Stuff"

    class Discord
    {
        private DiscordSocketClient client;
        //private CommandService commands;
        private IServiceProvider services;
        private Strawpoll strawpoll;

        private static string token;

        public Discord()
        {

        }

        public async Task MainAsync()
        {
            token = Token.getDiscordToken();
            client = new DiscordSocketClient();
            strawpoll = new Strawpoll();

            client.Log += Log;

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await client.SetGameAsync("Train Simulator 2017");

            client.Ready += () =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Bot is connected");
                Console.ForegroundColor = ConsoleColor.White;
                return Task.CompletedTask;
            };

            client.MessageReceived += HandleCommandAsync;


            await Task.Delay(-1);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
           

            if (message.Content[0] == Char.Parse(CustomCommands.prefix))
            {

                var context = new SocketCommandContext(client, message);
                Console.WriteLine($"{message.Author.Id} {message.Content}");
                switch (message.Content.Split(' ')[0].Substring(1))
                {
                    case "poll":
                        if (context.Guild.GetUser(message.Author.Id).Roles.Count > 0)
                        {
                            bool created;
                            Poll poll;
                            strawpoll.New(String.Join(" ", message.Content.Split().Where((x, i) => i != 0)), message.Author.Mention, out created, out poll);
                            if (created)
                            {
                                string output = "";
                                output += $"**{poll.title}**";
                                output += Environment.NewLine;
                                output += $"Created by {poll.createdby}";
                                if (poll.image != null && poll.image != "")
                                {
                                    output += Environment.NewLine;
                                    output += poll.image;
                                }
                                output += Environment.NewLine;
                                for (int i = 0; i < poll.choices.Count; i++)
                                {
                                    output += Environment.NewLine;
                                    output += $"{Tools.pollemoji[i]} {poll.choices[i].Item1}";
                                }
                                
                                var msg = await context.Channel.SendMessageAsync(output);
                                await message.DeleteAsync();
                                for (int i = 0; i < poll.choices.Count; i++)
                                {
                                    await Task.Delay(1000);
                                    Emoji e = new Emoji(Tools.pollemojisym[i]);
                                    await msg.AddReactionAsync(e);
                                }
                            }
                            else
                            {
                                await context.Channel.SendMessageAsync($"{message.Author.Mention} poll not valid :crying_cat_face:");
                                await message.AddReactionAsync(new Emoji(Tools.x));
                            }
                        }
                        break;
                    case "addexception":
                        var matches = Regex.Matches(message.Content, @"!addexception \""(?<command>[a-zA-Z0-9 ]*)\"" \""(?<output>.*)\""");
                        Console.WriteLine(matches.Count);
                        if (matches.Count == 1)
                        {
                            var match = matches[0];
                            var command = match.Groups["command"].Value;
                            var output = match.Groups["output"].Value;
                            if (command != "" && output != "")
                            {
                                if (CustomCommands.commands.ContainsKey(command))
                                {
                                    await context.Channel.SendMessageAsync($"{message.Author.Mention} there is another command registered (Output:`{CustomCommands.commands[command]}`), please use `!changeexception \"{command}\" \"{output}\"` if you wish to change this.");
                                    await message.AddReactionAsync(new Emoji(Tools.q));
                                }
                                else
                                {
                                    Console.WriteLine($"New command: {CustomCommands.prefix}{command} => {output}");
                                    CustomCommands.newCommand(command, output);
                                    await message.AddReactionAsync(new Emoji(Tools.t));
                                }
                            }
                            else
                            {
                                await message.AddReactionAsync(new Emoji(Tools.q));
                            }
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync($"{message.Author.Mention} unexpected format, use `!addexception \"223\" \"Text to use instead\"` to add a custom command");
                            await message.AddReactionAsync(new Emoji(Tools.x));
                        }

                        break;
                    case "removeexception":
                        var _matches = Regex.Matches(message.Content, @"!removeexception \""(?<command>[a-zA-Z0-9 ]*)\""");

                        if (_matches.Count == 1)
                        {
                            var match = _matches[0];
                            var command = match.Groups["command"].Value;
                            if (command != "")
                            {
                                Console.WriteLine($"Removed command: {CustomCommands.prefix}{command}");
                                CustomCommands.tryRemoveCommand(command);
                                await message.AddReactionAsync(new Emoji(Tools.t));
                            }
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync($"{message.Author.Mention} unexpected format, use `!removeexception 223` to remove a custom command");
                            await message.AddReactionAsync(new Emoji(Tools.x));
                        }
                        break;
                    case "changeexception":
                        var __matches = Regex.Matches(message.Content, @"!changeexception \""(?<command>[a-zA-Z0-9 ]*)\"" \""(?<output>.*)\""");

                        if (__matches.Count == 1)
                        {
                            var match = __matches[0];
                            var command = match.Groups["command"].Value;
                            var output = match.Groups["output"].Value;
                            if (command != "" && output != "")
                            {
                                bool wasChanged;

                                CustomCommands.tryChangeCommand(command, output, out wasChanged);
                                if (wasChanged)
                                {
                                    await message.AddReactionAsync(new Emoji(Tools.t));
                                }
                                else
                                {
                                    await context.Channel.SendMessageAsync($"{message.Author.Mention} no command for `{command}` was found, if you wish to add the command use `!addexception \"{command}\" \"{output}\"`");
                                    await message.AddReactionAsync(new Emoji(Tools.x));
                                }
                            }
                            else
                            {
                                await message.AddReactionAsync(new Emoji(Tools.q));
                            }
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync($"{message.Author.Mention} unexpected format, use `!changeexception \"223\" \"New output\"` to remove a custom command");
                            await message.AddReactionAsync(new Emoji(Tools.x));
                        }
                        break;
                    default:
                        await context.Channel.SendMessageAsync($"{message.Author.Mention} {Wikipedia.getPage(message.Content).Result}");
                        break;
                }
            }
        }

        

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
            
        }


        private static class Token
        {
            public static string getDiscordToken()
            {
                bool @new;
                string token = getDiscordTokenWorker(out @new);
                Console.ForegroundColor = ConsoleColor.White;

                if (@new) //save only if it has been written
                {
                    if (!Directory.Exists("files"))
                    {
                        Directory.CreateDirectory("files");
                    }
                    File.WriteAllText("files/discord.json", JsonConvert.SerializeObject(new TokenJSON(token)));
                }
                return token;
            }

            public static string getDiscordTokenWorker(out bool @new)
            {
                if (Directory.Exists("files") && File.Exists(@"files\discord.json"))
                {
                    var jo = JObject.Parse(File.ReadAllText(@"files\discord.json"));
                    if (jo.TryGetValue("token", out JToken cs) && (string)cs != "")
                    {
                        Console.WriteLine("Fetched Token");
                        @new = false;
                        return (string)cs;
                    }
                    else
                    {
                        Console.WriteLine("No Token at location");
                        @new = true;
                        return noDiscordToken();
                    }
                }
                else
                {
                    Console.WriteLine("No discord settings file");
                    @new = true;
                    return noDiscordToken();
                }
            }

            public static string noDiscordToken()
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There isn't a valid token for this bot.");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Go to: ");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("https://discordapp.com/developers/applications/me ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" and register an application, then create a \"Bot User\" and get the \"Token\" (the one where you click to reveal it).");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Enter Token: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                return Console.ReadLine();
            }

            private class TokenJSON
            {
                public string token { get; private set; }

                public TokenJSON(string t)
                {
                    token = t;
                }
            }
        }
    }

}
