# Que_Reservation

ระบบจองคิวออนไลน์สำหรับร้านอาหารและเครื่องดื่ม พัฒนาด้วย ASP.NET Core (.NET 6) MVC + EF Core (SQLite) + SignalR

## ฟีเจอร์หลัก
- ลูกค้าจองคิวผ่าน QR (ตรวจตำแหน่ง GPS ด้วยสูตร Haversine) หรือผ่านเมนู (ไม่บังคับ GPS)
- แดชบอร์ดพนักงานแบบ Kanban อัปเดต real-time ด้วย SignalR
- จอแสดงผลหน้าร้าน พร้อมเสียงเรียกคิวภาษาไทย (Web Speech API)
- พิมพ์สลิปคิว (58/80mm) พร้อม QR ผูกกับคิวสำหรับติดตามสถานะ
- หน้าติดตามสถานะคิวของลูกค้า (กระพริบ + เสียงเมื่อถึงคิว)
- ตั้งค่าเวลานั่งต่อกลุ่มโต๊ะ

## รันบนเครื่อง
```bash
dotnet run --project Reservation_Que.csproj
```
เปิด http://localhost:5022

## รันด้วย Docker
```bash
docker build -t que-reservation .
docker run -p 8080:8080 que-reservation
```
เปิด http://localhost:8080

## Deploy บน Render
- สร้าง Web Service แบบ Docker ชี้ไปที่ repo นี้
- Render จะ build จาก `Dockerfile` และส่งพอร์ตผ่าน env `PORT` (แอปอ่านค่าเองแล้ว)
