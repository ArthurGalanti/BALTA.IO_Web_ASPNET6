using System.Text.RegularExpressions;
using Azure.Storage.Blobs;
using BlogAPI.Data;
using BlogAPI.Extensions;
using BlogAPI.Models;
using BlogAPI.Services;
using BlogAPI.ViewModels;
using BlogAPI.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;

namespace BlogAPI.Controllers;

[ApiController]
public class AccountController : ControllerBase
{
    [HttpPost("v1/accounts")]
    public async Task<IActionResult> Post(
        [FromBody] RegisterViewModel model,
        [FromServices] BlogDataContext context,
        [FromServices] EmailService emailService
        )
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            Slug = model.Email.Replace("@", "-").Replace(".", "-"),
            PasswordHash = PasswordHasher.Hash(model.Password)
        };
        try
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            emailService.Send(
                user.Name,
                user.Email,
                "Bem vindo ao blog!",
                body: $"{user.Name}, sua senha é <strong>{model.Password}</strong>");

            return Ok(new ResultViewModel<dynamic>($"E-mail {user.Email} cadastrado com sucesso!",null));
        }
        catch (DbUpdateException)
        {
            return StatusCode(400, new ResultViewModel<string>("05X99 - Este E-mail já está cadastrado"));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
        }

    }

    [HttpPost("v1/accounts/login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginViewModel model,
        [FromServices] BlogDataContext context,
        [FromServices] TokenService tokenService)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

        var user = await context
            .Users
            .AsNoTracking()
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.Email == model.Email);

        if (user == null)
            return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválida"));

        if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
            return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválida"));

        try
        {
            var token = tokenService.GenerateToken(user);
            return Ok(new ResultViewModel<string>(token, null));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResultViewModel<string>($"05X08 - Falha interna no servidor {ex}"));
        }
    }

    [Authorize]
    [HttpPost("v1/accounts/upload-image")]
    public async Task<IActionResult> UploadImage(
        [FromBody] UploadImageViewModel model,
        [FromServices] BlogDataContext context)
    {
        var email = User.Identity?.Name;
        var fileName = $"{email}.jpg";
        var data = new Regex(@"^data:image\/[a-z]+;base64,")
            .Replace(model.Base64Image, "");
        var bytes = Convert.FromBase64String(data);
        var blobClient = new BlobClient(Configuration.AzureStorageConnectionString, "user-images", fileName);
        var user = await context
            .Users
            .FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
            return NotFound(new ResultViewModel<User>("Usuário não encontrado"));
        
        try
        {
            using var stream = new MemoryStream(bytes);
            await blobClient.UploadAsync(stream);
            user.Image = blobClient.Uri.AbsoluteUri;
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna"));
        }
        return Ok(new ResultViewModel<string>($"Imagem alterada com sucesso! URL: {blobClient.Uri.AbsoluteUri}", null));
    }
}