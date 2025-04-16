using System.Security.Cryptography.Xml;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DemoProgressBarAPI.Models.LinePay
{
    public class ResponseModel
    {
        [JsonPropertyName("returnCode")]
        public string ReturnCode { get; set; }

        [JsonPropertyName("returnMessage")]
        public string ReturnMessage { get; set; }

        [JsonPropertyName("info")]
        public Info Info { get; set; }
    }

    public class Info
    {
        [JsonPropertyName("paymentUrl")]
        public PaymentUrl PaymentUrl { get; set; }

        [JsonPropertyName("transactionId")]
        [JsonConverter(typeof(StringJsonConverter))]
        public string TransactionId { get; set; }

        [JsonPropertyName("paymentAccessToken")]
        public string PaymentAccessToken { get; set; }

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; }

        [JsonPropertyName("authorizationExpireDate")]
        public string AuthorizationExpireDate { get; set; }

        [JsonPropertyName("regKey")]
        public string RegKey { get; set; }

        [JsonPropertyName("payInfo")]
        public IEnumerable<PayInfo> PayInfo { get; set; }
        [JsonPropertyName("refundTransactionId")]
        public long? RefundTransactionId { get; set; }
        [JsonPropertyName("refundTransactionDate")]
        public DateTime? RefundTransactionDate { get; set; }
    }
    public class PayInfo
    {
        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonPropertyName("amount")]
        public int? Amount { get; set; }
    }
    public class PaymentUrl
    {
        [JsonPropertyName("web")]
        public string Web { get; set; }

        [JsonPropertyName("app")]
        public string App { get; set; }

        [JsonPropertyName("universal")]
        public string Universal { get; set; }
    }
    public class StringJsonConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt64().ToString();
            }
            return reader.GetString();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
