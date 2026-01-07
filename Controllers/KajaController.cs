using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class KajaController : ControllerBase
{
    private readonly ISessionFactory _sessionFactory;

    public KajaController(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    // GET
    [HttpGet]
    public ActionResult<IEnumerable<Kaja>> Get()
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var kaja = session.Query<Kaja>().OrderByDescending(k => k.Data).ToList();
            return Ok(kaja);
        }
    }

    // GET BY ID
    [HttpGet("{id}")]
    public ActionResult<Kaja> Get(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var kaja = session.Get<Kaja>(id);
            if (kaja == null)
                return NotFound();
            return Ok(kaja);
        }
    }

    // GET BY DATA
    [HttpGet("data/{data}")]
    public ActionResult<Kaja> GetByData(DateTime data)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var kaja = session.Query<Kaja>()
                .FirstOrDefault(k => k.Data.Date == data.Date);

            if (kaja == null)
                return NotFound($"Ez dago kajarik {data:yyyy-MM-dd} datarako");
            return Ok(kaja);
        }
    }

    // GET Gaurkoa
    [HttpGet("gaur")]
    public ActionResult<Kaja> GetGaurkoKaja()
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var kaja = session.Query<Kaja>()
                .FirstOrDefault(k => k.Data.Date == DateTime.Today);

            if (kaja == null)
            {
                
                var kajaBerria = new Kaja
                {
                    Data = DateTime.Today,
                    KajaHasiera = 0,
                    KajaBukaera = 0
                };

                using (var transaction = session.BeginTransaction())
                {
                    session.Save(kajaBerria);
                    transaction.Commit();
                    return Ok(kajaBerria);
                }
            }

            return Ok(kaja);
        }
    }

    // POST
    [HttpPost]
    public ActionResult<Kaja> Post([FromBody] Kaja kaja)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var existing = session.Query<Kaja>()
                    .FirstOrDefault(k => k.Data.Date == kaja.Data.Date);

                if (existing != null)
                    return BadRequest($"Badago dagoeneko kaja bat {kaja.Data:yyyy-MM-dd} datarako");

                session.Save(kaja);
                transaction.Commit();
                return CreatedAtAction(nameof(Get), new { id = kaja.Id }, kaja);
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
    public ActionResult Put(int id, [FromBody] Kaja kaja)
    {
        if (id != kaja.Id)
            return BadRequest("ID-ak ez datoz bat");

        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var existing = session.Get<Kaja>(id);
                if (existing == null)
                    return NotFound();

                existing.Data = kaja.Data;
                existing.KajaHasiera = kaja.KajaHasiera;
                existing.KajaBukaera = kaja.KajaBukaera;

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
                var kaja = session.Get<Kaja>(id);
                if (kaja == null)
                    return NotFound();

                session.Delete(kaja);
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

    // Itxi
    [HttpPatch("{id}/itxi")]
    public ActionResult ItxiKaja(int id, [FromBody] double bukaerakoDirua)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var kaja = session.Get<Kaja>(id);
                if (kaja == null)
                    return NotFound();

                kaja.KajaBukaera = bukaerakoDirua;
                session.Update(kaja);
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

    // Irabaziak Kalkulatu
    [HttpGet("{id}/irabaziak")]
    public ActionResult<double> KalkulatuIrabaziak(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var kaja = session.Get<Kaja>(id);
            if (kaja == null)
                return NotFound();

            
            var irabaziak = kaja.KajaBukaera - kaja.KajaHasiera;
            return Ok(irabaziak);
        }
    }
}