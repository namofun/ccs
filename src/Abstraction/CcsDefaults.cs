namespace Ccs
{
    /// <summary>
    /// The ccs defaults.
    /// </summary>
    public static class CcsDefaults
    {
        /// <summary>
        /// The menu for ccs jury navbar
        /// </summary>
        public const string NavbarJury = "Menu_CcsNavbar_Jury";

        /// <summary>
        /// The menu for ccs jury navbar
        /// </summary>
        public const string NavbarGym = "Menu_CcsNavbar_Gym";

        /// <summary>
        /// The menu for ccs jury navbar
        /// </summary>
        public const string NavbarTeam = "Menu_CcsNavbar_Team";

        /// <summary>
        /// The menu for ccs problemset navbar
        /// </summary>
        public const string NavbarProblemset = "Menu_CcsNavbar_Problemset";

        /// <summary>
        /// The menu for ccs jury navbar
        /// </summary>
        public const string NavbarPublic = "Menu_CcsNavbar_Public";

        /// <summary>
        /// The menu for jury home
        /// </summary>
        public const string JuryMenuList = "Menu_JuryHome";

        /// <summary>
        /// The menu for before contest
        /// </summary>
        public const string JuryMenuBefore = "Menu_JuryHome_BeforeContest";

        /// <summary>
        /// The menu for during contest
        /// </summary>
        public const string JuryMenuDuring = "Menu_JuryHome_DuringContest";

        /// <summary>
        /// The menu for administrator
        /// </summary>
        public const string JuryMenuAdmin = "Menu_JuryHome_Administrator";

        /// <summary>
        /// The menu for import/export components
        /// </summary>
        public const string ComponentImportExport = "Component_Contest_ImportExport";

        /// <summary>
        /// The configuration name for last rating change time
        /// </summary>
        public const string ConfigurationLastRatingChangeTime = "contest_last_rating_change_time";

        /// <summary>
        /// The kind for contests
        /// </summary>
        public const int KindDom = 0;

        /// <summary>
        /// The kind for practices
        /// </summary>
        public const int KindGym = 1;

        /// <summary>
        /// The kind for problemsets
        /// </summary>
        public const int KindProblemset = 2;

        /// <summary>
        /// The ranking strategy for XCPC
        /// </summary>
        public const int RuleXCPC = 0;

        /// <summary>
        /// The ranking strategy for IOI
        /// </summary>
        public const int RuleIOI = 1;

        /// <summary>
        /// The ranking strategy for Codeforces
        /// </summary>
        public const int RuleCodeforces = 2;

        /// <summary>
        /// The default scoreboard paging size
        /// </summary>
        public const int DefaultScoreboardPagingSize = 50;

        /// <summary>
        /// Whether current system supports rating
        /// </summary>
        public static bool SupportsRating { get; internal set; }
    }
}
