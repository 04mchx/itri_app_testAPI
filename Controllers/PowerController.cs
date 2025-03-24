using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using testAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace testAPI.Controllers
{
    [Route("api/power")]
    [ApiController]
    public class PowerController : ControllerBase
    {
        private readonly PowerDbContext _context;

        public PowerController(PowerDbContext context)
        {
            _context = context;
        }

        // 總用電量的折線圖
        [HttpGet("total")]
        public async Task<ActionResult<IEnumerable<object>>> GetTotalPower()
        {
            var result = await _context.PMMinP
                .OrderBy(p => p.Date_time) // 按時間排序
                .Select(p => new
                {
                    Date = p.Date_time,
                    TotalPower = p.kWh
                })
                .ToListAsync();

            return Ok(result);
        }

        // 空調 & 照明的折線圖
        [HttpGet("ac-light")]
        public async Task<ActionResult<IEnumerable<object>>> GetAcLightPower()
        {
            var acPower = await _context.ACControl
                .GroupBy(a => a.Date_Time)
                .Select(g => new
                {
                    Date = g.Key,
                    AC_Power = g.Sum(a => a.RunStatus) // 這裡假設 RunStatus 代表開啟的功率
                })
                .ToListAsync();

            var lightPower = await _context.LightControl
                .GroupBy(l => l.Date_Time)
                .Select(g => new
                {
                    Date = g.Key,
                    Light_Power = g.Sum(l => l.RunStatus) // 假設 RunStatus 代表照明功率
                })
                .ToListAsync();

            var mergedData = acPower.Join(lightPower,
                ac => ac.Date,
                light => light.Date,
                (ac, light) => new
                {
                    Date = ac.Date,
                    AC_Power = ac.AC_Power,
                    Light_Power = light.Light_Power
                }).ToList();

            return Ok(mergedData);
        }

        // 獲取本日/本月的總用電數據
        [HttpGet("summary")]
        public async Task<ActionResult<object>> GetPowerSummary([FromQuery] string type)
        {
            DateTime startDate, endDate;
            if (type == "daily")
            {
                startDate = DateTime.Today; // 今日 00:00
                endDate = DateTime.Now; // 現在時間
            }
            else if (type == "monthly")
            {
                startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); // 本月 1 日
                endDate = DateTime.Now;
            }
            else
            {
                return BadRequest("Invalid type. Use 'daily' or 'monthly'.");
            }

            // 總用電量
            var totalPower = await _context.PMMinP
                .Where(p => p.Date_time >= startDate && p.Date_time <= endDate)
                .SumAsync(p => p.kWh);

            // 1F 總用電 (Meter_ID 以 '1F' 開頭)
            var firstFloorPower = await _context.PMMinP
                .Where(p => p.Date_time >= startDate && p.Date_time <= endDate && p.Meter_Id.StartsWith("1F"))
                .SumAsync(p => p.kWh);

            // 2F 總用電 (Meter_ID 以 '2F' 開頭)
            var secondFloorPower = await _context.PMMinP
                .Where(p => p.Date_time >= startDate && p.Date_time <= endDate && p.Meter_Id.StartsWith("2F"))
                .SumAsync(p => p.kWh);

            // 1F 110V 總用電 (Meter_ID 以 '1F-110V' 開頭)
            var firstFloor110V = await _context.PMMinP
                .Where(p => p.Date_time >= startDate && p.Date_time <= endDate && p.Meter_Id.StartsWith("1F-110V"))
                .SumAsync(p => p.kWh);

            // 1F 220V 總用電
            var firstFloor220V = await _context.PMMinP
                .Where(p => p.Date_time >= startDate && p.Date_time <= endDate && p.Meter_Id.StartsWith("1F-220V"))
                .SumAsync(p => p.kWh);

            // 1F 220V 空調
            var firstFloor220V_AC = await _context.ACControl
                .Where(ac => ac.Date_Time >= startDate && ac.Date_Time <= endDate && ac.R_Id == 1) // R_Id=1 表示空調
                .SumAsync(ac => ac.RunStatus);

            // 1F 220V 照明
            var firstFloor220V_Light = await _context.LightControl
                .Where(lc => lc.Date_Time >= startDate && lc.Date_Time <= endDate && lc.COM_Id == 1) // COM_Id=1 表示照明
                .SumAsync(lc => lc.RunStatus);

            // 2F 110V 總用電
            var secondFloor110V = await _context.PMMinP
                .Where(p => p.Date_time >= startDate && p.Date_time <= endDate && p.Meter_Id.StartsWith("2F-110V"))
                .SumAsync(p => p.kWh);

            // 2F 220V 總用電
            var secondFloor220V = await _context.PMMinP
                .Where(p => p.Date_time >= startDate && p.Date_time <= endDate && p.Meter_Id.StartsWith("2F-220V"))
                .SumAsync(p => p.kWh);

            // 2F 220V 空調
            var secondFloor220V_AC = await _context.ACControl
                .Where(ac => ac.Date_Time >= startDate && ac.Date_Time <= endDate && ac.R_Id == 2) // R_Id=2 表示空調
                .SumAsync(ac => ac.RunStatus);

            // 2F 220V 照明
            var secondFloor220V_Light = await _context.LightControl
                .Where(lc => lc.Date_Time >= startDate && lc.Date_Time <= endDate && lc.COM_Id == 2) // COM_Id=2 表示照明
                .SumAsync(lc => lc.RunStatus);

            var result = new
            {
                DateRange = type,
                TotalPower = totalPower,
                FirstFloorPower = firstFloorPower,
                SecondFloorPower = secondFloorPower,
                FirstFloor110V = firstFloor110V,
                FirstFloor220V = firstFloor220V,
                FirstFloor220V_AC = firstFloor220V_AC,
                FirstFloor220V_Light = firstFloor220V_Light,
                SecondFloor110V = secondFloor110V,
                SecondFloor220V = secondFloor220V,
                SecondFloor220V_AC = secondFloor220V_AC,
                SecondFloor220V_Light = secondFloor220V_Light
            };

            return Ok(result);
        }
    }
}
