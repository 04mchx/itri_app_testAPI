using System;
using System.ComponentModel.DataAnnotations;

namespace testAPI.Models
{
    public class LightInfo
    {
        public byte COM_Id { get; set; }
        public int ICP_Id { get; set; }
        public int? Port { get; set; }
        public string? Note { get; set; }
    }
}

