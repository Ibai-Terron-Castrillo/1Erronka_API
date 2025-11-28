using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    [HttpPost]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        using var session = NHibernateHelper.OpenSession();

        var user = session.Query<Erabiltzailea>()
            .FirstOrDefault(u =>
                u.Izena == request.Erabiltzailea &&
                u.Pasahitza == request.Pasahitza
            );

        if (user == null)
            return Unauthorized(new { message = "Errorea: Erabiltzaile edo pasahitz okerra" });

        return Ok(new
        {
            message = "Login egokia",
            id = user.Id,
            langileId = user.LangileId
        });
    }
}

public class LoginRequest
{
    public string Erabiltzailea { get; set; }
    public string Pasahitza { get; set; }
}
