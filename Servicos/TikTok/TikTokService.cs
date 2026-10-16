using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Forms;

namespace SMAD
{
    public class TikTokService
    {
        private readonly HttpClient _httpClient;
        private readonly TikTokToken _tokenStore;

        public TikTokService(HttpClient httpClient, TikTokToken tokenStore)
        {
            _httpClient = httpClient;
            _tokenStore = tokenStore;
        }

        public async Task<bool> PublicarVideoAsync(string descricao, IBrowserFile ficheiroVideo)
        {
            if (!_tokenStore.TemTokenValido)
            {
                System.Diagnostics.Debug.WriteLine("ERRO TIKTOK: Token inválido ou não autenticado via OAuth.");
                return false;
            }

            var accessToken = _tokenStore.AccessToken;

            try
            {
                // Inicializar a publicação de vídeo (Inbox/Draft para contornar restrições de sandbox)
                var initUrl = "https://open.tiktokapis.com/v2/post/publish/inbox/video/init/";

                var payload = new
                {
                    post_info = new
                    {
                        title = descricao,
                        privacy_level = "SELF_ONLY",
                        disable_duet = false,
                        disable_comment = false,
                        disable_stitch = false,
                        video_cover_timestamp_ms = 1000
                    },
                    source_info = new
                    {
                        source = "FILE_UPLOAD",
                        video_size = ficheiroVideo.Size,
                        chunk_size = ficheiroVideo.Size,
                        total_chunk_count = 1
                    }
                };

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, initUrl)
                {
                    Content = JsonContent.Create(payload)
                };
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(requestMessage);
                if (!response.IsSuccessStatusCode)
                {
                    var erroDetalhe = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"ERRO TIKTOK INIT COMPLETO: {erroDetalhe}");
                    return false;
                }

                var resultado = await response.Content.ReadFromJsonAsync<TikTokInitResponse>();
                var publishId = resultado?.Data?.PublishId;
                var uploadUrl = resultado?.Data?.UploadUrl;

                if (string.IsNullOrEmpty(publishId) || string.IsNullOrEmpty(uploadUrl))
                {
                    return false;
                }

                // Enviar o conteúdo binário via PUT (SEM o Content-Length manual para evitar exceção)
                using var stream = ficheiroVideo.OpenReadStream(maxAllowedSize: 104857600); // 100MB
                using var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(ficheiroVideo.ContentType);
                streamContent.Headers.ContentRange = new ContentRangeHeaderValue(0, ficheiroVideo.Size - 1, ficheiroVideo.Size);

                var putRequest = new HttpRequestMessage(HttpMethod.Put, uploadUrl)
                {
                    Content = streamContent
                };

                var uploadResponse = await _httpClient.SendAsync(putRequest);
                if (!uploadResponse.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("ERRO AO ENVIAR O BINÁRIO DO VÍDEO PARA O TIKTOK.");
                    return false;
                }

                // PASSO 3: Polling de confirmação
                return await AguardarProcessamentoAsync(publishId, accessToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEÇÃO TIKTOK: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> AguardarProcessamentoAsync(string publishId, string accessToken)
        {
            var statusUrl = "https://open.tiktokapis.com/v2/post/publish/status/fetch/";
            int tentativas = 0;

            while (tentativas < 20)
            {
                await Task.Delay(6000);

                var payload = new { publish_id = publishId };
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, statusUrl)
                {
                    Content = JsonContent.Create(payload)
                };
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var statusRes = await response.Content.ReadFromJsonAsync<TikTokStatusResponse>();
                    var status = statusRes?.Data?.Status;

                    // Aceita ambos os estados como sucesso garantido
                    if (status == "PUBLISH_COMPLETE" || status == "SEND_TO_USER_INBOX") return true;
                    if (status == "FAILED" || status == "REJECTED") return false;
                }

                tentativas++;
            }

            return false;
        }
    }

    public class TikTokToken
    {
        public string? AccessToken { get; set; } = "act.wf9xMEitlIdFnZFqHrdtf0Ft6aNw0jZJZ2i1m6CSsKGTyIOdRO9SCXUq8tUd!4376.s1";
        public string? RefreshToken { get; set; } = "rft.16RdRR966H6wvnWxDOOJRxI1APtnZshgJiAvxBKJhPRA8JUVq7Kz3ImELXgf!4414.s1";
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(30);

        public bool TemTokenValido => !string.IsNullOrEmpty(AccessToken) && AccessToken != "SEU_ACCESS_TOKEN_AQUI";
    }

    public class TikTokInitResponse
    {
        [JsonPropertyName("data")]
        public TikTokData? Data { get; set; }
    }

    public class TikTokData
    {
        [JsonPropertyName("publish_id")]
        public string? PublishId { get; set; }

        [JsonPropertyName("upload_url")]
        public string? UploadUrl { get; set; }
    }

    public class TikTokStatusResponse
    {
        [JsonPropertyName("data")]
        public TikTokStatusData? Data { get; set; }
    }

    public class TikTokStatusData
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}