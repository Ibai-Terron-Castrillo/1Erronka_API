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
                AzkenPrezioa = o.AzkenPrezioa,
                Stock = o.Stock,
                GutxienekoStock = o.GutxienekoStock,
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

                var platerakCount = session.Query<Platerak>()
                    .Count(p => p.Osagaiak.Any(o => o.Id == id));

                if (platerakCount > 0)
                {
                    return BadRequest($"Ezin da osagaia ezabatu, {platerakCount} platan erabiltzen da");
                }

                var eskaerakCount = session.Query<Eskaera>()
                    .Count(e => e.Osagaiak.Any(o => o.Id == id));

                if (eskaerakCount > 0)
                {
                    return BadRequest($"Ezin da osagaia ezabatu, {eskaerakCount} eskaritan erabiltzen da");
                }

                var hornitzaileakCount = session.Query<Hornitzailea>()
                    .Count(h => h.Osagaiak.Any(o => o.Id == id));

                if (hornitzaileakCount > 0)
                {
                    return BadRequest($"Ezin da osagaia ezabatu, {hornitzaileakCount} hornitzailekin erlazionatuta dago");
                }

                osagaia.Platerak.Clear();
                osagaia.Hornitzaileak.Clear();

                session.Update(osagaia);

                session.Delete(osagaia);

                transaction.Commit();

                Console.WriteLine($"Osagaia {id} ezabatuta");   
                return NoContent();
            }
            catch (NHibernate.Exceptions.GenericADOException ex)
            {
                transaction.Rollback();

                if (TryGetSqlException(ex.InnerException, out var sqlNumber, out var sqlMessage))
                {
                    if (sqlNumber == 547)
                    {
                        var errorDetails = sqlMessage ?? string.Empty;
                        if (errorDetails.Contains("FK_"))
                        {
                            var fkName = errorDetails.Substring(
                                errorDetails.IndexOf("FK_"),
                                errorDetails.IndexOf("\"", errorDetails.IndexOf("FK_")) - errorDetails.IndexOf("FK_")
                            );
                            return BadRequest($"Ezin da osagaia ezabatu. Errorea: {fkName}");
                        }
                        return BadRequest("Ezin da osagaia ezabatu: beste taula batzuetan erabiltzen ari da.");
                    }
                }

                Console.WriteLine($"Errorea osagaia ezabatzean {id}: {ex}");
                return StatusCode(500, $"Errore teknikoa: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Errorea osagaia ezabatzean {id}: {ex}");
                return StatusCode(500, $"Errorea: {ex.Message}");
            }
        }
    }

    private static bool TryGetSqlException(Exception exception, out int number, out string message)
    {
        number = 0;
        message = null;

        if (exception == null) return false;

        var type = exception.GetType();
        var fullName = type.FullName;
        if (string.IsNullOrEmpty(fullName) || !fullName.EndsWith(".SqlException", StringComparison.Ordinal)) return false;

        var prop = type.GetProperty("Number");
        if (prop == null) return false;

        var raw = prop.GetValue(exception);
        if (raw is IConvertible convertible)
        {
            number = convertible.ToInt32(null);
            message = exception.Message;
            return true;
        }

        return false;
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
    public double AzkenPrezioa { get; set; }
    public int Stock { get; set; }
    public int GutxienekoStock { get; set; }
    public bool Eskatu { get; set; }
}
