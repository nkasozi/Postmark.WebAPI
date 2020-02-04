using Akka.Actor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Postmark.WebAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Postmark.WebAPI.Controllers
{
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IActorRef _apiListenerActor;

        public EmailController(ApiListenerActorProvider actorProvider)
        {
            _apiListenerActor = actorProvider();
        }

        // POST: api/Email
        [HttpPost("api/[controller]", Name = "ProcessSingleEmailRequest")]
        public async Task<IActionResult> ProcessSingleEmailRequest([FromBody] SingleEmail emailRequest)
        {
            var result = await _apiListenerActor.Ask(emailRequest);

            switch (result)
            {
                case SingleEmailResult response:
                    return Ok(response);

                default:
                    return StatusCode(StatusCodes.Status500InternalServerError, result);
            }
        }

        // POST: api/email/batch
        [HttpPost("api/[controller]/batch", Name = "ProcessBatchEmailRequest")]
        public async Task<IActionResult> ProcessBatchEmailRequest([FromBody] List<SingleEmail> emailRequests)
        {
            var cmd = new BulkEmail
            {
                Emails = emailRequests
            };

            var result = await _apiListenerActor.Ask(cmd);

            switch (result)
            {
                case BulkEmailResult response:
                    return Ok(response.Results);

                default:
                    return StatusCode(StatusCodes.Status500InternalServerError, result);
            }
        }

    }
}
