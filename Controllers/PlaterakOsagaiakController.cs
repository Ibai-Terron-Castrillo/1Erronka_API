using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class PlaterakOsagaiakController : ControllerBase
{
    private readonly ISessionFactory _sessionFactory;

    public PlaterakOsagaiakController(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    // GET
    [HttpGet]
    public ActionResult<IEnumerable<PlaterakOsagaia>> Get()
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var relations = session.Query<PlaterakOsagaia>().ToList();
            return Ok(relations);
        }
    }

    // GET BY PlaterakId
    [HttpGet("platera/{platerakId}")]
    public ActionResult<IEnumerable<PlaterakOsagaia>> GetByPlatera(int platerakId)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var relations = session.Query<PlaterakOsagaia>()
                .Where(po => po.PlaterakId == platerakId)
                .ToList();
            return Ok(relations);
        }
    }

    // GET By OsagaiaId
    [HttpGet("osagaia/{osagaiaId}")]
    public ActionResult<IEnumerable<PlaterakOsagaia>> GetByOsagaia(int osagaiaId)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var relations = session.Query<PlaterakOsagaia>()
                .Where(po => po.OsagaiakId == osagaiaId)
                .ToList();
            return Ok(relations);
        }
    }

    // GET BY PlaterId eta osagaiaId
    [HttpGet("{platerakId}/{osagaiaId}")]
    public ActionResult<PlaterakOsagaia> Get(int platerakId, int osagaiaId)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var relation = session.Query<PlaterakOsagaia>()
                .FirstOrDefault(po => po.PlaterakId == platerakId && po.OsagaiakId == osagaiaId);

            if (relation == null)
                return NotFound();
            return Ok(relation);
        }
    }

    // POST
    [HttpPost]
    public ActionResult<PlaterakOsagaia> Post([FromBody] PlaterakOsagaia relation)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var platera = session.Get<Platerak>(relation.PlaterakId);
                var osagaia = session.Get<Osagaia>(relation.OsagaiakId);

                if (platera == null || osagaia == null)
                    return BadRequest("Platera edo osagaia ez da existitzen");

                var existing = session.Query<PlaterakOsagaia>()
                    .FirstOrDefault(po => po.PlaterakId == relation.PlaterakId &&
                                         po.OsagaiakId == relation.OsagaiakId);

                if (existing != null)
                    return BadRequest("Erlazioa dagoeneko existitzen da");

                session.Save(relation);
                transaction.Commit();
                return CreatedAtAction(nameof(Get),
                    new { platerakId = relation.PlaterakId, osagaiaId = relation.OsagaiakId },
                    relation);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return StatusCode(500, $"Errorea: {ex.Message}");
            }
        }
    }

    // PUT
    [HttpPut("{id}")]
    public ActionResult Put(int id, [FromBody] PlaterakOsagaia relation)
    {
        if (id != relation.Id)
            return BadRequest("ID-ak ez datoz bat");

        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var existing = session.Get<PlaterakOsagaia>(id);
                if (existing == null)
                    return NotFound();

                existing.Kopurua = relation.Kopurua;
                existing.PlaterakId = relation.PlaterakId;
                existing.OsagaiakId = relation.OsagaiakId;

                session.Update(existing);
                transaction.Commit();
                return NoContent();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return StatusCode(500, $"Errorea: {ex.Message}");
            }
        }
    }

    // DELETE
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var relation = session.Get<PlaterakOsagaia>(id);
                if (relation == null)
                    return NotFound();

                session.Delete(relation);
                transaction.Commit();
                return NoContent();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return StatusCode(500, $"Errorea: {ex.Message}");
            }
        }
    }

    // Kalkulatu kostua
    [HttpGet("{platerakId}/kostua")]
    public ActionResult<double> KalkulatuKostua(int platerakId)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var relations = session.Query<PlaterakOsagaia>()
                .Where(po => po.PlaterakId == platerakId)
                .ToList();

            double totalKostua = 0;
            foreach (var relation in relations)
            {
                var osagaia = session.Get<Osagaia>(relation.OsagaiakId);
                if (osagaia != null)
                {
                    totalKostua += relation.Kopurua * osagaia.AzkenPrezioa;
                }
            }

            return Ok(totalKostua);
        }
    }
}