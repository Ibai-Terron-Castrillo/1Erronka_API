using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class FakturakController : ControllerBase
{
    private readonly ISessionFactory _sessionFactory;

    public FakturakController(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    // GET
    [HttpGet]
    public ActionResult<IEnumerable<Faktura>> Get()
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var fakturak = session.Query<Faktura>().ToList();
            return Ok(fakturak);
        }
    }

    // GET BY ID
    [HttpGet("{id}")]
    public ActionResult<Faktura> Get(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var faktura = session.Get<Faktura>(id);
            if (faktura == null)
                return NotFound();
            return Ok(faktura);
        }
    }

    // GET BY ERRESERBA ID
    [HttpGet("erreserba/{erreserbaId}")]
    public ActionResult<Faktura> GetByErreserba(int erreserbaId)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var faktura = session.Query<Faktura>()
                .FirstOrDefault(f => f.ErreserbakId == erreserbaId);

            if (faktura == null)
                return NotFound();
            return Ok(faktura);
        }
    }

    // POST
    [HttpPost]
    public ActionResult<Faktura> Post([FromBody] Faktura faktura)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var erreserba = session.Get<Erreserba>(faktura.ErreserbakId);
                if (erreserba == null)
                    return BadRequest("Erreserba ez da existitzen");

                var existingFaktura = session.Query<Faktura>()
                    .FirstOrDefault(f => f.ErreserbakId == faktura.ErreserbakId);

                if (existingFaktura != null)
                    return BadRequest("Erreserbak dagoeneko faktura bat du");

                session.Save(faktura);
                transaction.Commit();
                return CreatedAtAction(nameof(Get), new { id = faktura.Id }, faktura);
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
    public ActionResult Put(int id, [FromBody] Faktura faktura)
    {
        if (id != faktura.Id)
            return BadRequest("ID-ak ez datoz bat");

        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var existing = session.Get<Faktura>(id);
                if (existing == null)
                    return NotFound();

                existing.Totala = faktura.Totala;
                existing.Egoera = faktura.Egoera;
                existing.FakturaPdf = faktura.FakturaPdf;
                existing.ErreserbakId = faktura.ErreserbakId;

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
                var faktura = session.Get<Faktura>(id);
                if (faktura == null)
                    return NotFound();

                var komandakCount = session.Query<Komanda>()
                    .Count(k => k.FakturakId == id);

                if (komandakCount > 0)
                    return BadRequest($"Ezin da faktura ezabatu, {komandakCount} komanda dauzka");

                session.Delete(faktura);
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

    // Ordaindu
    [HttpPatch("{id}/ordaindu")]
    public ActionResult MarkAsPaid(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var faktura = session.Get<Faktura>(id);
                if (faktura == null)
                    return NotFound();

                faktura.Egoera = true;
                session.Update(faktura);
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

    // Totala Kalkulatu
    [HttpGet("{id}/totala-kalkulatu")]
    public ActionResult<double> KalkulatuTotala(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var faktura = session.Get<Faktura>(id);
            if (faktura == null)
                return NotFound();

            var totala = session.Query<Komanda>()
                .Where(k => k.FakturakId == id)
                .Sum(k => k.Totala);

            return Ok(totala);
        }
    }
}