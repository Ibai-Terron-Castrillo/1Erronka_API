using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
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

    [HttpGet("items")]
    public ActionResult<IEnumerable<FakturaDto>> GetItems()
    {
        using var session = _sessionFactory.OpenSession();

        var fakturak = session.Query<Faktura>()
            .Fetch(f => f.Erreserba)
            .Select(MapToDto)
            .ToList();

        return Ok(fakturak);
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

    [HttpGet("{id}/item")]
    public ActionResult<FakturaDto> GetItem(int id)
    {
        using var session = _sessionFactory.OpenSession();

        var faktura = session.Query<Faktura>()
            .Fetch(f => f.Erreserba)
            .FirstOrDefault(f => f.Id == id);

        if (faktura == null)
            return NotFound();

        return Ok(MapToDto(faktura));
    }

    // GET BY ERRESERBA ID
    [HttpGet("erreserba/{erreserbaId}")]
public ActionResult<Faktura> GetByErreserba(int erreserbaId)
{
    using var session = _sessionFactory.OpenSession();

    var faktura = session.Query<Faktura>()
        .FirstOrDefault(f => f.Erreserba.Id == erreserbaId && !f.Egoera);

    if (faktura == null)
        return NotFound();

    return Ok(faktura);
}

    [HttpGet("erreserba/{erreserbaId}/item")]
    public ActionResult<FakturaDto> GetItemByErreserba(int erreserbaId)
    {
        using var session = _sessionFactory.OpenSession();

        var faktura = session.Query<Faktura>()
            .Fetch(f => f.Erreserba)
            .FirstOrDefault(f => f.Erreserba.Id == erreserbaId && !f.Egoera);

        if (faktura == null)
            return NotFound();

        return Ok(MapToDto(faktura));
    }

    private static FakturaDto MapToDto(Faktura f)
    {
        return new FakturaDto
        {
            Id = f.Id,
            Totala = f.Totala,
            Egoera = f.Egoera,
            ErreserbaId = f.Erreserba?.Id ?? 0
        };
    }



    // POST

    [HttpPost("sortu-erreserbatik")]
    public ActionResult<FakturaDto> SortuFakturaErreserbatik([FromBody] SortuFakturaDto dto)
    {
        using var session = _sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        try
        {
            var erreserba = session.Get<Erreserba>(dto.ErreserbaId);
            if (erreserba == null)
                return BadRequest("Erreserba ez da existitzen");

            var existing = session.Query<Faktura>()
                .FirstOrDefault(f => f.Erreserba.Id == dto.ErreserbaId && !f.Egoera);

            var faktura = existing ?? new Faktura
            {
                Erreserba = erreserba,
                Totala = 0,
                Egoera = false
            };

            if (existing == null)
                session.Save(faktura);

            tx.Commit();

            return Ok(new FakturaDto
            {
                Id = faktura.Id,
                Totala = faktura.Totala,
                Egoera = faktura.Egoera,
                ErreserbaId = erreserba.Id
            });
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return StatusCode(500, ex.Message);
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
                existing.Erreserba.Id = faktura.Erreserba.Id;

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

    [HttpPatch("{id}/ordaindu-check")]
    public ActionResult MarkAsPaidChecked(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var faktura = session.Get<Faktura>(id);
                if (faktura == null)
                    return NotFound();

                var hasPending = session.Query<Komanda>()
                    .Any(k => k.Faktura.Id == id && !k.Egoera);

                if (hasPending)
                    return BadRequest("Oraindik kontsumizioak zerbitzatzea falta da.");

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
                .Where(k => k.Faktura.Id == id) 
                .Sum(k => k.Totala);

            return Ok(totala);
        }
    }


    public class SortuFakturaDto
    {
        public int ErreserbaId { get; set; }
    }
    

}
