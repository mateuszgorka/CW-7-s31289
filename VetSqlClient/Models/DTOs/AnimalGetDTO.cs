namespace VetSqlClient.Models.DTOs;

public class AnimalGetDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Weight { get; set; }
    public string Category { get; set; }
    public string CoatColor { get; set; }
}