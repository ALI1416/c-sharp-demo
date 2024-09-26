using ConsoleDemo.Util;
using System;
using System.Collections.Generic;

namespace ConsoleDemo.Test
{

    /// <summary>
    /// INI测试
    /// </summary>
    public class IniTest
    {

        public static void Test()
        {
            string filePath = "D:\\a.ini";
            // 读
            Console.WriteLine(IniUtils.GetAllSectionName(filePath));
            Console.WriteLine(IniUtils.GetSection("A", filePath));
            Console.WriteLine(IniUtils.GetInt("A", "int", filePath));
            Console.WriteLine(IniUtils.GetString("A", "string", filePath));
            Console.WriteLine(IniUtils.GetStruct("A", "struct", filePath));
            // 写
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic["s"] = "test";
            dic["i"] = "123";
            Console.WriteLine(IniUtils.WriteSection("B", dic, filePath));
            Console.WriteLine(IniUtils.WriteString("B", "string", "TEST", filePath));
            Console.WriteLine(IniUtils.WriteStruct("B", "struct", "TEST", filePath));
        }

    }

}
