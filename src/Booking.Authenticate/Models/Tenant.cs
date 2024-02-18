namespace Booking.Authenticate.Models
{
    public class Tenant
    {
        public Tenant()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }

        public string ApplicationUserId { get; set; }

        public string OwnerId { get; set; }

        public bool IsDelete { get; set; }

        public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; }
    }
}