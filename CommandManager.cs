using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static Github_Release_Manger.Program;

using Token = Github_Release_Manger.Program.Token;

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
                Console.Write("> ");
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
            if (!Enum.TryParse<Commands>(cmd.Replace("-", "_"), true, out enum_command)) enum_command = Commands.NONE;

            switch (enum_command)
            {

                //case Commands.:  break;

                case Commands.release: release(); break;
                case Commands.get_pack: get_pack(); break;
                case Commands.get_packs: get_packs(); break;
                case Commands.remove_pack: remove_pack(); break;
                case Commands.update_pack: update_pack(); break;
                case Commands.create_pack: create_pack(); break;
                case Commands.get_tokens: get_tokens(); break;
                case Commands.get_token: get_token(); break;
                case Commands.delete_token: delete_token(); break;
                case Commands.save_token: save_token(); break;
                case Commands.clear: Console.Clear(); break;
                case Commands.help:
                    Console.WriteLine("\n-------Commands-------\n" +
                        "\nexit \t Exit program." +
                        "\nquit \t Exit program.");
                    foreach (string enum_ in Enum.GetNames<Commands>().Skip(1).ToList())
                    {
                        Commands command_;
                        if (!Enum.TryParse<Commands>(enum_, true, out command_)) command_ = Commands.NONE;
                        string name = enum_.Replace("_", "-") + " \t ";
                        string description = command_.GetDescription(true, new String(' ', name.Replace("\t", "").Length));

                        Console.WriteLine("{0}{1}", name, description);
                    }
                    break;
                default:
                    Console.WriteLine("Command not foun '{0}'.", cmd);
                    break;
            }
        }
        #region Basics
        static bool YesNo(string text, params string[] args)
        {
            string in_ = "";
            while (in_ != "y" && in_ != "n")
            {
                Console.Write(text + " [y/n]", args);
                in_ = Console.ReadLine();
            }
            if (in_ == "n") return false;
            return true;
        }

        static bool YesNo(bool accept_enter, string text, params string[] args)
        {
            string in_ = null;
            while (in_ != "y" && in_ != "n" && in_ != "")
            {
                Console.Write(text + " [y/n]", args);
                in_ = Console.ReadLine();
            }
            if (in_ == "n") return false;
            return true;
        }
        #endregion Basics
        #region Args
        static string[] InitArgs()
        {
            string cmd = String.Join(" ", command.Split().Skip(1).ToList());

            string[] attr_data = cmd.Split(new string[] { " -", "\"" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w != "" && !Char.IsWhiteSpace(w.Trim('"')[0])).ToArray();
            List<string> data = new List<string>();
            attr_data.ToList().ForEach(f => data.Add(f.Trim('"')));
            return data.ToArray();
        }

        static Dictionary<string, string> InitArgs(bool parseIntoPairs)
        {
            string cmd = String.Join(" ", command.Split().Skip(1).ToList());
            string[] attr_data = command.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string,string> arg_pairs = new Dictionary<string,string>();
            foreach(string attr in attr_data)
            {
                string[] data = attr.Split("\"",StringSplitOptions.RemoveEmptyEntries);
                string name = data[0].Trim('"').Trim();
                string value = data[1].Trim('"').Trim();
                if (!arg_pairs.ContainsKey(name)) arg_pairs.Add(name, value);
            }
            return arg_pairs;
        }

        #region Commands
        #region Token Management

        static void save_token()
        {
            string[] args = InitArgs();
            if (!args.ArgsLengthTest(5, Commands.save_token)) return;
            string token = args[0];
            string repo_name = args[1];
            string owner_name = args[2];
            string save_name = args[3];
            string expiration_date = args[4];

            XmlWriter writer = XmlWriter.Create("Data/");
        }
        static void delete_token()
        {
            string[] args = InitArgs();
            if (!args.ArgsLengthTest(1, Commands.delete_token)) return;
            string name = args[0];
            bool silent = args.GetLogicItem(1, "q") || args.GetLogicItem(1, "silent");

        private static void get_tokens()
        {

        }
        #endregion Token Management
        #endregion Commands
    }
    static class ArgsExtensions
    {
        public static bool ArgsLengthTest(this string[] args, int length, Commands command)
        {
            if (args.Length < length)
            {
                Console.WriteLine("Useage: {0}", command.GetDescription(true, " "));
                return false;
            }
            return true;
        }

        public static bool ArgsLengthTest(this Dictionary<string, string> args, int length, Commands command)
        {
            if (args == null || args.Count < length)
            {
                Console.WriteLine("Useage: {0}", command.GetDescription(true, " "));
                return false;
            }
            return true;
        }

        public static bool GetLogicItem(this string[] args, int index, string arg_name = "")
        {
            if (args.Length > index) return (arg_name == "" ? true : arg_name == args[index] || "-" + arg_name == args[index]);
            return false;
        }
        public static string GetPairValue(this Dictionary<string, string> args, string key)
        {
            string value;
            if (args.TryGetValue(key, out  value) || args.TryGetValue("-" + key, out value)) return value;
            else return null;

        }
    }
    static class EnumExtensions
    {
        public static string GetDescription(this Enum value, bool attribute = false, string attr_prefix = "")
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            var field = type.GetField(name);
            var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return (attribute ? attr?.Description.Replace("{attr}", attr_prefix == "" ? new string(' ', name.Length) : attr_prefix) : attr?.Description) ?? "No description found.";
        }
    }

    static class DirectoryExtensions
    {
        public static void DeleteReadOnly(this FileSystemInfo fileSystemInfo)
        {
            var directoryInfo = fileSystemInfo as DirectoryInfo;
            if (directoryInfo != null)
            {
                foreach (FileSystemInfo childInfo in directoryInfo.GetFileSystemInfos())
                {
                    childInfo.DeleteReadOnly();
                }
            }

            fileSystemInfo.Attributes = FileAttributes.Normal;
            fileSystemInfo.Delete();
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
            "\n{attr}     \t'get-tokens -repo \"repo-name\"'\n")]
        get_tokens,

        [Description("Print out token with saved name.\n" +
            "\n{attr}'get-token \"token name\"'\n")] get_token,
        #endregion Token Management

    }
}