namespace Hotel_Backend.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public string? Image { get; set; }
        public bool Veg { get; set; }
        public bool Available { get; set; }
        public int Position { get; set; } = 0;
    }
}
