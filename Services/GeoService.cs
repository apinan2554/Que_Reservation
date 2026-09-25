namespace Reservation_Que.Services
{
    /// <summary>
    /// บริการคำนวณระยะทางเชิงภูมิศาสตร์ (Geolocation Protection)
    /// </summary>
    public static class GeoService
    {
        private const double EarthRadiusMeters = 6371000.0; // รัศมีโลกโดยเฉลี่ย (เมตร)

        /// <summary>
        /// คำนวณระยะห่างระหว่างสองพิกัด (ละติจูด/ลองจิจูด) ด้วยสูตร Haversine
        /// คืนค่าเป็นหน่วยเมตร
        /// </summary>
        public static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusMeters * c;
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    }
}
