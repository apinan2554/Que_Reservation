using Microsoft.AspNetCore.SignalR;

namespace Reservation_Que.Hubs
{
    /// <summary>
    /// SignalR Hub ศูนย์กลางการสื่อสารแบบ Real-time
    /// - แดชบอร์ดพนักงาน: ฟัง event "ReceiveQueueUpdate"
    /// - หน้าจอหน้าร้าน: ฟัง event "PlaySoundNotification"
    /// </summary>
    public class QueueHub : Hub
    {
        /// <summary>
        /// สั่งให้ทุก client (โดยเฉพาะแดชบอร์ดพนักงาน) ดึงข้อมูลคิวใหม่แบบ Real-time
        /// เรียกได้ทั้งจากฝั่ง client และจากฝั่ง server (ผ่าน IHubContext)
        /// </summary>
        public async Task UpdateQueueList()
        {
            await Clients.All.SendAsync("ReceiveQueueUpdate");
        }

        /// <summary>
        /// ยิงสัญญาณให้หน้าจอหน้าร้านส่งเสียงเรียกคิว
        /// </summary>
        /// <param name="queueNumber">หมายเลขคิว เช่น A001</param>
        /// <param name="tableType">กลุ่มโต๊ะ เช่น A</param>
        public async Task CallQueue(string queueNumber, string tableType)
        {
            await Clients.All.SendAsync("PlaySoundNotification", queueNumber, tableType);
        }
    }
}
