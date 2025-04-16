using AngleSharp.Dom;
using DemoProgressBarAPI.Models.Enums.LinePay;
using DemoProgressBarAPI.Models.LinePay;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DemoProgressBarAPI.Services
{
    public class LinePayService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://sandbox-api-pay.line.me";

        private readonly string _channelId; // 替換為您的 Channel ID
        private readonly string _channelSecret; // 替換為您的 Channel Secret

        private readonly JsonSerializerOptions _jsonoptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };

        public LinePayService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("X-LINE-ChannelId", _channelId);
            _channelId = configuration["LinaPay:ChannelID"];
            _channelSecret = configuration["LinaPay:ChannelSecret"];
        }

        public async Task<ResponseModel> RequestPaymentAsync(int amount, string currency, string orderId, string confirmUrl)
        {
            RequestPaymentModel requestBody = new RequestPaymentModel
            {
                Amount = amount,
                Currency = currency,
                OrderId = orderId,
                Packages = new Package[]
                {
                    new Package
                    {
                        Id = "1",
                        Amount = amount,
                        Name = "Demo Package",
                        Products = new Product[]
                        {
                            new Product
                            {
                                Id = "P001",
                                Name = "Demo Product",
                                Quantity = 1,
                                Price = amount
                            }
                        }
                    }
                },
                RedirectUrls = new RedirectUrls
                {
                    ConfirmUrlType = ConfirmUrlTypeEnum.NONE.ToString()
                },
            };
            string uri = "v3/payments/request";
            return await CallLineAPI<RequestPaymentModel>(uri, requestBody);
        }

        public async Task<ResponseModel> ConfirmAsync(int amount, string currency, string transactionId)
        {
            RequestPaymentModel requestBody = new RequestPaymentModel
            {
                Amount = amount,
                Currency = currency
            };
            string uri =$"v3/payments/{transactionId}/confirm";
            return await CallLineAPI<RequestPaymentModel>(uri, requestBody);
        }

        private string GenerateSignature(string uri, string body, string nonce)
        {
            // 組合簽名字串
            string signatureString = $"{_channelSecret}{uri}{body}{nonce}";

            // 使用 Channel Secret 作為密鑰進行 HMAC-SHA256 簽名
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_channelSecret));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureString));

            // 將簽名結果轉換為 Base64 字串
            return Convert.ToBase64String(hash);
        }

        private async Task<ResponseModel> CallLineAPI<T1>(string uri,T1 data)
        {
            // 將請求 Body 序列化為 JSON
            string requestBodyJson = JsonSerializer.Serialize(data, _jsonoptions);

            // 生成 X-LINE-Authorization-Nonce
            string nonce = Guid.NewGuid().ToString();
            Url url = new Url(uri, BaseUrl);
            // 生成 X-LINE-Authorization
            string signature = GenerateSignature(url.PathName, requestBodyJson, nonce);

            // 添加標頭
            _httpClient.DefaultRequestHeaders.Add("X-LINE-Authorization-Nonce", nonce);
            _httpClient.DefaultRequestHeaders.Add("X-LINE-Authorization", signature);

            // 發送請求
            var response = await _httpClient.PostAsync(url.Href, new StringContent(requestBodyJson, Encoding.UTF8, "application/json"));
            string responseData = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
               return JsonSerializer.Deserialize<ResponseModel>(responseData, _jsonoptions);
            }
            else
                throw new HttpRequestException($"Line Pay API Error: {responseData}");
        }
    }
}
