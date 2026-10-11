# System Design — ระบบจองคิวออนไลน์ (Que_Reservation)

ไดอะแกรมทั้งหมดเขียนด้วย Mermaid นำโค้ดในแต่ละบล็อกไปวางที่ https://mermaid.live ได้ทันที

---

## 1) High-Level Architecture (ภาพรวมระบบ)

```mermaid
graph TB
    subgraph Clients["ผู้ใช้งาน"]
        C["📱 ลูกค้า (มือถือ)"]
        S["🧑‍💼 พนักงาน (Dashboard)"]
        D["📺 จอแสดงผลหน้าร้าน"]
    end

    subgraph App["ASP.NET Core MVC (.NET 6) บน Render"]
        direction TB
        RP["Reverse Proxy (Render)<br/>HTTPS → HTTP + Forwarded Headers"]
        subgraph Controllers["Controllers"]
            HC["HomeController"]
            CC["CustomerController"]
            SC["StaffController"]
            DC["DisplayController"]
            QC["QrController"]
            STC["SettingsController"]
        end
        subgraph Services["Services (Business Logic)"]
            QS["QueueService<br/>สร้าง/เรียกคิว, จัดกลุ่มโต๊ะ"]
            GS["GeoService<br/>Haversine (ตรวจรัศมี)"]
        end
        HUB["QueueHub (SignalR)<br/>Real-time"]
        EF["EF Core (DbContext)"]
    end

    DB[("SQLite<br/>queue.db")]

    C -->|HTTPS| RP
    S -->|HTTPS| RP
    D -->|HTTPS| RP
    RP --> Controllers
    CC --> QS
    CC --> GS
    SC --> QS
    STC --> EF
    DC --> EF
    QS --> EF
    EF --> DB

    SC -. ส่งสัญญาณเรียกคิว .-> HUB
    CC -. คิวใหม่ .-> HUB
    HUB -. ReceiveQueueUpdate .-> S
    HUB -. PlaySoundNotification .-> D
    HUB -. อัปเดตสถานะ .-> C
```

---

## 2) Flow การจองคิวผ่าน QR (ตรวจ GPS)

```mermaid
flowchart TD
    A["ลูกค้าสแกน QR หน้าร้าน"] --> B["เปิดหน้า /Customer/Register?src=qr"]
    B --> C{"EnableGeoCheck<br/>เปิดอยู่ไหม?"}
    C -->|ไม่| G["กรอกชื่อ + จำนวนคน"]
    C -->|ใช่| D["ขอพิกัด GPS จากมือถือ"]
    D --> E{"ได้พิกัดไหม?"}
    E -->|ไม่| F["ปฏิเสธ: กรุณาเปิด GPS"]
    E -->|ใช่| G
    G --> H["POST /Customer/Register (ส่ง lat/lng + fromQr)"]
    H --> I{"fromQr && EnableGeoCheck?"}
    I -->|ใช่| J["คำนวณระยะด้วย Haversine"]
    J --> K{"อยู่ในรัศมี?"}
    K -->|เกินรัศมี| L["ปฏิเสธ: อยู่ไกลเกินไป"]
    K -->|อยู่ในรัศมี| M["สร้างคิว (QueueService)"]
    I -->|ไม่| M
    M --> N["ยิง SignalR → อัปเดต Dashboard"]
    N --> O["ไปหน้า /Customer/Status/{id}"]
```

---

## 3) Flow การเรียกคิว (พนักงาน → Real-time)

```mermaid
sequenceDiagram
    participant Staff as พนักงาน (Dashboard)
    participant SC as StaffController
    participant QS as QueueService
    participant DB as SQLite
    participant Hub as QueueHub (SignalR)
    participant Disp as จอหน้าร้าน
    participant Cust as มือถือลูกค้า

    Staff->>SC: POST /Staff/CallNextQueue?tableGroup=A
    SC->>QS: CallNextQueueAsync("A")
    QS->>DB: หาคิว Pending เก่าสุดในกลุ่ม A
    DB-->>QS: คิว A001
    QS->>DB: อัปเดตสถานะเป็น Calling
    QS-->>SC: คืนคิว A001
    SC->>Hub: PlaySoundNotification(A001)
    SC->>Hub: ReceiveQueueUpdate
    Hub-->>Disp: เล่นเสียงเรียก "คิว A001"
    Hub-->>Staff: รีเฟรช Kanban อัตโนมัติ
    Hub-->>Cust: อัปเดตสถานะ (ถึงคิว → กระพริบ + เสียง)
```

---

## 4) สถานะของคิว (State Diagram)

```mermaid
stateDiagram-v2
    [*] --> Pending: ลูกค้ารับคิว
    Pending --> Calling: พนักงานเรียกคิว
    Calling --> Served: ลูกค้าเข้าโต๊ะ
    Calling --> Calling: เรียกซ้ำ
    Pending --> Cancelled: ยกเลิก
    Calling --> Cancelled: ยกเลิก
    Served --> [*]
    Cancelled --> [*]
```

---

## 5) โครงสร้างข้อมูล (ER Diagram)

```mermaid
erDiagram
    QUEUE {
        int Id PK
        string QueueNumber "เช่น A001"
        string CustomerName
        int CustomerCount
        string TableGroup "A / B / C"
        int Status "0=Pending,1=Calling,2=Served,3=Cancelled"
        datetime CreatedAt
        datetime CalledAt "nullable"
    }
    TABLESETTING {
        int Id PK
        string TableGroup "unique"
        string Description "เช่น 1-2 คน"
        int MaxMinutes "เวลานั่งสูงสุด"
    }
    QUEUE }o--|| TABLESETTING : "จัดกลุ่มด้วย TableGroup"
```
```
