using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class ErreserbaMahaiakController : ControllerBase
{
    private readonly ISessionFactory _sessionFactory;

    public ErreserbaMahaiakController(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    // GET mahaiak por erreserba
    [HttpGet("erreserba/{erreserbaId}")]
    public ActionResult<List<int>> GetMahaiakByErreserba(int erreserbaId)
    {
        using var session = _sessionFactory.OpenSession();

        var mahaiIds = session.Query<ErreserbaMahai>()
            .Where(em => em.Erreserba.Id == erreserbaId)
            .Select(em => em.Mahai.Id)
            .ToList();

        return Ok(mahaiIds);
    }


    // POST
    [HttpPost]
    public IActionResult Post([FromBody] ErreserbaMahai dto)
    {
        using var session = _sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        try
        {
            var erreserba = session.Get<Erreserba>(dto.ErreserbakId);
            var mahai = session.Get<Mahai>(dto.MahaiakId);

            if (erreserba == null || mahai == null)
                return BadRequest("Erreserba edo Mahai ez da existitzen");

            var erreserbaMahai = new ErreserbaMahai
            {
                Erreserba = erreserba,
                Mahai = mahai
            };

            session.Save(erreserbaMahai);
            tx.Commit();

            return Ok();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return StatusCode(500, ex.Message);
        }
    }

    // DELETE( erreserba batek lotuta dituen mahaiena)
    [HttpDelete("erreserba/{erreserbaId}")]
    public IActionResult DeleteByErreserba(int erreserbaId)
    {
        using var session = _sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        try
        {
            var relaciones = session.Query<ErreserbaMahai>()
                .Where(em => em.Erreserba.Id == erreserbaId)
                .ToList();

            foreach (var r in relaciones)
                session.Delete(r);

            tx.Commit();
            return NoContent();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return StatusCode(500, ex.Message);
        }
    }


}
