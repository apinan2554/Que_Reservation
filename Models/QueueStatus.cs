namespace Reservation_Que.Models
{
    /// <summary>
    /// สถานะของคิว ใช้เป็นตัวเลขเพื่อเก็บลง Database และสื่อสารกับ Frontend
    /// </summary>
    public enum QueueStatus
    {
        Pending = 0,   // รอเรียก (Checking In)
        Calling = 1,   // กำลังเรียก (Calling)
        Served = 2,    // เข้าโต๊ะแล้ว (Seated / Served)
        Cancelled = 3  // ยกเลิก
    }
}
