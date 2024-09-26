using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleDemo.Util
{

    /// <summary>
    /// INI工具
    /// <see cref="https://learn.microsoft.com/zh-cn/windows/win32/api/winbase/nf-winbase-getprivateprofileint"/>
    /// </summary>
    public class IniUtils
    {

        /// <summary>
        /// 缓存长度
        /// </summary>
        private readonly static uint BUFFER_LENGTH = 1024;

        /// <summary>
        /// 获取所有段名
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="size">最大值的长度</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>无符号整型</returns>
        [DllImport("kernel32.dll")]

        private static extern uint GetPrivateProfileSectionNames(StringBuilder value, uint size, string filePath);

        /// <summary>
        /// 获取所有段名
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>所有段名(只能显示第一个)</returns>
        public static string GetAllSectionName(string filePath)
        {
            StringBuilder value = new StringBuilder();
            GetPrivateProfileSectionNames(value, BUFFER_LENGTH, filePath);
            return value.ToString();
        }

        /// <summary>
        /// 获取段
        /// </summary>
        /// <param name="section">段</param>
        /// <param name="value">值</param>
        /// <param name="size">最大值的长度</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>值的长度</returns>
        [DllImport("kernel32.dll")]
        private static extern uint GetPrivateProfileSection(string section, StringBuilder value, uint size, string filePath);

        /// <summary>
        /// 获取段
        /// </summary>
        /// <param name="section">段</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>段(只能显示第一个)</returns>
        public static string GetSection(string section, string filePath)
        {
            StringBuilder value = new StringBuilder();
            GetPrivateProfileSection(section, value, BUFFER_LENGTH, filePath);
            return value.ToString();
        }

        /// <summary>
        /// 获取整型
        /// </summary>
        /// <param name="section">段</param>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>值</returns>
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileInt(string section, string key, int defaultValue, string filePath);

        /// <summary>
        /// 获取整型
        /// </summary>
        /// <param name="section">段</param>
        /// <param name="key">键</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>值(不存在或错误返回0)</returns>
        public static int GetInt(string section, string key, string filePath)
        {
            return GetPrivateProfileInt(section, key, 0, filePath);
        }

        /// <summary>
        /// 获取字符串
        /// </summary>
        /// <param name="section">段</param>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <param name="value">值</param>
        /// <param name="size">最大值的长度</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>值的长度</returns>
        [DllImport("kernel32.dll")]
        private static extern uint GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder value, uint size, string filePath);

        /// <summary>
        /// 获取字符串
        /// </summary>
        /// <param name="section">段</param>
        /// <param name="key">键</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>值</returns>
        public static string GetString(string section, string key, string filePath)
        {
            StringBuilder value = new StringBuilder();
            GetPrivateProfileString(section, key, string.Empty, value, BUFFER_LENGTH, filePath);
            return value.ToString();
        }

        /// <summary>
        /// 获取结构体
        /// </summary>
        /// <param name="section">段</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="size">最大值的长度</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>值的长度</returns>
        [DllImport("kernel32.dll")]
        private static extern uint GetPrivateProfileStruct(string section, string key, StringBuilder value, uint size, string filePath);

        /// <summary>
        /// 获取结构体
        /// </summary>
        /// <param name="section">段</param>
        /// <param name="key">键</param>
        /// <param name="size">最大值的长度</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>结构体(错误)</returns>
        public static string GetStruct(string section, string key, string filePath)
        {
            StringBuilder value = new StringBuilder();
            GetPrivateProfileStruct(section, key, value, BUFFER_LENGTH, filePath);
            return value.ToString();
        }

        /// <summary>
        /// 写入段
        /// </summary>
        /// <param name="section">段</param>
        /// <param name="value">值</param>
        /// <returns>是否成功</returns>
        [DllImport("kernel32.dll")]
        private static extern bool WritePrivateProfileSection(string section, string value, string filePath);

        /// <summary>
        /// 写入段
        /// </summary>
        /// <param name="section">段</param>
        /// <param name="dic">Dictionary</param>
        /// <returns>是否成功</returns>
        public static bool WriteSection(string section, Dictionary<string, string> dic, string filePath)
        {
            string value = string.Empty;
            foreach (KeyValuePair<string, string> item in dic)
            {
                value += item.Key + "=" + item.Value + "\0";
            }
            return WritePrivateProfileSection(section, value, filePath);
        }

        /// <summary>
        /// 写入字符串
        /// </summary>
        /// <param name="section">段</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否成功</returns>
        [DllImport("kernel32.dll")]
        private static extern bool WritePrivateProfileString(string section, string key, string value, string filePath);

        /// <summary>
        /// 写入字符串
        /// </summary>
        /// <param name="section">段</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否成功</returns>
        public static bool WriteString(string section, string key, string value, string filePath)
        {
            return WritePrivateProfileString(section, key, value, filePath);
        }

        /// <summary>
        /// 写入结构体
        /// </summary>
        /// <param name="section">段</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="size">最大值的长度</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否成功</returns>
        [DllImport("kernel32.dll")]
        private static extern bool WritePrivateProfileStruct(string section, string key, string value, uint size, string filePath);

        /// <summary>
        /// 写入结构体
        /// </summary>
        /// <param name="section">段</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否成功(错误)</returns>
        public static bool WriteStruct(string section, string key, string value, string filePath)
        {
            return WritePrivateProfileStruct(section, key, value, (uint)value.Length, filePath);
        }

    }

}
