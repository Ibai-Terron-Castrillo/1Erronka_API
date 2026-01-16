using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class KategoriakController : ControllerBase
{
    private readonly ISessionFactory _sessionFactory;

    public KategoriakController(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    // GET ALL
    [HttpGet]
    // GET ALL
    [HttpGet]
    public IActionResult GetAll()
    {
        using var session = _sessionFactory.OpenSession();

        var kategoriak = session.Query<Kategoriak>()
            .Select(k => new KategoriaDto
            {
                Id = k.Id,
                Izena = k.Izena
            })
            .ToList();

        return Ok(kategoriak);
    }


    // GET BY ID
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        using var session = _sessionFactory.OpenSession();

        var kategoria = session.Get<Kategoriak>(id);

        if (kategoria == null)
            return NotFound(new { message = "Kategoria ez da existitzen" });

        return Ok(kategoria);
    }

    // CREATE
    [HttpPost]
    public IActionResult Create([FromBody] Kategoriak dto)
    {
        using var session = _sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        var exists = session.Query<Kategoriak>()
                            .FirstOrDefault(x => x.Izena == dto.Izena);

        if (exists != null)
            return BadRequest(new { message = "Kategoria dagoeneko existitzen da" });

        var kategoria = new Kategoriak
        {
            Izena = dto.Izena
        };

        session.Save(kategoria);
        tx.Commit();

        return Ok(new { message = "Sortuta!", data = kategoria });
    }

    // UPDATE
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Kategoriak dto)
    {
        using var session = _sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        var existing = session.Get<Kategoriak>(id);
        if (existing == null)
            return NotFound(new { message = "Ez da existitzen" });

        existing.Izena = dto.Izena;

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

        var kategoria = session.Get<Kategoriak>(id);
        if (kategoria == null)
            return NotFound(new { message = "Kategoria ez da existitzen" });

        session.Delete(kategoria);
        tx.Commit();

        return Ok(new { message = "Kategoria ezabatuta!" });
    }
}
