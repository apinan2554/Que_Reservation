using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservation_Que.Data;
using Reservation_Que.Models;

namespace Reservation_Que.Controllers
{
    /// <summary>
    /// [ก] หน้าตั้งค่าของร้าน: กำหนดเวลานั่งสูงสุด (นาที) ต่อกลุ่มโต๊ะ
    /// </summary>
    public class SettingsController : Controller
    {
        private readonly AppDbContext _db;

        public SettingsController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>หน้าตั้งค่า (View)</summary>
        public async Task<IActionResult> Index()
        {
            var settings = await _db.TableSettings
                .OrderBy(t => t.TableGroup)
                .ToListAsync();
            return View(settings);
        }

        /// <summary>API: ดึงค่าตั้งเวลาต่อกลุ่มโต๊ะทั้งหมด (ใช้โดยหน้าจองคิว)</summary>
        [HttpGet]
        public async Task<IActionResult> GetTableSettings()
        {
            var settings = await _db.TableSettings
                .OrderBy(t => t.TableGroup)
                .Select(t => new { t.TableGroup, t.Description, t.MaxMinutes })
                .ToListAsync();
            return Ok(settings);
        }

        /// <summary>API: บันทึกเวลานั่งสูงสุดของกลุ่มโต๊ะ</summary>
        [HttpPost]
        public async Task<IActionResult> UpdateMaxMinutes([FromBody] List<TableSettingDto> items)
        {
            if (items == null || items.Count == 0)
                return BadRequest(new { message = "ไม่มีข้อมูลให้บันทึก" });

            foreach (var item in items)
            {
                var setting = await _db.TableSettings
                    .FirstOrDefaultAsync(t => t.TableGroup == item.TableGroup);
                if (setting != null)
                    setting.MaxMinutes = item.MaxMinutes;
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "บันทึกเรียบร้อย" });
        }
    }

    /// <summary>ข้อมูลที่ส่งมาจากหน้าตั้งค่าเพื่อบันทึกเวลาต่อกลุ่ม</summary>
    public class TableSettingDto
    {
        public string TableGroup { get; set; } = string.Empty;
        public int MaxMinutes { get; set; }
    }
}
