using System;
using System.Data;
using Dapper;
using Newtonsoft.Json;

namespace Bonsai.Code.Utils.Date
{
    public partial struct FuzzyDate
    {
        #region JsonConverter

        public class FuzzyDateJsonConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var value = reader.Value?.ToString();

                if(objectType == typeof(FuzzyDate?))
                    return FuzzyDate.TryParse(value);

                return FuzzyDate.Parse(value);
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(FuzzyDate)
                       || objectType == typeof(FuzzyDate?);
            }
        }

        #endregion

        #region TypeHandler for Dapper

        public class NullableFuzzyDateTypeHandler : SqlMapper.TypeHandler<FuzzyDate?>
        {
            public override void SetValue(IDbDataParameter parameter, FuzzyDate? value)
            {
                parameter.Value = value?.ToString();
            }

            public override FuzzyDate? Parse(object value)
            {
                return FuzzyDate.TryParse(value?.ToString());
            }
        }

        public class FuzzyDateTypeHandler : SqlMapper.TypeHandler<FuzzyDate>
        {
            public override void SetValue(IDbDataParameter parameter, FuzzyDate value)
            {
                parameter.Value = value.ToString();
            }

            public override FuzzyDate Parse(object value)
            {
                return FuzzyDate.Parse(value.ToString());
            }
        }

        #endregion
    }
}
