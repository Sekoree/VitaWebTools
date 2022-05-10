using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VitaWebTools.Entities.PSN;

namespace VitaWebTools
{
    public class Utilities
    {
        private readonly ILogger<Utilities> _logger;
        private static string TokenUrl { get; } = "https://auth.api.sonyentertainmentnetwork.com/2.0/oauth/token";

        private HttpClient _client { get; } = new();

        public Utilities(ILogger<Utilities> logger)
        {
            _logger = logger;
            var byteArray = Encoding.ASCII.GetBytes("ba495a24-818c-472b-b12d-ff231c1b5745:mvaiZkRsAsI1IBkY");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        #region PSN Code Grant

        private async Task<TokenResponse?> GetTokenResponse(string codeUrl)
        {
            if (!codeUrl.Contains("code"))
            {
                _logger.LogError("\"codeUrl\" does not contain auth code!\nInput given: {0}", codeUrl);
                return default;
            }

            var code = codeUrl.Split('=')[1].Split('&')[0];

            using var request = new HttpRequestMessage(HttpMethod.Post, TokenUrl);
            var byteArray = Encoding.ASCII.GetBytes("ba495a24-818c-472b-b12d-ff231c1b5745:mvaiZkRsAsI1IBkY");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            request.Content = new StringContent($"grant_type=authorization_code&code={code}&redirect_uri=https://remoteplay.dl.playstation.net/remoteplay/redirect&", Encoding.ASCII, "application/x-www-form-urlencoded");

            var tokenResponse = await _client.SendAsync(request);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Requesting token was unsuccessful!\nResult code: {0}", tokenResponse.StatusCode);
                return default;
            }

            var json = await tokenResponse.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<TokenResponse>(json);
            return token;
        }

        private async Task<UserInfoResponse?> GetUserInfoResponse(TokenResponse? tokenResponse)
        {
            if (tokenResponse == default)
            {
                _logger.LogError("\"tokenResponse\" was null");
                return default;
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, TokenUrl + "/" + tokenResponse.AccessToken);
            var byteArray = Encoding.ASCII.GetBytes("ba495a24-818c-472b-b12d-ff231c1b5745:mvaiZkRsAsI1IBkY");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            var userResponseMessage = await _client.SendAsync(request);

            if (!userResponseMessage.IsSuccessStatusCode)
            {
                _logger.LogError("Requesting user info was unsuccessful!\nResult code: {0}", userResponseMessage.StatusCode);
                return default;
            }

            var json = await userResponseMessage.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<UserInfoResponse>(json);
            return userInfo;
        }

        private string? GetAID(UserInfoResponse userInfoResponse)
        {
            if (userInfoResponse.UserId == null)
            {
                _logger.LogError("\"userInfoResponse\" was null");
                return default;
            }

            var idBytes = BitConverter.GetBytes(ulong.Parse(userInfoResponse.UserId));

            var idString = Convert.ToBase64String(idBytes);

            var resolvedIdStringBytes = Convert.FromBase64String(idString);

            var resolvedString = BitConverter.ToString(resolvedIdStringBytes);

            return resolvedString.Replace("-", "").ToLower();

        }

        public async Task<string?> GetUserAID(string codeUrl)
        {
            var token = await GetTokenResponse(codeUrl);
            if (token == null)
                return default;

            var info = await GetUserInfoResponse(token);
            if (info == null)
                return default;

            var aid = GetAID(info);
            if (aid == null)
                return default;

            _logger.LogInformation("Successfully retrieved user AID: {0}", aid);
            return aid;
        }

        #endregion

        #region CMA key Gen

        //Taken from Chovy-Sign https://github.com/KuromeSan/chovy-sign
        //https://github.com/KuromeSan/chovy-sign/blob/master/CHOVY-SIGN/cmakeys.cs
        private static readonly byte[] _passphrase = Encoding.ASCII.GetBytes("Sri Jayewardenepura Kotte");
        private static readonly byte[] _key = { 0xA9, 0xFA, 0x5A, 0x62, 0x79, 0x9F, 0xCC, 0x4C, 0x72, 0x6B, 0x4E, 0x2C, 0xE3, 0x50, 0x6D, 0x38 };

        public string? GenerateCmaKey(string aid)
        {
            try
            {
                var asLong = Convert.ToInt64(aid, 16);
                var aidBytes = BitConverter.GetBytes(asLong);
                Array.Reverse(aidBytes);

                var keyBytes = GenerateKey(aidBytes);

                var cmaKey = BitConverter.ToString(keyBytes).Replace("-", "");

                _logger.LogInformation("Successfully generated CMA key for AID {0}\nCMA key: {1}", aid, cmaKey);
                return cmaKey;
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to generate CMA key for AID: {0}\nGot Exception: {1}", aid, ex);
                return default;
            }
        }
        private byte[] GenerateKey(byte[] aid)
        {
            using var ms = new MemoryStream();
            ms.Write(aid, 0, aid.Length);
            ms.Write(_passphrase, 0, _passphrase.Length);
            var keyBytes = ms.ToArray();
            ms.Dispose();

            using var sha = SHA256.Create();
            keyBytes = sha.ComputeHash(keyBytes);
            sha.Dispose();

            keyBytes = Decrypt(keyBytes, _key);

            return keyBytes;
        }

        private static byte[] Decrypt(byte[] cipherData, byte[] key)
        {
            using var ms = new MemoryStream();
            var alg = Aes.Create();
            alg.Mode = CipherMode.ECB;
            alg.Padding = PaddingMode.None;
            alg.KeySize = 128;
            alg.Key = key;
            using var cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();

            var decryptedData = ms.ToArray();

            return decryptedData;
        }

        #endregion
    }
}
