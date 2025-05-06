using System.ComponentModel.DataAnnotations;

namespace VetSqlClient.Models.DTOs;

public class VisitCreateDTO
{
    public required DateTime Date { get; set; }
    [Length(1, 500)]
    public required string Description { get; set; }
    public required double Price { get; set; }
}