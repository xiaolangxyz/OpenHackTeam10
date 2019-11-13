using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace GetRating
{
    public static class CreateRating
    {
        [FunctionName("CreateRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string userId = req.Query["userId"];
            string productId = req.Query["productId"];
            string locationName = req.Query["locationName"];
            string rating = req.Query["rating"];
            string userNotes = req.Query["userNotes"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            userId = userId ?? data?.userId;
            productId = productId ?? data?.productId;
            locationName = locationName ?? data?.locationName;
            rating = rating ?? data?.rating;
            userNotes = userNotes ?? data?.userNotes;

            var userIdCheck = await CheckUserId(userId);
            var productIdCheck = await CheckProductId(productId);
            bool ratingCheck = false;
            int test = int.Parse(rating);
            if (0 <= int.Parse(rating) && int.Parse(rating) <= 5)
            {
                ratingCheck = true;
            }

            Guid guidValue = Guid.NewGuid();
            DateTime dateTime = DateTime.UtcNow;

            // TODOFConnect DB

            // TODOFCreate Json


            // Error Message
            string errorMessage = null;
            if (!userIdCheck)
            {
                errorMessage += "Incorrect userId";
            }
            if (!productIdCheck)
            {
                errorMessage += " Incorrect productId";
            }
            if (!ratingCheck)
            {
                errorMessage += " Incorect rating";
            }


            return userIdCheck && productIdCheck && ratingCheck
                ? (ActionResult)new OkObjectResult($"Succeed. TODO JSON")
                : new BadRequestObjectResult($"Error:{errorMessage}");
        }

        private async static Task<bool> CheckUserId(string userid)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            using (var client = new HttpClient())
            {
                response = await client.GetAsync($"https://serverlessohuser.trafficmanager.net/api/GetUser?userid={userid}");


            }
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return false;
            }
            var json = await response.Content.ReadAsStringAsync();
            var jobj = JObject.Parse(json);
            JValue userIdValue = (JValue)jobj["userId"];
            string uId = (string)userIdValue.Value;

            return true;
        }

        private async static Task<bool> CheckProductId(string productid)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            using (var client = new HttpClient())
            {
                response = await client.GetAsync($"https://serverlessohproduct.trafficmanager.net/api/GetProduct?productid={productid}");


            }
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return false;
            }
            var json = await response.Content.ReadAsStringAsync();
            var jobj = JObject.Parse(json);
            JValue productIdValue = (JValue)jobj["productId"];
            string pId = (string)productIdValue.Value;

            return true;
        }
    }
}
