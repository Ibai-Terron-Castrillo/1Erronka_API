using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class OsagaiaHornitzaileaController : ControllerBase
{
    private readonly ISessionFactory _sessionFactory;

    public OsagaiaHornitzaileaController(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    // GET
    [HttpGet]
    public ActionResult<IEnumerable<OsagaiaHornitzailea>> Get()
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var relations = session.Query<OsagaiaHornitzailea>().ToList();
            return Ok(relations);
        }
    }

    // GET by osagaiId
    [HttpGet("osagaia/{osagaiaId}")]
    public ActionResult<IEnumerable<OsagaiaHornitzailea>> GetByOsagaia(int osagaiaId)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var relations = session.Query<OsagaiaHornitzailea>()
                .Where(oh => oh.Osagaia.Id == osagaiaId)  // Cambiado
                .ToList();
            return Ok(relations);
        }
    }

    // GET by hornitzaileId
    [HttpGet("hornitzailea/{hornitzaileaId}")]
    public ActionResult<IEnumerable<OsagaiaHornitzailea>> GetByHornitzailea(int hornitzaileaId)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var relations = session.Query<OsagaiaHornitzailea>()
                .Where(oh => oh.Hornitzailea.Id == hornitzaileaId)  // Cambiado
                .ToList();
            return Ok(relations);
        }
    }

    // POST
    [HttpPost]
    public ActionResult<OsagaiaHornitzailea> Post([FromBody] OsagaiaHornitzaileaDto dto)  // Cambiado a DTO
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var osagaia = session.Get<Osagaia>(dto.OsagaiaId);  // Cambiado
                var hornitzailea = session.Get<Hornitzailea>(dto.HornitzaileaId);  // Cambiado

                if (osagaia == null || hornitzailea == null)
                    return BadRequest("Osagaia edo hornitzailea ez da existitzen");

                var existing = session.Query<OsagaiaHornitzailea>()
                    .FirstOrDefault(oh => oh.Osagaia.Id == dto.OsagaiaId &&  // Cambiado
                                         oh.Hornitzailea.Id == dto.HornitzaileaId);  // Cambiado

                if (existing != null)
                    return BadRequest("Erlazioa dagoeneko existitzen da");

                var relation = new OsagaiaHornitzailea
                {
                    Osagaia = osagaia,
                    Hornitzailea = hornitzailea
                };

                session.Save(relation);
                transaction.Commit();
                return Ok(relation);
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
                var relation = session.Get<OsagaiaHornitzailea>(id);
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

    // DELETE By Osagaia eta Hornitzailea
    [HttpDelete("osagaia/{osagaiaId}/hornitzailea/{hornitzaileaId}")]
    public ActionResult DeleteByOsagaiaAndHornitzailea(int osagaiaId, int hornitzaileaId)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var relation = session.Query<OsagaiaHornitzailea>()
                    .FirstOrDefault(oh => oh.Osagaia.Id == osagaiaId &&  // Cambiado
                                         oh.Hornitzailea.Id == hornitzaileaId);  // Cambiado

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

    // GET osagaiaren hornitzaileak
    [HttpGet("{osagaiaId}/hornitzaileak")]
    public ActionResult<IEnumerable<Hornitzailea>> GetHornitzaileakByOsagaia(int osagaiaId)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var osagaia = session.Get<Osagaia>(osagaiaId);
            if (osagaia == null)
                return NotFound();

            return Ok(osagaia.Hornitzaileak);
        }
    }

    // GET hornitzailearen osagaiak
    [HttpGet("hornitzailea/{hornitzaileaId}/osagaiak")]
    public ActionResult<IEnumerable<Osagaia>> GetOsagaiakByHornitzailea(int hornitzaileaId)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var hornitzailea = session.Get<Hornitzailea>(hornitzaileaId);
            if (hornitzailea == null)
                return NotFound();

            return Ok(hornitzailea.Osagaiak);
        }
    }
}

public class OsagaiaHornitzaileaDto
{
    public int OsagaiaId { get; set; }
    public int HornitzaileaId { get; set; }
}