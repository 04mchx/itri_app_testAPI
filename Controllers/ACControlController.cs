using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using testAPI.Data;
using testAPI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace testAPI.Controllers
{
    [Route("api/ac")]
    [ApiController]
    public class ACControlController : ControllerBase
    {
        private readonly PowerDbContext _context;

        public ACControlController(PowerDbContext context)
        {
            _context = context;
        }

        // **控制單台空調**
        [HttpPost("control")]
        public async Task<IActionResult> ControlAC([FromQuery] string floor, [FromQuery] int ac_id, [FromQuery] int control)
        {
            if (floor != "1F" && floor != "2F")
                return BadRequest("Invalid floor. Use '1F' or '2F'.");

            if (control != 0 && control != 1)
                return BadRequest("Invalid control value. Use '0' (Off) or '1' (On).");

            DateTime now = DateTime.Now;

            // 查詢該 AC_Id 的設定
            var acCommand = await _context.ACCommand
                .Where(ac => ac.AC_Id == ac_id.ToString() && ac.Note.Contains(floor))
                .FirstOrDefaultAsync();

            if (acCommand == null)
                return NotFound($"AC_Id {ac_id} not found for {floor}.");

            // 插入新的 AC_Control 記錄
            var acControl = new ACControl
            {
                Date_Time = now,
                R_Id = (byte)acCommand.R_Id,
                Ch = acCommand.Ch,
                Com_Id = (int)acCommand.Com_Id,
                Control = control, // 0: 關, 1: 開
                RunStatus = 0 // 設備執行時，初始狀態為 0
            };

            _context.ACControl.Add(acControl);
            await _context.SaveChangesAsync();

            return Ok($"AC {ac_id} on {floor} set to {control}.");
        }

        // **查詢 1F 或 2F 所有空調狀態**
        [HttpGet("status")]
        public async Task<IActionResult> GetACStatus([FromQuery] string floor)
        {
            if (floor != "1F" && floor != "2F")
                return BadRequest("Invalid floor. Use '1F' or '2F'.");

            var acStatusQuery = _context.ACControl.AsQueryable();

            if (floor == "1F")
                acStatusQuery = acStatusQuery.Where(ac => ac.R_Id == 1);
            else
                acStatusQuery = acStatusQuery.Where(ac => ac.R_Id == 2);

            var acStatuses = await acStatusQuery
                .OrderByDescending(ac => ac.Date_Time)
                .GroupBy(ac => ac.Ch)  // 取每台空調最新的狀態
                .Select(g => g.FirstOrDefault())
                .ToListAsync();

            return Ok(acStatuses);
        }

        // **每 30 秒更新空調運轉狀態**
        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateRunStatus()
        {
            DateTime checkTime = DateTime.Now.AddSeconds(-30);

            var acControls = await _context.ACControl
                .Where(ac => ac.Date_Time >= checkTime)
                .ToListAsync();

            foreach (var ac in acControls)
            {
                ac.RunStatus = 1; // 設備開始運行
            }

            await _context.SaveChangesAsync();
            return Ok("AC RunStatus updated.");
        }
    }
}