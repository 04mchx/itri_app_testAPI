using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace testAPI.Models
{
    public class ACCommand
    {
        [Required] // 必填
        public string AC_Id { get; set; } = string.Empty;

        [Required] // 必填
        public int Ch { get; set; }

        public short? R_Id { get; set; }
        public short? Com_Id { get; set; }
        public short? AC_On { get; set; }
        public short? AC_Off { get; set; }
        public short? C_23 { get; set; }
        public short? C_24 { get; set; }
        public short? C_25 { get; set; }
        public short? C_26 { get; set; }
        public short? C_28 { get; set; }
        public byte? No_Ctl { get; set; }
        public string? Note { get; set; }
    }
}
