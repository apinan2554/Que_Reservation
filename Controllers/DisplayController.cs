using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservation_Que.Data;
using Reservation_Que.Models;

namespace Reservation_Que.Controllers
{
    /// <summary>
    /// [ส่วนที่ 4] หน้าจอแสดงผลหน้าร้าน (Queue Display) + API คิวที่กำลังเรียก/รอ
    /// </summary>
    public class DisplayController : Controller
    {
        private readonly AppDbContext _db;

        public DisplayController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>หน้าจอแสดงผลขนาดใหญ่สำหรับติดหน้าร้าน</summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// API: คิวที่กำลังเรียก (Calling) และคิวถัดไปที่รออยู่ (Pending)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDisplayData()
        {
            var calling = await _db.Queues
                .Where(q => q.Status == QueueStatus.Calling)
                .OrderByDescending(q => q.CalledAt)
                .Select(q => new { q.QueueNumber, q.TableGroup, q.CustomerCount })
                .ToListAsync();

            var waiting = await _db.Queues
                .Where(q => q.Status == QueueStatus.Pending)
                .OrderBy(q => q.CreatedAt)
                .Take(8)
                .Select(q => new { q.QueueNumber, q.TableGroup, q.CustomerCount })
                .ToListAsync();

            return Ok(new { calling, waiting });
        }
    }
}
