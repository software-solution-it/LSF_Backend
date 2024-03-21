using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LSF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public ProdutosController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/<ProdutosController>
        [HttpGet]
        public IEnumerable<Produtos> Get()
        {
            return _dbContext.Produtos.ToList();
        }

        // GET api/<ProdutosController>/5
        [HttpGet("{id}")]
        public Produtos Get(int id)
        {
            return _dbContext.Produtos.FirstOrDefault(t => t.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> PostTecnico(Produtos tecnico)
        {
            if (tecnico == null)
            {
                return BadRequest("Dados do técnico inválidos");
            }

            try
            {
                // Adicione o novo técnico ao contexto do banco de dados
                _dbContext.Produtos.Add(tecnico);
                await _dbContext.SaveChangesAsync(); // Salva as alterações no banco de dados

                // Retorna uma resposta de sucesso com o técnico adicionado
                return Ok(tecnico);
            }
            catch (Exception ex)
            {
                // Em caso de erro, retorna uma resposta de erro com uma mensagem detalhada
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }


        // PUT api/<ProdutosController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Produtos tecnico)
        {
            var tecnicoExistente = await _dbContext.Produtos.FindAsync(id);
            if (tecnicoExistente == null)
            {
                return NotFound("Técnico não encontrado");
            }

            // Atualiza as propriedades do técnico existente com as propriedades do técnico fornecido
            _dbContext.Entry(tecnicoExistente).CurrentValues.SetValues(tecnico);

            // Salva as alterações no banco de dados
            await _dbContext.SaveChangesAsync();

            // Retorna uma resposta de sucesso com o técnico atualizado
            return Ok(tecnicoExistente);
        }

        // DELETE api/<ProdutosController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            // Verifica se o recurso com o ID fornecido existe
            var tecnico = _dbContext.Produtos.Find(id);
            if (tecnico == null)
            {
                return NotFound("Técnico não encontrado");
            }

            try
            {
                // Remove o recurso do contexto do banco de dados
                _dbContext.Produtos.Remove(tecnico);

                // Salva as alterações no banco de dados
                _dbContext.SaveChanges();

                // Retorna uma resposta de sucesso
                return NoContent(); // Retorna um código 204 No Content
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

    }
}