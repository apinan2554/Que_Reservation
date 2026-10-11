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
        /// ดึง scheme จริงที่ลูกค้าใช้ (https บน Render, http บน local dev)
        /// Render ส่ง X-Forwarded-Proto มา ต้องอ่านตรงๆ เผื่อ UseForwardedHeaders ยังไม่ kick in
        /// </summary>
        private string GetScheme()
        {
            // อ่านจาก header X-Forwarded-Proto ก่อน (จาก Render proxy)
            var forwardedProto = Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedProto))
                return forwardedProto;
            // fallback: ใช้ Request.Scheme ปกติ (local dev = http)
            return Request.Scheme;
        }

        public IActionResult Index()
        {
            string registerUrl = Url.Action("Register", "Customer", new { src = "qr" }, GetScheme())!;
            ViewBag.RegisterUrl = registerUrl;
            return View();
        }

        [HttpGet]
        public IActionResult Image(string? url)
        {
            url ??= Url.Action("Register", "Customer", new { src = "qr" }, GetScheme())!;

            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var pngQr = new PngByteQRCode(data);
            byte[] qrBytes = pngQr.GetGraphic(20);
            return File(qrBytes, "image/png");
        }
    }
}
