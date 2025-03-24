using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using testAPI.Data;
using testAPI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace testAPI.Controllers
{
    [Route("api/light")]
    [ApiController]
    public class LightControlController : ControllerBase
    {
        private readonly PowerDbContext _context;

        public LightControlController(PowerDbContext context)
        {
            _context = context;
        }

        // **控制單個照明設備**
        [HttpPost("control")]
        public async Task<IActionResult> ControlLight([FromQuery] string floor, [FromQuery] int light_id, [FromQuery] int control)
        {
            if (floor != "1F" && floor != "2F")
                return BadRequest("Invalid floor. Use '1F' or '2F'.");

            if (control != 0 && control != 1)
                return BadRequest("Invalid control value. Use '0' (Off) or '1' (On).");

            DateTime now = DateTime.Now;

            // 查詢該 Light_Id 設備設定
            var lightInfo = await _context.LightInfo
                .Where(l => l.ICP_Id == light_id && l.Note.Contains(floor))
                .FirstOrDefaultAsync();

            if (lightInfo == null)
                return NotFound($"Light {light_id} not found for {floor}.");

            // 插入新的 Light_Control 記錄
            var lightControl = new LightControl
            {
                COM_Id = lightInfo.COM_Id,
                ICP_Id = lightInfo.ICP_Id,
                Port = lightInfo.Port,
                Control = control, // 0: 關, 1: 開
                RunStatus = 0, // 初始狀態
                Date_Time = now
            };

            _context.LightControl.Add(lightControl);
            await _context.SaveChangesAsync();

            return Ok($"Light {light_id} on {floor} set to {control}.");
        }

        // **控制某地點所有燈光**
        [HttpPost("control-all")]
        public async Task<IActionResult> ControlAllLights([FromQuery] string floor, [FromQuery] string location, [FromQuery] int control)
        {
            if (floor != "1F" && floor != "2F")
                return BadRequest("Invalid floor. Use '1F' or '2F'.");

            if (control != 0 && control != 1)
                return BadRequest("Invalid control value. Use '0' (Off) or '1' (On).");

            DateTime now = DateTime.Now;

            var lights = await _context.LightInfo
                .Where(l => l.Note.Contains(location) && l.Note.Contains(floor))
                .ToListAsync();

            if (!lights.Any())
                return NotFound($"No lights found for location: {location} on {floor}.");

            foreach (var light in lights)
            {
                var lightControl = new LightControl
                {
                    COM_Id = light.COM_Id,
                    ICP_Id = light.ICP_Id,
                    Port = light.Port,
                    Control = control,
                    RunStatus = 0,
                    Date_Time = now
                };

                _context.LightControl.Add(lightControl);
            }

            await _context.SaveChangesAsync();
            return Ok($"All lights at {location} on {floor} set to {control}.");
        }
        // **查詢 1F 或 2F 所有照明設備狀態**
        [HttpGet("status")]
        public async Task<IActionResult> GetLightStatus([FromQuery] string floor)
        {
            if (floor != "1F" && floor != "2F")
                return BadRequest("Invalid floor. Use '1F' or '2F'.");

            var lightStatusQuery = _context.LightControl.AsQueryable();

            var lightStatuses = await lightStatusQuery
                .OrderByDescending(l => l.Date_Time)
                .GroupBy(l => l.ICP_Id) // 取每個燈的最新狀態
                .Select(g => g.FirstOrDefault())
                .ToListAsync();

            return Ok(lightStatuses);
        }
        // **每 30 秒更新燈光運轉狀態**
        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateLightRunStatus()
        {
            DateTime checkTime = DateTime.Now.AddSeconds(-30);

            var lightControls = await _context.LightControl
                .Where(l => l.Date_Time >= checkTime)
                .ToListAsync();

            foreach (var light in lightControls)
            {
                light.RunStatus = 1; // 設備開始運行
            }

            await _context.SaveChangesAsync();
            return Ok("Light RunStatus updated.");
        }
    }
}
