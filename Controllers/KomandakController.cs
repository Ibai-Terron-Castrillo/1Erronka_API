using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;




[ApiController]
[Route("api/[controller]")]
public class KomandakController : ControllerBase
{
    private readonly ISessionFactory _sessionFactory;

    public KomandakController(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    // GET
    [HttpGet]
    public ActionResult<IEnumerable<Komanda>> Get()
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var komandak = session.Query<Komanda>().ToList();
            return Ok(komandak);
        }
    }

    // GET BY ID
    [HttpGet("{id}")]
    public ActionResult<Komanda> Get(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var komanda = session.Get<Komanda>(id);
            if (komanda == null)
                return NotFound();
            return Ok(komanda);
        }
    }

    // GET BY FAKTURA ID
    [HttpGet("faktura/{fakturaId}")]
    public ActionResult<IEnumerable<Komanda>> GetByFaktura(int fakturaId)
    {
        using var session = _sessionFactory.OpenSession();

        var komandak = session.Query<Komanda>()
            .Where(k => k.Faktura.Id == fakturaId)
            .ToList();

        return Ok(komandak);
    }

    [HttpGet("faktura/{fakturaId}/items")]
    public ActionResult<IEnumerable<KomandaItemDto>> GetItemsByFaktura(int fakturaId)
    {
        using var session = _sessionFactory.OpenSession();

        var items = session.Query<Komanda>()
            .Where(k => k.Faktura.Id == fakturaId)
            .Select(k => new KomandaItemDto
            {
                Id = k.Id,
                PlaterakId = k.Platerak.Id,
                FakturakId = k.Faktura.Id,
                Kopurua = k.Kopurua,
                Oharrak = k.Oharrak
            })
            .ToList();

        return Ok(items);
    }


    // POST
    [HttpPost]
    public IActionResult SortuKomanda([FromBody] KomandaSortuDto dto)
    {
        using var session = _sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        
        var platera = session.Get<Platerak>(dto.PlaterakId);
        if (platera == null)
            return BadRequest("Platera ez da existitzen");

        var faktura = session.Get<Faktura>(dto.FakturakId);
        if (faktura == null)
            return BadRequest("Faktura ez da existitzen");

        
        if (platera.Stock < dto.Kopurua)
            return BadRequest("Ez dago stock nahikorik");

        
        var komanda = new Komanda
        {
            Platerak = platera,
            Faktura = faktura,
            Kopurua = dto.Kopurua,
            Totala = dto.Kopurua * platera.Prezioa,
            Egoera = false
        };


        session.Save(komanda);

        
        platera.Stock -= dto.Kopurua;
        session.Update(platera);

        tx.Commit();
        return Ok();
    }

    [HttpPut("{id}/quantity")]
    public ActionResult UpdateQuantity(int id, [FromBody] KomandaQuantityDto dto)
    {
        using var session = _sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        try
        {
            var komanda = session.Get<Komanda>(id);
            if (komanda == null) return NotFound();

            if (dto == null) return BadRequest("Body hutsik");
            if (dto.Kopurua <= 0) return BadRequest("Kopurua okerra");

            var plateraId = komanda.Platerak?.Id ?? 0;
            if (plateraId <= 0) return StatusCode(500, "Platera ez da aurkitu komandan");

            var platera = session.Get<Platerak>(plateraId);
            if (platera == null) return StatusCode(500, "Platera ez da existitzen");

            var delta = dto.Kopurua - komanda.Kopurua;
            if (delta > 0)
            {
                if (platera.Stock < delta) return BadRequest("Ez dago stock nahikorik");
                platera.Stock -= delta;
            }
            else if (delta < 0)
            {
                platera.Stock += -delta;
            }

            komanda.Kopurua = dto.Kopurua;
            komanda.Totala = dto.Kopurua * platera.Prezioa;

            session.Update(platera);
            session.Update(komanda);
            tx.Commit();
            return NoContent();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return StatusCode(500, $"Errorea: {ex.Message}");
        }
    }

    [HttpPut("{id}/oharrak")]
    public ActionResult UpdateOharrak(int id, [FromBody] KomandaOharrakDto dto)
    {
        using var session = _sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        try
        {
            var komanda = session.Get<Komanda>(id);
            if (komanda == null) return NotFound();

            komanda.Oharrak = dto?.Oharrak;
            session.Update(komanda);
            tx.Commit();
            return NoContent();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return StatusCode(500, $"Errorea: {ex.Message}");
        }
    }




    // PUT
    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] Komanda komanda)
    {
        if (id != komanda.Id)
            return BadRequest("ID-ak ez datoz bat");

        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var existing = session.Get<Komanda>(id);
                if (existing == null)
                    return NotFound();

                if (existing.Kopurua != komanda.Kopurua || existing.Platerak != komanda.Platerak)
                {
                    var plateraZaharraId = existing.Platerak?.Id ?? 0;
                    var plateraBerriaId = komanda.Platerak?.Id ?? 0;
                    var plateraZaharra = plateraZaharraId > 0 ? session.Get<Platerak>(plateraZaharraId) : null;
                    var plateraBerria = plateraBerriaId > 0 ? session.Get<Platerak>(plateraBerriaId) : null;

                    if (plateraBerria == null)
                        return BadRequest("Platera berria ez da existitzen");

                    if (plateraZaharra != null)
                    {
                        plateraZaharra.Stock += existing.Kopurua;
                        session.Update(plateraZaharra);
                    }

                    if (plateraBerria.Stock < komanda.Kopurua)
                        return BadRequest($"Stock nahikorik ez platera berrian: {plateraBerria.Stock}");

                    plateraBerria.Stock -= komanda.Kopurua;
                    session.Update(plateraBerria);
                }

                existing.Platerak = komanda.Platerak;
                existing.Faktura = komanda.Faktura;
                existing.Kopurua = komanda.Kopurua;
                existing.Totala = komanda.Totala;
                existing.Oharrak = komanda.Oharrak;
                existing.Egoera = komanda.Egoera;

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
                var komanda = session.Get<Komanda>(id);
                if (komanda == null)
                    return NotFound();

                var plateraId = komanda.Platerak?.Id ?? 0;
                var platera = plateraId > 0 ? session.Get<Platerak>(plateraId) : null;
                if (platera != null)
                {
                    platera.Stock += komanda.Kopurua;
                    session.Update(platera);
                }

                session.Delete(komanda);
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

    // PATCH EGOERA
    [HttpPatch("{id}/egoera")]
    public ActionResult UpdateEgoera(int id, [FromBody] bool egoera)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var komanda = session.Get<Komanda>(id);
                if (komanda == null)
                    return NotFound();

                komanda.Egoera = egoera;
                session.Update(komanda);
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

public class KomandaItemDto
{
    public int Id { get; set; }
    public int PlaterakId { get; set; }
    public int FakturakId { get; set; }
    public int Kopurua { get; set; }
    public string Oharrak { get; set; }
}

public class KomandaQuantityDto
{
    public int Kopurua { get; set; }
}

public class KomandaOharrakDto
{
    public string Oharrak { get; set; }
}
