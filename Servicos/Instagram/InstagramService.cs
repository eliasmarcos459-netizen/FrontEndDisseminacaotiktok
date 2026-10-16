using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SMAD
{
    public class InstagramService
    {
        private readonly HttpClient _httpClient;
        private const string AccessToken = "EAAig4KnxCWgBSirJG6ZBEsRu4iHZBeqg9SZAZAcbzSBsVR2Pdurw102QZB9UQMiZCqt9u1x5DJTfJjvW71Fzn8pS3dj4TWBZBXntPeAEUY36dEjv41u0DT62inrlGlyymuudoR3UqD9hB480dI8yQL8UbzEYz3YwZCLEzULHuz8YpywN5oI1XNvLa79XsiZCX6BiJLGGI";
        private const string InstagramUserId = "17841479647866812";

        public InstagramService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> PublicarFotoAsync(string imageUrl, string legenda)
        {
            try
            {
                // Criar o contentor de mídia
                var containerUrl = $"https://graph.facebook.com/v21.0/{InstagramUserId}/media";

                var containerPayload = new Dictionary<string, string>
                {
                    { "image_url", imageUrl },
                    { "caption", legenda },
                    { "access_token", AccessToken }
                };

                var response = await _httpClient.PostAsync(containerUrl, new FormUrlEncodedContent(containerPayload));
                if (!response.IsSuccessStatusCode)
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"ERRO INSTAGRAM CONTAINER: {erro}");
                    return false;
                }

                var containerResult = await response.Content.ReadFromJsonAsync<IgContainerResponse>();
                var creationId = containerResult?.Id;

                if (string.IsNullOrEmpty(creationId))
                    return false;

                // PASSO 2: Publicar o contentor criado
                var publishUrl = $"https://graph.facebook.com/v21.0/{InstagramUserId}/media_publish";
                var publishPayload = new Dictionary<string, string>
                {
                    { "creation_id", creationId },
                    { "access_token", AccessToken }
                };

                var publishResponse = await _httpClient.PostAsync(publishUrl, new FormUrlEncodedContent(publishPayload));
                return publishResponse.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEÇÃO INSTAGRAM: {ex.Message}");
                return false;
            }
        }
    }

    public class IgContainerResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }
}