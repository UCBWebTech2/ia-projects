using Newtonsoft.Json;
using PFEstudioJuridico.DTOs;

namespace PFEstudioJuridico.Services
{
    public class CategoriaApiService
    {
        private readonly HttpClient _httpClient;

        public CategoriaApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CategoriaDTO>> ObtenerCategoriasAsync()
        {
            var response = await _httpClient.GetAsync("http://tuapi.com/api/categoria/lista");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<CategoriaDTO>>(content)
                ??new List<CategoriaDTO>();
        }
    }
}
