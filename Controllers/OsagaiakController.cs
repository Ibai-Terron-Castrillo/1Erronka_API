using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class OsagaiakController : ControllerBase
{
    private readonly ISessionFactory _sessionFactory;

    public OsagaiakController(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    // GET
    [HttpGet]
    public IActionResult Get()
    {
        using var session = _sessionFactory.OpenSession();

        var osagaiak = session.Query<Osagaia>()
            .Select(o => new OsagaiaDto
            {
                Id = o.Id,
                Izena = o.Izena,
                Stock = o.Stock,
                Eskatu = o.Eskatu
            })
            .ToList();

        return Ok(osagaiak);
    }


    // GET BY ID
    [HttpGet("{id}")]
    public ActionResult<Osagaia> Get(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var osagaia = session.Get<Osagaia>(id);
            if (osagaia == null)
                return NotFound();
            return Ok(osagaia);
        }
    }

    // GET Stock gutxi
    [HttpGet("stock-gutxi")]
    public ActionResult<IEnumerable<Osagaia>> GetStockGutxi()
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var osagaiak = session.Query<Osagaia>()
                .Where(o => o.Stock <= o.GutxienekoStock)
                .ToList();
            return Ok(osagaiak);
        }
    }

    // GET Eskatzeko Daudenak
    [HttpGet("eskatzeko")]
    public ActionResult<IEnumerable<Osagaia>> GetEskatzeko()
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var osagaiak = session.Query<Osagaia>()
                .Where(o => o.Eskatu)
                .ToList();
            return Ok(osagaiak);
        }
    }

    // POST
    [HttpPost]
    public ActionResult<Osagaia> Post([FromBody] Osagaia osagaia)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                session.Save(osagaia);
                transaction.Commit();
                return CreatedAtAction(nameof(Get), new { id = osagaia.Id }, osagaia);
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
    public ActionResult Put(int id, [FromBody] Osagaia osagaia)
    {
        if (id != osagaia.Id)
            return BadRequest("ID-ak ez datoz bat");

        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var existing = session.Get<Osagaia>(id);
                if (existing == null)
                    return NotFound();

                existing.Izena = osagaia.Izena;
                existing.AzkenPrezioa = osagaia.AzkenPrezioa;
                existing.Stock = osagaia.Stock;
                existing.GutxienekoStock = osagaia.GutxienekoStock;
                existing.Eskatu = osagaia.Eskatu;

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
                var osagaia = session.Get<Osagaia>(id);
                if (osagaia == null)
                    return NotFound();

                var platerakCount = session.Query<PlaterakOsagaia>()
                    .Count(po => po.OsagaiakId == id);

                if (platerakCount > 0)
                    return BadRequest($"Ezin da osagaia ezabatu, {platerakCount} platan erabiltzen da");

                session.Delete(osagaia);
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

    // Stock eguneratu
    [HttpPatch("{id}/stock")]
    public ActionResult UpdateStock(int id, [FromBody] StockUpdateDto update)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var osagaia = session.Get<Osagaia>(id);
                if (osagaia == null)
                    return NotFound();

                if (update.Kopurua == 0)
                    return BadRequest("Kopurua ezin da 0 izan");

                osagaia.Stock += update.Kopurua;

                if (osagaia.Stock <= osagaia.GutxienekoStock)
                    osagaia.Eskatu = true;

                session.Update(osagaia);
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

    // Eskatu
    [HttpPatch("{id}/eskatu")]
    public ActionResult ToggleEskatu(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var osagaia = session.Get<Osagaia>(id);
                if (osagaia == null)
                    return NotFound();

                osagaia.Eskatu = !osagaia.Eskatu;
                session.Update(osagaia);
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

    // GET Osagaia duten platerak
    [HttpGet("{id}/platerak")]
    public ActionResult<IEnumerable<Platerak>> GetPlaterak(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var osagaia = session.Get<Osagaia>(id);
            if (osagaia == null)
                return NotFound();

            var platerakIds = session.Query<PlaterakOsagaia>()
                .Where(po => po.OsagaiakId == id)
                .Select(po => po.PlaterakId)
                .ToList();

            var platerak = session.Query<Platerak>()
                .Where(p => platerakIds.Contains(p.Id))
                .ToList();

            return Ok(platerak);
        }
    }
}

public class StockUpdateDto
{
    public int Kopurua { get; set; }
}

public class OsagaiaDto
{
    public int Id { get; set; }
    public string Izena { get; set; }
    public int Stock { get; set; }
    public bool Eskatu { get; set; }
}
