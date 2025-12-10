using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class ErabiltzaileaController : ControllerBase
{
    private readonly ISessionFactory _sessionFactory;

    public ErabiltzaileaController(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    // GET ALL
    [HttpGet]
    public IActionResult GetAll()
    {
        using var session = _sessionFactory.OpenSession();

        var erabiltzaileak = session.Query<Erabiltzailea>()
            .Fetch(x => x.Langilea)
            .ThenFetch(x => x.Lanpostua)
            .ToList();

        return Ok(erabiltzaileak);
    }

    // GET BY ID
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        using var session = _sessionFactory.OpenSession();

        var erabiltzailea = session.Query<Erabiltzailea>()
            .Fetch(x => x.Langilea)
            .ThenFetch(x => x.Lanpostua)
            .FirstOrDefault(x => x.Id == id);

        if (erabiltzailea == null)
            return NotFound(new { message = "Erabiltzailea ez da existitzen" });

        return Ok(erabiltzailea);
    }

    // GET BY LANGILE ID
    [HttpGet("langile/{langileId}")]
    public IActionResult GetByLangile(int langileId)
    {
        using var session = _sessionFactory.OpenSession();

        var erabiltzailea = session.Query<Erabiltzailea>()
            .Fetch(x => x.Langilea)
            .ThenFetch(x => x.Lanpostua)
            .FirstOrDefault(x => x.Langilea.Id == langileId);

        if (erabiltzailea == null)
            return NotFound();

        return Ok(erabiltzailea);
    }

    // CREATE
    [HttpPost]
    public IActionResult Create([FromBody] Erabiltzailea dto)
    {
        using var session = _sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        var langilea = session.Get<Langilea>(dto.Langilea.Id);
        if (langilea == null)
            return BadRequest(new { message = "Langilea ez da existitzen" });

        var exists = session.Query<Erabiltzailea>()
                            .FirstOrDefault(x => x.Langilea.Id == langilea.Id);

        if (exists != null)
            return BadRequest(new { message = "Erabiltzailea dagoeneko existitzen da" });

        var newUser = new Erabiltzailea
        {
            Izena = dto.Izena,
            Pasahitza = dto.Pasahitza,
            Langilea = langilea
        };

        session.Save(newUser);
        tx.Commit();

        return Ok(new { message = "Sortuta!", data = newUser });
    }

    // UPDATE
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Erabiltzailea dto)
    {
        using var session = _sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        var existing = session.Get<Erabiltzailea>(id);
        if (existing == null)
            return NotFound(new { message = "Ez da existitzen" });

        existing.Izena = dto.Izena;
        existing.Pasahitza = dto.Pasahitza;

        session.Update(existing);
        tx.Commit();

        return Ok(new { message = "Eguneratua!", data = existing });
    }

    // DELETE
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        using var session = _sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        var erabiltzailea = session.Get<Erabiltzailea>(id);
        if (erabiltzailea == null)
            return NotFound(new { message = "Erabiltzailea ez da existitzen" });

        session.Delete(erabiltzailea);
        tx.Commit();

        return Ok(new { message = "Erabiltzailea ezabatuta!" });
    }
}
