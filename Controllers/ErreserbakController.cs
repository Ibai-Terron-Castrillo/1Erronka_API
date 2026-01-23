using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class ErreserbakController : ControllerBase
{
    private readonly ISessionFactory _sessionFactory;

    public ErreserbakController(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    // GET
    [HttpGet]
    public ActionResult<IEnumerable<Erreserba>> Get()
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var erreserbak = session.Query<Erreserba>().ToList();
            return Ok(erreserbak);
        }
    }

    // GET BY ID
    [HttpGet("{id}")]
    public ActionResult<Erreserba> Get(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var erreserba = session.Get<Erreserba>(id);
            if (erreserba == null)
                return NotFound();
            return Ok(erreserba);
        }
    }

    // GET BY DATA
    [HttpGet("data/{data}")]
    public ActionResult<IEnumerable<Erreserba>> GetByData(DateTime data)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var erreserbak = session.Query<Erreserba>()
                .Where(e => e.Data.Date == data.Date)
                .ToList();
            return Ok(erreserbak);
        }
    }

    // GET Gaurkoak
    [HttpGet("gaur")]
    public ActionResult<IEnumerable<Erreserba>> GetGaurkoak()
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var erreserbak = session.Query<Erreserba>()
                .Where(e => e.Data.Date == DateTime.Today)
                .ToList();
            return Ok(erreserbak);
        }
    }

    // POST
    [HttpPost]
    public ActionResult<Erreserba> Post([FromBody] Erreserba erreserba)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                if (erreserba.Mahaiak != null && erreserba.Mahaiak.Any())
                {
                    foreach (var mahai in erreserba.Mahaiak)
                    {
                        var mahaiDb = session.Get<Mahai>(mahai.Id);
                        if (mahaiDb == null)
                            return BadRequest($"Mahai {mahai.Id} ez da existitzen");
                    }
                }

                session.Save(erreserba);
                transaction.Commit();
                return CreatedAtAction(nameof(Get), new { id = erreserba.Id }, erreserba);
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
    public ActionResult Put(int id, [FromBody] Erreserba erreserba)
    {
        if (id != erreserba.Id)
            return BadRequest("ID-ak ez datoz bat");

        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var existing = session.Get<Erreserba>(id);
                if (existing == null)
                    return NotFound();

                existing.Izena = erreserba.Izena;
                existing.Telefonoa = erreserba.Telefonoa;
                existing.Txanda = erreserba.Txanda;
                existing.PertsonaKopurua = erreserba.PertsonaKopurua;
                existing.Data = erreserba.Data;

                existing.Mahaiak.Clear();
                if (erreserba.Mahaiak != null)
                {
                    foreach (var mahai in erreserba.Mahaiak)
                    {
                        var mahaiDb = session.Get<Mahai>(mahai.Id);
                        if (mahaiDb != null)
                            existing.Mahaiak.Add(mahaiDb);
                    }
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
                var erreserba = session.Get<Erreserba>(id);
                if (erreserba == null)
                    return NotFound();

                var faktura = session.Query<Faktura>()
                    .FirstOrDefault(f => f.Erreserba.Id == id);

                if (faktura != null)
                    return BadRequest("Ezin da erreserba ezabatu, faktura bat dauka");

                session.Delete(erreserba);
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

    // Mahaia Gehitu
    [HttpPost("{id}/mahaiak")]
    public ActionResult AddMahaiak(int id, [FromBody] List<int> mahaiIds)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var erreserba = session.Get<Erreserba>(id);
                if (erreserba == null)
                    return NotFound();

                foreach (var mahaiId in mahaiIds)
                {
                    var mahai = session.Get<Mahai>(mahaiId);
                    if (mahai != null && !erreserba.Mahaiak.Contains(mahai))
                    {
                        erreserba.Mahaiak.Add(mahai);
                    }
                }

                session.Update(erreserba);
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

    // Mahaia Kendu
    [HttpDelete("{id}/mahaiak/{mahaiId}")]
    public ActionResult RemoveMahai(int id, int mahaiId)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var erreserba = session.Get<Erreserba>(id);
                if (erreserba == null)
                    return NotFound();

                var mahai = erreserba.Mahaiak.FirstOrDefault(m => m.Id == mahaiId);
                if (mahai != null)
                {
                    erreserba.Mahaiak.Remove(mahai);
                    session.Update(erreserba);
                }

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