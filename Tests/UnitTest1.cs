using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            if (false)
            {
                Console.WriteLine();
            }
            Assert.AreEqual(0, 1);
        }
    }
}
