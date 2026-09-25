using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Reservation_Que.Hubs;
using Reservation_Que.Models;
using Reservation_Que.Services;

namespace Reservation_Que.Controllers
{
    /// <summary>
    /// [ส่วนที่ 2] ระบบสำหรับลูกค้า: หน้าลงทะเบียนรับคิว + ตรวจสอบพิกัด
    /// </summary>
    public class CustomerController : Controller
    {
        private readonly QueueService _queueService;
        private readonly IHubContext<QueueHub> _hub;
        private readonly StoreSettings _store;

        public CustomerController(
            QueueService queueService,
            IHubContext<QueueHub> hub,
            IOptions<StoreSettings> storeOptions)
        {
            _queueService = queueService;
            _hub = hub;
            _store = storeOptions.Value;
        }

        /// <summary>
        /// หน้าฟอร์มลงทะเบียนรับคิว
        /// - มาจาก QR (src=qr) → บังคับ GPS
        /// - มาจากเมนู (ไม่มี src) → ไม่บังคับ GPS
        /// </summary>
        public IActionResult Register(string? src)
        {
            // ส่งไปให้ View รู้ว่าต้องบังคับ GPS หรือไม่
            ViewBag.RequireGps = src == "qr";
            return View();
        }

        /// <summary>หน้าแสดงผลหลังรับคิวสำเร็จ (พร้อมพิมพ์สลิป)</summary>
        public IActionResult Ticket(int id)
        {
            return View(id);
        }

        /// <summary>หน้าติดตามสถานะคิวของลูกค้า (real-time + กระพริบ + เสียงเมื่อถึงคิว)</summary>
        public IActionResult Status(int id)
        {
            return View(id);
        }

        /// <summary>
        /// API: สถานะคิวของลูกค้ารายบุคคล
        /// - myStatus: สถานะคิวของตัวเอง (0-3)
        /// - callingNow: เลขคิวที่กำลังเรียกอยู่ (กลุ่มเดียวกัน)
        /// - aheadCount: จำนวนคิวที่รออยู่ก่อนหน้าเรา (ในกลุ่มเดียวกัน)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> QueueStatus(int id)
        {
            var me = await _queueService.GetByIdAsync(id);
            if (me == null)
                return NotFound(new { message = "ไม่พบคิวนี้" });

            // จำนวนคิวที่รออยู่ก่อนเรา (Pending และสร้างก่อนเรา ในกลุ่มเดียวกัน)
            int aheadCount = await _queueService.CountAheadAsync(me);

            // คิวที่กำลังเรียกอยู่ในกลุ่มเดียวกัน (ล่าสุด)
            var callingNow = await _queueService.GetLatestCallingNumberAsync(me.TableGroup);

            return Ok(new
            {
                id = me.Id,
                myNumber = me.QueueNumber,
                tableGroup = me.TableGroup,
                myStatus = (int)me.Status,
                isMyTurn = me.Status == Models.QueueStatus.Calling,
                aheadCount,
                callingNow
            });
        }

        /// <summary>
        /// [ส่วนที่ 2.3] API รับคิว + ตรวจสอบพิกัดด้วยสูตร Haversine
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterQueueDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "ข้อมูลไม่ถูกต้อง" });

            // --- Geolocation Protection ---
            // ตรวจ GPS เฉพาะกรณีเข้ามาจาก QR (จากเมนูไม่ต้องตรวจ)
            if (_store.EnableGeoCheck && dto.FromQr)
            {
                if (dto.Latitude == null || dto.Longitude == null)
                    return BadRequest(new { message = "กรุณาเปิดการเข้าถึงตำแหน่ง (GPS) เพื่อจองคิว" });

                double distance = GeoService.HaversineDistance(
                    _store.StoreLatitude, _store.StoreLongitude,
                    dto.Latitude.Value, dto.Longitude.Value);

                if (distance > _store.AllowedRadiusMeters)
                {
                    return BadRequest(new
                    {
                        message = $"คุณอยู่ห่างจากร้านเกินกว่าที่กำหนด (~{distance:F0} ม.) กรุณาสแกนคิวที่หน้าร้าน",
                        distance
                    });
                }
            }

            // --- สร้างคิว ---
            var queue = await _queueService.CreateQueueAsync(dto.CustomerName, dto.CustomerCount);

            // แจ้งแดชบอร์ดพนักงานให้อัปเดตแบบ Real-time
            await _hub.Clients.All.SendAsync("ReceiveQueueUpdate");

            return Ok(new
            {
                id = queue.Id,
                queueNumber = queue.QueueNumber,
                tableGroup = queue.TableGroup,
                customerName = queue.CustomerName,
                customerCount = queue.CustomerCount
            });
        }
    }
}
