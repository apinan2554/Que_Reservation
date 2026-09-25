namespace Reservation_Que.Models
{
    /// <summary>
    /// ค่าตั้งค่าของร้าน (ผูกกับ section "StoreSettings" ใน appsettings.json)
    /// </summary>
    public class StoreSettings
    {
        public double StoreLatitude { get; set; }
        public double StoreLongitude { get; set; }

        /// <summary>รัศมีที่อนุญาตให้จองคิวได้ (เมตร)</summary>
        public double AllowedRadiusMeters { get; set; } = 100;

        /// <summary>เปิด/ปิดการตรวจสอบพิกัด (ปิดไว้เพื่อทดสอบได้สะดวก)</summary>
        public bool EnableGeoCheck { get; set; } = true;
    }
}
