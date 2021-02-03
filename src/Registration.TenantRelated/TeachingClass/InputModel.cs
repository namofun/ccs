#nullable disable
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.ComponentModel;
using Tenant.Entities;

namespace Ccs.Registration.TeachingClass
{
    public class InputModel
    {
        [BindNever]
        public IReadOnlyDictionary<int, Category> Categories { get; set; }

        [BindNever]
        public IReadOnlyList<Class> Classes { get; set; }

        [DisplayName("Class")]
        public int ClassId { get; set; }

        [DisplayName("Category")]
        public int CategoryId { get; set; }
    }
}
