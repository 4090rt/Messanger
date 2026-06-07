using System.Text.Json.Serialization;

namespace Messangers.ModelData
{
    public class AttachmentMetadata
    {
        public Int64 Id { get; set; }

        [JsonPropertyName("messageld")] // Обратите внимание: messageld (с маленькой L)
        public int? MessageId { get; set; }

        [JsonPropertyName("fileName")]
        public string FileName { get; set; }

        [JsonPropertyName("url")] // Сервер возвращает url, а не filePath
        public string FilePath { get; set; }

        [JsonPropertyName("fileSize")]
        public long FileSize { get; set; }

        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; }

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("UserOtpr")]
        public string User { get; set; }
    }
}
