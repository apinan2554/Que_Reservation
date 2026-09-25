using System.ComponentModel.DataAnnotations;

namespace Reservation_Que.Models
{
    /// <summary>
    /// ข้อมูลที่ลูกค้าส่งมาเพื่อรับคิว (จากฟอร์มลงทะเบียน)
    /// </summary>
    public class RegisterQueueDto
    {
        [Required(ErrorMessage = "กรุณากรอกชื่อ")]
        public string CustomerName { get; set; } = string.Empty;

        [Range(1, 50, ErrorMessage = "จำนวนคนไม่ถูกต้อง")]
        public int CustomerCount { get; set; }

        /// <summary>พิกัด GPS ของลูกค้า (จาก HTML5 Geolocation)</summary>
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        /// <summary>true = เข้ามาจาก QR (ต้องตรวจ GPS), false = จากเมนู (ไม่ตรวจ)</summary>
        public bool FromQr { get; set; }
    }

    /// <summary>
    /// รูปแบบข้อมูลคิวที่ส่งกลับให้ Frontend (สะอาด ไม่ผูกกับ EF entity โดยตรง)
    /// </summary>
    public class QueueViewDto
    {
        public int Id { get; set; }
        public string QueueNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int CustomerCount { get; set; }
        public string TableGroup { get; set; } = string.Empty;
        public int Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CalledAt { get; set; }
    }
}
