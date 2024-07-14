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
        if (numeroPedido.Contains("99"))
        {
            var response = new { numeroPedido = (object)null, erro = "Numero Pedido é nulo" };
            _logger.LogInformation("NumeroPedido é nulo");
            return Ok(response);
        }
        else
        {
            var response = new { numeroPedido = numeroPedido, erro = (object)null };
            _logger.LogInformation("NumeroPedido: {NumeroPedido}", numeroPedido);
            return Ok(response);
        }
    }
}
