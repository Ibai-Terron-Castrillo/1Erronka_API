using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;
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
            .Where(u =>
                u.Izena == request.Erabiltzailea &&
                u.Pasahitza == request.Pasahitza
            )
            .Fetch(u => u.Langilea)
            .FirstOrDefault();

        if (user == null)
            return Unauthorized(new { message = "Erabiltzaile edo pasahitz okerra" });

        if (user.Langilea.Lanpostua.Id != 1)
            return Unauthorized(new { message = "Ez duzu baimenik sartzeko" });

        return Ok(new
        {
            message = "Login zuzena!",
            id = user.Id,
            langileId = user.Langilea.Id
        });
    }

}

public class LoginRequest
{
    public string Erabiltzailea { get; set; }
    public string Pasahitza { get; set; }
}
