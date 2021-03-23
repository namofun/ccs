namespace SatelliteSite.ContestModule.Routing
{
    public interface IMinimalSiteFeature
    {
        int ContestId { get; }

        string OriginalPath { get; }

        string Prefix { get; }
    }

    public class MinimalSiteFeature : IMinimalSiteFeature
    {
        public int ContestId { get; }

        public string OriginalPath { get; }

        public string Prefix { get; }

        public MinimalSiteFeature(int cid, string origPath, string prefix)
        {
            ContestId = cid;
            OriginalPath = origPath;
            Prefix = prefix;
        }
    }
}
