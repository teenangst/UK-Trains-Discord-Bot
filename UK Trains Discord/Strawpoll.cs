using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace UK_Trains_Discord
{
    /// To-do:
    /// • Yes/No options


    static partial class Tools
    {
        public static void Save(this Strawpoll strawpoll)
        {
            //if (!Directory.Exists("files"))
            //{
            //    Directory.CreateDirectory("files");
            //}
            //File.WriteAllText(@"files\polls.json", JsonConvert.SerializeObject(strawpoll));
        }
        public static void Load(this Strawpoll strawpoll)
        {
            //if (File.Exists(@"files\polls.json"))
            //{
            //    strawpoll.Polls = JsonConvert.DeserializeObject<Strawpoll>(File.ReadAllText(@"files\polls.json")).Polls;
            //}
        }

        public static List<string> pollemoji = new List<string>() { ":one:", ":two:", ":three:", ":four:", ":five:", ":six:", ":seven:", ":eight:", ":nine:", ":keycap_ten:" };
        public static List<string> pollemojisym = new List<string>() { "\U00000031\U000020e3", "\U00000032\U000020e3", "\U00000033\U000020e3", "\U00000034\U000020e3", "\U00000035\U000020e3", "\U00000036\U000020e3", "\U00000037\U000020e3", "\U00000038\U000020e3", "\U00000039\U000020e3", "🔟" };
        public static string x = "❌";
        public static string t = "✅";
        public static string q = "❓";
    }

    class Strawpoll
    {
        public List<Poll> Polls = new List<Poll>();
        public bool created;
        public Poll poll;

        public Strawpoll()
        {
            this.Load();   
        }

        public void New(string options, string creator, out bool created, out Poll poll)
        {
            bool made;
            Poll p = new Poll(options, creator, out made);
            created = made;
            poll = p;
            if (!p.dontUse)
            {
                Polls.Add(p);
            }
            else
            {
                Console.WriteLine("Poll not valid.");
            }
            this.Save();
        }
    }

    public class Poll
    {
        private const bool avoidtime = true;

        public string title = "";
        public string image = null;
        public DateTime timeEnd;
        public List<Tuple<string, int>> choices = new List<Tuple<string, int>>();

        public string postID = null;
        public bool dontUse = false;

        public string createdby = null;

        public Poll(string options, string createdby, out bool made) //"Example poll" "https://website.com/path/to/image.png" "00:12" "One" "Two" "Three"
        {
            ///Title must be the first item given, 
            ///Then image and timer are both optional and doesn't matter which order they are in
            ///Then options, these can contain image URLs and time formats (assuming you have already set an image and a timer - otherwise add a space at the end of the string inside the speech marks "Like so "
            this.createdby = createdby;
            List<string> workingList = new List<string>();

            foreach (Match m in Regex.Matches(options, @"(?<=\"")[^\""]*(?=\"")"))
            {
                if (m.Value != " ")
                {
                    workingList.Add(m.Value);
                }
            }
            if (workingList.Count == 0)
            {
                dontUse = true;
                made = false;
                return;
            }
            var k = 0;
            title = workingList[k];
            k++;

            bool imageDone = false,
                timerDone = false;
            
            for (int i = 1; i < workingList.Count; i++)
            {
                if (!imageDone && Regex.IsMatch(workingList[i], @"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,8}[-a-zA-Z0-9@:%_\+~#?&\.\/=]*$"))
                {
                    image = workingList[k];
                    k++;
                    imageDone = true;
                }
                else if (!avoidtime && !timerDone && Regex.IsMatch(workingList[i], @"^\d{1,2}(:\d{2}){0,2}$"))
                {
                    timeEnd = DateTime.UtcNow.Add(TimeSpan.Parse(workingList[k]));
                    k++;
                    timerDone = true;
                }
                else
                { 
                    choices.Add(new Tuple<string, int>(workingList[i], 0));
                }
            }
            if (choices.Count == 0) //Can't have 0 choices
            {
                dontUse = true;
                made = false;
            }
            else
            {
                choices = choices.Where((x, i) => i < 10).ToList(); //Restrict choices to 10 of them
                made = true;
            }

            //Console.WriteLine($"Title: {title}");
            //Console.WriteLine($"Image: {image}");
            //Console.WriteLine($"TimeEnd: {timeEnd.ToString("HH:mm:ss")}");
            //for (int i = 0; i < choices.Count; i++)
            //{
            //    Console.WriteLine($"Choice #{i+1}: {choices[i]}");
            //}
        }
    }
}
