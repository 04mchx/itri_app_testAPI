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

        // 本日每小時 / 本月每日 用電柱狀圖（若該時間無資料則補下一筆）
        [HttpGet("total-chart")]
        public async Task<IActionResult> GetTotalPowerChart([FromQuery] string type)
        {
            if (type != "daily" && type != "monthly")
                return BadRequest("Invalid type. Use 'daily' or 'monthly'.");

            var now = DateTime.Now;
            var result = new List<object>();

            if (type == "daily")
            {
                DateTime start = DateTime.Today;
                DateTime end = now;

                var data = await _context.PMMinP
                    .Where(p => p.Date_time >= start && p.Date_time <= end)
                    .OrderBy(p => p.Date_time)
                    .ToListAsync();

                for (int hour = 0; hour < 24; hour++)
                {
                    var segmentStart = start.AddHours(hour);
                    var segmentEnd = segmentStart.AddHours(1);

                    var segmentData = data
                        .Where(p => p.Date_time >= segmentStart && p.Date_time < segmentEnd)
                        .ToList();

                    if (segmentData.Any())
                    {
                        var power = segmentData.Sum(p => p.kWh);
                        result.Add(new
                        {
                            Time = segmentStart.ToString("yyyy-MM-dd HH:mm:ss"),
                            Power = Math.Round(power.GetValueOrDefault(), 2)
                        });
                    }
                    else
                    {
                        var fallback = data.FirstOrDefault(p => p.Date_time > segmentStart);
                        if (fallback != null)
                        {
                            result.Add(new
                            {
                                Time = segmentStart.ToString("yyyy-MM-dd HH:mm:ss"),
                                Power = Math.Round(fallback.kWh ?? 0, 2)
                            });
                        }
                        else
                        {
                            result.Add(new
                            {
                                Time = segmentStart.ToString("yyyy-MM-dd HH:mm:ss"),
                                Power = 0.0
                            });
                        }
                    }
                }
            }
            else if (type == "monthly")
            {
                DateTime start = new DateTime(now.Year, now.Month, 1);
                int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                DateTime end = now;

                var data = await _context.PMMinP
                    .Where(p => p.Date_time >= start && p.Date_time <= end)
                    .OrderBy(p => p.Date_time)
                    .ToListAsync();

                for (int i = 0; i < daysInMonth; i++)
                {
                    var dayStart = start.AddDays(i);
                    var dayEnd = dayStart.AddDays(1);

                    var dayData = data
                        .Where(p => p.Date_time >= dayStart && p.Date_time < dayEnd)
                        .ToList();

                    if (dayData.Any())
                    {
                        var power = dayData.Sum(p => p.kWh);
                        result.Add(new
                        {
                            Time = dayStart.ToString("yyyy-MM-dd"),
                            Power = Math.Round(power.GetValueOrDefault(), 2)
                        });
                    }
                    else
                    {
                        var fallback = data.FirstOrDefault(p => p.Date_time > dayStart);
                        if (fallback != null)
                        {
                            result.Add(new
                            {
                                Time = dayStart.ToString("yyyy-MM-dd"),
                                Power = Math.Round(fallback.kWh ?? 0, 2)
                            });
                        }
                        else
                        {
                            result.Add(new
                            {
                                Time = dayStart.ToString("yyyy-MM-dd"),
                                Power = 0.0
                            });
                        }
                    }
                }
            }

            return Ok(result);
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
