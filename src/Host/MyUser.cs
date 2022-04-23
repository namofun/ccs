using SatelliteSite.IdentityModule.Entities;
using Xylab.Contesting.Entities;
using Xylab.Tenant.Entities;

namespace SatelliteSite
{
    public class MyUser : User, IUserWithStudent, IUserWithRating
    {
        public string StudentId { get; set; }

        public string StudentEmail { get; set; }

        public bool StudentVerified { get; set; }

        public int? Rating { get; set; }
    }
}
