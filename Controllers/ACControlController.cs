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

        // 控制單台空調（避免重複關機）
        [HttpPost("control")]
        public async Task<IActionResult> ControlAC([FromQuery] string floor, [FromQuery] int ac_id, [FromQuery] int control)
        {
            // 驗證樓層參數
            if (floor != "1F" && floor != "2F")
                return BadRequest("Invalid floor. Use '1F' or '2F'.");

            // 驗證開關參數（0: 關，1: 開）
            if (control != 0 && control != 1)
                return BadRequest("Invalid control value. Use '0' (Off) or '1' (On).");

            DateTime now = DateTime.Now;

            // 依據 AC_Id 與 Note 樓層關鍵字找到空調指令資料
            var acCommand = await _context.ACCommand
                .Where(ac => ac.AC_Id == ac_id.ToString() && ac.Note.Contains(floor))
                .FirstOrDefaultAsync();

            if (acCommand == null)
                return NotFound($"AC_Id {ac_id} not found for {floor}.");

            // 查詢該台主機+頻道最新的控制紀錄
            var latestStatus = await _context.ACControl
                .Where(ac => ac.R_Id == acCommand.R_Id && ac.Ch == acCommand.Ch)
                .OrderByDescending(ac => ac.Date_Time)
                .FirstOrDefaultAsync();

            // 如果目前狀態已是關機，且再次送出關機 → 不執行
            if (latestStatus != null && latestStatus.Control == 0 && control == 0)
            {
                return Ok($"AC {ac_id} on {floor} is already OFF. No action taken.");
            }

            // 寫入新的控制紀錄
            var acControl = new ACControl
            {
                Date_Time = now,
                R_Id = (byte)acCommand.R_Id,     // 主機編號
                Ch = acCommand.Ch,               // 頻道編號（分機）
                Com_Id = (int)acCommand.Com_Id,  // 通訊介面
                Control = control,               // 開關狀態
                RunStatus = 0                    // 初始運作狀態
            };

            _context.ACControl.Add(acControl);
            await _context.SaveChangesAsync();

            return Ok($"AC {ac_id} on {floor} set to {control}.");
        }

        // 查詢某樓層所有空調的最新狀態
        [HttpGet("status")]
        public async Task<IActionResult> GetACStatus([FromQuery] string floor)
        {
            if (floor != "1F" && floor != "2F")
                return BadRequest("Invalid floor. Use '1F' or '2F'.");

            // 從 ACCommand 過濾出該樓層所有空調設定
            var acCommands = await _context.ACCommand
                .Where(ac => ac.Note.Contains(floor))
                .ToListAsync();

            var statuses = new List<ACControl>();

            // 對每台空調查詢它的最新控制狀態
            foreach (var cmd in acCommands)
            {
                var latest = await _context.ACControl
                    .Where(ac => ac.R_Id == cmd.R_Id && ac.Ch == cmd.Ch)
                    .OrderByDescending(ac => ac.Date_Time)
                    .FirstOrDefaultAsync();

                if (latest != null)
                {
                    statuses.Add(latest);
                }
            }

            return Ok(statuses);
        }

        // 每 30 秒內有控制紀錄的空調 → 更新為正在運行
        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateRunStatus()
        {
            DateTime checkTime = DateTime.Now.AddSeconds(-30);

            // 找出近 30 秒有被操作的空調
            var acControls = await _context.ACControl
                .Where(ac => ac.Date_Time >= checkTime)
                .ToListAsync();

            foreach (var ac in acControls)
            {
                ac.RunStatus = 1; // 設為運行中
            }

            await _context.SaveChangesAsync();
            return Ok("AC RunStatus updated.");
        }
    }
}
