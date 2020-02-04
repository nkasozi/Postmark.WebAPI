using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Postmark.WebAPI.Models;

namespace Postmark.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        

        // POST: api/Email
        [HttpPost]
        public void Post([FromBody] SingleEmail emailRequest)
        {
        }

        // POST: api/Email
        [HttpPost]
        public void Post([FromBody] BulkEmail emailRequest)
        {
        }

    }
}
