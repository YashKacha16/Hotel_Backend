using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hotel_Backend.Models
{
    public class TableCategory
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public int Position { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public ICollection<RestaurantTable> Tables { get; set; } = new List<RestaurantTable>();
    }
}
