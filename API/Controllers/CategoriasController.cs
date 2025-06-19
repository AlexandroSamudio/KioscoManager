using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class CategoriasController(DataContext context) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Categoria>>> GetCategorias(CancellationToken cancellationToken)
        {
            if (context.Categorias == null)
            {
                return NotFound();
            }
            var categorias = await context.Categorias.ToListAsync(cancellationToken);
            return Ok(categorias);
        }
    }
}
