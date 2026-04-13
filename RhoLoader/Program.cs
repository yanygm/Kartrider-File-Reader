using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Net;
using RhoLoader.Update;
using KartLibrary.IO;
using KartLibrary.Game.Engine.Tontrollers;
using KartLibrary.Game.Engine.Relements;
using KartLibrary.Game.Engine.Properities;
using KartLibrary.Game.Engine.Render;
using KartLibrary.File;
using KartLibrary.Consts;
using KartLibrary.Xml;
using KartLibrary.Data;
using KartLibrary.IO;
using System.Numerics;
using System.Text;
using System.Text.Json;
using RhoLoader.Setting;

namespace RhoLoader
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        public static CountryCode CC = CountryCode.CN;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Setting.json"))
                return;
            SettingLoader Language = new SettingLoader();
            using (FileStream file_stream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "Setting.json", FileMode.Open))
            {
                Language.Setting = JsonSerializer.Deserialize<RhoLoader.Setting.Setting>(file_stream);
            }
            string region_str = Language.Setting.Language switch
            {
                "ko-kr" => "KR",
                "zh-cn" => "CN",
                "zh-tw" => "TW",
                _ => "CN"
            };
            if (region_str == "KR" || region_str == "CN" || region_str == "TW")
                CC = (CountryCode)Enum.Parse(typeof(CountryCode), region_str);

            if (args != null && args.Length > 0)
            {
                AllocConsole();
                RhoPacker.PackTool(args, CC);
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(true);
                Application.Run(new MainWindow());
            }
        }
    }
}