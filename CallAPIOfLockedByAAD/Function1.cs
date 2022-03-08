using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Services.AppAuthentication;
using System.Net.Http;

namespace CallAPIOfLockedByAAD
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name ??= data?.name;
            string responseMessage;

            try {
                const string audienceId = "hogehoge"; // �A�N�Z�X�Ώہifunc-cs02��������func-j02�jAPI��Application ID
                string accessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync(audienceId);
                HttpClient httpClient = new();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage ResponseMessage = await httpClient.GetAsync("https://ad-linkage-40017-api.azurewebsites.net/WeatherForecast");
                responseMessage = await ResponseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";
            } 
            return new OkObjectResult(responseMessage);
        }
    }
}
