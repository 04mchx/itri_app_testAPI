using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace testAPI.Models
{
    public class PMMinP
    {
        public DateTime Date_time { get; set; }

        public string Meter_Id { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")] // ✅ 設定精度，確保 SQL Server 會正確存數值
        public decimal? kW { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal? kWh { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? PF { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? kVA { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? kvar { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? I_r { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? I_s { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? I_t { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? V_rs { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? V_st { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? V_tr { get; set; }
    }
}

