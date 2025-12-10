using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class LanpostuaController : ControllerBase
{
    // GET ALL
    [HttpGet]
    public IActionResult GetAll()
    {
        using var session = NHibernateHelper.OpenSession();

        var data = session.Query<Lanpostua>()
                          .Select(l => new {
                              l.Id,
                              l.Izena
                          })
                          .ToList();

        return Ok(data);
    }


    // GET BY ID
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        using var session = NHibernateHelper.OpenSession();
        var entity = session.Get<Lanpostua>(id);

        if (entity == null)
            return NotFound(new { message = "Lanpostua ez da existitzen" });

        return Ok(entity);
    }

    // CREATE
    [HttpPost]
    public IActionResult Create([FromBody] Lanpostua model)
    {
        using var session = NHibernateHelper.OpenSession();
        using var tx = session.BeginTransaction();

        session.Save(model);
        tx.Commit();

        return Ok(new { message = "Lanpostua sortuta!", data = model });
    }

    // UPDATE
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Lanpostua data)
    {
        using var session = NHibernateHelper.OpenSession();
        using var tx = session.BeginTransaction();

        var existing = session.Get<Lanpostua>(id);
        if (existing == null)
            return NotFound(new { message = "Lanpostua ez da existitzen" });

        existing.Izena = data.Izena;

        session.Update(existing);
        tx.Commit();

        return Ok(new { message = "Lanpostua eguneratua!", data = existing });
    }

    // DELETE
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        using var session = NHibernateHelper.OpenSession();
        using var tx = session.BeginTransaction();

        var entity = session.Get<Lanpostua>(id);
        if (entity == null)
            return NotFound(new { message = "Lanpostua ez da existitzen" });

        session.Delete(entity);
        tx.Commit();

        return Ok(new { message = "Lanpostua ezabatuta!" });
    }
}
