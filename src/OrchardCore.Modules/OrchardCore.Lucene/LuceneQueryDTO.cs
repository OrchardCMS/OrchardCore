using Newtonsoft.Json;

namespace OrchardCore.Lucene
{

   [JsonConverter(typeof(LuceneQueryDTOConverter))]
   public class LuceneQueryDTO
   {
      public string IndexName { set; get; }
      public string Query { set; get; }
      public string Parameters { set; get; }
   }
}
