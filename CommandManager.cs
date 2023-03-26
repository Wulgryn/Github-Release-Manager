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

            bool path = true;
            foreach (string str in cmd.Split('"'))
            {
                //if (str == "'")
                   // if (path)
            }


            string[] attr_data = cmd.Split(new string[] { " -" }, StringSplitOptions.RemoveEmptyEntries);
            if (attr_data.Length == 0) return null;
            Dictionary<string, string> arg_pairs = new Dictionary<string, string>();
            foreach (string attr in attr_data)
            {
                string[] data = attr.Split("\"", StringSplitOptions.RemoveEmptyEntries);
                if (data.Length == 1) return arg_pairs.Count == 0 ? null : arg_pairs;
                string name = data[0].Trim('"').Trim();
                string value = data[1].Trim('"').Trim();
                if (!arg_pairs.ContainsKey(name)) arg_pairs.Add(name, value);
            }
            return arg_pairs;
        }
        #endregion Args

        #region Tokens
        public static void PrintToken(Token token, string action)
        {
            Console.WriteLine("Token {4}:" +
                    "\n\tToken:          {0}" +
                    "\n\tOwner:          {1}" +
                    "\n\tRepo:           {2}" +
                    "\n\tName:           {3}",
                    token.token, token.owner, token.repo, token.name, action);
            Console.Write("\n\tExpration Date: ");
            (string, ConsoleColor) token_exp_date = token.GetExpirationDate();
            Console.ForegroundColor = token_exp_date.Item2;
            Console.WriteLine("{0}", token_exp_date.Item1);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void PrintToken(string token_name, string action)
        {
            Token token = GetTokenByName(token_name);
            if (token.name == null)
            {
                Console.WriteLine("Not found.");
                return;
            }
            Console.WriteLine("Token {4}:" +
                    "\n\tToken:          {0}" +
                    "\n\tOwner:          {1}" +
                    "\n\tRepo:           {2}" +
                    "\n\tName:           {3}",
                    token.token, token.owner, token.repo, token.name, action);
            Console.Write("\n\tExpration Date: ");
            (string, ConsoleColor) token_exp_date = token.GetExpirationDate();
            Console.ForegroundColor = token_exp_date.Item2;
            Console.WriteLine("{0}", token_exp_date.Item1);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static Token GetTokenByName(string name)
        {
            foreach (Program.Token token in LoadTokens()) if (token.name == name) return token;
            return new Program.Token();
        }
        public static List<Token> LoadTokens()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("Data/Tokens.grm");

            List<Token> tokens = new List<Token>();
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                Token token = new Token(node);
                tokens.Add(token);
            }
            return tokens;
        }
        static XmlDocument LoadTokens(ref XmlNode root)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("Data/Tokens.grm");

            if (root != null) root = doc.DocumentElement;
            return doc;
        }
        static void ChangeTokenAttribute(string token_name, string attribute, string new_value)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("Data/Tokens.grm");

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.Attributes.GetNamedItem("Name").Value == token_name)
                {
                    node.Attributes.GetNamedItem(attribute).Value = new_value;
                    break;
                }
            }
            doc.Save("Data/Tokens.grm");
        }
        static void ChangeTokenInnerText(string token_name, string new_value)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("Data/Tokens.grm");

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.Attributes.GetNamedItem("Name").Value == token_name)
                {
                    node.InnerText = new_value;
                    break;
                }
            }
            doc.Save("Data/Tokens.grm");
        }
        static bool RemoveToken(string token_name)
        {
            XmlNode root = null;
            XmlDocument doc = LoadTokens(ref root);

            bool found = false;

            foreach (XmlElement node in doc.DocumentElement.ChildNodes)
            {
                if (node.Attributes.GetNamedItem("Name").Value == token_name)
                {
                    node.ParentNode.RemoveChild(node);
                    found = true;
                    break;
                }
            }

            doc.Save("Data/Tokens.grm");
            return found;
        }
        #endregion Tokens

        #region Packages
        struct Pack
        {
            public string path;
            public string name;
            public string release_version;
            public string build_version;
            public string full_release_version;

            private string build_ver;

            private string config;
            public Pack(string configPath)
            {
                path = Config.GetItem(configPath, "path");
                name = Config.GetItem(configPath, "name");
                release_version = Config.GetItem(configPath, "release_version");
                if (File.Exists(path + "/build.version"))
                {
                    build_version = File.ReadAllText(path + "/build.version");
                    build_ver = build_version;
                }
                else
                {
                    build_version = "No version file found.";
                    build_ver = "0";
                }
                full_release_version = Config.GetItem(configPath, "full_release_version");

                config = configPath;
            }

            public string[] GetSourceFiles()
            {
                return Directory.GetFiles(path);
            }
            public string[] GetSourceDirectories()
            {
                return Directory.GetDirectories(path);
            }

            public string[] GetPackFiles()
            {
                return Directory.GetFiles("Data/Packages/" + name);
            }

            public void AddReleaseVersion()
            {
                release_version = (int.Parse(release_version) + 1) + "";
                Config.EditItem(config,"release_version",release_version);
            }
            public void AddFullReleaseVersion()
            {
                full_release_version = (int.Parse(full_release_version) + 1) + "";
                Config.EditItem(config, "full_release_version", full_release_version);
            }
            public string GetBuildVersion()
            {
                return build_ver;
            }
        }
        #endregion Packages



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
            bool silent = args.GetLogicItem(5, "q") || args.GetLogicItem(1, "silent");

            foreach (Program.Token token_ in LoadTokens())
            {
                if (token_.name == save_name)
                {
                    Console.WriteLine("Token with name '{0}' already exists", token_.name);
                    if (!silent && !YesNo("Would you like to overwrite?")) return;
                    ChangeTokenAttribute(token_.name, "Owner", owner_name);
                    ChangeTokenAttribute(token_.name, "Repo", repo_name);
                    ChangeTokenAttribute(token_.name, "ExpirationDate", DateTime.Parse(expiration_date).ToString("d"));
                    ChangeTokenInnerText(token_.name, token);

                    PrintToken(token_.name, "overwrited");
                    return;
                }
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("Data/Tokens.grm");
                XmlNode root = doc.DocumentElement;
                XmlNode new_token = doc.CreateElement("Token");

                XmlAttribute owner = doc.CreateAttribute("Owner");
                owner.Value = owner_name;

                XmlAttribute repo = doc.CreateAttribute("Repo");
                repo.Value = repo_name;

                XmlAttribute expirationDate = doc.CreateAttribute("ExpirationDate");
                expirationDate.Value = DateTime.Parse(expiration_date).ToString("d");

                XmlAttribute name = doc.CreateAttribute("Name");
                name.Value = save_name;

                new_token.InnerText = token;

                new_token.Attributes.Append(owner);
                new_token.Attributes.Append(repo);
                new_token.Attributes.Append(expirationDate);
                new_token.Attributes.Append(name);
                root.AppendChild(new_token);
                doc.Save("Data/Tokens.grm");
                PrintToken(new Program.Token(new_token), "saved");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR --> Message: {0}", ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }


        }
        static void delete_token()
        {
            string[] args = InitArgs();
            if (!args.ArgsLengthTest(1, Commands.delete_token)) return;
            string name = args[0];
            bool silent = args.GetLogicItem(1, "q") || args.GetLogicItem(1, "silent");

            if (!silent)
            {
                if (!YesNo(true, "Are you sure?")) return;
                if (!RemoveToken(name)) Console.WriteLine("Token not found...");
                Console.WriteLine("Token deleted succesfully.");
                return;
            }
            RemoveToken(name);

        }
        static void get_token()
        {
            string[] args = InitArgs();
            if (!args.ArgsLengthTest(1, Commands.get_token)) return;
            string name = args[0];
            PrintToken(name, "found");
        }
        static void get_tokens()
        {
            Dictionary<string, string> args = InitArgs(true);
            if (args == null || args.Count == 0)
            {
                LoadTokens().ForEach(token => PrintToken(token, "found"));
                return;
            }
            if (!args.ArgsLengthTest(1, Commands.get_tokens)) return;
            string owner = args.GetPairValue("owner");
            string repo = args.GetPairValue("repo");

            if (owner != null)
            {
                bool found = false;
                foreach (Token token in LoadTokens())
                {
                    if (token.owner == owner)
                    {
                        found = true;
                        PrintToken(token, "found");
                    }
                }
                if (!found) Console.WriteLine("No token found according to owner name.");
            }
            if (repo != null)
            {
                bool found = false;
                foreach (Token token in LoadTokens())
                {
                    if (token.repo == repo)
                    {
                        found = true;
                        PrintToken(token, "found");
                    }
                }
                if (!found) Console.WriteLine("No token found according to repo name.");
            }
        }
        #endregion Token Management

        #region Release Managemnt

        #region Package
        
        static void create_pack()
        {
            Dictionary<string, string> args = InitArgs(true);
            if (!args.ArgsLengthTest(1, Commands.create_pack)) return;
            string path = args.GetPairValue("path");
            string name = args.GetPairValue("name");
            string[] args_ = InitArgs();
            bool silent = args_.GetLogicItem(4,"q") || args_.GetLogicItem(1, "silent");

            if (path == null)
            {
                Console.WriteLine("Argument missing error.");
                return;
            }
            if (name == null) name = new DirectoryInfo(path).Name;
            if (Directory.Exists("Data/Packages/" + name))
            {
                Console.WriteLine($"Package with the same name exist. {(silent ? "Overwritted silently." : "")}");
                if (!silent && !YesNo(true, "Would you like to overwrite?")) return;
                new DirectoryInfo("Data/Packages/" + name).DeleteReadOnly();
            }
            Directory.CreateDirectory("Data/Packages/" + name);
            string pack_path = "Data/packages/" + name + "/";
            File.WriteAllLines(pack_path + "pack.cfg", new string[]
            {
                    $"path={path}",
                    $"name={name}",
                    "release_version=0",
                    "full_release_version=0"
            });

            Pack pack = new Pack(pack_path + "pack.cfg");

            foreach (string file in pack.GetSourceFiles())
            {
                FileInfo fi = new FileInfo(file);
                if (fi.Name == "build.version" || fi.Name == "desktop.ini") continue;
                File.Copy(file, pack_path + "/" + fi.Name.Replace(" ","-"), true);
                Console.WriteLine("Packed: " + fi.Name);
            }
            foreach(string directory in pack.GetSourceDirectories())
            {
                DirectoryInfo di = new DirectoryInfo(directory);
                ZipFile.CreateFromDirectory(directory, pack_path + "/" + di.Name.Replace(" ","-") + ".zip");
                Console.WriteLine("Packed: " + di.Name);
            }
        }

        static void update_pack()
        {
            string[] args = InitArgs();
            if (!args.ArgsLengthTest(1, Commands.create_pack)) return;
            string name = args[0];

            string path = "Data/Packages/" + name;
            if (!Directory.Exists(path))
            {
                Console.WriteLine("Package does not exist, use \"create-pack\" to create one.");
                return;
            }

            Pack pack = new Pack(path + "/pack.cfg");

            foreach(string file in pack.GetSourceFiles())
            {
                FileInfo sourceFile = new FileInfo(file);
                FileInfo packFile = new FileInfo(path + "/" + sourceFile.Name.Replace(" ","-"));
                if (sourceFile.Name == "build.version" || sourceFile.Name == "desktop.ini") continue;
                if (!packFile.Exists || (sourceFile.LastWriteTime != packFile.LastWriteTime))
                {
                    File.Copy(sourceFile.FullName, packFile.FullName, true);
                    Console.WriteLine("Packed: " + sourceFile.Name);
                }
            }

            foreach(string file in pack.GetPackFiles())
            {
                FileInfo packFile = new FileInfo(file);
                FileInfo sourceFile = new FileInfo(pack.path + "/" + packFile.Name.Replace("-"," "));

                if (packFile.Exists && !sourceFile.Exists && (packFile.Extension != ".zip" && packFile.Name != "pack.cfg"))
                {
                    File.Delete(packFile.FullName);
                    Console.WriteLine("Deleted pack: " + sourceFile.Name);
                }
            }


            foreach(string directory in pack.GetSourceDirectories())
            {
                DirectoryInfo sourceDir = new DirectoryInfo(directory);
                FileInfo packDir = new FileInfo(path + "/" + sourceDir.Name.Replace(" ","-") + ".zip");

                if(packDir.Exists) packDir.Delete();
                ZipFile.CreateFromDirectory(sourceDir.FullName, packDir.FullName);
                Console.WriteLine("ZIP Packed: " + sourceDir.Name);
            }

            foreach(string directory in pack.GetPackFiles())
            {
                FileInfo packDir = new FileInfo(directory);
                DirectoryInfo sourceDir = new DirectoryInfo(pack.path + "/" + Path.GetFileNameWithoutExtension(packDir.Name.Replace("-", " ")));

                if(packDir.Extension == ".zip" && packDir.Exists && !sourceDir.Exists)
                {
                    File.Delete(packDir.FullName);
                    Console.WriteLine("Deleted ZIP pack: " + sourceDir.Name);
                }
            }
            Console.WriteLine("Pack '" + pack.name + "' is up to date.");
        }

        static void remove_pack()
        {
            string[] args = InitArgs();
            if (!args.ArgsLengthTest(1, Commands.remove_pack)) return;
            string name = args[0];
            bool silent = args.GetLogicItem(1, "q") || args.GetLogicItem(1,"silent");

            string path = "Data/Packages/" + name;
            DirectoryInfo di = new DirectoryInfo(path);
            if(!di.Exists)
            {
                Console.WriteLine($"Package '{name}' does not exist.");
                return;
            }
            if(!silent && YesNo(true,"Are you sure?"))
            {
                di.DeleteReadOnly();
            }
            Console.WriteLine($"Package '{name}' deleted.");
        }

        static void get_packs()
        {
            string[] args = InitArgs();
            bool v = args.GetLogicItem(0, "v") || args.GetLogicItem(0, "verbose");
            bool verbose = args.GetLogicItem(0, "verbose");

            string[] packs = Directory.GetDirectories("Data/Packages");
            foreach(string pack in packs)
            {
                Console.WriteLine("Package found:");
                Pack p = new Pack(pack + "/pack.cfg");
                Console.WriteLine("\tName: \t {0}",p.name);
                if (v)
                {
                    Console.WriteLine("\n\tBuild version: \t {0}" +
                        "\n\tRelease version: \t {1}" +
                        "\n\tFull release version: \t {2}" +
                        "\n\n\tPath: \t {3}",
                        p.build_version,p.release_version,p.full_release_version,p.path);
                }
                if(verbose)
                {
                    string[] packFiles = p.GetPackFiles();

                    foreach(string file in packFiles)
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.Name == "pack.cfg" || fi.Name == "desktop.ini") continue;
                        if (fi.Extension == ".zip") Console.WriteLine("Folder:" +
                            "\n\tPack name: \t {0}" +
                            "\n\tSource name: \t {1}",fi.Name, Path.GetFileNameWithoutExtension(fi.Name.Replace("-"," ")));
                        else Console.WriteLine("File:" +
                            "\n\tPack name: \t {0}" +
                            "\n\tSource name: \t {1}",fi.Name,fi.Name.Replace("-"," "));
                    }
                }
            }
        }

        static void get_pack()
        {
            string[] args = InitArgs();
            if (!args.ArgsLengthTest(1, Commands.get_pack)) return;
            string name = args[0];
            bool verbose = args.GetLogicItem(1, "v") || args.GetLogicItem(1, "verbose");

            if (!Directory.Exists("Data/Packages/" + name))
            {
                Console.WriteLine($"Package '{name}' does not exist.");
                return;
            }
            Pack pack = new Pack("Data/Packages/" + name + "/pack.cfg");

            Console.WriteLine("Package found:");
            Console.WriteLine("\tName: \t {0}", pack.name);
            Console.WriteLine("\n\tBuild version: \t {0}" +
                    "\n\tRelease version: \t {1}" +
                    "\n\tFull release version: \t {2}" +
                    "\n\n\tPath: \t {3}",
                    pack.build_version, pack.release_version, pack.full_release_version, pack.path);
            if (verbose)
            {
                string[] packFiles = pack.GetPackFiles();

                foreach (string file in packFiles)
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.Name == "pack.cfg" || fi.Name == "desktop.ini") continue;
                    if (fi.Extension == ".zip") Console.WriteLine("Folder:" +
                        "\n\tPack name: \t {0}" +
                        "\n\tSource name: \t {1}", fi.Name, Path.GetFileNameWithoutExtension(fi.Name.Replace("-", " ")));
                    else Console.WriteLine("File:" +
                        "\n\tPack name: \t {0}" +
                        "\n\tSource name: \t {1}", fi.Name, fi.Name.Replace("-", " "));
                }
            }
        }

        #endregion package

        #region Release

        static void release()
        {
            string[] args = InitArgs();
            if (!args.ArgsLengthTest(2, Commands.release)) return;
            string name = args[0];
            string token_name = args[1];
            bool silent = args.GetLogicItem(2, "q") || args.GetLogicItem(2, "silent");

            Pack pack = new Pack("Data/Package/" + name + "/pack.cfg");
            Token token = GetTokenByName(token_name);

            GitHubClient client = new GitHubClient(new ProductHeaderValue(name));
            client.Credentials = new Credentials(token.token);

            string tag = $"{pack.full_release_version}.{pack.release_version}.{pack.GetBuildVersion()}";

            NewRelease newRelease = new NewRelease(tag);
            newRelease.Name = $"{DateTime.Now.Year}.{DateTime.Now.Month}.{DateTime.Now.Day} - {tag}";
            newRelease.GenerateReleaseNotes = true;

            Release release = client.Repository.Release.Create(token.owner,token.repo, newRelease).Result;

            //ReleaseAssetUpload assetUpload = new ReleaseAssetUpload(,,);

            //client.Repository.Release.UploadAsset(release,)
        }

        #endregion Release

        #endregion Release Management
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
            "\n{attr}'save-token \"token\" \"repo-name\" \"owner-name\" \"save-name\" \"expiration-date(yyyy-mm-dd)\" [-q:silent]'\n")]
        save_token,

        [Description("Delete token.\n" +
            "\n{attr}'delete-token \"token-name\" [-q:silent]'\n")]
        delete_token,

        [Description("Print out all the saved tokens.\n" +
            "\n{attr}-owner\tPrint out all tokens releated to this owner." +
            "\n{attr}      \t'get-tokens -owner \"repo-owner\"'\n" +
            "\n{attr}-repo\tPrint out all tokens releated to this repo." +
            "\n{attr}     \t'get-tokens -repo \"repo-name\"'\n")]
        get_tokens,

        [Description("Print out token with saved name.\n" +
            "\n{attr}'get-token \"token-name\"'\n")]
        get_token,
        #endregion Token Management

        #region Package Management
        [Description("Saves the files with a github supportive name. Also create .zip from folders.\n" +
            "\n{attr}-path\tPath of folder that contains items such as folders and files." +
            "\n{attr}-name\tSet the package name. Path directory by default." +
            "\n{attr}     \t'create-pack -path \"absolute-path-directory\" [-name \"package-name\"] [-q:silent]'\n")]
        create_pack,
        [Description("Update package items to prepare for the newest release upload.\n" +
            "\n{attr}'update-pack \"package-name\"'\n")]
        update_pack,
        [Description("Remove package.\n" +
            "\n{attr}'remove-pack \"package-name\" [-q:silent]'\n")]
        remove_pack,
        [Description("Print package.\n" +
            "\n{attr}'get-pack \"package-name\" [-v:verbose]'")]
        get_pack,
        [Description("List packages.\n" +
            "\n{attr}'get-packs [-v:verbose]'")]
        get_packs,
        #endregion Package Management

        #region Release Management

        [Description("Create a release.\n" +
            "\n{attr}'release \"package-name\" \"token-name\"[-q:silent]'")]//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
        release

        #endregion Release Management

    }
}