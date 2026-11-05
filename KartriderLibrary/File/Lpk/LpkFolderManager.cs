using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KartLibrary.Consts;

namespace KartLibrary.File.Lpk
{
    /// <summary>
    /// 单个虚拟文件（对应一个 LPK 记录）。
    /// </summary>
    public class LpkFileInfo
    {
        public string FileName { get; set; } = "";
        public string FullName { get; set; } = "";
        public int FileSize { get; set; }

        internal Func<byte[]>? DataLoader { get; set; }

        public byte[]? GetData()
        {
            return DataLoader?.Invoke();
        }
    }

    /// <summary>
    /// 读取一个或多个 LPK v4 分包，构建虚拟目录树，并按 <see cref="PackFolderInfo"/> 树对外提供文件访问。
    /// 与 <see cref="PackFolderManager"/> 实现同一个 <see cref="IPackFolderManager"/> 接口，因此
    /// 界面层可以用同一套代码浏览 rho / rho5 / lpk 三种格式：
    /// <list type="bullet">
    /// <item>单文件：直接打开一个 .lpk 文件；</item>
    /// <item>Data 目录：LPK 版本的客户端不再依赖 aaa.pk，这里会自动扫描目录下的所有 .lpk 并合并成一棵树；</item>
    /// <item>多文件：一次打开多个 .lpk 分包。</item>
    /// </list>
    /// </summary>
    public class LpkFolderManager : IPackFolderManager
    {
        private PackFolderInfo _root = new PackFolderInfo();

        /// <summary>记录表中出现过的包内完整路径（去掉单文件挂载时添加的前缀），用于推断区服。</summary>
        private readonly List<string> _recordPaths = new List<string>();

        /// <summary>解析失败被跳过的分包（例如不属于版本 4 的包）。</summary>
        private readonly List<string> _failedPacks = new List<string>();

        public string[] FailedPacks => _failedPacks.ToArray();

        public bool Initizated { get; private set; }

        public CountryCode regionCode { get; private set; } = CountryCode.None;

        #region 打开

        /// <summary>
        /// 打开整个 Data 目录。
        /// <paramref name="input"/> 可以是 Data 目录本身、目录内的 aaa.pk，或单个 .lpk 文件。
        /// LPK 格式的 Data 目录中 aaa.pk 不可用时也能正常打开（只按目录内的 .lpk 分包建立索引）。
        /// </summary>
        public void OpenDataFolder(string input)
        {
            Reset();

            string? folder;
            string[] packs;
            if (Directory.Exists(input))
            {
                folder = input;
                packs = EnumerateLpk(input);
            }
            else if (System.IO.File.Exists(input))
            {
                folder = Path.GetDirectoryName(input);
                packs = IsLpkFile(input)
                    ? new[] { input }
                    : (string.IsNullOrEmpty(folder) ? Array.Empty<string>() : EnumerateLpk(folder));
            }
            else
            {
                throw new LpkException("找不到 .lpk 文件或目录: " + input);
            }

            if (packs.Length == 0)
                throw new LpkException(string.IsNullOrEmpty(folder)
                    ? "未找到任何 .lpk 文件"
                    : "未找到任何 .lpk 文件: " + folder);

            Array.Sort(packs, StringComparer.OrdinalIgnoreCase);

            int mounted = 0;
            Exception? firstError = null;
            foreach (string pack in packs)
            {
                try
                {
                    MountPack(pack, _root);
                    mounted++;
                }
                catch (Exception ex) when (ex is LpkException || ex is IOException)
                {
                    _failedPacks.Add(pack);
                    firstError ??= ex;
                }
            }

            // 一个都打不开时把首个错误抛出来，避免界面显示空目录。
            if (mounted == 0 && firstError is not null)
                throw new LpkException("没有可用的 .lpk 分包: " + firstError.Message);

            UpdateRegionCode();
            Initizated = true;
        }

        /// <summary>
        /// 打开单个 .lpk 文件。包内记录本身带完整路径，直接挂到根目录，
        /// 不额外套一层以文件名命名的目录（与 rho5 单文件打开方式一致）。
        /// </summary>
        public void OpenSingleFile(string file, CountryCode regionCode = CountryCode.None)
        {
            Reset();
            if (!System.IO.File.Exists(file))
                throw new FileNotFoundException(file);
            if (!IsLpkFile(file))
                throw new NotSupportedException("LpkFolderManager 只支持 .lpk 文件: " + file);

            MountPack(file, _root);

            UpdateRegionCode();
            Initizated = true;
        }

        /// <summary>
        /// 打开多个 .lpk 文件，每个文件挂在各自以文件名命名的根目录下。
        /// </summary>
        public void OpenMultipleFiles(CountryCode regionCode, params string[] files)
        {
            Reset();
            if (files is null || files.Length == 0)
                throw new ArgumentException("未指定任何 .lpk 文件", nameof(files));

            int mounted = 0;
            Exception? firstError = null;
            foreach (string file in files)
            {
                if (!IsLpkFile(file))
                    continue;
                if (!System.IO.File.Exists(file))
                    throw new FileNotFoundException(file);

                string name = Path.GetFileName(file);
                PackFolderInfo mountFolder = new PackFolderInfo(name, name, _root, Array.Empty<PackFolderInfo>(), Array.Empty<PackFileInfo>());
                _root.Folders.Add(mountFolder);
                try
                {
                    MountPack(file, mountFolder);
                    mounted++;
                }
                catch (Exception ex) when (ex is LpkException || ex is IOException)
                {
                    _root.Folders.Remove(mountFolder);
                    _failedPacks.Add(file);
                    firstError ??= ex;
                }
            }

            if (mounted == 0)
                throw new LpkException("没有可用的 .lpk 分包" + (firstError is null ? "" : ": " + firstError.Message));

            UpdateRegionCode();
            Initizated = true;
        }

        #endregion

        #region 目录树访问

        public PackFolderInfo[]? GetDirectories(string Path)
        {
            PackFolderInfo? folder = FindFolder(Path);
            return folder?.Folders.ToArray();
        }

        public PackFileInfo[]? GetFiles(string Path)
        {
            PackFolderInfo? folder = FindFolder(Path);
            return folder?.Files.ToArray();
        }

        public PackFileInfo? GetFile(string Path)
        {
            string path = Normalize(Path);
            if (path.Length == 0)
                return null;

            int idx = path.LastIndexOf('/');
            string folderPath = idx < 0 ? "" : path.Substring(0, idx);
            string fileName = idx < 0 ? path : path.Substring(idx + 1);

            PackFolderInfo? folder = FindFolder(folderPath);
            if (folder is null)
                return null;
            return folder.Files.FirstOrDefault(x => string.Equals(x.FileName, fileName, StringComparison.OrdinalIgnoreCase));
        }

        public PackFolderInfo GetRootFolder() => (PackFolderInfo)_root.Clone();

        public void Reset()
        {
            // 数据按需从磁盘读取，没有需要释放的持久句柄。
            Initizated = false;
            regionCode = CountryCode.None;
            _root = new PackFolderInfo();
            _recordPaths.Clear();
            _failedPacks.Clear();
        }

        /// <summary>
        /// 依据 LPK 内记录路径推断区服代码（kr / cn / tw），默认 cn。
        /// </summary>
        public string GetRegionCode()
        {
            foreach (string p in _recordPaths)
            {
                string low = p.ToLowerInvariant();
                // LPK 里的路径有几种写法：zeta/cn/...、zeta_/cn/...、zeta_cn.rho/...
                // 只认第一种会让非 CN 客户端一律退回默认 cn，导致 quest/channel 等数据按错区服路径匹配不到。
                foreach (string region in new[] { "kr", "cn", "tw" })
                {
                    if (low.StartsWith("zeta/" + region + "/")
                        || low.StartsWith("zeta_/" + region + "/")
                        || low.StartsWith("zeta_" + region + ".rho")
                        || low.StartsWith("zeta_" + region + "/"))
                    {
                        return region;
                    }
                }
            }
            return "cn";
        }

        #endregion

        #region 内部实现

        private void MountPack(string lpkPath, PackFolderInfo mountFolder)
        {
            using var fs = new FileStream(lpkPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            LpkIndex index = LpkV4.Decode(fs, Path.GetFileName(lpkPath));
            var (digits, nonce) = LpkV4.Parameters(index.Mixing);

            string packPath = lpkPath;
            long dataBegin = index.DataBegin;
            foreach (LpkRecord record in index.Records)
                AddFile(mountFolder, record, packPath, dataBegin, digits, nonce);
        }

        private void AddFile(PackFolderInfo mountFolder, LpkRecord record, string lpkPath, long dataBegin, int[] digits, byte[] nonce)
        {
            string path = Normalize(record.Path);
            string[] parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return;

            PackFolderInfo cur = mountFolder;
            for (int i = 0; i < parts.Length - 1; i++)
            {
                string part = parts[i];
                PackFolderInfo? sub = cur.Folders.FirstOrDefault(
                    x => string.Equals(x.FolderName, part, StringComparison.OrdinalIgnoreCase));
                if (sub is null)
                {
                    string subFullName = cur.FullName.Length == 0 ? part : cur.FullName + "/" + part;
                    sub = new PackFolderInfo(part, subFullName, cur, Array.Empty<PackFolderInfo>(), Array.Empty<PackFileInfo>());
                    cur.Folders.Add(sub);
                }
                cur = sub;
            }

            string fileName = parts[^1];
            // 不同分包可能包含相同路径，保留首个（与 rho 的实现一致）。
            if (cur.Files.Any(x => string.Equals(x.FileName, fileName, StringComparison.OrdinalIgnoreCase)))
                return;

            string fullName = cur.FullName.Length == 0 ? fileName : cur.FullName + "/" + fileName;
            var fileInfo = new LpkFileInfo
            {
                FileName = fileName,
                FullName = fullName,
                FileSize = (int)record.Dsize
            };

            string packPath = lpkPath;
            LpkRecord lpkRecord = record;
            long begin = dataBegin;
            // 数据按需解密解压：每次读取时重新打开分包，避免长期占用文件句柄。
            fileInfo.DataLoader = () =>
            {
                using var fs = new FileStream(packPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return LpkV4.Extract(fs, lpkRecord, begin, digits, nonce);
            };

            cur.Files.Add(new PackFileInfo
            {
                FileName = fileName,
                FullName = fullName,
                FileSize = fileInfo.FileSize,
                PackFileType = PackFileType.LpkFile,
                OriginalFile = fileInfo
            });

            _recordPaths.Add(path);
        }

        private PackFolderInfo? FindFolder(string path)
        {
            string normalized = Normalize(path);
            if (normalized.Length == 0)
                return _root;

            PackFolderInfo cur = _root;
            foreach (string part in normalized.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                PackFolderInfo? sub = cur.Folders.FirstOrDefault(
                    x => string.Equals(x.FolderName, part, StringComparison.OrdinalIgnoreCase));
                if (sub is null)
                    return null;
                cur = sub;
            }
            return cur;
        }

        private void UpdateRegionCode()
        {
            regionCode = (CountryCode)Enum.Parse(typeof(CountryCode), GetRegionCode(), true);
        }

        private static string[] EnumerateLpk(string folder)
        {
            return Directory.GetFiles(folder, "*.lpk", SearchOption.TopDirectoryOnly);
        }

        private static bool IsLpkFile(string path)
        {
            return path.EndsWith(".lpk", StringComparison.OrdinalIgnoreCase);
        }

        private static string Normalize(string p)
        {
            p = (p ?? "").Replace('\\', '/');
            while (p.StartsWith("/"))
                p = p.Substring(1);
            return p;
        }

        #endregion
    }
}
