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
    public class SameResponseTests
    {
        public static NancyTestServer TestServer;
        public static string Url;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            TestServer = new NancyTestServer();
            TestServer.Start();
            Url = "http://localhost:5000/";
        }

        [ClassCleanup]
        public static void Teardown()
        {
            TestServer.Stop();
        }

        private CircuitBreakerHttpClient _circuitBreakerHttpClient;
        private HttpClient _httpClient;

        [TestInitialize]
        public void SetupSingleTest()
        {
            var config = new CircuitBreakerConfig();
            var timerConfig = new CircuitBreakerTimerConfig();
            _circuitBreakerHttpClient = new CircuitBreakerHttpClient(new CircuitBreaker(config, timerConfig));
            _httpClient = new HttpClient();
        }

        [TestCleanup]
        public void TeardownSingleTest()
        {
            _circuitBreakerHttpClient = null;
            _httpClient = null;
        }

        [TestMethod]
        public async Task TestGetStreamAsync()
        {
            var httpClientResult = await _httpClient.GetStreamAsync(Url);
            var circuitBreakerResult = await _circuitBreakerHttpClient.GetStreamAsync(Url);

            var httpString = await new StreamReader(httpClientResult).ReadToEndAsync();
            var circuitString = await new StreamReader(circuitBreakerResult).ReadToEndAsync();

            Assert.AreEqual(httpString, circuitString);
        }

        [TestMethod]
        public async Task TestGetStringAsync()
        {
            var httpClientResult = await _httpClient.GetStringAsync(Url);
            var circuitBreakerResult = await _circuitBreakerHttpClient.GetStringAsync(Url);

            Assert.AreEqual(httpClientResult, circuitBreakerResult);
        }

        [TestMethod]
        public async Task TestSendAsync()
        {
            var httpClientResult = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, Url));
            var circuitBreakerResult = await _circuitBreakerHttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, Url));

            var httpString = await httpClientResult.Content.ReadAsStringAsync();
            var circuitString = await circuitBreakerResult.Content.ReadAsStringAsync();

            Assert.AreEqual(httpString, circuitString);
        }

        [TestMethod]
        public async Task TestGetStringAsyncConflict()
        {
            Exception httpClientException = new Exception("http");
            Exception circuitBreakerException = new Exception("circuit");

            try
            {
                await _httpClient.GetStringAsync(Url + "badrequest/");
            }
            catch (Exception e)
            {
                httpClientException = e;
            }

            try
            {
                await _circuitBreakerHttpClient.GetStringAsync(Url + "badrequest/");
            }
            catch (Exception e)
            {
                circuitBreakerException = e;
            }

            Assert.AreEqual(httpClientException.Source, circuitBreakerException.Source);
            Assert.AreEqual(httpClientException.Message, circuitBreakerException.Message);
        }
    }
}
