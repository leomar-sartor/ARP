using ARP.Entity.Helpers;

namespace ARP.Entity
{
    public class Base
    {
        public Base()
        {
            UserId = 0;
            CreatedAt = DateTimeHelper.CreateUtcDateTime();
            UpdatedAt = DateTimeHelper.CreateUtcDateTime();
        }

        public long Id { get; set; }

        public long? UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}
