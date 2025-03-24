using System.ComponentModel.DataAnnotations;

namespace testAPI.Models
{
    public class MeterInfo
    {
        [Key] // 設定主鍵
        public string Meter_ID { get; set; } = string.Empty;

        public string? Note { get; set; }
    }
}

