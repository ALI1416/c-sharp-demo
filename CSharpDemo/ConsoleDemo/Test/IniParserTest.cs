using IniParser;
using IniParser.Model;
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
            FileIniDataParser parser = new FileIniDataParser();
            IniData data = parser.ReadFile(filePath);
            Console.WriteLine(data);
            Console.WriteLine(data["A"]["a"]);
            // 写
            data["B"]["a"] = "123";
            parser.WriteFile(filePath, data);
        }

    }

}
