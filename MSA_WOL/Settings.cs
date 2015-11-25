using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSA_WOL
{
    class Settings
    {

        public static string SETTINGS_FILE = AppDomain.CurrentDomain.BaseDirectory + "settings.ini";
        public static string SUBNET_DIR = AppDomain.CurrentDomain.BaseDirectory + "locations";
        public static string GetDHCPServer()
        {
            if (!File.Exists(SETTINGS_FILE))
            {
                //Create file with current known dhcp server
                using (StreamWriter writer = new StreamWriter(SETTINGS_FILE))
                {
                    writer.WriteLine("10.40.1.2");
                }
            }
            using (StreamReader reader = new StreamReader("settings.ini"))
            {
                return reader.ReadLine();
            }
        }


        //These accessgroups are not included

        public static string[] SubnetFiles()
        {
            return Directory.GetFiles(SUBNET_DIR, "*.txt");
        }

        public static string[] GetSubnetsFromFile(string file)
        {
            return File.ReadAllLines(file);
        }
    }
}
