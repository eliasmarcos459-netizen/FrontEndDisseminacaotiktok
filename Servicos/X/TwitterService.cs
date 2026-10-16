using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;

namespace SMAD
{
    public class TwitterService
    {
        private readonly HttpClient _httpClient;

        // As suas 4 credenciais OAuth 1.0a User Context
        private const string ApiKey = "LLyorGJ4r1Pi2lZL577VUBOnk";
        private const string ApiSecret = "QQbQ8uc6XD0hlirpsNeRsf3Q51WaG2YBkasvx1PQQflyT3TCvj";
        private const string AccessToken = "2110348157842272256-tWTAmySvxP03t4K1GvlxBTeS7uFvkj";
        private const string AccessTokenSecret = "gDYis5S6kScZcUNy7gHK8NdfOElwdaS5HNFAaqPJB9h9Z";

        public TwitterService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> PublicarTweetAsync(string mensagem)
        {
            try
            {
                var url = "https://api.x.com/2/tweets";
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

                var jsonPayload = JsonSerializer.Serialize(new { text = mensagem });
                requestMessage.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Adicionar o cabeçalho de assinatura OAuth 1.0a estrito
                AddOAuthHeader(requestMessage, HttpMethod.Post, url);

                var response = await _httpClient.SendAsync(requestMessage);
                var responseString = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"RESPOSTA X (Status {response.StatusCode}): {responseString}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEÇÃO TWITTER: {ex.Message}");
                return false;
            }
        }

        private void AddOAuthHeader(HttpRequestMessage request, HttpMethod method, string url)
        {
            var oauthNonce = Convert.ToBase64String(Encoding.ASCII.GetBytes(Guid.NewGuid().ToString("N"))).Substring(0, 32);
            var oauthTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            var parameters = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", ApiKey },
                { "oauth_nonce", oauthNonce },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", oauthTimestamp },
                { "oauth_token", AccessToken },
                { "oauth_version", "1.0" }
            };

            var parameterString = string.Join("&", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
            var baseString = $"{method.Method.ToUpper()}&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(parameterString)}";
            var signingKey = $"{Uri.EscapeDataString(ApiSecret)}&{Uri.EscapeDataString(AccessTokenSecret)}";

            using var hmacsha1 = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey));
            var signatureBytes = hmacsha1.ComputeHash(Encoding.ASCII.GetBytes(baseString));
            var oauthSignature = Convert.ToBase64String(signatureBytes);

            parameters.Add("oauth_signature", oauthSignature);

            var formattedHeader = "OAuth " + string.Join(", ", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}=\"{Uri.EscapeDataString(p.Value)}\""));

            request.Headers.Remove("Authorization");
            request.Headers.Add("Authorization", formattedHeader);
        }
    }
}