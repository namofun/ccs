using System.ComponentModel;

namespace Ccs.Registration.TeachingClass
{
    public class InputModel
    {
        [DisplayName("Class")]
        public int GroupId { get; set; }

        [DisplayName("Only temporary users are added")]
        public bool AddNonTemporaryUser { get; set; }

        [DisplayName("Affiliation")]
        public int AffiliationId { get; set; }

        [DisplayName("Category")]
        public int CategoryId { get; set; }
    }
}
