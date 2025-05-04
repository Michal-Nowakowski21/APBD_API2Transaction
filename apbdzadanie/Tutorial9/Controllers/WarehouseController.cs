using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model;
using Tutorial9.Services;

namespace Tutorial9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly IDbService _dbservice;

        public WarehouseController(IDbService idbservice)
        {
            _dbservice = idbservice;
        }

        [HttpPost]
        public async Task<IActionResult> Insert([FromBody] WarehouseSomethingDTO dto)
        {
            try
            {
                int id = await _dbservice.DoSomethingAsync(dto.IdProduct, dto.IdWarehouse, dto.Amount, dto.CreatedAt);
                return Ok(new { InsertedId = id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpPost("procedure")]
        public async Task<IActionResult> InsertWithProcedure([FromBody] WarehouseSomethingDTO dto)
        {
            try
            {
                int id = await _dbservice.task2withprocedure(dto.IdProduct, dto.IdWarehouse, dto.Amount, dto.CreatedAt);
                return Ok(new { InsertedId = id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
