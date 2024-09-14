using System;
using System.Windows.Forms;

namespace CosturaDemo
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            /* 打包到同一个文件 */
            // 需要删掉`.csproj`文件里`Costura.Fody`项里的`IncludeAssets`项
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
