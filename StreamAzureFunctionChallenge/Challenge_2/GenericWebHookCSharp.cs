using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Challenge_2
{
    public static class GenericWebHookCSharp
    {
        [FunctionName("DecipherWebHookCsharp")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info($"Webhook was triggered!");

            string jsonContent = await req.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<MessageJson>(jsonContent);
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < data.Msg.Length; i += 2)
            {
                var m = data.Msg.Substring(i, 2);
                log.Info(m);
                var key = data.Cipher.FirstOrDefault(ent => ent.Value.ToString() == m).Key;
                result.Append(key);
            }

            return req.CreateResponse(HttpStatusCode.OK, new
            {
                key = data.Key,
                result = result.ToString()
            });
        }
    }

    public class MessageJson
    {
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }
        [JsonProperty(PropertyName = "msg")]
        public string Msg { get; set; }
        [JsonProperty(PropertyName = "cipher")]
        public Dictionary<string, int> Cipher { get; set; }
    }
}