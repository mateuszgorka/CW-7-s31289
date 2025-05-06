using System.ComponentModel.DataAnnotations;

namespace VetSqlClient.Models.DTOs;

public class AnimalCreateDTO
{
    [Length(1, 30)]
    public required string Name { get; set; }
    public required double Weight { get; set; }
    [Length(1, 30)]
    public required string Category { get; set; }
    [Length(1, 30)]
    public required string CoatColor { get; set; }
}