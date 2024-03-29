﻿#nullable disable
using Microsoft.AspNetCore.Mvc.DataTables;

namespace Xylab.Contesting.Registration.TeachingClass
{
    public class RegisterResult
    {
        [DtDisplay(1, "teamname")]
        public string Name { get; set; }

        [DtDisplay(2, "result")]
        public string Result { get; set; }
    }
}
