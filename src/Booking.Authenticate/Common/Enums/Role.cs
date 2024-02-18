using System.ComponentModel;

namespace Booking.Authenticate.Common.Enums
{
    public enum Role
    {
        [Description("Administrator")]
        Administrator = 0,

        [Description("TenantAdmin")]
        TenantAdmin = 5,

        [Description("TenantOperator")]
        TenantOperator = 10,

        [Description("TenantStaff")]
        TenantStaff = 15,

        [Description("TenantUser")]
        TenantUser = 20
    }
}
