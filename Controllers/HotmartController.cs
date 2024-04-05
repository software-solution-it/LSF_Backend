﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PurchaseObjects;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HotmartController : ControllerBase
    {
        // GET: api/<Hotmart>
        [HttpGet("/hotmart/purchase")]
        public IActionResult Get(PurchaseObject objeto)
        {
            return Ok(objeto);
        }

        [HttpPost("/hotmart/purchase")]
        public IActionResult PostPurchase([FromBody] PurchaseObject purchase)
        {
            return Ok(purchase);
        }

        // PUT api/<Hotmart>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<Hotmart>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
