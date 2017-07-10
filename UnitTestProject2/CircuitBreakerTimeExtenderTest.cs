using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TCB;
using UnitTestProject1;

namespace UnitTestProject2
{
    [TestClass]
    public class CircuitBreakerTimeExtenderTest
    {

        [ClassInitialize]
        public static void Setup(TestContext context)
        {

        }

        [ClassCleanup]
        public static void Teardown()
        {

        }



        [TestInitialize]
        public void SetupSingleTest()
        {

        }

        [TestCleanup]
        public void TeardownSingleTest()
        {

        }

        [TestMethod]
        public async Task TestGetStringAsync()
        {
            CircuitBreakerConfig config = new CircuitBreakerConfig();

            CircuitBreaker cb = new CircuitBreaker()
            {
                
            };

            TimeToAllowCall ttac = new TimeToAllowCall(null)
            {
                ExtendTimeFunc = i => 2^i,
                InitialExtend = 5,
                MaxExtend = 15,

            };

            Assert.Fail();

        }




    }
}