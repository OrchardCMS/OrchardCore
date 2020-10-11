using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Lucene
{
    public class LuceneQueryDTOConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var luceneQuery = new LuceneQueryDTO();
            var jsonObject = JObject.Load(reader);

            var propQuery = jsonObject.Properties().FirstOrDefault(prop => prop.Name == "query");

            if (propQuery != null)
            {
                luceneQuery.Query = jsonObject.Properties().FirstOrDefault(prop => prop.Name == "query").Value.ToString();
            }

            var propIndexName = jsonObject.Properties().FirstOrDefault(prop => prop.Name == "indexName");

            if (propIndexName != null)
            {
                luceneQuery.IndexName = jsonObject.Properties().FirstOrDefault(prop => prop.Name == "indexName").Value.ToString();
            }

            var propParameters = jsonObject.Properties().FirstOrDefault(i => i.Name == "parameters");

            if (propParameters != null)
            {
                luceneQuery.Parameters = jsonObject.Properties().FirstOrDefault(i => i.Name == "parameters").Value.ToString();
            }

            return luceneQuery;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LuceneQueryDTO);
        }


    }
}
