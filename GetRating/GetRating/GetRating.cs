using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using System.Configuration;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;
using System.Web.Http;

namespace GetRating
{
    public class Rating
    {
        public string id { get; set; }
        public string userId { get; set; }
        public string productId { get; set; }
        public string timestamp { get; set; }
        public string locationName { get; set; }
        public int rating { get; set; }
        public string userNotes { get; set; }
    }
    public static class GetRating
    {


        [FunctionName("GetRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string ratingId = req.Query["ratingId"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            ratingId = ratingId ?? data?.ratingId;
            var rating = new List<Rating>();
              // ADD THIS PART TO YOUR CODE
             try
                {
                    Console.WriteLine("Beginning operations...\n");
                    CosmosDBAccess p = new CosmosDBAccess();
                    rating = await p.GetRatingDataAsync(ratingId);
                }
                catch (CosmosException de)
                {
                    Exception baseException = de.GetBaseException();
                    Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: {0}", e);
                }
                finally
                {
                    Console.WriteLine("End of demo, press any key to exit.");
                    //Console.ReadKey();
                }

            if (String.IsNullOrEmpty(ratingId)) {
                return new BadRequestObjectResult("Please pass a ratingId on the query string or in the request body");
            }
            else if (rating.Count == 0) {
                return (ActionResult)new NotFoundObjectResult("Not found dataXXXXX");
            }
            else {
               return  (ActionResult)new OkObjectResult(JsonConvert.SerializeObject(rating));
            }

            ////return JsonConvert.SerializeObject(rating);
            //return (ratingId != null && rating.Count != 0)
            //    ? (ActionResult)new OkObjectResult(JsonConvert.SerializeObject(rating))
            //    : new BadRequestObjectResult("Please pass a ratingId on the query string or in the request body");

        }

        public class CosmosDBAccess
        {
            // ADD THIS PART TO YOUR CODE

            // The Azure Cosmos DB endpoint for running this sample.
            private static readonly string EndpointUri = System.Environment.GetEnvironmentVariable("CosmosDBEndpointUri");//ConfigurationManager.AppSettings.Get("CosmosDBEndpointUri");

            // The primary key for the Azure Cosmos account.
            private static readonly string PrimaryKey = System.Environment.GetEnvironmentVariable("CosmosDBPrimaryKey");//ConfigurationManager.AppSettings.Get("CosmosDBPrimaryKey");

            // The Cosmos client instance
            private CosmosClient cosmosClient;

            // The database we will create
            private Database database;

            // The container we will create.
            private Container container;

            // The name of the database and container we will create
            private string databaseId = "RatingDatabase";
            private string containerId = "RatingContainer";
            public CosmosDBAccess()
            {
                // Create a new instance of the Cosmos Client
                this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            }
            public async Task GetStartedDemoAsync()
            {
                //ADD THIS PART TO YOUR CODE
                await this.CreateDatabaseAsync();
            }

            public async Task<List<Rating>> GetRatingDataAsync(string ratingId = "")
            {
                //ADD THIS PART TO YOUR CODE
                await this.CreateDatabaseAsync();

                var sqlQueryText = "SELECT * FROM c " + (string.IsNullOrEmpty(ratingId) ? "" : $"WHERE c.id = '{ratingId}'");

                Console.WriteLine("Running query: {0}\n", sqlQueryText);

                this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/userId");



                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<Rating> queryResultSetIterator = this.container.GetItemQueryIterator<Rating>(queryDefinition);


                List<Rating> Ratings = new List<Rating>();

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Rating> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (Rating Rating in currentResultSet)
                    {
                        Ratings.Add(Rating);
                        Console.WriteLine("\tRead {0}\n", Rating);
                    }
                }

                return Ratings;
            }


            /// <summary>
            /// Create the database if it does not exist
            /// </summary>
            private async Task CreateDatabaseAsync()
            {
                // Create a new database
                this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
                Console.WriteLine("Created Database: {0}\n", this.database.Id);
            }
        }

      
    }
}
