using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class EskaerakController : ControllerBase
{
    private readonly ISessionFactory _sessionFactory;

    public EskaerakController(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    // GET
    [HttpGet]
    public ActionResult<IEnumerable<Eskaera>> Get()
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var eskaerak = session.Query<Eskaera>().OrderByDescending(e => e.EskaeraZenbakia).ToList();
            return Ok(eskaerak);
        }
    }

    // GET BY id
    [HttpGet("{id}")]
    public ActionResult<Eskaera> Get(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var eskaera = session.Get<Eskaera>(id);
            if (eskaera == null)
                return NotFound();
            return Ok(eskaera);
        }
    }

    // Pendienteak lortu
    [HttpGet("pendienteak")]
    public ActionResult<IEnumerable<Eskaera>> GetPendienteak()
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var eskaerak = session.Query<Eskaera>()
                .Where(e => !e.Egoera)
                .OrderByDescending(e => e.EskaeraZenbakia)
                .ToList();
            return Ok(eskaerak);
        }
    }

    // Bukatuak lortu
    [HttpGet("bukatuak")]
    public ActionResult<IEnumerable<Eskaera>> GetBukatuak()
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var eskaerak = session.Query<Eskaera>()
                .Where(e => e.Egoera)
                .OrderByDescending(e => e.EskaeraZenbakia)
                .ToList();
            return Ok(eskaerak);
        }
    }

    // POST
    [HttpPost]
    public ActionResult<Eskaera> Post([FromBody] Eskaera eskaera)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                if (eskaera.EskaeraZenbakia == 0)
                {
                    var lastEskaera = session.Query<Eskaera>()
                        .OrderByDescending(e => e.EskaeraZenbakia)
                        .FirstOrDefault();

                    eskaera.EskaeraZenbakia = (lastEskaera?.EskaeraZenbakia ?? 0) + 1;
                }

                session.Save(eskaera);
                transaction.Commit();
                return CreatedAtAction(nameof(Get), new { id = eskaera.Id }, eskaera);
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
    public ActionResult Put(int id, [FromBody] Eskaera eskaera)
    {
        if (id != eskaera.Id)
            return BadRequest("ID-ak ez datoz bat");

        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var existing = session.Get<Eskaera>(id);
                if (existing == null)
                    return NotFound();

                existing.EskaeraZenbakia = eskaera.EskaeraZenbakia;
                existing.Totala = eskaera.Totala;
                existing.Egoera = eskaera.Egoera;
                existing.EskaeraPdf = eskaera.EskaeraPdf;

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
                var eskaera = session.Get<Eskaera>(id);
                if (eskaera == null)
                    return NotFound();

                session.Delete(eskaera);
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

    // Eskaera bukatu
    [HttpPatch("{id}/bukatu")]
    public ActionResult MarkAsCompleted(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var eskaera = session.Get<Eskaera>(id);
                if (eskaera == null)
                    return NotFound();

                eskaera.Egoera = true;

                var eskaeraOsagaiak = session.Query<EskaeraOsagaia>()
                    .Where(eo => eo.EskaerakId == id)
                    .ToList();

                foreach (var eo in eskaeraOsagaiak)
                {
                    var osagaia = session.Get<Osagaia>(eo.OsagaiakId);
                    if (osagaia != null)
                    {
                        osagaia.Stock += eo.Kopurua;
                        osagaia.AzkenPrezioa = eo.Prezioa;
                        session.Update(osagaia);
                    }
                }

                session.Update(eskaera);
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

    // Eskaeraren osagaiak lortu
    [HttpGet("{id}/osagaiak")]
    public ActionResult<IEnumerable<EskaeraOsagaia>> GetEskaeraOsagaiak(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var eskaeraOsagaiak = session.Query<EskaeraOsagaia>()
                .Where(eo => eo.EskaerakId == id)
                .ToList();
            return Ok(eskaeraOsagaiak);
        }
    }

    // Osagaia Gehitu
    [HttpPost("{id}/osagaiak")]
    public ActionResult AddOsagaiaToEskaera(int id, [FromBody] EskaeraOsagaiaDto dto)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var eskaera = session.Get<Eskaera>(id);
                var osagaia = session.Get<Osagaia>(dto.OsagaiaId);

                if (eskaera == null || osagaia == null)
                    return NotFound();

                var eskaeraOsagaia = new EskaeraOsagaia
                {
                    EskaerakId = id,
                    OsagaiakId = dto.OsagaiaId,
                    Kopurua = dto.Kopurua,
                    Prezioa = dto.Prezioa,
                    Totala = dto.Kopurua * dto.Prezioa
                };

                session.Save(eskaeraOsagaia);

                eskaera.Totala += eskaeraOsagaia.Totala;
                session.Update(eskaera);

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
}

public class EskaeraOsagaiaDto
{
    public int OsagaiaId { get; set; }
    public int Kopurua { get; set; }
    public double Prezioa { get; set; }
}