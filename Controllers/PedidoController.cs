using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PedidoController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public PedidoController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("{numeroPedido}")]
        public ActionResult<object> NumeroPedido(string numeroPedido)
        {
            if (numeroPedido == null)
            {
                var response = new { numeroPedido = "Numero Pedido é nullo" };
                return Ok(response);
            }
            else
            {
                var response = new { numeroPedido = numeroPedido };
                return Ok(response);
            }
        }

    }
}