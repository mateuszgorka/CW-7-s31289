namespace VetSqlClient.Models;

public class Visit
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; }
    public double Price { get; set; }
    public int AnimalId { get; set; }
}