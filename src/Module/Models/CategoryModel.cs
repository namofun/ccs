using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SatelliteSite.ContestModule.Models
{
    public class CategoryModel
    {
        public int Id { get; set; }

        [DisplayName("Name")]
        [Required]
        public string Name { get; set; }

        [DisplayName("Color")]
        [Required]
        public string Color { get; set; }

        [DisplayName("Sort Order")]
        public int SortOrder { get; set; }

        [DisplayName("Is Public")]
        public bool IsPublic { get; set; }

        [DisplayName("Is eligible")]
        public bool IsEligible { get; set; }
    }
}
