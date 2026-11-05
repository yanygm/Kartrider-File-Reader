using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhoLoader.Setting
{
    public class Setting
    {
        public string Language { get; set; } = "zh-cn";

        /// <summary>
        /// 数据包封包格式。可选值：
        /// <list type="bullet">
        /// <item><c>"rho5"</c>（默认）：生成 <c>*.rho5</c> 数据包；</item>
        /// <item><c>"lpk"</c>：生成 <c>*.lpk</c> 分包（P3563 及之后的 LPK v4 客户端）。</item>
        /// </list>
        /// 只在命令行封包模式（<c>RhoLoader.exe &lt;DataPack 目录&gt;</c>）下生效，
        /// 且只作用于 rho5 那条路径；rho 封包始终生成 <c>*.rho</c>。
        /// </summary>
        public string PackFormat { get; set; } = "rho5";
    }
}
