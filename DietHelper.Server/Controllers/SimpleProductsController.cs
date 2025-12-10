using DietHelper.Common.Data;
using DietHelper.Common.Models.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DietHelper.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimpleProductsController : Controller
    {
        private readonly DietHelperDbContext _dbContext;

        public SimpleProductsController(DietHelperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("mock")]
        public async Task<ActionResult<Product>> GetMockProduct()
        {
            try
            {
                var products = await _dbContext.Products
                    .Where(p => !p.IsDeleted)
                    .FirstOrDefaultAsync();           

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }
    }
}
