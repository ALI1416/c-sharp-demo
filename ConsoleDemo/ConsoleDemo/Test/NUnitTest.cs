using NUnit.Framework;
using System;

namespace ConsoleDemo.Test
{

    /// <summary>
    /// NUnit测试
    /// 添加NuGet包`NUnit`和`NUnit3TestAdapter`
    /// [TestFixture]标识测试类
    /// </summary>
    [TestFixture]
    public class NUnitTest
    {

        /// <summary>
        /// 无参测试
        /// [Test]标识无参测试方法
        /// </summary>
        [Test]
        public void Test01ParameterlessFunction()
        {
            Console.WriteLine("无参测试");
        }

        /// <summary>
        /// 无结果含参测试
        /// [TestCase()]标识无结果含参方法
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <param name="c">a+b</param>
        [TestCase(1, 2, 3)]
        [TestCase(2, 3, 5)]
        [TestCase(3, 4, 7)]
        public void Test02ParameterFunction(int a, int b, int c)
        {
            // 断言(预期结果,实际结果)
            Assert.That(c, Is.EqualTo(a + b));
        }

        /// <summary>
        /// 有结果含参测试
        /// [TestCase(ExpectedResult)]标识无结果含参方法
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        [TestCase(1, 2, ExpectedResult = 3)]
        [TestCase(2, 3, ExpectedResult = 5)]
        [TestCase(3, 4, ExpectedResult = 7)]
        public int Test03ParameterFunction(int a, int b)
        {
            return a + b;
        }

        static readonly int[][] addCase =
        {
            new int[] {1,2,3},
            new int[] {2, 3, 5},
            new int[] {3, 4, 7},
        };

        /// <summary>
        /// 来源测试
        /// [TestCaseSource()]标识测试源
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <param name="c">a+b</param>
        [TestCaseSource(nameof(addCase))]
        public void Test04Source(int a, int b, int c)
        {
            Assert.That(c, Is.EqualTo(a + b));
        }

        /// <summary>
        /// 测试用例前执行
        /// </summary>
        [SetUp]
        public void TestSetup()
        {
            Console.WriteLine("前");
        }

        /// <summary>
        /// 测试用例后执行
        /// </summary>
        [TearDown]
        public void TestTearDown()
        {
            Console.WriteLine("后");
        }

        /// <summary>
        /// 忽略此测试
        /// </summary>
        [Ignore("忽略")]
        [Test]
        public void TestIgnore()
        {
            Console.WriteLine("忽略");
        }

    }
}
