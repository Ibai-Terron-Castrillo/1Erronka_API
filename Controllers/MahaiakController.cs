using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class MahaiakController : ControllerBase
{
    private readonly ISessionFactory _sessionFactory;

    public MahaiakController(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    [HttpGet("/api/Mahaiak")]
    [HttpGet("/api/Mahai")]
    [HttpGet("/mahaiak")]
    public ActionResult<IEnumerable<MahaiDto>> Get()
    {
        using var session = _sessionFactory.OpenSession();

        var now = DateTime.Now;
        var txanda = now.Hour >= 12 && now.Hour < 19 ? "Bazkaria" : "Afaria";
        var date = now.Date;

        IList<object[]> occupiedInfo;
        try
        {
            occupiedInfo = session.CreateSQLQuery(@"
SELECT
    em.mahaiak_id,
    MIN(e.id) AS erreserba_id,
    SUM(e.pertsona_kopurua) AS pertsona_kopurua
FROM Erreserbak_Mahaiak em
JOIN Erreserbak e ON e.id = em.erreserbak_id
WHERE DATE(e.data) = :date AND e.txanda = :txanda
GROUP BY em.mahaiak_id
")
                .SetParameter("date", date)
                .SetParameter("txanda", txanda)
                .List<object[]>();
        }
        catch
        {
            occupiedInfo = new List<object[]>();
        }

        var occupiedByMahaiId = occupiedInfo.ToDictionary(
            r => Convert.ToInt32(r[0]),
            r => new OccupiedInfo(
                ErreserbaId: Convert.ToInt32(r[1]),
                PertsonaKopurua: Convert.ToInt32(r[2])
            )
        );

        var rows = session
            .CreateSQLQuery("SELECT id, mahai_zenbakia, pertsona_max FROM Mahaiak ORDER BY mahai_zenbakia")
            .List<object[]>();

        var mahaiak = rows
            .Select(row => new MahaiDto
            {
                Id = Convert.ToInt32(row[0]),
                Zenbakia = Convert.ToInt32(row[1]),
                PertsonaMax = Convert.ToInt32(row[2]),
                Occupied = occupiedByMahaiId.ContainsKey(Convert.ToInt32(row[0])),
                ErreserbaId = occupiedByMahaiId.TryGetValue(Convert.ToInt32(row[0]), out var occ) ? occ.ErreserbaId : null,
                PertsonaKopurua = occupiedByMahaiId.TryGetValue(Convert.ToInt32(row[0]), out var occ2) ? occ2.PertsonaKopurua : null
            })
            .ToList();

        return Ok(mahaiak);
    }

    [HttpGet("_debug")]
    public IActionResult Debug()
    {
        using var session = _sessionFactory.OpenSession();

        var now = DateTime.Now;
        var txanda = now.Hour >= 12 && now.Hour < 19 ? "Bazkaria" : "Afaria";
        var date = now.Date;

        var db = session.CreateSQLQuery("SELECT DATABASE()").UniqueResult<string>();
        var hostName = session.CreateSQLQuery("SELECT @@hostname").UniqueResult<string>();
        var port = Convert.ToInt32(session.CreateSQLQuery("SELECT @@port").UniqueResult());

        var tableCount = Convert.ToInt32(session.CreateSQLQuery("SELECT COUNT(*) FROM Mahaiak").UniqueResult());
        var non4Count = Convert.ToInt32(session.CreateSQLQuery("SELECT COUNT(*) FROM Mahaiak WHERE pertsona_max <> 4").UniqueResult());
        var distinctValues = session
            .CreateSQLQuery("SELECT pertsona_max, COUNT(*) FROM Mahaiak GROUP BY pertsona_max ORDER BY pertsona_max")
            .List<object[]>()
            .Select(r => new { pertsona_max = Convert.ToInt32(r[0]), count = Convert.ToInt32(r[1]) })
            .ToList();

        var sample = session
            .CreateSQLQuery("SELECT id, mahai_zenbakia, pertsona_max FROM Mahaiak ORDER BY mahai_zenbakia LIMIT 20")
            .List<object[]>()
            .Select(r => new { id = Convert.ToInt32(r[0]), mahai_zenbakia = Convert.ToInt32(r[1]), pertsona_max = Convert.ToInt32(r[2]) })
            .ToList();

        List<int> occupiedMahaiIds;
        try
        {
            occupiedMahaiIds = session.CreateSQLQuery(@"
SELECT DISTINCT em.mahaiak_id
FROM Erreserbak_Mahaiak em
JOIN Erreserbak e ON e.id = em.erreserbak_id
WHERE DATE(e.data) = :date AND e.txanda = :txanda
")
                .SetParameter("date", date)
                .SetParameter("txanda", txanda)
                .List<object>()
                .Select(Convert.ToInt32)
                .OrderBy(x => x)
                .ToList();
        }
        catch
        {
            occupiedMahaiIds = new List<int>();
        }

        var version = typeof(MahaiakController).Assembly.GetName().Version?.ToString();

        return Ok(new
        {
            now,
            txanda,
            database = db,
            hostName,
            port,
            table = "mahaiak",
            tableCount,
            non4Count,
            distinctValues,
            version,
            sample,
            occupiedMahaiIds
        });
    }

    public class ComandaSessionRequest
    {
        public string Action { get; set; }
    }

    [HttpPost("{mahaiId}/comanda-session")]
    public ActionResult<TableSessionDto> EnsureComandaSession(int mahaiId, [FromBody] ComandaSessionRequest request)
    {
        using var session = _sessionFactory.OpenSession();
        using var transaction = session.BeginTransaction();

        var now = DateTime.Now;
        var txanda = now.Hour >= 12 && now.Hour < 19 ? "Bazkaria" : "Afaria";
        var date = now.Date;

        var row = session.CreateSQLQuery(@"
SELECT
    em.id AS erreserba_mahai_id,
    em.erreserbak_id AS erreserba_id
FROM Erreserbak_Mahaiak em
JOIN Erreserbak e ON e.id = em.erreserbak_id
WHERE em.mahaiak_id = :mahaiId AND DATE(e.data) = :date AND e.txanda = :txanda
ORDER BY em.id
LIMIT 1
")
            .SetParameter("mahaiId", mahaiId)
            .SetParameter("date", date)
            .SetParameter("txanda", txanda)
            .UniqueResult<object[]>();

        if (row == null)
        {
            transaction.Rollback();
            return Conflict(new { message = "Mahai honek ez dauka erreserbarik txanda honetan" });
        }

        var erreserbaMahaiId = Convert.ToInt32(row[0]);
        var erreserbaId = Convert.ToInt32(row[1]);

        var erreserbaRef = session.Load<Erreserba>(erreserbaId);

        var action = request?.Action?.Trim();
        var faktura = session.Query<Faktura>()
            .Where(f => f.Erreserba.Id == erreserbaId)
            .OrderByDescending(f => f.Id)
            .FirstOrDefault();

        if (faktura == null)
        {
            faktura = new Faktura
            {
                ErreserbakId = erreserbaId,
                Erreserba = erreserbaRef,
                Totala = 0,
                Egoera = false,
                FakturaPdf = null
            };
            session.Save(faktura);
        }
        else if (faktura.Egoera)
        {
            if (string.Equals(action, "reopen", StringComparison.OrdinalIgnoreCase))
            {
                faktura.Egoera = false;
                session.Update(faktura);
            }
            else if (string.Equals(action, "new", StringComparison.OrdinalIgnoreCase))
            {
                faktura = new Faktura
                {
                    ErreserbakId = erreserbaId,
                    Erreserba = erreserbaRef,
                    Totala = 0,
                    Egoera = false,
                    FakturaPdf = null
                };
                session.Save(faktura);
            }
            else
            {
                var computedTotalaRaw = session.CreateSQLQuery(@"
SELECT COALESCE(SUM(k.totala), 0)
FROM Komandak k
WHERE k.fakturak_id = :fakturaId
")
                    .SetParameter("fakturaId", faktura.Id)
                    .UniqueResult();

                var computedTotala = Convert.ToDouble(computedTotalaRaw);

                if (Math.Abs(faktura.Totala - computedTotala) > 0.0001)
                {
                    faktura.Totala = computedTotala;
                    session.Update(faktura);
                }

                transaction.Commit();
                return Ok(new TableSessionDto
                {
                    MahaiId = mahaiId,
                    ErreserbaMahaiId = erreserbaMahaiId,
                    ErreserbaId = erreserbaId,
                    FakturaId = faktura.Id,
                    FakturaEgoera = faktura.Egoera,
                    FakturaTotala = computedTotala,
                    RequiresDecision = true,
                    Txanda = txanda,
                    Data = date
                });
            }
        }

        transaction.Commit();

        return Ok(new TableSessionDto
        {
            MahaiId = mahaiId,
            ErreserbaMahaiId = erreserbaMahaiId,
            ErreserbaId = erreserbaId,
            FakturaId = faktura.Id,
            FakturaEgoera = faktura.Egoera,
            FakturaTotala = faktura.Totala,
            RequiresDecision = false,
            Txanda = txanda,
            Data = date
        });
    }

    // POST: api/mahaiak
    [HttpPost]
    public ActionResult<MahaiDto> CreateMahai([FromBody] MahaiCreateDto createDto)
    {
        using var session = _sessionFactory.OpenSession();
        using var transaction = session.BeginTransaction();

        try
        {
            if (createDto.MahaiZenbakia <= 0)
                return BadRequest(new { error = "Mahai zenbakia 0 baino handiagoa izan behar da" });

            if (createDto.PertsonaMax <= 0 || createDto.PertsonaMax > 20)
                return BadRequest(new { error = "Pertsona maximoak 1 eta 20 artean izan behar da" });

            // Egiaztatu zenbakia errepikatuta ez dagoela
            var exists = session.Query<Mahai>()
                .Any(m => m.MahaiZenbakia == createDto.MahaiZenbakia);

            if (exists)
                return BadRequest(new { error = "Mahai zenbakia dagoeneko existitzen da" });

            var mahai = new Mahai
            {
                MahaiZenbakia = createDto.MahaiZenbakia,
                PertsonaMax = createDto.PertsonaMax
            };

            session.Save(mahai);
            transaction.Commit();

            var dto = new MahaiDto
            {
                Id = mahai.Id,
                Zenbakia = mahai.MahaiZenbakia,
                PertsonaMax = mahai.PertsonaMax,
                Occupied = false
            };

            return CreatedAtAction(nameof(Get), new { id = mahai.Id }, dto);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // PUT: api/mahaiak/{id}
    [HttpPut("{id}")]
    public ActionResult UpdateMahai(int id, [FromBody] MahaiUpdateDto updateDto)
    {
        using var session = _sessionFactory.OpenSession();
        using var transaction = session.BeginTransaction();

        try
        {
            if (id != updateDto.Id)
                return BadRequest(new { error = "ID-ak ez datoz bat" });

            var mahai = session.Get<Mahai>(id);
            if (mahai == null)
                return NotFound(new { error = "Mahai ez da existitzen" });

            if (updateDto.MahaiZenbakia <= 0)
                return BadRequest(new { error = "Mahai zenbakia 0 baino handiagoa izan behar da" });

            if (updateDto.PertsonaMax <= 0 || updateDto.PertsonaMax > 20)
                return BadRequest(new { error = "Pertsona maximoak 1 eta 20 artean izan behar da" });

            // Egiaztatu zenbakia errepikatuta ez dagoela (beste mahai batean)
            var exists = session.Query<Mahai>()
                .Any(m => m.MahaiZenbakia == updateDto.MahaiZenbakia && m.Id != id);

            if (exists)
                return BadRequest(new { error = "Mahai zenbakia dagoeneko existitzen da" });

            mahai.MahaiZenbakia = updateDto.MahaiZenbakia;
            mahai.PertsonaMax = updateDto.PertsonaMax;

            session.Update(mahai);
            transaction.Commit();

            return NoContent();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // DELETE: api/mahaiak/{id}
    [HttpDelete("{id}")]
    public ActionResult DeleteMahai(int id)
    {
        using var session = _sessionFactory.OpenSession();
        using var transaction = session.BeginTransaction();

        try
        {
            var mahai = session.Get<Mahai>(id);
            if (mahai == null)
                return NotFound(new { error = "Mahai ez da existitzen" });

            // Egiaztatu erreserbarik ez duela
            var hasReservations = session.CreateSQLQuery(
                "SELECT COUNT(*) FROM Erreserbak_Mahaiak WHERE mahaiak_id = :id")
                .SetParameter("id", id)
                .UniqueResult<int>() > 0;

            if (hasReservations)
                return BadRequest(new { error = "Ezin da mahai ezabatu, erreserbak ditu" });

            session.Delete(mahai);
            transaction.Commit();

            return NoContent();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // GET: api/mahaiak/{id}
    [HttpGet("{id}")]
    public ActionResult<MahaiDto> GetMahai(int id)
    {
        using var session = _sessionFactory.OpenSession();

        var mahai = session.Get<Mahai>(id);
        if (mahai == null)
            return NotFound();

        // Egiaztatu okupatuta dagoen
        var now = DateTime.Now;
        var txanda = now.Hour >= 12 && now.Hour < 19 ? "Bazkaria" : "Afaria";
        var date = now.Date;

        var occupied = session.CreateSQLQuery(@"
SELECT COUNT(*) FROM Erreserbak_Mahaiak em
JOIN Erreserbak e ON e.id = em.erreserbak_id
WHERE em.mahaiak_id = :mahaiId AND DATE(e.data) = :date AND e.txanda = :txanda
")
            .SetParameter("mahaiId", id)
            .SetParameter("date", date)
            .SetParameter("txanda", txanda)
            .UniqueResult<int>() > 0;

        var dto = new MahaiDto
        {
            Id = mahai.Id,
            Zenbakia = mahai.MahaiZenbakia,
            PertsonaMax = mahai.PertsonaMax,
            Occupied = occupied
        };

        return Ok(dto);
    }
}

public record OccupiedInfo(int ErreserbaId, int PertsonaKopurua);
