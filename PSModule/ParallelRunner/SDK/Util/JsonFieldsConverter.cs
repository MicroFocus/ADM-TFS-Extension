using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;

namespace PSModule.ParallelRunner.SDK.Util
{
    public class JsonFieldsConverter : JsonPathConverter
    {
        public override object ReadJson(JsonReader reader, Type objType, object existingValue, JsonSerializer serializer)
        {
            JObject jo;
            try
            {
                jo = JObject.Load(reader);
            }
            catch
            {
                return null;
            }
            object targetObj = existingValue ?? Activator.CreateInstance(objType);

            foreach (var field in objType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                var pathAttr = field.GetCustomAttributes(true).OfType<JsonPropertyAttribute>().FirstOrDefault();

                string jsonPath = pathAttr?.PropertyName ?? field.Name;
                var token = jo.SelectToken(jsonPath);

                if (token != null && token.Type != JTokenType.Null)
                {
                    object value = token.ToObject(field.FieldType, serializer);
                    field.SetValue(targetObj, value);
                }
            }

            return targetObj;
        }

        /// <remarks>
        /// CanConvert is not called when <see cref="JsonConverterAttribute">JsonConverterAttribute</see> is used.
        /// </remarks>
        public override bool CanConvert(Type objectType)
        {
            return objectType.GetCustomAttributes(true).OfType<JsonFieldsConverter>().Any();
        }
    }
}
