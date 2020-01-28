using System;
using System.Data;
using Dapper;
using Newtonsoft.Json;

namespace Bonsai.Code.Utils.Date
{
    public partial struct FuzzyRange
    {
        #region JsonConverter

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
                    return FuzzyRange.TryParse(value);

                return FuzzyRange.Parse(value);
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(FuzzyRange)
                       || objectType == typeof(FuzzyRange?);
            }
        }

        #endregion

        #region TypeHandler for Dapper

        public class NullableFuzzyRangeTypeHandler : SqlMapper.TypeHandler<FuzzyRange?>
        {
            public override void SetValue(IDbDataParameter parameter, FuzzyRange? value)
            {
                parameter.Value = value?.ToString();
            }

            public override FuzzyRange? Parse(object value)
            {
                return FuzzyRange.TryParse(value?.ToString());
            }
        }

        public class FuzzyRangeTypeHandler : SqlMapper.TypeHandler<FuzzyRange>
        {
            public override void SetValue(IDbDataParameter parameter, FuzzyRange value)
            {
                parameter.Value = value.ToString();
            }

            public override FuzzyRange Parse(object value)
            {
                return FuzzyRange.Parse(value.ToString());
            }
        }

        #endregion
    }
}
