using Newtonsoft.Json.Linq;

namespace Bonsai.Code.Utils.Helpers
{
    /// <summary>
    /// Helper for working with JSON data.
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Removes all null properties and array items recursively.
        /// </summary>
        public static T RemoveEmptyChildren<T>(T token) where T: JToken
        {
            if (token.Type == JTokenType.Object)
            {
                var copy = new JObject();
                foreach (var prop in token.Children<JProperty>())
                {
                    var child = prop.Value;
                    if (child.HasValues)
                        child = RemoveEmptyChildren(child);

                    if (!IsEmpty(child))
                        copy.Add(prop.Name, child);
                }

                return copy as T;
            }

            if (token.Type == JTokenType.Array)
            {
                var copy = new JArray();
                foreach (var item in token.Children())
                {
                    var child = item;
                    if (child.HasValues)
                        child = RemoveEmptyChildren(child);

                    if (!IsEmpty(child))
                        copy.Add(child);
                }

                return copy as T;
            }

            return token;
        }

        /// <summary>
        /// Checks if the token is nullable.
        /// </summary>
        public static bool IsEmpty(JToken token)
        {
            return token.Type == JTokenType.Null;
        }
    }
}
