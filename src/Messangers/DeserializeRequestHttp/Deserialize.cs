using MessangersUI.Delegate;
using System.Text;
using System.Text.Json;

namespace Messangers.DeserializeRequestHttp
{
    public class Deserialize
    {
        private readonly ILogger<Deserialize> _logger;
        public Deserialize(ILogger<Deserialize> logger)
        {
            _logger = logger;
        }

        public async Task<List<T>> Parsing<T>(Stream stream)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };

                using var reader = new StreamReader(stream, Encoding.UTF8);
                var toend = await reader.ReadToEndAsync();

                var trimmed = toend.Trim();
                if (trimmed.StartsWith('['))
                {
                    var result = await JsonSerializer.DeserializeAsync<List<T>>(new MemoryStream(Encoding.UTF8.GetBytes(toend)), options);
                    if (result != null)
                    {
                        return result;
                    }
                    else
                    {
                        _logger.LogError("Данные после десериализации списка null");
                        return new List<T>();
                    }
                }
                else if (trimmed.StartsWith('{'))
                {
                    var result = await JsonSerializer.DeserializeAsync<T>(new MemoryStream(Encoding.UTF8.GetBytes(toend)), options);
                    if (result != null)
                    {
                        return new List<T> { result };
                    }
                    else
                    {
                        _logger.LogError("Данные после десериализации объекта null");
                        return new List<T>();
                    }
                }
                else
                {
                    _logger.LogError("Не удалось распознать формат");
                    return new List<T>();
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError("Возникло исключение при парсинге ответа" + ex.Message + ex.StackTrace);
                return new List<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение в классе десериализации" + ex.Message + ex.StackTrace);
                return new List<T>();
            }
        }
    }
}
