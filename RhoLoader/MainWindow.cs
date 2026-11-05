using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Resources;
using KartLibrary.Xml;
using KartLibrary.IO;
using KartLibrary.File;
using KartLibrary.File.Lpk;
using KartLibrary.Consts;
using RhoLoader.PreviewWindow;
using RhoLoader.Setting;
using System.Reflection;
using System.Security.Cryptography;

using RhoLoader.Controls;

namespace RhoLoader
{
    public partial class MainWindow : Form
    {
        private SettingLoader BaseSettingLoader = new SettingLoader();

        // rho/rho5 与 lpk 两种分包格式共用同一套目录树接口，打开时按格式自动切换实现。
        private IPackFolderManager BaseFolderManager = new PackFolderManager();

        private PackFolderInfo _root_folder;

        private PackFolderInfo _cur_folder;

        private int _sortColumn = 0;
        private bool _sortAscending = true;

        public MainWindow()
        {
            InitializeComponent();
            LanguageManager.LoadLang();
            LanguageManager.LanguageName = "zh-cn";
            AddLanguages();
            LoadSetting();
            LoadLang();
            UpdatePackFormatMenu();
            listview_main.SmallImageList = imageList_listview;
            listview_main.SmallImageList.Images.Add("file", new Bitmap(global::RhoLoader.Resource.baseline_insert_drive_file_black_18dp));
            listview_main.SmallImageList.Images.Add("folder", new Bitmap(global::RhoLoader.Resource.folder_close));
            listview_main.ColumnClick += action_listview_columnclick;
        }
        public MainWindow(StartupOption startupOption) : this()
        {

        }

        #region Globalization
        private void LoadLang()
        {
            this.menu.Text = ((string)this.menu.Tag).GetStringBag();
            this.menu_file.Text = ((string)this.menu_file.Tag).GetStringBag();
            this.menu_file_open.Text = ((string)this.menu_file_open.Tag).GetStringBag();
            this.menu_file_openFiles.Text = ((string)this.menu_file_openFiles.Tag).GetStringBag();
            this.menu_file_openFolder.Text = ((string)this.menu_file_openFolder.Tag).GetStringBag();
            this.menu_file_exit.Text = ((string)this.menu_file_exit.Tag).GetStringBag();
            this.menu_extract.Text = ((string)this.menu_extract.Tag).GetStringBag();
            this.menu_extract_all.Text = ((string)this.menu_extract_all.Tag).GetStringBag();
            this.menu_extract_current.Text = ((string)this.menu_extract_current.Tag).GetStringBag();
            this.filemenu_extractfile.Text = ((string)this.filemenu_extractfile.Tag).GetStringBag();
            this.menu_about.Text = ((string)this.menu_about.Tag).GetStringBag();
            this.columnHeader1.Text = ((string)this.columnHeader1.Tag).GetStringBag();
            this.columnHeader2.Text = ((string)this.columnHeader2.Tag).GetStringBag();
            this.columnHeader3.Text = ((string)this.columnHeader3.Tag).GetStringBag();
            this.columnHeader4.Text = ((string)this.columnHeader4.Tag).GetStringBag();
            this.Text = ((string)this.Tag).GetStringBag();
            this.menu_lang.Text = ((string)this.menu_lang.Tag).GetStringBag();
            this.menu_packformat.Text = ((string)this.menu_packformat.Tag).GetStringBag();
            this.filemenu_extract_selected.Text = ((string)this.filemenu_extract_selected.Tag).GetStringBag();
            this.filemenu_convertPNG.Text = ((string)this.filemenu_convertPNG.Tag).GetStringBag();
            this.filemenu_convertXML.Text = ((string)this.filemenu_convertXML.Tag).GetStringBag();
            LoadLangFont();
            if (_cur_folder is not null)
                UpdateUIFolder();
            /*
            if (!(BaseRhoFile is null))
                UpdateFolders();
            */
        }
        private void AddLanguages()
        {
            List<ToolStripItem> temp = new List<ToolStripItem>();
            Language[] langs = LanguageManager.ListLanguages();
            foreach (Language lang in langs)
            {
                ToolStripMenuItem menu_langname = new ToolStripMenuItem();
                menu_langname.Text = lang.DisplayName;
                menu_langname.AutoSize = true;
                menu_langname.Click += action_changeLanguage;
                menu_langname.Font = lang.GetLangFontWithBase(this.Font);
                temp.Add(menu_langname);
            }
            menu_lang.DropDownItems.AddRange(temp.ToArray());
        }
        private void LoadLangFont()
        {
            this.menu.Font = LanguageManager.GetLangFontWithBase(this.menu.Font);
            this.menu_file.Font = LanguageManager.GetLangFontWithBase(this.menu_file.Font);
            this.menu_file_open.Font = LanguageManager.GetLangFontWithBase(this.menu_file_open.Font);
            this.menu_file_openFiles.Font = LanguageManager.GetLangFontWithBase(this.menu_file_openFiles.Font);
            this.menu_file_openFolder.Font = LanguageManager.GetLangFontWithBase(this.menu_file_openFolder.Font);
            this.menu_file_exit.Font = LanguageManager.GetLangFontWithBase(this.menu_file_exit.Font);
            this.menu_extract.Font = LanguageManager.GetLangFontWithBase(this.menu_extract.Font);
            this.menu_extract_all.Font = LanguageManager.GetLangFontWithBase(this.menu_extract_all.Font);
            this.menu_extract_current.Font = LanguageManager.GetLangFontWithBase(this.menu_extract_current.Font);
            this.filemenu_extractfile.Font = LanguageManager.GetLangFontWithBase(this.filemenu_extractfile.Font);
            this.menu_about.Font = LanguageManager.GetLangFontWithBase(this.menu_about.Font);
            this.menu_packformat.Font = LanguageManager.GetLangFontWithBase(this.menu_packformat.Font);
            this.listview_main.Font = LanguageManager.GetLangFontWithBase(this.listview_main.Font);
            this.Text = ((string)this.Tag).GetStringBag();
            //this.menu_lang.Font = LanguageManager.GetLangFontWithBase(this.menu_lang.Font);
            this.filemenu_convertPNG.Font = LanguageManager.GetLangFontWithBase(this.filemenu_convertPNG.Font);
            this.filemenu_convertXML.Font = LanguageManager.GetLangFontWithBase(this.filemenu_convertXML.Font);
            this.filemenu_extract_selected.Font = LanguageManager.GetLangFontWithBase(this.filemenu_extract_selected.Font);
        }

        #endregion
        #region Setting Loading
        private void LoadSetting()
        {
            if (!File.Exists("Setting.json"))
                return;
            BaseSettingLoader.LoadSetting("Setting.json");
            RhoLoader.Setting.Setting baseSetting = BaseSettingLoader.Setting;
            LanguageManager.SetLanguage(LanguageName: baseSetting.Language);
        }
        #endregion
        #region Control Action
        private void action_changeLanguage(object sender, EventArgs e)
        {
            ToolStripItem menu_langname = (ToolStripItem)sender;
            LanguageManager.SetLanguage(DisplayName: menu_langname.Text);
            BaseSettingLoader.Setting.Language = LanguageManager.LanguageName;
            BaseSettingLoader.SaveSetting("Setting.json");
            LoadLang();
            string region_str = BaseSettingLoader.Setting.Language switch
            {
                "ko-kr" => "KR",
                "zh-cn" => "CN",
                "zh-tw" => "TW",
                _ => "CN"
            };
            if (region_str == "KR" || region_str == "CN" || region_str == "TW")
                Program.CC = (CountryCode)Enum.Parse(typeof(CountryCode), region_str);
        }
        /// <summary>
        /// 切换封包格式（写入 Setting.json 的 PackFormat）：
        /// rho5 生成 *.rho5 数据包，lpk 生成 *.lpk 分包（LPK v4 客户端）。
        /// 该参数在命令行封包（RhoLoader.exe &lt;DataPack 目录&gt;）时生效。
        /// </summary>
        private void action_changePackFormat(object sender, EventArgs e)
        {
            string format = ReferenceEquals(sender, menu_packformat_lpk) ? "lpk" : "rho5";
            if (BaseSettingLoader.Setting is null)
                BaseSettingLoader.Setting = new RhoLoader.Setting.Setting();
            BaseSettingLoader.Setting.PackFormat = format;
            BaseSettingLoader.SaveSetting("Setting.json");
            UpdatePackFormatMenu();
        }
        /// <summary>
        /// 按当前设置刷新“封包格式”菜单的勾选状态。
        /// </summary>
        private void UpdatePackFormatMenu()
        {
            string format = BaseSettingLoader.Setting?.PackFormat ?? "rho5";
            bool lpk = string.Equals(format, "lpk", StringComparison.OrdinalIgnoreCase);
            menu_packformat_rho5.Checked = !lpk;
            menu_packformat_lpk.Checked = lpk;
        }
        private void action_openFolder(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Please select Data folder including aaa.pk or *.lpk.";
            if (fbd.ShowDialog() != DialogResult.OK)
                return;

            string dataFolder = fbd.SelectedPath;
            string aaaPkPath = Path.Combine(dataFolder, "aaa.pk");
            // LPK 版客户端的 Data 目录由 *.lpk 分包组成，aaa.pk 已不可读，
            // 因此先看是否存在 .lpk 分包，没有才退回 aaa.pk + rho/rho5 的旧流程。
            bool lpkMode = HasLpkPackages(dataFolder);
            if (!lpkMode && !File.Exists(aaaPkPath))
            {
                MessageBox.Show("aaa.pk or *.lpk cannot found in the folder.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            CloseCurrentFile();
            try
            {
                if (lpkMode)
                {
                    BaseFolderManager = new LpkFolderManager();
                    BaseFolderManager.OpenDataFolder(dataFolder);
                }
                else
                {
                    BaseFolderManager = new PackFolderManager();
                    BaseFolderManager.OpenDataFolder(aaaPkPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            BuildExplorerTree();
            _cur_folder = _root_folder = BaseFolderManager.GetRootFolder();
            UpdateUIFolder();
        }
        private void action_open(object sender, EventArgs e)
        {
            dialog_multiFile.InitialDirectory = dialog_singleFile.InitialDirectory;
            if (dialog_singleFile.ShowDialog() != DialogResult.OK)
                return;

            string fileName = dialog_singleFile.FileName;
            CloseCurrentFile();
            try
            {
                // .lpk 走 LPK 分包管理器，其余（.rho/.rho5/.nho）仍走原有实现。
                IPackFolderManager manager = IsLpkFile(fileName) ? new LpkFolderManager() : new PackFolderManager();
                manager.OpenSingleFile(fileName, Program.CC);
                BaseFolderManager = manager;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            PackFolderInfo[] rootFolders = BuildExplorerTree();
            _cur_folder = _root_folder = rootFolders.Length > 0 ? rootFolders[0] : BaseFolderManager.GetRootFolder();
            //PathStack.Push(rootFolders[0].FullName);
            UpdateUIFolder();
        }
        private void action_open_files(object sender, EventArgs e)
        {
            dialog_singleFile.InitialDirectory = dialog_multiFile.InitialDirectory;
            if (dialog_multiFile.ShowDialog() != DialogResult.OK)
                return;

            string[] files = dialog_multiFile.FileNames;
            CloseCurrentFile();
            try
            {
                bool lpkMode = files.Length > 0 && files.All(IsLpkFile);
                IPackFolderManager manager;
                string[] targets;
                if (lpkMode)
                {
                    manager = new LpkFolderManager();
                    targets = files;
                }
                else
                {
                    // 混选时 rho/rho5 走原实现，.lpk 交给 LPK 管理器单独打开。
                    manager = new PackFolderManager();
                    targets = files.Where(x => !IsLpkFile(x)).ToArray();
                }
                manager.OpenMultipleFiles(Program.CC, targets);
                BaseFolderManager = manager;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            BuildExplorerTree();
            _cur_folder = _root_folder = BaseFolderManager.GetRootFolder();
            //PathStack.Push(rootFolders[0].FullName);
            UpdateUIFolder();
        }
        private void action_aboutWindow(object sender, EventArgs e)
        {
            AboutMe aboutDialog = new AboutMe();
            aboutDialog.ShowDialog();
        }
        private void action_exit(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void action_listview_click(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            if (listview_main.SelectedItems.Count == 0)
                return;
            contextMenu_list.Show((Control)sender, e.X, e.Y);
            filemenu_extract_selected.Enabled = true;
            if (listview_main.SelectedItems.Count == 1 && listview_main.SelectedItems[0].Tag is PackFileInfo)
            {
                filemenu_extractfile.Enabled = true;
                filemenu_convertPNG.Enabled = listview_main.SelectedItems[0].SubItems[0].Text.EndsWith(".tga") || listview_main.SelectedItems[0].SubItems[0].Text.EndsWith(".dds");
                filemenu_convertXML.Enabled = listview_main.SelectedItems[0].SubItems[0].Text.EndsWith(".bml");
            }
            else
            {
                filemenu_extractfile.Enabled = false;
                filemenu_convertPNG.Enabled = false;
                filemenu_convertXML.Enabled = false;
            }
        }
        private void action_listview_columnclick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == _sortColumn)
            {
                _sortAscending = !_sortAscending;
            }
            else
            {
                _sortColumn = e.Column;
                _sortAscending = true;
            }
            List<ListViewItem> folderItems = new List<ListViewItem>();
            List<ListViewItem> fileItems = new List<ListViewItem>();
            foreach (ListViewItem item in listview_main.Items)
            {
                if (item.Tag is PackFolderInfo)
                    folderItems.Add(item);
                else
                    fileItems.Add(item);
            }
            folderItems.Sort((x, y) =>
            {
                int result = string.Compare(x.Text, y.Text, StringComparison.OrdinalIgnoreCase);
                return _sortAscending ? result : -result;
            });
            fileItems.Sort((x, y) =>
            {
                int result = 0;
                switch (_sortColumn)
                {
                    case 0:
                        result = string.Compare(x.Text, y.Text, StringComparison.OrdinalIgnoreCase);
                        break;
                    case 1:
                        result = string.Compare(x.SubItems[1].Text, y.SubItems[1].Text, StringComparison.OrdinalIgnoreCase);
                        break;
                    case 2:
                        double numX = ParseFileSize(x.SubItems[2].Text);
                        double numY = ParseFileSize(y.SubItems[2].Text);
                        result = numX.CompareTo(numY);
                        break;
                    case 3:
                        result = string.Compare(x.SubItems[3].Text, y.SubItems[3].Text, StringComparison.OrdinalIgnoreCase);
                        break;
                }
                return _sortAscending ? result : -result;
            });
            listview_main.Items.Clear();
            listview_main.Items.AddRange(folderItems.ToArray());
            listview_main.Items.AddRange(fileItems.ToArray());
        }
        private double ParseFileSize(string sizeStr)
        {
            if (string.IsNullOrEmpty(sizeStr))
                return 0;
            string trimmed = sizeStr.Trim();
            double multiplier = 1;
            if (trimmed.EndsWith("MiB"))
            {
                multiplier = 1024 * 1024;
                trimmed = trimmed.Substring(0, trimmed.Length - 3).Trim();
            }
            else if (trimmed.EndsWith("KiB"))
            {
                multiplier = 1024;
                trimmed = trimmed.Substring(0, trimmed.Length - 3).Trim();
            }
            else if (trimmed.EndsWith("Bytes"))
            {
                multiplier = 1;
                trimmed = trimmed.Substring(0, trimmed.Length - 5).Trim();
            }
            if (double.TryParse(trimmed, out double value))
            {
                return value * multiplier;
            }
            return 0;
        }
        private void action_listview_doubleclick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || listview_main.SelectedItems.Count == 0)
                return;
            ListViewItem sel_item = listview_main.SelectedItems[0];
            if (sel_item.Tag is PackFileInfo sel_file)
            {
                string FileName = sel_item.Text;
                string ext = sel_file.FullName[^3..^0];
                if (ext == "dds" || ext == "tga")
                {
                    TgaDDsViewer tdv = new TgaDDsViewer();
                    tdv.Data = sel_file.GetData();
                    tdv.Type = ext == "dds" ? TgaDDsViewer.FileType.dds : ext == "tga" ? TgaDDsViewer.FileType.tga : throw new Exception();
                    tdv.ShowBox();
                }
                else if (ext == "bml")
                {
                    byte[] bmlData = sel_file.GetData();
                    bmlViewer bv = new bmlViewer(bmlData, sel_file.FullName);
                    bv.Show();
                }
                else if (ext == "ksv")
                {
                    byte[] ksvData = sel_file.GetData();
                    string region_str = LanguageManager.LanguageName switch
                    {
                        "ko-kr" => "kr",
                        "zh-cn" => "cn",
                        "zh-tw" => "tw",
                        _ => "kr"
                    };
                    PackFileInfo? track_info = BaseFolderManager.GetFile($"track_/common/trackLocale@{region_str}.bml");
                    if (track_info is not null)
                    {
                        byte[] data = track_info.GetData();
                        BinaryXmlDocument bmlDoc = new BinaryXmlDocument();
                        bmlDoc.Read(Encoding.GetEncoding("UTF-16"), data);
                        KSVPreview preview = new KSVPreview(ksvData, bmlDoc.RootTag);
                        preview.Show();
                    }
                    else
                    {
                        KSVPreview preview = new KSVPreview(ksvData);
                        preview.Show();
                    }
                }
                else
                {
                    if (ext == "kml")
                    {
                        FileName = $"{FileName[0..^3]}.xml";
                    }
                    FileStream fs = new FileStream(Environment.GetEnvironmentVariable("TEMP") + $"\\{FileName}", FileMode.Create);
                    byte[] data = sel_file.GetData();
                    fs.Write(data, 0, data.Length);
                    fs.Close();
                    data = null;
                    Process ps = new Process();
                    ps.StartInfo.FileName = "explorer.exe";
                    ps.StartInfo.Arguments = Environment.GetEnvironmentVariable("TEMP") + $"\\{FileName}";
                    ps.Start();
                }
            }
            else if (sel_item.Tag is PackFolderInfo sel_folder)
            {
                _cur_folder = sel_folder;
                UpdateUIFolder();
            }
        }
        private void action_back(object sender, EventArgs e)
        {
            if (_cur_folder != _root_folder)
            {
                _cur_folder = _cur_folder.ParentFolder == null ? _root_folder : _cur_folder.ParentFolder;
                UpdateUIFolder();
            }
            else
            {
                icon_back.Enabled = false;
            }
        }
        private void action_icon_enable_changed(object sender, EventArgs e)
        {
            if (icon_back.Enabled)
                this.icon_back.Image = global::RhoLoader.Resource.ic_fluent_arrow_hook_up_left_24_filled;
            else
                this.icon_back.Image = global::RhoLoader.Resource.ic_fluent_arrow_hook_up_left_24_filled_disabled;
        }
        private void action_node_select(object sender, TreeViewEventArgs e)
        {
            var click_node = e.Node;
            if (click_node is null)
                return;
            var node_info = click_node.Tag as NodeInfoContainer;
            if (node_info is null)
                return;
            var folder_info = node_info.BaseData as PackFolderInfo;
            if (folder_info is null)
                return;
            _cur_folder = folder_info;
            UpdateUIFolder();
        }
        private void action_extract_all(object sender, EventArgs e)
        {
            if (_cur_folder is null)
            {
                MessageBox.Show("msg_open_plz".GetStringBag(), "msg_level_error".GetStringBag(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            FolderBrowserDialog _folderDialog = new FolderBrowserDialog();
            ExtractOption _extractOptionDialog = new ExtractOption();
            if (_folderDialog.ShowDialog() == DialogResult.OK && _extractOptionDialog.ShowDialog() == DialogResult.OK)
            {

                ExtractFolder _extractFolderDialog = new ExtractFolder(_root_folder, _folderDialog.SelectedPath, _extractOptionDialog.SelectOption);
                if (_extractFolderDialog.ShowDialog() != DialogResult.OK)
                    MessageBox.Show("msg_cancel_operation".GetStringBag(), "msg_level_info".GetStringBag(), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void action_extract_current(object sender, EventArgs e)
        {
            if (_cur_folder is null)
            {
                MessageBox.Show("msg_open_plz".GetStringBag(), "msg_level_error".GetStringBag(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            FolderBrowserDialog _folderDialog = new FolderBrowserDialog();
            ExtractOption _extractOptionDialog = new ExtractOption();
            if (_folderDialog.ShowDialog() == DialogResult.OK && _extractOptionDialog.ShowDialog() == DialogResult.OK)
            {

                ExtractFolder _extractFolderDialog = new ExtractFolder(_cur_folder, _folderDialog.SelectedPath, _extractOptionDialog.SelectOption);
                if (_extractFolderDialog.ShowDialog() != DialogResult.OK)
                    MessageBox.Show("msg_cancel_operation".GetStringBag(), "msg_level_info".GetStringBag(), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void action_extractfile(object sender, EventArgs e)
        {
            if (listview_main.SelectedItems.Count == 0)
                return;
            ListViewItem sel_item = listview_main.SelectedItems[0];
            if (sel_item.Tag is PackFileInfo sel_file)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                string FileName = listview_main.SelectedItems[0].SubItems[0].Text;
                sfd.Filter = "AllFiles|*.*";
                sfd.FileName = FileName;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                    byte[] data = sel_file.GetData();
                    fs.Write(data, 0, data.Length);
                    fs.Close();
                    data = null;
                }
            }
        }
        private void action_extract_selected(object sender, EventArgs e)
        {
            if (listview_main.SelectedItems.Count == 0)
                return;
            if (_cur_folder is null)
            {
                MessageBox.Show("msg_open_plz".GetStringBag(), "msg_level_error".GetStringBag(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            FolderBrowserDialog _folderDialog = new FolderBrowserDialog();
            ExtractOption _extractOptionDialog = new ExtractOption();
            if (_folderDialog.ShowDialog() == DialogResult.OK && _extractOptionDialog.ShowDialog() == DialogResult.OK)
            {
                List<PackFileInfo> output_files = new List<PackFileInfo>();
                List<PackFolderInfo> output_folders = new List<PackFolderInfo>();
                foreach (ListViewItem sel_item in listview_main.SelectedItems)
                {
                    if (sel_item.Tag is PackFileInfo sel_file)
                    {
                        output_files.Add((PackFileInfo)sel_file.Clone());
                    }
                    else if (sel_item.Tag is PackFolderInfo sel_folder)
                    {
                        output_folders.Add((PackFolderInfo)sel_folder.Clone());
                    }
                }
                PackFolderInfo extract_virtual_folder = new PackFolderInfo("", "", null, output_folders, output_files);
                ExtractFolder _extractFolderDialog = new ExtractFolder(extract_virtual_folder, _folderDialog.SelectedPath, _extractOptionDialog.SelectOption);
                if (_extractFolderDialog.ShowDialog() != DialogResult.OK)
                    MessageBox.Show("msg_cancel_operation".GetStringBag(), "msg_level_info".GetStringBag(), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void action_convert_png(object sender, EventArgs e)
        {
            if (listview_main.SelectedItems.Count == 0)
                return;
            ListViewItem sel_item = listview_main.SelectedItems[0];
            if (sel_item.Tag is PackFileInfo sel_file)
            {
                string FileName = listview_main.SelectedItems[0].SubItems[0].Text;
                byte[] data = sel_file.GetData();
                TgaDDsViewer tga_viewer = new TgaDDsViewer();
                tga_viewer.Data = data;
                tga_viewer.ConvertTGADDSToPng();
                data = null;
            }
        }
        private void action_convert_xml(object sender, EventArgs e)
        {
            if (listview_main.SelectedItems.Count == 0)
                return;
            ListViewItem sel_item = listview_main.SelectedItems[0];
            if (sel_item.Tag is PackFileInfo sel_file)
            {
                string FileName = listview_main.SelectedItems[0].SubItems[0].Text;
                byte[] data = sel_file.GetData();
                BinaryXmlDocument bxd = new BinaryXmlDocument();
                bxd.Read(Encoding.GetEncoding("UTF-16"), data);
                string output = bxd.RootTag.ToString();
                byte[] output_data = Encoding.GetEncoding("UTF-16").GetBytes(output);
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = $"{sel_file.FullName[0..^3]}.xml";
                sfd.Filter = "XML File|*.xml";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                    fs.Write(output_data, 0, output_data.Length);
                    fs.Close();
                }
            }
        }
        #endregion
        #region Other Function
        private string GetCurrentPath()
        {
            if (_cur_folder is null)
                return "";
            return _cur_folder.FullName;
        }
        private void UpdateUIFolder()
        {
            if (_cur_folder is null)
            {
                return;
            }
            List<ListViewItem> temp_list = new List<ListViewItem>();
            this.listview_main.Items.Clear();
            foreach (PackFolderInfo sub_folder in _cur_folder.GetFoldersInfo())
            {
                ListViewItem lvi = new ListViewItem(new string[] { sub_folder.FolderName, ("listview_item2_folder").GetStringBag(), "", "" });
                lvi.ImageKey = "folder";
                lvi.Tag = sub_folder;
                temp_list.Add(lvi);
            }
            foreach (PackFileInfo sub_file in _cur_folder.GetFilesInfo())
            {
                string ext = Path.GetExtension(sub_file.FileName);
                ListViewItem lvi = new ListViewItem(new string[] { sub_file.FileName, string.IsNullOrEmpty(ext) ? "" : ext.Substring(1), $"{FormatDataLength(sub_file.FileSize)}", sub_file.SourceFileName });
                lvi.ImageKey = "file";
                lvi.Tag = sub_file;
                temp_list.Add(lvi);
            }
            this.listview_main.Items.AddRange(temp_list.ToArray());
            this.textbox_path.Text = GetCurrentPath();
            if (_cur_folder != _root_folder)
                this.icon_back.Enabled = true;
            else
                this.icon_back.Enabled = false;
        }
        /// <summary>
        /// 依据当前分包管理器的根目录重建左侧目录树，返回根一级的目录列表。
        /// </summary>
        private PackFolderInfo[] BuildExplorerTree()
        {
            treeview_explorer.Nodes.Clear();
            Queue<PackFolderInfo> folderQueue = new Queue<PackFolderInfo>();
            Queue<TreeNode> nodeQueue = new Queue<TreeNode>();
            PackFolderInfo[] rootFolders = BaseFolderManager.GetDirectories("") ?? Array.Empty<PackFolderInfo>();
            foreach (PackFolderInfo folder in rootFolders)
            {
                folderQueue.Enqueue(folder);
                TreeNode rootNode = new TreeNode()
                {
                    Text = folder.FolderName,
                    Tag = new NodeInfoContainer(NodeType.Folder, folder)
                };
                nodeQueue.Enqueue(rootNode);
                treeview_explorer.Nodes.Add(rootNode);
            }
            while (folderQueue.Count > 0 && nodeQueue.Count > 0)
            {
                TreeNode node = nodeQueue.Dequeue();
                PackFolderInfo packFolderInfo = folderQueue.Dequeue();
                foreach (PackFolderInfo folder in packFolderInfo.GetFoldersInfo())
                {
                    TreeNode subnode = new TreeNode()
                    {
                        Text = folder.FolderName,
                        Tag = new NodeInfoContainer(NodeType.Folder, folder)
                    };
                    node.Nodes.Add(subnode);
                    folderQueue.Enqueue(folder);
                    nodeQueue.Enqueue(subnode);
                }
            }
            return rootFolders;
        }

        private static bool IsLpkFile(string fileName)
        {
            return fileName.EndsWith(".lpk", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasLpkPackages(string folder)
        {
            try
            {
                return Directory.Exists(folder)
                    && Directory.GetFiles(folder, "*.lpk", SearchOption.TopDirectoryOnly).Length > 0;
            }
            catch
            {
                return false;
            }
        }

        private void CloseCurrentFile()
        {
            /*
            FolderStack.Clear();
            PathStack.Clear();
            */
            treeview_explorer.Nodes.Clear();
            listview_main.Items.Clear();
            BaseFolderManager.Reset();
        }
        private string FormatDataLength(int length)
        {
            string[] units = { "Bytes", "KiB", "MiB" };
            double dlen = length;
            foreach (string unit in units)
            {
                if (dlen > 1024)
                    dlen /= 1024;
                else
                    return $"{dlen: 0.00} {unit}";
            }
            return $"{dlen} {units[units.Length - 1]}";
        }
        #endregion

        #region StartupSetting
        public class StartupOption
        {
            public string FileName { get; set; } = "";
            public string DataFolderPath { get; set; } = "";
        }
        #endregion
    }
}