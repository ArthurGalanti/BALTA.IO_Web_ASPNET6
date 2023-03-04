using BlogAPI.Data;
using BlogAPI.Extensions;
using BlogAPI.Models;
using BlogAPI.ViewModels;
using BlogAPI.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BlogAPI.Controllers;

[ApiController]
public class CategoryController : ControllerBase
{
    [HttpGet ("v1/categories")]
    public async Task<IActionResult> GetAsync(
        [FromServices] IMemoryCache cache,
        [FromServices] BlogDataContext context)
    {
        try
        {
            var categories = cache.GetOrCreateAsync("CategoriesCache", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return context.Categories.ToListAsync();
            });
            return Ok(new ResultViewModel<List<Category>>(await categories));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<List<Category>>(" 05XE51 - Erro interno do servidor"));
        }
    }
    
    [HttpGet ("v1/categories/{id:int}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromRoute] int id,
        [FromServices] BlogDataContext context)
    {
        try
        {
            var category = await context
                .Categories
                .FirstOrDefaultAsync(x=>x.Id == id);
            if (category == null)
                return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));
            return Ok(new ResultViewModel<Category>(category));
        }
        catch
        {
            return StatusCode(500,new ResultViewModel<List<Category>>(" 05XE51 - Erro interno do servidor"));
        }
    }
    
    [HttpPost ("v1/categories/")]
    public async Task<IActionResult> PostAsync(
        [FromBody] EditorCategoryViewModel model,
        [FromServices] BlogDataContext context)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));
        try
        {
            var category = new Category
            {
                Id = 0,
                Name = model.Name,
                Slug = model.Slug.ToLower()
            };
            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();
            return Created($"v1/categories/{category.Id}", new ResultViewModel<Category>(category));
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, new ResultViewModel<Category>(" 05XE1 - Não foi possível incluir a categoria"));
        }
        catch 
        {
            return StatusCode(500, new ResultViewModel<Category>(" 05XE53 - Erro interno do servidor"));
        }
    }
    
    [HttpPut ("v1/categories/{id:int}")]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute]int id,
        [FromBody] EditorCategoryViewModel model,
        [FromServices] BlogDataContext context)
    {
        try
        {
            var category = await context
                .Categories
                .FirstOrDefaultAsync(x=>x.Id == id);
            if (category == null)
                return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));

            category.Name = model.Name;
            category.Slug = model.Slug;

            context.Categories.Update(category);
            await context.SaveChangesAsync();
        
            return Ok(new ResultViewModel<Category>(category));
        }
        catch (DbUpdateException)
        {
            return StatusCode(500,new ResultViewModel<Category>(" 05XE2 - Não foi possível alterar a categoria"));
        }
        catch (Exception)
        {
            return StatusCode(500,new ResultViewModel<Category>( " 05XE54 - Erro interno do servidor"));
        }
    }
    
    [HttpDelete ("v1/categories/{id:int}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute]int id,
        [FromServices] BlogDataContext context)
    {
        try
        {
            var category = await context
                .Categories
                .FirstOrDefaultAsync(x=>x.Id == id);
            if (category == null)
                return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));

            context.Categories.Remove(category);
            await context.SaveChangesAsync();
        
            return Ok(new ResultViewModel<Category>(category));
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, new ResultViewModel<Category>(" 05XE3 - Não foi possível excluir a categoria"));
        }
        catch (Exception)
        {
            return StatusCode(500, new ResultViewModel<Category>(" 05XE55 - Erro interno do servidor"));
        }
    }
    
}