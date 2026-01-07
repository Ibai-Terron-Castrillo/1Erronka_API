using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class HornitzaileakController : ControllerBase
{
    private readonly ISessionFactory _sessionFactory;

    public HornitzaileakController(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    // GET
    [HttpGet]
    public ActionResult<IEnumerable<HornitzaileDto>> Get()
    {
        using var session = _sessionFactory.OpenSession();

        var hornitzaileak = session.Query<Hornitzailea>()
            .Select(h => new HornitzaileDto
            {
                Id = h.Id,
                Izena = h.Izena,
                Cif = h.Cif,
                Sektorea = h.Sektorea,
                Telefonoa = h.Telefonoa,
                Email = h.Email
            })
            .ToList();

        return Ok(hornitzaileak);
    }

    // GET BY Id
    [HttpGet("{id}")]
    public ActionResult<Hornitzailea> Get(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var hornitzailea = session.Get<Hornitzailea>(id);
            if (hornitzailea == null)
                return NotFound();
            return Ok(hornitzailea);
        }
    }

    // GET BY Izena
    [HttpGet("search/{izena}")]
    public ActionResult<IEnumerable<Hornitzailea>> Search(string izena)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var hornitzaileak = session.Query<Hornitzailea>()
                .Where(h => h.Izena.Contains(izena) || h.Sektorea.Contains(izena))
                .ToList();
            return Ok(hornitzaileak);
        }
    }

    // POST
    [HttpPost]
    public ActionResult<Hornitzailea> Post([FromBody] Hornitzailea hornitzailea)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var existingCif = session.Query<Hornitzailea>()
                    .FirstOrDefault(h => h.Cif == hornitzailea.Cif);

                if (existingCif != null)
                    return BadRequest($"CIF {hornitzailea.Cif} dagoeneko erregistratuta dago");

                session.Save(hornitzailea);
                transaction.Commit();
                return CreatedAtAction(nameof(Get), new { id = hornitzailea.Id }, hornitzailea);
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
    public ActionResult Put(int id, [FromBody] Hornitzailea hornitzailea)
    {
        if (id != hornitzailea.Id)
            return BadRequest("ID-ak ez datoz bat");

        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var existing = session.Get<Hornitzailea>(id);
                if (existing == null)
                    return NotFound();

                if (existing.Cif != hornitzailea.Cif)
                {
                    var existingCif = session.Query<Hornitzailea>()
                        .FirstOrDefault(h => h.Cif == hornitzailea.Cif && h.Id != id);

                    if (existingCif != null)
                        return BadRequest($"CIF {hornitzailea.Cif} beste hornitzaile batena da");
                }

                existing.Cif = hornitzailea.Cif;
                existing.Helbidea = hornitzailea.Helbidea;
                existing.Izena = hornitzailea.Izena;
                existing.Sektorea = hornitzailea.Sektorea;
                existing.Telefonoa = hornitzailea.Telefonoa;
                existing.Email = hornitzailea.Email;

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
                var hornitzailea = session.Get<Hornitzailea>(id);
                if (hornitzailea == null)
                    return NotFound();

                var osagaiakCount = session.Query<OsagaiaHornitzailea>()
                    .Count(oh => oh.Hornitzailea.Id == id);

                if (osagaiakCount > 0)
                    return BadRequest($"Ezin da hornitzailea ezabatu, {osagaiakCount} osagairekin dago erlazionatuta");

                session.Delete(hornitzailea);
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

    // Hornitzailearen Osagaiak lortu
    [HttpGet("{id}/osagaiak")]
    public ActionResult<IEnumerable<OsagaiaSimpleDto>> GetOsagaiak(int id)
    {
        using var session = _sessionFactory.OpenSession();

        var exists = session.Get<Hornitzailea>(id);
        if (exists == null)
            return NotFound();

        var osagaiak = session.Query<OsagaiaHornitzailea>()
            .Where(oh => oh.Hornitzailea.Id == id)
            .Select(oh => new OsagaiaSimpleDto
            {
                Id = oh.Osagaia.Id,
                Izena = oh.Osagaia.Izena
            })
            .ToList();

        return Ok(osagaiak);
    }

    // Hornitzaileari Osagaia gehitu
    [HttpPost("{id}/osagaiak/{osagaiaId}")]
    public ActionResult AddOsagaia(int id, int osagaiaId)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var hornitzailea = session.Get<Hornitzailea>(id);
                var osagaia = session.Get<Osagaia>(osagaiaId);

                if (hornitzailea == null || osagaia == null)
                    return NotFound();

                var exists = session.Query<OsagaiaHornitzailea>()
                    .Any(oh => oh.Hornitzailea.Id == id && oh.Osagaia.Id == osagaiaId);

                if (exists)
                    return BadRequest("Osagaia dagoeneko hornitzaile honetan dago");

                var rel = new OsagaiaHornitzailea
                {
                    Hornitzailea = hornitzailea,
                    Osagaia = osagaia
                };

                session.Save(rel);
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

    // Hornitzaileari Osagaia kendu
    [HttpDelete("{id}/osagaiak/{osagaiaId}")]
    public ActionResult RemoveOsagaia(int id, int osagaiaId)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var rel = session.Query<OsagaiaHornitzailea>()
                    .FirstOrDefault(oh => oh.Hornitzailea.Id == id && oh.Osagaia.Id == osagaiaId);

                if (rel == null)
                    return NotFound();

                session.Delete(rel);
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

    public class HornitzaileDto
    {
        public int Id { get; set; }
        public string Izena { get; set; }
        public string Cif { get; set; }
        public string Sektorea { get; set; }
        public string Telefonoa { get; set; }
        public string Email { get; set; }
    }

    public class OsagaiaSimpleDto
    {
        public int Id { get; set; }
        public string Izena { get; set; }
    }
}
