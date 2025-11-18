namespace WebApplication1.Domain.Entities
{
    public class Product
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ProductCategory Category { get; set; }
        public string? Content { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public enum ProductCategory
    {
        Food,
        NonFood
    }
}
