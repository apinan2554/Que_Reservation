using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Reservation_Que.Data;
using Reservation_Que.Hubs;
using Reservation_Que.Models;
using Reservation_Que.Services;

namespace Reservation_Que.Controllers
{
    /// <summary>
    /// [ส่วนที่ 3] หน้าแดชบอร์ดพนักงาน + API ควบคุมสถานะคิว
    /// </summary>
    public class StaffController : Controller
    {
        private readonly AppDbContext _db;
        private readonly QueueService _queueService;
        private readonly IHubContext<QueueHub> _hub;

        public StaffController(AppDbContext db, QueueService queueService, IHubContext<QueueHub> hub)
        {
            _db = db;
            _queueService = queueService;
            _hub = hub;
        }

        /// <summary>หน้าแดชบอร์ด Kanban ของพนักงาน</summary>
        public IActionResult Dashboard()
        {
            return View();
        }

        /// <summary>
        /// API: ดึงคิวทั้งหมด (ยกเว้นที่ยกเลิก) สำหรับให้ Frontend render Kanban
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetQueues()
        {
            var queues = await _db.Queues
                .Where(q => q.Status != QueueStatus.Cancelled)
                .OrderBy(q => q.CreatedAt)
                .Select(q => new QueueViewDto
                {
                    Id = q.Id,
                    QueueNumber = q.QueueNumber,
                    CustomerName = q.CustomerName,
                    CustomerCount = q.CustomerCount,
                    TableGroup = q.TableGroup,
                    Status = (int)q.Status,
                    StatusText = q.Status.ToString(),
                    CreatedAt = q.CreatedAt,
                    CalledAt = q.CalledAt
                })
                .ToListAsync();

            return Ok(queues);
        }

        /// <summary>
        /// [ส่วนที่ 3.3] เรียกคิวถัดไปของกลุ่มโต๊ะที่ระบุ
        /// - หาคิวเก่าสุดที่ Pending ในกลุ่มนั้น → เปลี่ยนเป็น Calling
        /// - ยิงสัญญาณ SignalR สั่งเสียงเรียก + อัปเดตแดชบอร์ด
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CallNextQueue(string tableGroup)
        {
            var queue = await _queueService.CallNextQueueAsync(tableGroup);
            if (queue == null)
                return NotFound(new { message = $"ไม่มีคิวรอในกลุ่ม {tableGroup}" });

            // ยิงเสียงเรียกไปหน้าจอหน้าร้าน
            await _hub.Clients.All.SendAsync("PlaySoundNotification", queue.QueueNumber, queue.TableGroup);
            // อัปเดตแดชบอร์ด/จอแสดงผลทั้งหมด
            await _hub.Clients.All.SendAsync("ReceiveQueueUpdate");

            return Ok(new { queue.Id, queue.QueueNumber, queue.TableGroup });
        }

        /// <summary>
        /// เปลี่ยนสถานะคิว (เช่น เข้าโต๊ะแล้ว = Served, เรียกซ้ำ, ยกเลิก)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, QueueStatus status)
        {
            var queue = await _queueService.UpdateStatusAsync(id, status);
            if (queue == null)
                return NotFound();

            // ถ้าเป็นการเรียก (Calling) ให้ยิงเสียงด้วย
            if (status == QueueStatus.Calling)
                await _hub.Clients.All.SendAsync("PlaySoundNotification", queue.QueueNumber, queue.TableGroup);

            await _hub.Clients.All.SendAsync("ReceiveQueueUpdate");
            return Ok(new { queue.Id, queue.QueueNumber, status = (int)queue.Status });
        }
    }
}
