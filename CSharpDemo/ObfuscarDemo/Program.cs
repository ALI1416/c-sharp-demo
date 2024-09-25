using System;
using System.Windows.Forms;

namespace ObfuscarDemo
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            /* 混淆 */
            // 需要添加`Obfuscar.xml`文件，属性`复制到输出目录`修改为`始终复制`，并在`.csproj`文件添加
            //   <PropertyGroup>
            //     <PostBuildEvent>"$(Obfuscar)" Obfuscar.xml</PostBuildEvent>
            //   </PropertyGroup>
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
