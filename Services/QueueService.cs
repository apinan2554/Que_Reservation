using Microsoft.EntityFrameworkCore;
using Reservation_Que.Data;
using Reservation_Que.Models;

namespace Reservation_Que.Services
{
    /// <summary>
    /// Logic หลักในการจัดการคิว (สร้างเลขคิว, จัดกลุ่มโต๊ะ, เรียกคิวถัดไป)
    /// แยกออกมาเป็น service เพื่อให้เขียน Unit Test ได้สะดวก
    /// </summary>
    public class QueueService
    {
        private readonly AppDbContext _db;

        public QueueService(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// จัดกลุ่มโต๊ะตามจำนวนคน
        /// A = 1-2 คน, B = 3-4 คน, C = 5 คนขึ้นไป
        /// </summary>
        public static string ResolveTableGroup(int customerCount)
        {
            if (customerCount <= 2) return "A";
            if (customerCount <= 4) return "B";
            return "C";
        }

        /// <summary>
        /// สร้างเลขคิวถัดไปของกลุ่มนั้น เช่น A001, A002 ...
        /// </summary>
        public async Task<string> GenerateQueueNumberAsync(string tableGroup)
        {
            int count = await _db.Queues.CountAsync(q => q.TableGroup == tableGroup);
            return $"{tableGroup}{(count + 1):D3}";
        }

        /// <summary>
        /// สร้างคิวใหม่ลงฐานข้อมูล
        /// </summary>
        public async Task<Queue> CreateQueueAsync(string customerName, int customerCount)
        {
            string tableGroup = ResolveTableGroup(customerCount);
            string queueNumber = await GenerateQueueNumberAsync(tableGroup);

            var queue = new Queue
            {
                CustomerName = customerName,
                CustomerCount = customerCount,
                TableGroup = tableGroup,
                QueueNumber = queueNumber,
                Status = QueueStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _db.Queues.Add(queue);
            await _db.SaveChangesAsync();
            return queue;
        }

        /// <summary>
        /// เรียกคิวถัดไปของกลุ่มโต๊ะที่ระบุ:
        /// - ค้นหาคิวสถานะ Pending ที่เก่าที่สุด (OrderBy CreatedAt) ในกลุ่มนั้น
        /// - เปลี่ยนสถานะเป็น Calling และบันทึกเวลา CalledAt
        /// คืนค่า null หากไม่มีคิวรออยู่
        /// </summary>
        public async Task<Queue?> CallNextQueueAsync(string tableGroup)
        {
            var next = await _db.Queues
                .Where(q => q.TableGroup == tableGroup && q.Status == QueueStatus.Pending)
                .OrderBy(q => q.CreatedAt)
                .FirstOrDefaultAsync();

            if (next == null)
                return null;

            next.Status = QueueStatus.Calling;
            next.CalledAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return next;
        }

        /// <summary>ดึงคิวตาม Id</summary>
        public async Task<Queue?> GetByIdAsync(int id)
        {
            return await _db.Queues.FindAsync(id);
        }

        /// <summary>
        /// นับจำนวนคิวที่รออยู่ก่อนหน้าคิวที่ระบุ (Pending, กลุ่มเดียวกัน, สร้างก่อนหน้า)
        /// </summary>
        public async Task<int> CountAheadAsync(Queue me)
        {
            return await _db.Queues.CountAsync(q =>
                q.TableGroup == me.TableGroup &&
                q.Status == QueueStatus.Pending &&
                q.CreatedAt < me.CreatedAt);
        }

        /// <summary>
        /// เลขคิวที่กำลังเรียกอยู่ล่าสุดในกลุ่มโต๊ะที่ระบุ (คืน null ถ้าไม่มี)
        /// </summary>
        public async Task<string?> GetLatestCallingNumberAsync(string tableGroup)
        {
            return await _db.Queues
                .Where(q => q.TableGroup == tableGroup && q.Status == QueueStatus.Calling)
                .OrderByDescending(q => q.CalledAt)
                .Select(q => q.QueueNumber)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// เปลี่ยนสถานะคิว (เช่น กด "เข้าโต๊ะแล้ว" หรือ "ยกเลิก")
        /// </summary>
        public async Task<Queue?> UpdateStatusAsync(int id, QueueStatus status)
        {
            var queue = await _db.Queues.FindAsync(id);
            if (queue == null) return null;

            queue.Status = status;
            if (status == QueueStatus.Calling && queue.CalledAt == null)
                queue.CalledAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return queue;
        }
    }
}
