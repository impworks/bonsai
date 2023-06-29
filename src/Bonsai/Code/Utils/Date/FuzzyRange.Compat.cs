using System;
using Newtonsoft.Json;

namespace Bonsai.Code.Utils.Date
{
    public partial struct FuzzyRange
    {
        public class FuzzyRangeJsonConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var value = reader.Value?.ToString();

                if(objectType == typeof(FuzzyRange?))
                    return TryParse(value);

                return Parse(value);
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(FuzzyRange)
                       || objectType == typeof(FuzzyRange?);
            }
        }
    }
}
