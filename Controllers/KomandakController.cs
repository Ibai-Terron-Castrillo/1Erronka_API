using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

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
        using (var session = _sessionFactory.OpenSession())
        {
            var komandak = session.Query<Komanda>()
                .Where(k => k.FakturakId == fakturaId)
                .ToList();
            return Ok(komandak);
        }
    }

    // POST
    [HttpPost]
    public ActionResult<Komanda> Post([FromBody] Komanda komanda)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var platera = session.Get<Platerak>(komanda.PlaterakId);
                if (platera == null)
                    return BadRequest("Platera ez da existitzen");

                if (platera.Stock < komanda.Kopurua)
                    return BadRequest($"Stock nahikorik ez: {platera.Stock} geratzen da");

                if (komanda.Totala <= 0)
                    komanda.Totala = komanda.Kopurua * platera.Prezioa;

                session.Save(komanda);

                platera.Stock -= komanda.Kopurua;
                session.Update(platera);

                transaction.Commit();
                return CreatedAtAction(nameof(Get), new { id = komanda.Id }, komanda);
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
    public ActionResult Put(int id, [FromBody] Komanda komanda)
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

                if (existing.Kopurua != komanda.Kopurua || existing.PlaterakId != komanda.PlaterakId)
                {
                    var plateraZaharra = session.Get<Platerak>(existing.PlaterakId);
                    var plateraBerria = session.Get<Platerak>(komanda.PlaterakId);

                    if (plateraBerria == null)
                        return BadRequest("Platera berria ez da existitzen");

                    plateraZaharra.Stock += existing.Kopurua;
                    session.Update(plateraZaharra);

                    if (plateraBerria.Stock < komanda.Kopurua)
                        return BadRequest($"Stock nahikorik ez platera berrian: {plateraBerria.Stock}");

                    plateraBerria.Stock -= komanda.Kopurua;
                    session.Update(plateraBerria);
                }

                existing.PlaterakId = komanda.PlaterakId;
                existing.FakturakId = komanda.FakturakId;
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

                var platera = session.Get<Platerak>(komanda.PlaterakId);
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