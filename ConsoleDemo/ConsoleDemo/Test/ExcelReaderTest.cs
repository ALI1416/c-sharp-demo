using ConsoleDemo.Properties;
using ExcelDataReader;
using NUnit.Framework;
using System;
using System.IO;

namespace ConsoleDemo.Test
{

    /// <summary>
    /// Excel阅读器测试
    /// </summary>
    public class ExcelReaderTest
    {
        /// <summary>
        /// 测试
        /// </summary>
        [Test]
        public static void Test()
        {
            using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(new MemoryStream(Resources.data)))
            {
                // 工作表个数
                int shellCount = reader.ResultsCount;
                Console.WriteLine("工作表个数 " + shellCount);
                do
                {
                    // 工作表名称
                    string shellName = reader.Name;
                    // 行数
                    int rowCount = reader.RowCount;
                    // 列数
                    int colCount = reader.FieldCount;
                    Console.WriteLine("工作表名称 " + shellName + " 行数 " + rowCount + " 列数 " + colCount);
                    // 读取一行
                    while (reader.Read())
                    {
                        string rowData = "";
                        for (int i = 0; i < colCount; i++)
                        {
                            rowData += reader.GetString(i) + " ";
                        }
                        Console.WriteLine(rowData);
                    }
                    // 下一个工作表
                } while (reader.NextResult());
            }
        }

    }
}
