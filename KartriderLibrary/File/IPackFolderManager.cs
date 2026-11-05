using KartLibrary.Consts;

namespace KartLibrary.File
{
    /// <summary>
    /// 统一的分包管理器接口。Rho/Rho5 分包（<see cref="PackFolderManager"/>）与
    /// LPK 分包（<see cref="Lpk.LpkFolderManager"/>）都实现该接口，
    /// 使界面层可以用同一套 <see cref="PackFolderInfo"/> 目录树浏览两种格式。
    /// </summary>
    public interface IPackFolderManager
    {
        /// <summary>是否已经成功打开过分包。</summary>
        bool Initizated { get; }

        /// <summary>
        /// 打开整个 Data 目录。<paramref name="input"/> 可以是 Data 目录本身，
        /// 也可以是目录内的索引文件（rho 格式的 aaa.pk）。
        /// </summary>
        void OpenDataFolder(string input);

        /// <summary>打开单个分包文件。</summary>
        void OpenSingleFile(string file, CountryCode regionCode);

        /// <summary>打开多个分包文件。</summary>
        void OpenMultipleFiles(CountryCode regionCode, params string[] files);

        /// <summary>按路径取子目录，空串表示根目录。</summary>
        PackFolderInfo[]? GetDirectories(string Path);

        /// <summary>按路径取目录下的文件。</summary>
        PackFileInfo[]? GetFiles(string Path);

        /// <summary>按完整路径取单个文件。</summary>
        PackFileInfo? GetFile(string Path);

        /// <summary>取根目录副本。</summary>
        PackFolderInfo GetRootFolder();

        /// <summary>关闭并清空当前分包。</summary>
        void Reset();
    }
}
