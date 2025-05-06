using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using VetSqlClient.Exceptions;
using VetSqlClient.Models;
using VetSqlClient.Models.DTOs;
using VetSqlClient.Services;

namespace VetSqlClient.Controllers;

[ApiController]
[Route("[controller]")]
public class AnimalsController(IDbService dbService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllAnimals()
    {
        return Ok(await dbService.GetAnimalsDetailsAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAnimalById(
        [FromRoute] int id
    )
    {
        try
        {
            return Ok(await dbService.GetAnimalDetailsByIdAsync(id));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateAnimal(
        [FromBody] AnimalCreateDTO body
    )
    {
        var animal = await dbService.CreateAnimalAsync(body);
        return Created($"animals/{animal.Id}", animal);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ReplaceAnimalById(
        [FromRoute] int id,
        [FromBody] AnimalCreateDTO body
    )
    {
        try
        {
            await dbService.ReplaceAnimalByIdAsync(id, body);
            return NoContent();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAnimalById(
        [FromRoute] int id
    )
    {
        try
        {
            await dbService.RemoveAnimalByIdAsync(id);
            return NoContent();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpGet("{id}/visits")]
    public async Task<IActionResult> GetVisitsByAnimalId(
        [FromRoute]int id
    )
    {
        try
        {
            return Ok(await dbService.GetVisitsByAnimalIdAsync(id));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost("{id}/visits")]
    public async Task<IActionResult> AddVisit(
        [FromRoute] int id, 
        [FromBody] VisitCreateDTO body
    )
    {
        try
        {
            var visit = await dbService.CreateVisitAsync(id, body);
            return Created($"visits/{visit.Id}", visit);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    
}