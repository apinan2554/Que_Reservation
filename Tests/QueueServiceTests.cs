using Microsoft.EntityFrameworkCore;
using Reservation_Que.Data;
using Reservation_Que.Models;
using Reservation_Que.Services;
using Xunit;

namespace Reservation_Que.Tests
{
    /// <summary>
    /// [ส่วนที่ 5.2] Unit Test ตรวจสอบ Logic การเรียกคิวถัดไป
    /// ใช้ EF Core In-Memory Database
    /// </summary>
    public class QueueServiceTests
    {
        // สร้าง DbContext ใหม่บน In-Memory DB แยกกันในแต่ละเทสต์
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CallNextQueue_Should_Pick_Oldest_Pending_In_Group()
        {
            // Arrange: เพิ่มคิว 3 ใบในกลุ่ม A โดยเรียงเวลาต่างกัน
            using var db = CreateContext();
            var baseTime = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

            db.Queues.AddRange(
                new Queue { QueueNumber = "A001", CustomerName = "คนที่1", CustomerCount = 2, TableGroup = "A", Status = QueueStatus.Pending, CreatedAt = baseTime },
                new Queue { QueueNumber = "A002", CustomerName = "คนที่2", CustomerCount = 1, TableGroup = "A", Status = QueueStatus.Pending, CreatedAt = baseTime.AddMinutes(5) },
                new Queue { QueueNumber = "A003", CustomerName = "คนที่3", CustomerCount = 2, TableGroup = "A", Status = QueueStatus.Pending, CreatedAt = baseTime.AddMinutes(10) }
            );
            await db.SaveChangesAsync();

            var service = new QueueService(db);

            // Act
            var called = await service.CallNextQueueAsync("A");

            // Assert: ต้องได้คิวที่เก่าที่สุด (A001) และสถานะเปลี่ยนเป็น Calling
            Assert.NotNull(called);
            Assert.Equal("A001", called!.QueueNumber);
            Assert.Equal(QueueStatus.Calling, called.Status);
            Assert.NotNull(called.CalledAt);
        }

        [Fact]
        public async Task CallNextQueue_Should_Respect_TableGroup()
        {
            // Arrange: คิวกลุ่ม B เก่ากว่า แต่เราเรียกกลุ่ม A → ต้องได้คิวกลุ่ม A เท่านั้น
            using var db = CreateContext();
            var baseTime = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);

            db.Queues.AddRange(
                new Queue { QueueNumber = "B001", CustomerName = "กลุ่มB", CustomerCount = 4, TableGroup = "B", Status = QueueStatus.Pending, CreatedAt = baseTime },
                new Queue { QueueNumber = "A001", CustomerName = "กลุ่มA", CustomerCount = 2, TableGroup = "A", Status = QueueStatus.Pending, CreatedAt = baseTime.AddMinutes(30) }
            );
            await db.SaveChangesAsync();

            var service = new QueueService(db);

            // Act
            var called = await service.CallNextQueueAsync("A");

            // Assert: ต้องได้ A001 ไม่ใช่ B001 แม้ B จะเก่ากว่า
            Assert.NotNull(called);
            Assert.Equal("A001", called!.QueueNumber);
            Assert.Equal("A", called.TableGroup);
        }

        [Fact]
        public async Task CallNextQueue_Should_Skip_NonPending_Queues()
        {
            // Arrange: A001 ถูกเรียกไปแล้ว (Calling) → ต้องข้ามไปเรียก A002
            using var db = CreateContext();
            var baseTime = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);

            db.Queues.AddRange(
                new Queue { QueueNumber = "A001", CustomerName = "เรียกแล้ว", CustomerCount = 2, TableGroup = "A", Status = QueueStatus.Calling, CreatedAt = baseTime },
                new Queue { QueueNumber = "A002", CustomerName = "รออยู่", CustomerCount = 1, TableGroup = "A", Status = QueueStatus.Pending, CreatedAt = baseTime.AddMinutes(5) }
            );
            await db.SaveChangesAsync();

            var service = new QueueService(db);

            // Act
            var called = await service.CallNextQueueAsync("A");

            // Assert: ต้องได้ A002 (คิว Pending ที่เก่าสุด)
            Assert.NotNull(called);
            Assert.Equal("A002", called!.QueueNumber);
        }

        [Fact]
        public async Task CallNextQueue_Should_Return_Null_When_No_Pending()
        {
            // Arrange: ไม่มีคิว Pending ในกลุ่ม C
            using var db = CreateContext();
            var service = new QueueService(db);

            // Act
            var called = await service.CallNextQueueAsync("C");

            // Assert
            Assert.Null(called);
        }

        [Theory]
        [InlineData(1, "A")]
        [InlineData(2, "A")]
        [InlineData(3, "B")]
        [InlineData(4, "B")]
        [InlineData(5, "C")]
        [InlineData(10, "C")]
        public void ResolveTableGroup_Should_Map_By_CustomerCount(int count, string expected)
        {
            // ตรวจสอบการจัดกลุ่มโต๊ะตามจำนวนคน
            Assert.Equal(expected, QueueService.ResolveTableGroup(count));
        }
    }
}
