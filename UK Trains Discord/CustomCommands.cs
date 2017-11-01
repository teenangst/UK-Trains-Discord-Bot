using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace UK_Trains_Discord
{
    public static class CustomCommands
    {
        public static string prefix = "!";
        public static Dictionary<string, string> commands = new Dictionary<string, string>();

        static CustomCommands()
        {
            load();
        }

        public static void newCommand(string command, string output)
        {
            commands.Add(command.ToLower(), output);
            save();
        }

        public static void tryRemoveCommand(string command)
        {
            if (commands.ContainsKey(command.ToLower()))
            {
                commands.Remove(command.ToLower());
                save();
            }
        }

        public static void tryChangeCommand(string command, string output, out bool changed)
        {
            if (commands.ContainsKey(command.ToLower()))
            {
                commands[command.ToLower()] = output;
                save();
                changed = true;
            }
            else
            {
                changed = false;
            }
        }

        private static void save()
        {
            if (!Directory.Exists("files"))
            {
                Directory.CreateDirectory("files");
            }
            File.WriteAllText(@"files\customCommands.json", JsonConvert.SerializeObject(commands));
            Console.WriteLine("customCommands.json saved");
        }

        private static void load()
        {
            if (File.Exists(@"files\customCommands.json"))
            {
                commands = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(@"files\customCommands.json"));
                Console.WriteLine("customCommands.json loaded");
            }
        }
    }
}
