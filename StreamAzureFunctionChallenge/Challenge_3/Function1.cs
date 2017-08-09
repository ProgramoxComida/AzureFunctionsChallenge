using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Challenge_3
{
    public static class Function1
    {
        [FunctionName("HttpTriggerCSharpStorage")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req,
            [Table(tableName: "Challenge3Input", Connection = "AzureWebJobsStorage")] IQueryable<Challenge3Input> tableChallengeReaderBinding,
            [Table(tableName: "Challenge3Input", Connection = "AzureWebJobsStorage")] ICollector<Challenge3Input> tableChallengeWriterBinding,
            TraceWriter log)
        {
            try
            {
                // Get request body
                log.Info($"Received message to trigger");
                string jsonContent = await req.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<Challenge3Message>(jsonContent);
                // Check if dont exist duplicate records
                var len = tableChallengeReaderBinding.Where(ent => ent.ChallengeKey == data.Key).ToList().Count;
                if (len > 0)
                {
                    throw new Exception($"This key {data.Key} exist in the Queue");
                }
                // Add the new record
                var record = new Challenge3Input
                {
                    PartitionKey = data.Key,
                    RowKey = Guid.NewGuid().ToString(),
                    ChallengeKey = data.Key,
                    ArrayOfValues = string.Join(",", data.ArrayOfValues)
                };
                tableChallengeWriterBinding.Add(record);


                return req.CreateResponse(HttpStatusCode.OK, new { });
            }
            catch (Exception ex)
            {
                log.Info(ex.Message);
                return req.CreateResponse(HttpStatusCode.InternalServerError, "An error occured. Please notify the Code Challenge Team. functionschallenge@microsoft.com");
            }
        }

        // represents request msg going to  users challenge3 postback function
        class Challenge3Message
        {
            [JsonProperty(PropertyName = "key")]
            public string Key { get; set; }
            [JsonProperty(PropertyName = "ArrayOfValues")]
            public int[] ArrayOfValues { get; set; }
        }
        public class Challenge3Input : TableEntity
        {
            public string ChallengeKey { get; set; }
            public string ArrayOfValues { get; set; }
        }
    }
}