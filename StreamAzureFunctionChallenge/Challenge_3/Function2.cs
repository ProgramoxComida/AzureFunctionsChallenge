using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using Newtonsoft.Json;

namespace Challenge_3
{
    public static class Function2
    {
        [FunctionName("HttpTriggerCSharpSort")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req,
            [Table(tableName: "Challenge3Input", Connection = "AzureWebJobsStorage")] IQueryable<Challenge3Input> tableChallengeReaderBinding,
            TraceWriter log)
        {
            try
            {
                log.Info("C# HTTP trigger function processed a request.");

                string jsonContent = await req.Content.ReadAsStringAsync();
                dynamic data = JsonConvert.DeserializeObject(jsonContent);

                var key = $"{data.key}";
                var result = tableChallengeReaderBinding.Where(ent => ent.PartitionKey == key).FirstOrDefault();
                if (result == null)
                {
                    throw new Exception("");
                }
                int[] ia = result.ArrayOfValues.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                Array.Sort(ia);
                var response = new Challenge3Message() { key = key, ArrayOfValues = ia };
                // parse query parameter
                return req.CreateResponse(HttpStatusCode.OK, response);
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
            public string key { get; set; }
            public int[] ArrayOfValues { get; set; }
        }

        public class Challenge3Input : TableEntity
        {
            public string ArrayOfValues { get; set; }
        }
    }
}