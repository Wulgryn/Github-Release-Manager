using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Github_Release_Manger
{
    internal class Config
    {

        public static string GetItem(string path,string name)
        {
            string[] config = File.ReadAllLines(path);
            List<string> items = new List<string>();
            config.ToList().ForEach(line =>
            {
                if (line.Trim()[0] != '#')
                {
                    items.Add(line);
                }
            });

            string item = "=";
            foreach (string i in items)
            {
                if (i.Contains(name + "="))
                {
                    item = i;
                    break;
                }
            }
            return item.Split("=")[1];
        }

        public static void EditItem(string path,string name, string value)
        {
            string[] config = File.ReadAllLines(path);
            List<string> items = new List<string>();
            config.ToList().ForEach(line =>
            {
                if (line.Trim()[0] != '#')
                {
                    items.Add(line);
                }
            });

            string item = "=";
            foreach (string i in items)
            {
                if (i.Contains(name + "="))
                {
                    item = i;
                    break;
                }
            }

            string itemName = item.Split("=")[0];
            int itemLine = config.ToList().IndexOf(item);
            config[itemLine] = itemName + "=" + value;
            File.WriteAllLines(path, config);
        }
    }
}
