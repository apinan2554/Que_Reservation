using System.ComponentModel.DataAnnotations;

namespace Reservation_Que.Models
{
    /// <summary>
    /// โมเดลข้อมูลคิว 1 ใบ ที่จะถูกเก็บลงตาราง Queues
    /// </summary>
    public class Queue
    {
        public int Id { get; set; }

        /// <summary>หมายเลขคิวที่แสดงให้ลูกค้า เช่น A001, B012</summary>
        [Required]
        [MaxLength(10)]
        public string QueueNumber { get; set; } = string.Empty;

        /// <summary>ชื่อลูกค้า</summary>
        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>จำนวนคน</summary>
        [Range(1, 50)]
        public int CustomerCount { get; set; }

        /// <summary>กลุ่มโต๊ะ เช่น A (1-2 คน), B (3-4 คน)</summary>
        [Required]
        [MaxLength(5)]
        public string TableGroup { get; set; } = string.Empty;

        /// <summary>สถานะปัจจุบันของคิว</summary>
        public QueueStatus Status { get; set; } = QueueStatus.Pending;

        /// <summary>เวลาที่สร้างคิว (ใช้จัดลำดับก่อน-หลัง)</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>เวลาที่ถูกเรียก (null หากยังไม่ถูกเรียก)</summary>
        public DateTime? CalledAt { get; set; }
    }
}
