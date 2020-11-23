using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using MaintenanceEventRouter.EventData;
using MaintenanceEventRouter.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MaintenanceEventRouter
{
    public static class MaintenanceEventRouter
    {
        [FunctionName("MaintenanceEventRouter")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function,  "post", Route = null)]
            HttpRequest req, ILogger log, ExecutionContext context)
        {
            ArcusEventGridClient.SaveContext(context);
            
            var events = new List<MaintenanceNeededEvent> { };
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogInformation(requestBody);
                var maintenanceList = JsonConvert.DeserializeObject<IEnumerable<MaintenanceData>>(requestBody);
                events.AddRange(maintenanceList.Select(maintenanceData =>
                    new MaintenanceNeededEvent(Guid.NewGuid().ToString("N"), $"maintenance/{maintenanceData.Engine}", maintenanceData)));
                await ArcusEventGridClient.Publisher.PublishManyAsync(events);

                return (ActionResult)new OkObjectResult("received");
            }
            catch (Exception e)
            {
                log.LogError(e, $"Error occurred when sending event grid messages: {e.Message}" );
                return new InternalServerErrorResult();
            }
        }
    }
}