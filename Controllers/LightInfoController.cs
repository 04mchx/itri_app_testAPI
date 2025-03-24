using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using testAPI.Data;
using testAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace testAPI.Controllers
{
    [Route("api/lightinfo")]
    [ApiController]
    public class LightInfoController : ControllerBase
    {
        private readonly PowerDbContext _context;

        public LightInfoController(PowerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LightInfo>>> GetAllLights()
        {
            return await _context.LightInfo.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LightInfo>> GetLightById(int id)
        {
            var light = await _context.LightInfo.FindAsync(id);
            if (light == null)
            {
                return NotFound();
            }
            return light;
        }

        [HttpPost]
        public async Task<ActionResult<LightInfo>> CreateLight(LightInfo light)
        {
            _context.LightInfo.Add(light);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetLightById), new { id = light.ICP_Id }, light);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLight(int id, LightInfo light)
        {
            if (id != light.ICP_Id)
            {
                return BadRequest();
            }

            _context.Entry(light).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLight(int id)
        {
            var light = await _context.LightInfo.FindAsync(id);
            if (light == null)
            {
                return NotFound();
            }

            _context.LightInfo.Remove(light);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
