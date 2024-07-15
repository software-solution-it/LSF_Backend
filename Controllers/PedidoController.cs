using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Mvc;

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
        public ActionResult<PedidoResponse> NumeroPedido(string numeroPedido)
        {
            if (numeroPedido.Contains("555"))
            {
                var response = new PedidoResponse
                {
                    Pedido = null,
                    Status = "Numero Pedido é nullo"
                };
                return Ok(response);
            }
            else
            {
                var response = new PedidoResponse
                {
                    Pedido = numeroPedido,
                    Status = null
                };
                return Ok(response);
            }
        }
    }
}
