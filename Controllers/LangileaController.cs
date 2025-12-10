using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class LangileaController : ControllerBase
{
    // GET ALL
    [HttpGet]
    public IActionResult GetAll()
    {
        using var session = NHibernateHelper.OpenSession();
        var data = session.Query<Langilea>()
                          .Fetch(x => x.Lanpostua)
                          .ToList();
        return Ok(data);
    }

    // GET BY ID
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        using var session = NHibernateHelper.OpenSession();
        var entity = session.Get<Langilea>(id);

        if (entity == null)
            return NotFound(new { message = "Langilea ez da existitzen" });

        return Ok(entity);
    }

    // CREATE
    [HttpPost]
    public IActionResult Create([FromBody] Langilea model)
    {
        using var session = NHibernateHelper.OpenSession();
        using var tx = session.BeginTransaction();

        session.Save(model);
        tx.Commit();

        return Ok(new { message = "Langilea sortuta!", data = model });
    }

    // UPDATE
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Langilea data)
    {
        using var session = NHibernateHelper.OpenSession();
        using var tx = session.BeginTransaction();

        var existing = session.Get<Langilea>(id);
        if (existing == null)
            return NotFound(new { message = "Langilea ez da existitzen" });

        existing.Izena = data.Izena;
        existing.Abizena1 = data.Abizena1;
        existing.Abizena2 = data.Abizena2;
        existing.Telefonoa = data.Telefonoa;
        existing.Lanpostua = data.Lanpostua;

        session.Update(existing);
        tx.Commit();

        return Ok(new { message = "Langilea eguneratua!", data = existing });
    }

    // DELETE
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        using var session = NHibernateHelper.OpenSession();
        using var tx = session.BeginTransaction();

        var entity = session.Get<Langilea>(id);
        if (entity == null)
            return NotFound(new { message = "Langilea ez da existitzen" });

        session.Delete(entity);
        tx.Commit();

        return Ok(new { message = "Langilea ezabatuta!" });
    }
}
