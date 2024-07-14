using LSF.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("pedido")]
public class PedidoController : ControllerBase
{
    private readonly ILogger<PedidoController> _logger;

    public PedidoController(ILogger<PedidoController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public ActionResult<PedidoResponse> NumeroPedido([FromQuery] string numeroPedido)
    {
        PedidoResponse response;

        if (numeroPedido.Contains("99"))
        {
            response = new PedidoResponse
            {
                NumeroPedido = null,
                Erro = "Numero Pedido é nulo"
            };
            _logger.LogInformation("NumeroPedido é nulo");
        }
        else
        {
            response = new PedidoResponse
            {
                NumeroPedido = numeroPedido,
                Erro = null
            };
            _logger.LogInformation("NumeroPedido: {NumeroPedido}", numeroPedido);
        }

        return Ok(response);
    }
}
