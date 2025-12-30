using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly ISessionFactory _sessionFactory;

    public LoginController(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    // Login
    [HttpPost]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Erabiltzailea) || string.IsNullOrEmpty(request.Pasahitza))
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Erabiltzailea eta pasahitza beharrezkoak dira"
            });
        }

        using var session = _sessionFactory.OpenSession();

        try
        {
            var user = session.Query<Erabiltzailea>()
                .Where(u => u.Izena == request.Erabiltzailea &&
                           u.Pasahitza == request.Pasahitza)
                .Fetch(u => u.Langilea)
                .ThenFetch(l => l.Lanpostua)
                .FirstOrDefault();

            if (user == null)
            {
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "Erabiltzaile edo pasahitz okerra"
                });
            }

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Login arrakastatsua",
                User = new UserInfo
                {
                    Id = user.Id,
                    Erabiltzailea = user.Izena,
                    LangileId = user.Langilea.Id,
                    Izena = user.Langilea.Izena,
                    Abizena1 = user.Langilea.Abizena1,
                    Abizena2 = user.Langilea.Abizena2,
                    Lanpostua = user.Langilea.Lanpostua.Izena,
                    LanpostuaId = user.Langilea.Lanpostua.Id,
                    IsAdmin = user.Langilea.Lanpostua.Id == 1
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new LoginResponse
            {
                Success = false,
                Message = $"Errorea: {ex.Message}"
            });
        }
    }

    // Login ADMIN
    [HttpPost("admin")]
    public IActionResult LoginAdmin([FromBody] LoginRequest request)
    {
        var result = Login(request);

        if (result is OkObjectResult okResult && okResult.Value is LoginResponse response)
        {
            if (response.Success)
            {
                if (response.User.LanpostuaId != 1) // 1 = Admin
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Admin baimena behar da"
                    });
                }

                response.Message = "Admin login arrakastatsua";
                response.User.IsAdmin = true;
                return Ok(response);
            }
            return Unauthorized(response);
        }

        return result;
    }

    // Login TPV
    [HttpPost("tpv")]
    public IActionResult LoginTPV([FromBody] LoginRequest request)
    {
        var result = Login(request);

        if (result is OkObjectResult okResult && okResult.Value is LoginResponse response)
        {
            if (response.Success)
            {
                var allowedRoles = new[] { 1, 2, 3 }; // Admin(1), Zerbitzaria(2), Sukaldaria(3)

                if (!allowedRoles.Contains(response.User.LanpostuaId))
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Ez duzu baimenik TPV erabiltzeko"
                    });
                }

                response.Message = "TPV login arrakastatsua";
                response.User.CanCreateOrders = response.User.LanpostuaId == 2 || response.User.LanpostuaId == 1;
                response.User.CanViewKitchen = response.User.LanpostuaId == 3 || response.User.LanpostuaId == 1;
                return Ok(response);
            }
            return Unauthorized(response);
        }

        return result;
    }



    public class LoginRequest
    {
        public string Erabiltzailea { get; set; }
        public string Pasahitza { get; set; }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UserInfo User { get; set; }
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Erabiltzailea { get; set; }
        public int LangileId { get; set; }
        public string Izena { get; set; }
        public string Abizena1 { get; set; }
        public string Abizena2 { get; set; }
        public string Lanpostua { get; set; }
        public int LanpostuaId { get; set; }

        public bool IsAdmin { get; set; }
        public bool CanCreateOrders { get; set; }
        public bool CanViewKitchen { get; set; }
    }
}