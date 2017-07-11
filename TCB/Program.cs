using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace TCB
{
    class Program
    { 
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            ApiWorkerRole().Wait();
        }

        private static async Task ApiWorkerRole()
        {
            CircuitBreakerHttpClient client = new CircuitBreakerHttpClient(new CircuitBreaker());
            HttpClient clientNormal = new HttpClient();

            while (true)
            {
                try
                {
                    await Task.Delay(100);
                    var result = await client.GetAsync("http://localhost:5000");

                     
                    if (result.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Success");
                    }
                    else
                    {
                        Console.WriteLine(result.ReasonPhrase);
                    }
                }
                catch (Exception)
                {
                    
                }
            }
        }

    }
}