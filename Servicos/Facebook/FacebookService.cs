using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SMAD
{
    public class FacebookService
    {
        private readonly HttpClient _httpClient;
        private const string PageId = "1307627695775240";
        private const string PageAccessToken = "EAAig4KnxCWgBSirJG6ZBEsRu4iHZBeqg9SZAZAcbzSBsVR2Pdurw102QZB9UQMiZCqt9u1x5DJTfJjvW71Fzn8pS3dj4TWBZBXntPeAEUY36dEjv41u0DT62inrlGlyymuudoR3UqD9hB480dI8yQL8UbzEYz3YwZCLEzULHuz8YpywN5oI1XNvLa79XsiZCX6BiJLGGI";

        public FacebookService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> PublicarNaPaginaAsync(string mensagem)
        {
            try
            {
                var url = $"https://graph.facebook.com/v21.0/{PageId}/feed";

                var payload = new Dictionary<string, string>
                {
                    { "message", mensagem },
                    { "access_token", PageAccessToken }
                };

                var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(payload));

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"ERRO DA GRAPH API: {errorContent}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEÇÃO: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> PublicarNaPaginaComMidiaAsync(string mensagem, IReadOnlyList<IBrowserFile> ficheiros)
        {
            try
            {
                if (ficheiros == null || !ficheiros.Any())
                {
                    return await PublicarNaPaginaAsync(mensagem);
                }

                // Apenas 1 imagem selecionada (Publicação normal de foto com legenda)
                if (ficheiros.Count == 1)
                {
                    var ficheiro = ficheiros.First();
                    var url = $"https://graph.facebook.com/v21.0/{PageId}/photos";
                    using var content = new MultipartFormDataContent();

                    if (!string.IsNullOrWhiteSpace(mensagem))
                    {
                        content.Add(new StringContent(mensagem), "message");
                    }

                    content.Add(new StringContent(PageAccessToken), "access_token");

                    using var stream = ficheiro.OpenReadStream(maxAllowedSize: 15728640); // 15MB max
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(ficheiro.ContentType);
                    content.Add(fileContent, "source", ficheiro.Name);

                    var response = await _httpClient.PostAsync(url, content);
                    return response.IsSuccessStatusCode;
                }

                // Várias imagens selecionadas (Criar Álbum / Post Único Consolidado)
                var mediaIds = new List<string>();

                foreach (var ficheiro in ficheiros)
                {
                    var uploadUrl = $"https://graph.facebook.com/v21.0/{PageId}/photos";
                    using var multiContent = new MultipartFormDataContent();

                    multiContent.Add(new StringContent("false"), "published"); //guarda como rascunho para não sujar o feed individualmente
                    multiContent.Add(new StringContent(PageAccessToken), "access_token");

                    using var stream = ficheiro.OpenReadStream(maxAllowedSize: 15728640);
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(ficheiro.ContentType);
                    multiContent.Add(fileContent, "source", ficheiro.Name);

                    var uploadResponse = await _httpClient.PostAsync(uploadUrl, multiContent);
                    if (!uploadResponse.IsSuccessStatusCode)
                    {
                        var err = await uploadResponse.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"ERRO AO FAZER UPLOAD DE FOTO PARA O ÁLBUM: {err}");
                        return false;
                    }

                    var jsonResult = await uploadResponse.Content.ReadFromJsonAsync<FbPhotoResponse>();
                    if (jsonResult != null && !string.IsNullOrEmpty(jsonResult.Id))
                    {
                        mediaIds.Add(jsonResult.Id);
                    }
                }

                // Se conseguimos fazer upload de todas, criamos o post final agrupado no feed
                if (mediaIds.Any())
                {
                    var feedUrl = $"https://graph.facebook.com/v21.0/{PageId}/feed";
                    var feedPayload = new Dictionary<string, string>
                    {
                        { "message", mensagem ?? string.Empty },
                        { "access_token", PageAccessToken }
                    };

                    // Adiciona cada ID de mídia mapeado ao parâmetro attached_media da Graph API
                    for (int i = 0; i < mediaIds.Count; i++)
                    {
                        feedPayload.Add($"attached_media[{i}]", $"{{\"media_fbid\":\"{mediaIds[i]}\"}}");
                    }

                    var feedResponse = await _httpClient.PostAsync(feedUrl, new FormUrlEncodedContent(feedPayload));
                    return feedResponse.IsSuccessStatusCode;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEÇÃO MIDIA MULTIPLA: {ex.Message}");
                return false;
            }
        }
    }

    public class FbPhotoResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }
}