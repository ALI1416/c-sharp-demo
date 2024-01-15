using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleDemo.Test
{

    /// <summary>
    /// JSON测试
    /// </summary>
    [TestFixture]
    public class JsonTest
    {

        private static readonly string jsonString = @"{
        'a' : 123,
        'b' : 123456789012345,
        'c' : 456.78,
        'd' : '字符串',
        'e' : true,
        'f' : null,
        'g' : [
                -1,
                0,
                1
            ],
        'h' : {
            'i' : 'abc',
            'j' : 321
            }
        }";

        /// <summary>
        /// 测试
        /// </summary>
        [Test]
        public static void Test()
        {
            // 反序列化1
            JObject json = JsonConvert.DeserializeObject<JObject>(jsonString);
            Console.WriteLine(json);
            Console.WriteLine("a " + json.GetValue("a") + " " + json.GetValue("a").Type);
            Console.WriteLine("b " + json.GetValue("b") + " " + json.GetValue("b").Type);
            Console.WriteLine("c " + json.GetValue("c") + " " + json.GetValue("c").Type);
            Console.WriteLine("d " + json.GetValue("d") + " " + json.GetValue("d").Type);
            Console.WriteLine("e " + json.GetValue("e") + " " + json.GetValue("e").Type);
            Console.WriteLine("f " + json.GetValue("f") + " " + json.GetValue("f").Type);
            Console.WriteLine("g " + json.GetValue("g") + " " + json.GetValue("g").Type);
            Console.WriteLine("g[0] " + json.GetValue("g").ElementAt(0) + " " + json.GetValue("g").ElementAt(0).Type);
            Console.WriteLine("h " + json.GetValue("h") + " " + json.GetValue("h").Type);
            Console.WriteLine("h.i " + json.GetValue("h").SelectToken("i") + " " + json.GetValue("h").SelectToken("i").Type);
            // 反序列化2
            Console.WriteLine(JsonConvert.DeserializeObject<TestClass>(jsonString));
            // 序列化
            TestClass testClass = new TestClass()
            {
                A = 1,
                B = 234567890123456,
                C = 2.34,
                D = null,
                E = false,
                F = "aaa",
                G = new List<int> { 1, 2, 3 },
                H = new TestClass2()
                {
                    I = "我的字符串",
                    J = 654
                }
            };
            Console.WriteLine(JsonConvert.SerializeObject(testClass));
        }

        class TestClass
        {

            public int A { get; set; }
            public long B { get; set; }
            public double C { get; set; }
            public string D { get; set; }
            public bool E { get; set; }
            public string F { get; set; }
            public List<int> G { get; set; }
            public TestClass2 H { get; set; }

            public override string ToString()
            {
                return "TestClass2{" +
                    "A=" + A +
                    ", B=" + B +
                    ", C=" + C +
                    ", D=" + D +
                    ", E=" + E +
                    ", F=" + F +
                    ", G=" + IterateList(G) +
                    ", H=" + H +
                    "}";
            }

        }

        class TestClass2
        {

            public string I { get; set; }
            public int J { get; set; }

            public override string ToString()
            {
                return "TestClass2{" +
                    "I=" + I +
                    ", J=" + J +
                    "}";
            }

        }

        public static string IterateList(List<int> list)
        {
            StringBuilder sb = new StringBuilder("[");
            list.ForEach((int value) =>
            {
                sb.Append(value);
                sb.Append(", ");
            });
            sb.Remove(sb.Length - 2, 2);
            sb.Append("]");
            return sb.ToString();
        }

    }
}
