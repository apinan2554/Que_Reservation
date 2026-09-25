using System.ComponentModel.DataAnnotations;

namespace Reservation_Que.Models
{
    /// <summary>
    /// การตั้งค่าของแต่ละกลุ่มโต๊ะ เช่น เวลานั่งสูงสุด (นาที)
    /// </summary>
    public class TableSetting
    {
        public int Id { get; set; }

        /// <summary>กลุ่มโต๊ะ เช่น A, B, C (unique)</summary>
        [Required]
        [MaxLength(5)]
        public string TableGroup { get; set; } = string.Empty;

        /// <summary>คำอธิบายกลุ่ม เช่น "1-2 คน"</summary>
        [MaxLength(50)]
        public string Description { get; set; } = string.Empty;

        /// <summary>เวลานั่งสูงสุดต่อโต๊ะ (นาที) ค่าเริ่มต้น 90 = 1.30 ชม.</summary>
        [Range(1, 1440)]
        public int MaxMinutes { get; set; } = 90;
    }
}
