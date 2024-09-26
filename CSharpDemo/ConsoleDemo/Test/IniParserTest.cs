using ConsoleDemo.Util;
using System.Collections.Generic;
using System;

namespace ConsoleDemo.Test
{

    /// <summary>
    /// INI解析测试
    /// </summary>
    public class IniParserTest
    {

        public static void Test()
        {
            string filePath = "D:\\a.ini";
            // 读
            Console.WriteLine(IniUtils.GetAllSectionName(filePath));
            Console.WriteLine(IniUtils.GetSection("A", filePath));
            Console.WriteLine(IniUtils.GetInt("A", "int", filePath));
            Console.WriteLine(IniUtils.GetString("A", "string", filePath));
            Console.WriteLine(IniUtils.GetStruct("A", "string", filePath));
            // 写
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic["a"] = "test";
            dic["b"] = "123";
            Console.WriteLine(IniUtils.WriteSection("B", dic, filePath));
            Console.WriteLine(IniUtils.WriteString("B", "C", "TEST", filePath));
            Console.WriteLine(IniUtils.WriteStruct("B", "D", "TEST", filePath));
        }

    }

}
