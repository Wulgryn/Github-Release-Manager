using System.Drawing;
using System.Net;
using System.Xml;

#nullable disable

namespace Github_Release_Manger
{
    internal class Program
    {
        struct Token
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
            if (args.Length != 0) return;
            Console.WriteLine("Loading tokens...\n");
            XmlDocument doc = new XmlDocument();
            doc.Load("Data/Tokens.grm");

            List<Token> tokens = new List<Token>();
            foreach(XmlNode node in  doc.DocumentElement.ChildNodes)
            {
                Token token = new Token(node);
                tokens.Add(token);
                Console.WriteLine("Token found:" +
                    "\n\tToken:          {0}" +
                    "\n\tOwner:          {1}" +
                    "\n\tRepo:           {2}",
                    token.token,token.owner,token.repo);
                Console.Write("\n\tExpration Date: ");
                (string,ConsoleColor) token_exp_date = token.GetExpirationDate();
                Console.ForegroundColor = token_exp_date.Item2;
                Console.WriteLine("{0}",token_exp_date.Item1);
                Console.ForegroundColor = ConsoleColor.White;
            }

            CommandManager.GetCommand();
        }
    }
}