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

    // GET all
    [HttpGet]
    public ActionResult<IEnumerable<EskaeraDto>> Get()
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var eskaerak = session.Query<Eskaera>()
                .OrderByDescending(e => e.EskaeraZenbakia)
                .Select(e => new EskaeraDto
                {
                    Id = e.Id,
                    EskaeraZenbakia = e.EskaeraZenbakia,
                    Totala = e.Totala,
                    Egoera = e.Egoera,
                    EskaeraPdf = e.EskaeraPdf
                })
                .ToList();

            return Ok(eskaerak);
        }
    }

    // GET by id
    [HttpGet("{id}")]
    public ActionResult<EskaeraDetailDto> Get(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var eskaeraExists = session.Query<Eskaera>()
                .Any(e => e.Id == id);

            if (!eskaeraExists)
                return NotFound();

            var eskaeraDto = new EskaeraDetailDto();

            var eskaera = session.Get<Eskaera>(id);
            if (eskaera != null)
            {
                eskaeraDto.Id = eskaera.Id;
                eskaeraDto.EskaeraZenbakia = eskaera.EskaeraZenbakia;
                eskaeraDto.Totala = eskaera.Totala;
                eskaeraDto.Egoera = eskaera.Egoera;
                eskaeraDto.EskaeraPdf = eskaera.EskaeraPdf;
            }

            var osagaiak = session.Query<EskaeraOsagaia>()
                .Where(eo => eo.Eskaera.Id == id) 
                .Select(eo => new EskaeraOsagaiaDto
                {
                    Id = eo.Id,
                    OsagaiaId = eo.Osagaia.Id,
                    OsagaiaIzena = eo.Osagaia.Izena,
                    Kopurua = eo.Kopurua,
                    Prezioa = eo.Prezioa,
                    Totala = eo.Totala
                })
                .ToList();

            eskaeraDto.Osagaiak = osagaiak;

            return Ok(eskaeraDto);
        }
    }

    // GET pendienteak
    [HttpGet("pendienteak")]
    public ActionResult<IEnumerable<EskaeraDto>> GetPendienteak()
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var eskaerak = session.Query<Eskaera>()
                .Where(e => !e.Egoera)
                .OrderByDescending(e => e.EskaeraZenbakia)
                .Select(e => new EskaeraDto
                {
                    Id = e.Id,
                    EskaeraZenbakia = e.EskaeraZenbakia,
                    Totala = e.Totala,
                    Egoera = e.Egoera,
                    EskaeraPdf = e.EskaeraPdf
                })
                .ToList();

            return Ok(eskaerak);
        }
    }

    // GET bukatuak
    [HttpGet("bukatuak")]
    public ActionResult<IEnumerable<EskaeraDto>> GetBukatuak()
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var eskaerak = session.Query<Eskaera>()
                .Where(e => e.Egoera)
                .OrderByDescending(e => e.EskaeraZenbakia)
                .Select(e => new EskaeraDto
                {
                    Id = e.Id,
                    EskaeraZenbakia = e.EskaeraZenbakia,
                    Totala = e.Totala,
                    Egoera = e.Egoera,
                    EskaeraPdf = e.EskaeraPdf
                })
                .ToList();

            return Ok(eskaerak);
        }
    }

    // POST
    [HttpPost]
    public ActionResult<EskaeraDto> Post([FromBody] EskaeraCreateDto dto)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var eskaera = new Eskaera
                {
                    Totala = 0,
                    Egoera = dto.Egoera,
                    EskaeraPdf = dto.EskaeraPdf,
                    Osagaiak = new List<Osagaia>()
                };

                var lastEskaera = session.Query<Eskaera>()
                    .OrderByDescending(e => e.EskaeraZenbakia)
                    .FirstOrDefault();

                eskaera.EskaeraZenbakia = (lastEskaera?.EskaeraZenbakia ?? 0) + 1;

                session.Save(eskaera);
                transaction.Commit();

                var result = new EskaeraDto
                {
                    Id = eskaera.Id,
                    EskaeraZenbakia = eskaera.EskaeraZenbakia,
                    Totala = eskaera.Totala,
                    Egoera = eskaera.Egoera,
                    EskaeraPdf = eskaera.EskaeraPdf
                };

                return CreatedAtAction(nameof(Get), new { id = eskaera.Id }, result);
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
    public ActionResult Put(int id, [FromBody] EskaeraUpdateDto dto)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var existing = session.Get<Eskaera>(id);
                if (existing == null)
                    return NotFound();

                existing.EskaeraZenbakia = dto.EskaeraZenbakia;
                existing.Totala = dto.Totala;
                existing.Egoera = dto.Egoera;
                existing.EskaeraPdf = dto.EskaeraPdf;

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
                Console.WriteLine($"Error: {ex.Message}\n{ex.StackTrace}");
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
                    .Where(eo => eo.Eskaera.Id == id)
                    .ToList();

                foreach (var eo in eskaeraOsagaiak)
                {
                    var osagaia = eo.Osagaia;
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
    public ActionResult<IEnumerable<EskaeraOsagaiaDto>> GetEskaeraOsagaiak(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var eskaeraOsagaiak = session.Query<EskaeraOsagaia>()
                .Where(eo => eo.Eskaera.Id == id)
                .Select(eo => new EskaeraOsagaiaDto
                {
                    Id = eo.Id,
                    OsagaiaId = eo.Osagaia.Id,
                    OsagaiaIzena = eo.Osagaia.Izena,
                    Kopurua = eo.Kopurua,
                    Prezioa = eo.Prezioa,
                    Totala = eo.Totala
                })
                .ToList();

            return Ok(eskaeraOsagaiak);
        }
    }

    // Osagaia gehitu eskaerari
    [HttpPost("{id}/osagaiak")]
    public ActionResult<EskaeraOsagaiaDto> AddOsagaiaToEskaera(int id, [FromBody] EskaeraOsagaiaCreateDto dto)
    {
        Console.WriteLine($"=== OSAGAIA GEHITZEN ESKAERARI {id} ===");
        Console.WriteLine($"DTO jaso: OsagaiaId={dto?.OsagaiaId}, Kopurua={dto?.Kopurua}, Prezioa={dto?.Prezioa}");

        if (dto == null)
        {
            Console.WriteLine("ERROR: DTO nulua da");
            return BadRequest("Datuak ezin dira nuluan egon");
        }

        if (dto.OsagaiaId <= 0 || dto.Kopurua <= 0 || dto.Prezioa <= 0)
        {
            Console.WriteLine($"ERROR: Datuak baliogabeak - OsagaiaId:{dto.OsagaiaId}, Kopurua:{dto.Kopurua}, Prezioa:{dto.Prezioa}");
            return BadRequest("Datuak baliogabeak");
        }

        using (var session = _sessionFactory.OpenSession())
        {
            Console.WriteLine("NHibernate saioa irekia");

            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    Console.WriteLine("Transakzioa hasita");

                    var eskaera = session.Get<Eskaera>(id);
                    if (eskaera == null)
                    {
                        Console.WriteLine($"ERROR: {id} ID-ko eskaera ez da aurkitu");
                        return NotFound($"{id} ID-ko eskaera ez da aurkitu");
                    }
                    Console.WriteLine($"Eskaera kargatua: ID={eskaera.Id}, EskaeraZenbakia={eskaera.EskaeraZenbakia}");

                    var osagaia = session.Get<Osagaia>(dto.OsagaiaId);
                    if (osagaia == null)
                    {
                        Console.WriteLine($"ERROR: {dto.OsagaiaId} ID-ko osagaia ez da aurkitu");
                        return NotFound($"{dto.OsagaiaId} ID-ko osagaia ez da aurkitu");
                    }
                    Console.WriteLine($"Osagaia kargatua: ID={osagaia.Id}, Izena={osagaia.Izena}");

                    var existingRelation = session.Query<EskaeraOsagaia>()
                        .FirstOrDefault(eo => eo.Eskaera.Id == id && eo.Osagaia.Id == dto.OsagaiaId);

                    EskaeraOsagaia savedRelation = null;

                    if (existingRelation != null)
                    {
                        Console.WriteLine($"OHARRA: {id} eskaeraren eta {dto.OsagaiaId} osagaiaren arteko harremana dagoeneko existitzen da");
                        existingRelation.Kopurua += dto.Kopurua;
                        existingRelation.Prezioa = dto.Prezioa;
                        existingRelation.Totala = existingRelation.Kopurua * existingRelation.Prezioa;

                        session.Update(existingRelation);
                        savedRelation = existingRelation;
                        Console.WriteLine($"Harremana eguneratua: ID={existingRelation.Id}, KopuruaBerria={existingRelation.Kopurua}");
                    }
                    else
                    {
                        var eskaeraOsagaia = new EskaeraOsagaia
                        {
                            Eskaera = eskaera,
                            Osagaia = osagaia,
                            Kopurua = dto.Kopurua,
                            Prezioa = dto.Prezioa,
                            Totala = dto.Kopurua * dto.Prezioa
                        };

                        Console.WriteLine($"Harreman berria sortzen: Kopurua={eskaeraOsagaia.Kopurua}, Prezioa={eskaeraOsagaia.Prezioa}, Totala={eskaeraOsagaia.Totala}");

                        session.Save(eskaeraOsagaia);
                        savedRelation = eskaeraOsagaia;
                        Console.WriteLine($"Harremana gordeta ID honekin: {eskaeraOsagaia.Id}");
                    }

                    Console.WriteLine("Eskaeraren totala berrikalkulatzen...");

                    var totalak = session.Query<EskaeraOsagaia>()
                        .Where(eo => eo.Eskaera.Id == id)
                        .Select(eo => eo.Totala)
                        .ToList();

                    var eskaerarenTotala = totalak.Any() ? totalak.Sum() : 0;

                    Console.WriteLine($"Berrikalkulatutako totala: {eskaerarenTotala}");
                    Console.WriteLine($"Aurreko totala: {eskaera.Totala}");

                    eskaera.Totala = Math.Round(eskaerarenTotala, 2);
                    session.Update(eskaera);
                    Console.WriteLine($"Eskaera eguneratuta total berriarekin: {eskaera.Totala}");

                    transaction.Commit();
                    Console.WriteLine("Transakzioa arrakastaz burututa");

                    if (savedRelation == null)
                    {
                        Console.WriteLine("ERROR: Gordetako harremana ezin izan da berreskuratu");
                        return StatusCode(500, "Errorea harremana berreskuratzean");
                    }

                    var result = new EskaeraOsagaiaDto
                    {
                        Id = savedRelation.Id,
                        OsagaiaId = osagaia.Id,
                        OsagaiaIzena = osagaia.Izena,
                        Kopurua = savedRelation.Kopurua,
                        Prezioa = savedRelation.Prezioa,
                        Totala = savedRelation.Totala
                    };

                    Console.WriteLine($"DTOa itzultzen: ID={result.Id}, OsagaiaIzena={result.OsagaiaIzena}");
                    return Created($"api/eskaerak/{id}/osagaiak", result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"=== ERROREA AddOsagaiaToEskaera metoduan ===");
                    Console.WriteLine($"Mezua: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");

                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Barneko Salbuespena: {ex.InnerException.Message}");
                        Console.WriteLine($"Barneko StackTrace: {ex.InnerException.StackTrace}");
                    }

                    try
                    {
                        transaction.Rollback();
                        Console.WriteLine("Transakzioa atzeratuta");
                    }
                    catch (Exception rollbackEx)
                    {
                        Console.WriteLine($"Errorea atzeratzean: {rollbackEx.Message}");
                    }

                    return StatusCode(500, $"Errorea: {ex.Message}");
                }
            }
        }
    }

    // Osagaia ezabatu eskaeratik
    [HttpDelete("{id}/osagaiak/{osagaiaId}")]
    public ActionResult RemoveOsagaiaFromEskaera(int id, int osagaiaId)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var eskaeraOsagaia = session.Query<EskaeraOsagaia>()
                    .FirstOrDefault(eo => eo.Eskaera.Id == id && eo.Osagaia.Id == osagaiaId);

                if (eskaeraOsagaia == null)
                    return NotFound();

                var eskaera = session.Get<Eskaera>(id);

                session.Delete(eskaeraOsagaia);

                var totalak = session.Query<EskaeraOsagaia>()
                    .Where(eo => eo.Eskaera.Id == id)
                    .Select(eo => eo.Totala)
                    .ToList();

                var eskaerarenTotala = totalak.Any() ? totalak.Sum() : 0;

                eskaera.Totala = Math.Round(eskaerarenTotala, 2);
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

public class EskaeraDto
{
    public int Id { get; set; }
    public int EskaeraZenbakia { get; set; }
    public double Totala { get; set; }
    public bool Egoera { get; set; }
    public string EskaeraPdf { get; set; }
}

public class EskaeraDetailDto : EskaeraDto
{
    public List<EskaeraOsagaiaDto> Osagaiak { get; set; } = new List<EskaeraOsagaiaDto>();
}

public class EskaeraCreateDto
{
    public double Totala { get; set; }
    public bool Egoera { get; set; }
    public string EskaeraPdf { get; set; }
}

public class EskaeraUpdateDto
{
    public int EskaeraZenbakia { get; set; }
    public double Totala { get; set; }
    public bool Egoera { get; set; }
    public string EskaeraPdf { get; set; }
}

public class EskaeraOsagaiaDto
{
    public int Id { get; set; }
    public int OsagaiaId { get; set; }
    public string OsagaiaIzena { get; set; }
    public int Kopurua { get; set; }
    public double Prezioa { get; set; }
    public double Totala { get; set; }
}

public class EskaeraOsagaiaCreateDto
{
    public int OsagaiaId { get; set; }
    public int Kopurua { get; set; }
    public double Prezioa { get; set; }
}