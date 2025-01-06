﻿using System;
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

namespace RhoLoader
{
    static class Program
    {
        public static CountryCode CC = CountryCode.CN;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            string FileName = AppDomain.CurrentDomain.BaseDirectory + "CountryCode.ini";
            if (File.Exists(FileName))
            {
                string textValue = System.IO.File.ReadAllText(FileName);
                CC = (CountryCode)Enum.Parse(typeof(CountryCode), textValue);
            }
            else
            {
                using (StreamWriter streamWriter = new StreamWriter(FileName, false))
                {
                    streamWriter.Write(CC.ToString());
                }
            }
            string input;
            string output;
            if (args == null || args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(true);
                Application.Run(new MainWindow());
                input = "";
                output = "";
            }
            else if (args.Length == 1)
            {
                input = args[0];
                output = args[0];
            }
            else
            {
                if (args.Length != 2)
                    return;
                input = args[0];
                output = args[1];
            }
            if (input.EndsWith(".rho") || input.EndsWith(".rho5"))
            {
                Program.decode(input, output);
            }
            if (input.EndsWith(".xml"))
            {
                Program.XtoB(input);
            }
            if (input.EndsWith("aaa.bml"))
            {
                Program.AAAD(input);
            }
            else if (input.EndsWith(".bml"))
            {
                Program.BtoX(input);
            }
            if (input.EndsWith(".pk"))
            {
                Program.AAAR(input);
            }
            else
            {
                if (!Directory.Exists(input))
                    return;
                if (input.Contains("_0"))
                {
                    Program.encode(input, output);
                }
                else
                {
                    string[] files = Directory.GetFiles(input, "*.rho");
                    if (files.Length > 0)
                    {
                        Program.AAAC(input, files);
                    }
                    else
                    {
                        Program.encodea(input, output);
                    }
                }
            }
        }

        private static void encodea(string input, string output)
        {
            RhoArchive rhoArchive = new RhoArchive();
            if (!output.EndsWith(".rho"))
                output += ".rho";

            Program.SaveFolder(input, output);
        }


        private static void SaveFolder(string intput, string output)
        {
            RhoArchive rhoArchive = new RhoArchive();
            string lastFolderName = Path.GetFileName(intput);
            Console.WriteLine(lastFolderName);
            string array = lastFolderName.Replace('_', '\\'); ;
            GetAllFiles(intput + "\\" + array, new List<string>(), rhoArchive.RootFolder);

            rhoArchive.SaveTo(output);
        }

        private static void GetAllFiles(string folderPath, List<string> fileList, RhoFolder folder)
        {
            // 获取当前目录下的所有文件
            string[] files = Directory.GetFiles(folderPath);

            // 将文件路径添加到文件列表中
            foreach (string file in files)
            {
                RhoFile item = new RhoFile
                {
                    DataSource = new FileDataSource(file),
                    Name = Path.GetFileName(file),
                    FileEncryptionProperty = RhoFileProperty.Compressed
                };
                folder.AddFile(item);
            }

            // 获取当前目录下的所有子目录
            string[] subdirectories = Directory.GetDirectories(folderPath);

            // 对每个子目录递归调用
            foreach (string subdirectory in subdirectories)
            {
                RhoFolder folder2 = new RhoFolder
                {
                    Name = Path.GetFileName(subdirectory)
                };
                folder.AddFolder(folder2);
                GetAllFiles(subdirectory, fileList, folder2);
            }
        }

        private static void encode(string input, string output)
        {
            Rho5Archive rho5Archive = new Rho5Archive();
            if (!output.EndsWith(".rho5"))
                output += ".rho5";
            var fileInfo = new FileInfo(output);
            string fullName = "";
            if (fileInfo.Directory != null)
            {
                fullName = fileInfo.Directory.FullName;
                if (!Directory.Exists(fullName))
                    Directory.CreateDirectory(fullName);
                string[] strArray = fileInfo.Name.Replace(".rho5", "").Split("_", StringSplitOptions.None);
                string dataPackName = strArray[0];
                int dataPackID = 0;
                if (strArray.Length == 2)
                    dataPackID = Convert.ToInt32(strArray[1]);
                input = input.Replace("\\", "/");
                if (!input.EndsWith("/"))
                    input += "/";
                rho5Archive.SaveFolder(input, dataPackName, fullName, CC, dataPackID);
            }
            else
            {
                Console.WriteLine($"路径不存在：{output}");
            }
        }

        private static void decode(string input, string output)
        {
            if (output.EndsWith(".rho"))
                output = output.Replace(".rho", "");
            if (output.EndsWith(".rho5"))
                output = output.Replace(".rho5", "");
            PackFolderManager packFolderManager = new PackFolderManager();
            packFolderManager.OpenSingleFile(CC, input);
            Queue<PackFolderInfo> packFolderInfoQueue = new Queue<PackFolderInfo>();
            packFolderInfoQueue.Enqueue(packFolderManager.GetRootFolder());
            packFolderManager.GetRootFolder();
            while (packFolderInfoQueue.Count > 0)
            {
                PackFolderInfo packFolderInfos = packFolderInfoQueue.Dequeue();
                foreach (PackFolderInfo packFolderInfo in packFolderInfos.GetFoldersInfo())
                {
                    string fileName = Path.GetFileNameWithoutExtension(packFolderInfo.FolderName);
                    RhoFolders(output, output + "/" + fileName, packFolderInfo);
                }
            }
        }

        private static void RhoFolders(string input, string output, PackFolderInfo rhoFolders)
        {
            if (rhoFolders.GetFilesInfo() != null)
            {
                foreach (var item in rhoFolders.GetFilesInfo())
                {
                    string fullName = input + "/" + ReplacePath(item.FullName);
                    string Name = Path.GetDirectoryName(fullName);
                    if (!Directory.Exists(Name))
                        Directory.CreateDirectory(Name);
                    byte[] data = item.GetData();
                    using (FileStream fileStream = new FileStream(fullName, FileMode.OpenOrCreate))
                    {
                        fileStream.Write(data, 0, data.Length);
                    }
                }
            }
            if (rhoFolders.Folders != null)
            {
                foreach (var rhoFolder in rhoFolders.Folders)
                {
                    string Folder = output + "/" + rhoFolder.FolderName;
                    if (!Directory.Exists(Folder))
                        Directory.CreateDirectory(Folder);
                    RhoFolders(input, Folder, rhoFolder);
                }
            }
        }

        private static string ReplacePath(string file)
        {
            return file.IndexOf(".rho") > -1 ? file.Substring(0, file.IndexOf(".rho")).Replace("_", "/") + file.Substring(file.IndexOf(".rho") + 4) : file;
        }

        private static void BtoX(string input)
        {
            byte[] data = File.ReadAllBytes(input);
            BinaryXmlDocument bxd = new BinaryXmlDocument();
            bxd.Read(Encoding.GetEncoding("UTF-16"), data);
            string output_bml = bxd.RootTag.ToString();
            Console.WriteLine(output_bml);
            byte[] output_data = Encoding.GetEncoding("UTF-16").GetBytes(output_bml);
            string filePath = System.IO.Path.ChangeExtension(input, "xml");
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                fs.Write(output_data, 0, output_data.Length);
            }
        }

        private static void XtoB(string input)
        {
            XDocument xdoc = XDocument.Load(input);
            if (xdoc.Root == null)
                return;
            List<int> childCounts = CountChildren(xdoc.Root, 0, new List<int>());
            using (XmlReader reader = XmlReader.Create(input))
            {
                using (OutPacket outPacket = new OutPacket())
                {
                    int Count = 0;
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            string elementName = reader.Name;
                            int attCount = reader.AttributeCount;
                            Console.WriteLine($"元素: {elementName}");
                            outPacket.WriteString(elementName);
                            outPacket.WriteInt(0);
                            Console.WriteLine($"属性数量: {attCount}");
                            outPacket.WriteInt(attCount);
                            for (int i = 0; i < attCount; i++)
                            {
                                reader.MoveToAttribute(i);
                                string attName = reader.Name;
                                outPacket.WriteString(attName);
                                string attValue = reader.Value;
                                outPacket.WriteString(attValue);
                                Console.WriteLine($"属性名: {attName}, 属性值: {attValue}");
                            }
                            Console.WriteLine($"子元素数量: {childCounts[Count]}");
                            outPacket.WriteInt(childCounts[Count]);
                            Count++;
                            reader.MoveToElement();
                        }
                    }
                    byte[] byteArray = outPacket.ToArray();
                    string filePath = System.IO.Path.ChangeExtension(input, "bml");
                    using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(byteArray, 0, byteArray.Length);
                    }
                }
            }
        }

        public static List<int> CountChildren(XElement element, int level, List<int> childCounts)
        {
            int childCount = element.Elements().Count();
            childCounts.Add(childCount);
            foreach (XElement child in element.Elements())
            {
                CountChildren(child, level + 1, childCounts);
            }
            return childCounts;
        }

        private static void AAAR(string input)
        {
            using FileStream fileStream = new FileStream(input, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            int totalLength = binaryReader.ReadInt32();
            byte[] array = binaryReader.ReadKRData(totalLength);
            fileStream.Close();
            BinaryXmlDocument bxd = new BinaryXmlDocument();
            bxd.Read(Encoding.GetEncoding("UTF-16"), array);
            string output_bml = bxd.RootTag.ToString();
            Console.WriteLine(output_bml);
            byte[] output_data = Encoding.GetEncoding("UTF-16").GetBytes(output_bml);
            string filePath = System.IO.Path.ChangeExtension(input, "xml");
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                fs.Write(output_data, 0, output_data.Length);
            }
        }

        private static void AAAD(string input)
        {
            byte[] array = File.ReadAllBytes(input);
            string filePath = System.IO.Path.ChangeExtension(input, "pk");
            using FileStream fileStream = new FileStream(filePath, FileMode.Create);
            {
                BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                binaryWriter.Write((int)0);
                int KRDataLength = binaryWriter.WriteKRData(array, false, true);
                Console.WriteLine(KRDataLength);
                binaryWriter.BaseStream.Seek(0, SeekOrigin.Begin);
                binaryWriter.Write(KRDataLength);
            }
        }

        private static void AAAC(string input, string[] files)
        {
            string[] whitelist = { "_I04_sn", "_I05_sn", "_R01_sn", "_R02_sn", "_I02_sn", "_I01_sn", "_I03_sn", "_L01_", "_L02_", "_L03_03_", "_L03_", "_L04_", "bazzi_", "arthur_", "bero_", "brodi_", "camilla_", "chris_", "contender_", "crowdr_", "CSO_", "dao_", "dizni_", "erini_", "ethi_", "Guazi_", "halloween_", "homrunDao_", "innerWearSonogong_", "innerWearWonwon_", "Jianbing_", "kephi_", "kero_", "kwanwoo_", "Lingling_", "lodumani_", "mabi_", "Mahua_", "marid_", "mobi_", "mos_", "narin_", "neoul_", "neo_", "nymph_", "olympos_", "panda_", "referee_", "ren_", "Reto_", "run_", "zombie_", "santa_", "sophi_", "taki_", "tiera_", "tutu_", "twoTop_", "twotop_", "uni_", "wonwon_", "zhindaru_", "zombie_", "flyingBook_", "flyingMechanic_", "flyingRedlight_", "crow_", "dragonBoat_", "GiLin_", "maple_", "beach_", "village_", "china_", "factory_", "ice_", "mine_", "nemo_", "world_", "forest_", "_I", "_R", "_S", "_F", "_P", "_K", "_D", "_jp" };
            string[] blacklist = { "character_" };
            string Whitelist = AppDomain.CurrentDomain.BaseDirectory + "Whitelist.ini";
            string Blacklist = AppDomain.CurrentDomain.BaseDirectory + "Blacklist.ini";
            if (File.Exists(Whitelist))
            {
                whitelist = File.ReadAllLines(Whitelist);
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(Whitelist))
                {
                    foreach (string white in whitelist)
                    {
                        writer.WriteLine(white);
                    }
                }
            }
            if (File.Exists(Blacklist))
            {
                blacklist = File.ReadAllLines(Blacklist);
            }
            else
            {
                using (StreamWriter blackr = new StreamWriter(Blacklist))
                {
                    foreach (string black in blacklist)
                    {
                        blackr.WriteLine(black);
                    }
                }
            }
            XElement root = new XElement("PackFolder", new XAttribute("name", "KartRider"));
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string result = fileName;
                foreach (string white in whitelist)
                {
                    result = result.Replace(white, white.Replace("_", "!"));
                }
                foreach (string black in blacklist)
                {
                    result = result.Replace(black.Replace("_", "!"), black);
                }
                Console.WriteLine(result);
                string[] splitParts = result.Split('_');
                XElement currentFolder = root;
                for (int i = 0; i < splitParts.Length - 1; i++)
                {
                    string folderName = splitParts[i];
                    XElement? subFolder = currentFolder.Elements("PackFolder")
                                                     .FirstOrDefault(f => (string?)f.Attribute("name") == folderName);
                    if (subFolder == null)
                    {
                        if (folderName == "character" || folderName == "flyingPet" || folderName == "pet" || folderName == "track")
                        {
                            subFolder = new XElement("PackFolder", new XAttribute("name", folderName), new XAttribute("loadPass", "1"));
                        }
                        else
                        {
                            subFolder = new XElement("PackFolder", new XAttribute("name", folderName));
                        }
                        currentFolder.Add(subFolder);
                    }
                    currentFolder = subFolder;
                }
                Rho rho = new Rho(file);
                uint rhoKey = rho.GetFileKey();
                uint dataHash = rho.GetDataHash();
                long size = rho.baseStream.Length;
                string rhoFolderName = splitParts.Length > 0 ? Path.ChangeExtension(splitParts[splitParts.Length - 1], null) : "";
                XElement rhoFolder = new XElement("RhoFolder",
                    new XAttribute("name", rhoFolderName.Replace('!', '_')),
                    new XAttribute("fileName", fileName),
                    new XAttribute("key", rhoKey.ToString()),
                    new XAttribute("dataHash", dataHash.ToString()),
                    new XAttribute("mediaSize", size.ToString()));
                currentFolder.Add(rhoFolder);
            }

            root.Save(input + "\\aaa.xml");
        }
    }
}