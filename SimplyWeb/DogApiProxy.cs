using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TCB;

namespace SimplyWeb
{
    public class DogApiProxy
    {
        private readonly CircuitBreakerHttpClient _httpClient;

        public DogApiProxy(CircuitBreaker circuitBreaker)
        {
            _httpClient = new CircuitBreakerHttpClient(circuitBreaker)
            {
                BaseAddress = new Uri("https://dog.ceo/api/")
            }; 
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GetAllBreeds()
        {
            return await _httpClient.GetStringAsync(new Uri("breeds/list/all", UriKind.Relative));
        }

        public async Task<string> GetRandomDogImageUrl()
        {
            return await _httpClient.GetStringAsync(new Uri("breeds/image/random", UriKind.Relative));
        }
        
        public async Task<string> GetRandomDogImageUrlForSpecificBreed(string breed)
        {
            return await _httpClient.GetStringAsync(new Uri($"breed/{breed}/images/random", UriKind.Relative));
        }
    }
}