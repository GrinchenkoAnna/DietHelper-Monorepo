using DietHelper.Common.DTO;
using DietHelper.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DietHelper.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OpenFoodFactsController : Controller
    {
        private readonly IOpenFoodFactsService _openFoodFactsService;

        public OpenFoodFactsController(IOpenFoodFactsService openFoodFactsService)
        {
            _openFoodFactsService = openFoodFactsService;
        }

        [HttpGet("barcode/{barcode}")]
        public async Task<ActionResult<OpenFoodFactsDto>> GetProductByBarcode(string barcode)
        {
            var result = await _openFoodFactsService.GetProductByBarcodeAsync(barcode);

            return Ok(result);
        }
    }
}
