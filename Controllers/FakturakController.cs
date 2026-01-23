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
    public ActionResult<IEnumerable<FakturaDto>> Get()
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var fakturak = session.Query<Faktura>()
                .Select(ToDto)
                .ToList();
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
                .Where(f => f.Erreserba.Id == erreserbaId)
                .OrderBy(f => f.Egoera)
                .ThenByDescending(f => f.Id)
                .FirstOrDefault();

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

                var hasOpenFaktura = session.Query<Faktura>()
                    .Any(f => f.Erreserba.Id == faktura.ErreserbakId && !f.Egoera);
                if (hasOpenFaktura)
                    return BadRequest("Erreserbak dagoeneko faktura bat du");

                faktura.Erreserba = erreserba;
                faktura.Egoera = false;
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
                if (faktura.ErreserbakId > 0)
                {
                    var erreserba = session.Get<Erreserba>(faktura.ErreserbakId);
                    if (erreserba == null)
                        return BadRequest("Erreserba ez da existitzen");
                    existing.Erreserba = erreserba;
                }

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
                    .Count(k => k.Faktura.Id == id); 

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

                var totalaRaw = session.CreateSQLQuery(@"
SELECT COALESCE(SUM(k.totala), 0)
FROM Komandak k
WHERE k.fakturak_id = :id
")
                    .SetParameter("id", id)
                    .UniqueResult();

                faktura.Totala = Convert.ToDouble(totalaRaw);
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

            var totalaRaw = session.CreateSQLQuery(@"
SELECT COALESCE(SUM(k.totala), 0)
FROM Komandak k
WHERE k.fakturak_id = :id
")
                .SetParameter("id", id)
                .UniqueResult();

            var totala = Convert.ToDouble(totalaRaw);

            return Ok(totala);
        }
    }

    private static FakturaDto ToDto(Faktura f)
    {
        return new FakturaDto
        {
            Id = f.Id,
            Totala = f.Totala,
            Egoera = f.Egoera,
            FakturaPdf = f.FakturaPdf,
            ErreserbakId = f.Erreserba?.Id ?? f.ErreserbakId
        };
    }
    



}
