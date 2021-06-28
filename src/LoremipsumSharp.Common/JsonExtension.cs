using Newtonsoft.Json;

namespace LoremipsumSharp.Common
{
public static class JsonConvertExtensions
    {
        public static bool TryDeserializeObject<T>(this string value, out T model, JsonSerializerSettings jsonSerializerSettings = null)
        {
            model = default(T);
            try
            {
                model = JsonConvert.DeserializeObject<T>(value, jsonSerializerSettings);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool TrySerializeObject<T>(this T value, out string model, JsonSerializerSettings jsonSerializerSettings = null)
        {
            model = default;
            try
            {
                model = JsonConvert.SerializeObject(value, jsonSerializerSettings);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}