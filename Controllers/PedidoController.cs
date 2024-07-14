using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("[controller]")]
public class PedidoController : ControllerBase
{
    private readonly ILogger<PedidoController> _logger;

    public PedidoController(ILogger<PedidoController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<object> NumeroPedido([FromQuery] string numeroPedido)
    {
        if (numeroPedido == null)
        {
            var response = new { numeroPedido = "Numero Pedido é nullo" };
            _logger.LogInformation("NumeroPedido é null");
            return Ok(response);
        }
        else
        {
            var response = new { numeroPedido = numeroPedido };
            _logger.LogInformation("NumeroPedido: {NumeroPedido}", numeroPedido);
            return Ok(response);
        }
    }
}
