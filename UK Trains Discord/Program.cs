using System;
using System.Threading.Tasks;

//&permissions=60480 Bot permissions

namespace UK_Trains_Discord
{
    class Program
    {
        Discord discord;
        
        public static void Main(string[] args) => new Program().Start();

        public void Start()
        {
            init();
            discord = new Discord();

            Task t = discord.MainAsync();
            System.Threading.Thread.Sleep(-1);
        }

        public void init()
        {
            Console.Title = "UK Trains Discord Bot";
        }
    }
}
