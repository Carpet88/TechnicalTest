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
using Microsoft.Azure.Cosmos.Table;
using static TechnicalTestApp.GetAllProducts;

namespace TechnicalTestApp
{
    public static class ProductStorage
    {
        [FunctionName("ProductStorage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Lagring av produkter i tabell.");

            try
            {
            //Läser inkommen data från Http-anropet och converterar från JSOn till .net
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            //Koppling till Azure Storage
               
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            //Skapande av cloudtabel 
            string tableName = "ProductTable";
            CloudTable table = tableClient.GetTableReference(tableName);

            //Skapa ett TableEntity-objekt och lagra det i tabellen
            ProductEntity productEntity = JsonConvert.DeserializeObject<ProductEntity>(requestBody);
            bool isSuccess = await InsertOrMergeEntityAsync(table, productEntity);

                
                return new OkObjectResult("Function executed successfully");
            }
            //felhantering
            catch (Exception ex)
            {
                log.LogError($"Error. {ex.Message}");
                return new BadRequestObjectResult($"Misslyckad exekvering av funtionen: {ex.Message}");
            }
          
             
        }

        //Product entity för att för att kunna lägga till i Azure Table
        public class ProductEntity : TableEntity
        {
            public ProductEntity()
            {
              
                
            }
           
            public string Name { get; set; }
            public string Description { get; set; }
            
        }

        public static async Task<bool> InsertOrMergeEntityAsync(CloudTable table, ProductEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            try
            {
                //Skapar operation för att lägga till entity
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

               //exkvera operation
                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);

                // Kontrollera om exekvering lyckades
                if (result.HttpStatusCode >= 200 && result.HttpStatusCode < 300)
                {
                    return true;
                }
                else
                {

                    Console.WriteLine($"Error: {result.HttpStatusCode} - {result.Result}");
                    return false;
                }
            }
            //felhantering
            catch (Exception ex)
            {
                
                throw new Exception($"Error: {ex.Message}", ex);
                
            }
        }

    }
}
