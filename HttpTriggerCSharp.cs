using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;


namespace Company.Function
{
    public class MyProgram
    { 
        public static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect("yaronprredis.redis.cache.windows.net:6380,password=D3eUGdf+jxZNYBaZntbbmuIQn6Pj5WHzSDJBcKuzHio=,ssl=True,abortConnect=False");
        });        
    }
    
    public static class HttpTriggerCSharp
    {
        [FunctionName("HttpTriggerCSharp")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            IDatabase cache = MyProgram.lazyConnection.Value.GetDatabase();
            string cacheCommand = "PING";
            log.LogInformation("\nCache command  : " + cacheCommand);
            log.LogInformation("Cache response : " + cache.Execute(cacheCommand).ToString());

            cacheCommand = "GET Message";
            log.LogInformation("\nCache command  : " + cacheCommand + " or StringGet()");
            log.LogInformation("Cache response : " + cache.StringGet("Message").ToString());

            cacheCommand = "SET Message \"Hello! The cache is working from a .NET Core console app!\"";
            log.LogInformation("\nCache command  : " + cacheCommand + " or StringSet()");
            log.LogInformation("Cache response : " + cache.StringSet("Message", "Hello! The cache is working from a .NET Core console app!").ToString());

            cacheCommand = "GET Message";
            log.LogInformation("\nCache command  : " + cacheCommand + " or StringGet()");
            log.LogInformation("Cache response : " + cache.StringGet("Message").ToString());


            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
