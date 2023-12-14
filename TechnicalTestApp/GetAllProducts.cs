using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TechnicalTestApp.ProductStorage;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Azure.Documents;

namespace TechnicalTestApp
{
    public static class GetAllProducts
    {
        [FunctionName("GetAllProducts")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Hämtning av tabell.");

            try
            {
                //Skapa en CloudTableClient för att kommunicera med Azure Table Storage
               
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

               
                string tableName = "ProductTable";
                CloudTable table = tableClient.GetTableReference(tableName);

                //Skapar fråga för att hämta entiteter från tabell
                TableQuery<ProductEntity> query = new TableQuery<ProductEntity>();
                var products = table.ExecuteQuery(query).ToList();

                //lista för att lagra produkterna
                List<Product> productList = new List<Product>();

                //Konvertera varje entitet till ett Product-objekt och lägg till i listan
                foreach (var productEntity in products)
                {
                    productList.Add(new Product
                    {
                        PartitionKey = productEntity.PartitionKey,
                        RowKey = productEntity.RowKey,
                        Name = productEntity.Name,
                        Description = productEntity.Description
                        
                    });
                }

                //Konvertera listan till JSON och returnera som svar
                string responseJson = JsonConvert.SerializeObject(productList);
                return new OkObjectResult(responseJson);
            }
            catch (Exception ex)
            {
                log.LogError($"Error. {ex.Message}");
                return new BadRequestObjectResult($"Misslyckades att hämta produkterna från tabell: {ex.Message}");
            }
        }
        public class ProductEntity : TableEntity
        {
            public ProductEntity()
            {
                
                
            }
            public string Name { get; set; }
            public string Description { get; set; }
            
        }
        public class Product
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
    }
}
