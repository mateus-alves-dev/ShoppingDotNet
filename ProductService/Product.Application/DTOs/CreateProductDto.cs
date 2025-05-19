namespace Product.Application.DTOs;

public class CreateProductDto
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string ImageUrl { get; set; } = "";
}
