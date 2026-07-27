using System;
using System.Collections.Generic;

namespace Hotel_Backend.Models
{
    public class TableMergeGroup
    {
        public int Id { get; set; }
        public DateTime MergedAt { get; set; } = DateTime.UtcNow;
        public string MergedBy { get; set; } = string.Empty;

        public ICollection<RestaurantTable> Tables { get; set; } = new List<RestaurantTable>();
    }
}
