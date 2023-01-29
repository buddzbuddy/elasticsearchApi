using System.Collections.Generic;

namespace elasticsearchApi.Models
{
    public static class Refs
    {

        public class RegionDistrictItem
        {
            public int RegionNo { get; set; }
            public int DistrictNo { get; set; }
        }
        public static List<RegionDistrictItem> RegionDistricts { get; set; } = new List<RegionDistrictItem>();
    }
}
