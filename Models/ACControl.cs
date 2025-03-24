using System;
using System.ComponentModel.DataAnnotations;
namespace testAPI.Models
{
    public class ACControl
    {
        public DateTime Date_Time { get; set; }
        public byte R_Id { get; set; }
        public int Ch { get; set; }
        public int Com_Id { get; set; }
        public int? UnLoad { get; set; }
        public int? Control { get; set; }
        public int? RunStatus { get; set; }
    }
}

