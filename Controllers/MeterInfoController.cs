using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using testAPI.Data;
using testAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace testAPI.Controllers
{
    [Route("api/meterinfo")]
    [ApiController]
    public class MeterInfoController : ControllerBase
    {
        private readonly PowerDbContext _context;

        public MeterInfoController(PowerDbContext context)
        {
            _context = context;
        }

        // 查詢所有 MeterInfo
        [HttpGet]
        [Produces("application/json")] //強制回傳json
        public async Task<ActionResult<IEnumerable<MeterInfo>>> GetAllMeters()
        {
            return await _context.MeterInfo.ToListAsync();
        }

        // 透過 ID 查詢特定 MeterInfo
        [HttpGet("{id}")]
        public async Task<ActionResult<MeterInfo>> GetMeterById(string id)
        {
            var meter = await _context.MeterInfo.FindAsync(id);
            if (meter == null)
            {
                return NotFound();
            }
            return meter;
        }

        // 新增 MeterInfo
        [HttpPost]
        public async Task<ActionResult<MeterInfo>> CreateMeter(MeterInfo meter)
        {
            _context.MeterInfo.Add(meter);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMeterById), new { id = meter.Meter_ID }, meter);
        }

        // 更新 MeterInfo
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMeter(string id, MeterInfo meter)
        {
            if (id != meter.Meter_ID)
            {
                return BadRequest();
            }

            _context.Entry(meter).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.MeterInfo.Any(e => e.Meter_ID == id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }
    }
}
