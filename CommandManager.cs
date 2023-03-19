using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

#nullable disable

namespace Github_Release_Manger
{
    internal class CommandManager
    {
        private static string command;
        public static void GetCommand()
        {
            bool run = true;
            while (run)
            {
                command = Console.ReadLine();
                if (command.ToLower() is "exit" or "quit") Environment.Exit(0);
                InitCommand(command);
            }
        }

        private static void InitCommand(string command)
        {
            command = command.Trim();
            if (command == "") return;
            string cmd = command.Split()[0];
            Commands enum_command;
            if(!Enum.TryParse<Commands>(cmd.Replace("-","_"), true, out enum_command)) enum_command = Commands.NONE;


            switch (enum_command)
            {

                //case Commands.:  break;




                case Commands.clear: Console.Clear(); break;
                case Commands.help:
                    Console.WriteLine("\n-------Commands-------\n");
                    foreach (string enum_ in Enum.GetNames<Commands>().Skip(1).ToList())
                    {
                        Commands command_;
                        if (!Enum.TryParse<Commands>(enum_, true, out command_)) command_ = Commands.NONE;
                        string name = enum_ + " \t ".Replace("_", "-");
                        string description = command_.GetDescription().Replace("{attr}",new String(' ',name.Replace("\t","").Length));

                        Console.WriteLine("{0}{1}",name,description);
                    }
                    break;
                default:
                    Console.WriteLine("Command not foun '{0}'.",cmd);
                    break;
            }
        }

        static string[] InitArgs()
        {
            string cmd = String.Join(" ", command.Split().Skip(1).ToList());
            string[] attr_data = command.Split(new string[] { "-", "\"" }, StringSplitOptions.RemoveEmptyEntries);
            return attr_data;
        }

        static Dictionary<string,string> InitArgs(bool parseIntoPairs)
        {
            string cmd = String.Join(" ", command.Split().Skip(1).ToList());
            string[] attr_data = command.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string,string> arg_pairs = new Dictionary<string,string>();
            foreach(string attr in attr_data)
            {
                string[] data = attr.Split("\"",StringSplitOptions.RemoveEmptyEntries);
                string name = data[0].Trim('"').Trim();
                string value = data[1].Trim('"').Trim();
                if(!arg_pairs.ContainsKey(name))arg_pairs.Add(name, value);
            }
            return arg_pairs;
        }

        #region Commands
        #region Token Management

        private static void save_token()
        {
            string[] args = InitArgs();
            string token = args[0];
            string repo_name = args[1];
            string owner_name = args[2];
            string save_name = args[3];
            string expiration_date = args[4];

            XmlWriter writer = XmlWriter.Create("Data/");
        }

        private static void get_tokens()
        {

        }
        #endregion Token Management
        #endregion Commands
    }
    static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            var field = type.GetField(name);
            var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attr?.Description ?? "No description found.";
        }
    }

    internal enum Commands
    {
        //[Description("")],
        #region Basics
        [Description()] NONE,
        [Description("Print this message.")] help,
        [Description("Clears the terminal.")] clear,
        #endregion Basics

        #region Token Management
        [Description("Save token.\n" +
            "\n{attr}'save-token \"token\" \"repo-name\" \"owner-name\" \"save-name\" \"expiration-date(yyyy-mm-dd)\"'\n")] save_token,
        [Description("Print out all the saved tokens.\n" +
            "\n{attr}-owner\tPrint out all tokens releated to this owner." +
            "\n{attr}      \t'get-tokens -owner \"repo-owner\"'\n" +
            "\n{attr}-repo\tPrint out all tokens releated to this repo." +
            "\n{attr}     \t'get-tokens -repo \"repo-name\"'\n")] get_tokens,
        [Description("Print out token with saved name.\n" +
            "\n{attr}'get-token \"token name\"'\n")] get_token,
        #endregion Token Management

    }
}
