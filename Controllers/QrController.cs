using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace Reservation_Que.Controllers
{
    /// <summary>
    /// [ส่วนที่ 2.1] สร้าง QR Code ของ URL หน้าจองคิว เพื่อให้ลูกค้าสแกนหน้าร้าน
    /// </summary>
    public class QrController : Controller
    {
        /// <summary>
        /// หน้าเว็บสำหรับโชว์ QR Code (ให้พนักงานพิมพ์ไปติดหน้าร้าน)
        /// </summary>
        public IActionResult Index()
        {
            // สร้าง URL หน้าจองคิวแบบเต็ม (absolute) ตาม host ปัจจุบัน (ติด src=qr)
            string registerUrl = Url.Action("Register", "Customer", new { src = "qr" }, Request.Scheme)!;
            ViewBag.RegisterUrl = registerUrl;
            return View();
        }

        /// <summary>
        /// คืนค่าไฟล์ภาพ QR Code (PNG) ของ URL ที่กำหนด
        /// เรียกใช้: /Qr/Image  (จะเจน QR ของหน้าจองคิวอัตโนมัติ)
        /// </summary>
        [HttpGet]
        public IActionResult Image(string? url)
        {
            // ถ้าไม่ระบุ url ให้ใช้ URL หน้าจองคิวของร้าน (ติด src=qr เพื่อบังคับ GPS)
            url ??= Url.Action("Register", "Customer", new { src = "qr" }, Request.Scheme)!;

            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

            // PngByteQRCode: แปลง QR เป็น byte[] ของไฟล์ PNG
            var pngQr = new PngByteQRCode(data);
            byte[] qrBytes = pngQr.GetGraphic(20);

            return File(qrBytes, "image/png");
        }
    }
}
