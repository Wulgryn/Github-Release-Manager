using GRM_Build_Tracker;
using System.Drawing;
using System.Net;
using System.Xml;

#nullable disable

namespace Github_Release_Manger
{
    internal class Program
    {
        public struct Token
        {
            public string name;
            public string owner;
            public string token;
            public string repo;
            public string expiration_date;
            public Token(XmlNode token) 
            {
                this.token = token.InnerText;
                owner = token.Attributes.GetNamedItem("Owner").Value;
                repo = token.Attributes.GetNamedItem("Repo").Value;
                expiration_date = token.Attributes.GetNamedItem("ExpirationDate").Value;
                name = token.Attributes.GetNamedItem("Name").Value;
            }

            public (string,ConsoleColor) GetExpirationDate()
            {
                DateTime exp_date = DateTime.Parse(expiration_date);
                TimeSpan days = exp_date - DateTime.Now;
                if (DateTime.Now.Ticks > exp_date.Ticks) return ("EXPIRED " + exp_date.ToString("d") + $" ({days.Days})",ConsoleColor.Red);
                if (exp_date.AddDays(-14).Ticks < DateTime.Now.Ticks) return (exp_date.ToString("d") + $" ({days.Days})", ConsoleColor.DarkYellow);
                return (exp_date.ToString("d") + $" ({days.Days})", ConsoleColor.Green);
            }
        }
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ONLY OPTIMALISED FOR VS COMMUNITY!");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Title = "Github Release Manager FOR VS COMMUNITY";
            BuildTracker.AddBuildNumber(ref args);
            if (args.Length != 0)
            {
                CommandManager.command = string.Join(" ", args).Replace("'","\"");
                Console.WriteLine(CommandManager.command);
                CommandManager.InitCommand(CommandManager.command);
                return;
            }
            /*Console.WriteLine("Loading tokens...\n");

            List<Token> tokens = CommandManager.LoadTokens();
            if(tokens.Count == 0) Console.WriteLine("No tokens found.");
            else tokens.ForEach(token => CommandManager.PrintToken(token, "found"));*/

            Console.WriteLine("Type 'help' for help.");

            CommandManager.GetCommand();
        }
    }
}