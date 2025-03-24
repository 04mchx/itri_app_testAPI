using System;
using System.ComponentModel.DataAnnotations;
namespace testAPI.Models
{
    public class LightControl
    {
        public byte COM_Id { get; set; }
        public int ICP_Id { get; set; }
        public int? Port { get; set; }
        public int? Status { get; set; }
        public int? Control { get; set; }
        public int? RunStatus { get; set; }
        public DateTime Date_Time { get; set; }
    }

}
