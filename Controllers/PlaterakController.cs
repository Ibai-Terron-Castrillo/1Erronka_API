using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class PlaterakController : ControllerBase
{
    private readonly ISessionFactory _sessionFactory;

    public PlaterakController(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    // ================= GET ALL =================
    [HttpGet]
    public IActionResult Get()
    {
        using var session = _sessionFactory.OpenSession();

        var platerak = session.Query<Platerak>()
            .Select(p => new PlaterakDto
            {
                Id = p.Id,
                Izena = p.Izena,
                Prezioa = p.Prezioa,
                Stock = p.Stock,
                KategoriaId = p.Kategoriak != null ? p.Kategoriak.Id : 0
            })
            .ToList();

        return Ok(platerak);
    }

    // ================= GET BY ID =================
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        using var session = _sessionFactory.OpenSession();

        var platera = session.Query<Platerak>()
            .Fetch(p => p.Kategoriak)
            .FirstOrDefault(p => p.Id == id);

        if (platera == null)
            return NotFound();

        return Ok(platera);
    }

    // ================= GET BY KATEGORIA =================
    [HttpGet("kategoria/{kategoriaId}")]
    public IActionResult GetByKategoria(int kategoriaId)
    {
        using var session = _sessionFactory.OpenSession();

        var platerak = session.Query<Platerak>()
            .Where(p => p.Kategoriak != null && p.Kategoriak.Id == kategoriaId)
            .Select(p => new PlaterakDto
            {
                Id = p.Id,
                Izena = p.Izena,
                Prezioa = p.Prezioa,
                Stock = p.Stock,
                KategoriaId = p.Kategoriak.Id
            })
            .ToList();

        return Ok(platerak);
    }


    // ================= CREATE =================
    [HttpPost]
    public IActionResult Post([FromBody] Platerak platera)
    {
        using var session = _sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        if (string.IsNullOrWhiteSpace(platera.Izena))
            return BadRequest("Izena ezin da hutsik egon");

        if (platera.Prezioa <= 0)
            return BadRequest("Prezioa okerra da");

        if (platera.Stock < 0)
            return BadRequest("Stock ezin da negatiboa izan");

        if (platera.Kategoriak == null || platera.Kategoriak.Id <= 0)
            return BadRequest("Kategoria derrigorrezkoa da");

        var kategoria = session.Get<Kategoriak>(platera.Kategoriak.Id);
        if (kategoria == null)
            return BadRequest("Kategoria ez da existitzen");

        platera.Kategoriak = kategoria;

        session.Save(platera);
        tx.Commit();

        return Ok();
    }

    // ================= UPDATE =================
    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] Platerak platera)
    {
        if (id != platera.Id)
            return BadRequest("ID-ak ez datoz bat");

        using var session = _sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        var existing = session.Query<Platerak>()
            .Fetch(p => p.Kategoriak)
            .FirstOrDefault(p => p.Id == id);

        if (existing == null)
            return NotFound();

        existing.Izena = platera.Izena;
        existing.Prezioa = platera.Prezioa;
        existing.Stock = platera.Stock;

        if (platera.Kategoriak != null && platera.Kategoriak.Id > 0)
        {
            var kat = session.Get<Kategoriak>(platera.Kategoriak.Id);
            if (kat != null)
                existing.Kategoriak = kat;
        }

        session.Update(existing);
        tx.Commit();

        return NoContent();
    }

    // ================= DELETE =================
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        using var session = _sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        var platera = session.Get<Platerak>(id);
        if (platera == null)
            return NotFound();

        var erlazioak = session.Query<PlaterakOsagaia>()
            .Count(po => po.Platerak.Id == id);

        if (erlazioak > 0)
            return BadRequest("Platera osagaiekin erlazionatuta dago");

        session.Delete(platera);
        tx.Commit();

        return NoContent();
    }

    // ================= STOCK =================
    [HttpPost("jaitsi-stock")]
    public IActionResult JaitsiStock([FromBody] StockAldaketaDto dto)
    {
        using var session = _sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        var platera = session.Get<Platerak>(dto.PlaterId);
        if (platera == null)
            return NotFound();

        if (platera.Stock < dto.Kopurua)
            return BadRequest(false);

        
        var osagaiak = session.Query<PlaterakOsagaia>()
            .Where(po => po.Platerak.Id == dto.PlaterId)
            .Fetch(po => po.Osagaia)
            .ToList();

        
        foreach (var po in osagaiak)
        {
            int beharrezkoa = po.Kopurua * dto.Kopurua;

            if (po.Osagaia.Stock < beharrezkoa)
                return BadRequest(false);
        }

        
        platera.Stock -= dto.Kopurua;
        session.Update(platera);

     
        foreach (var po in osagaiak)
        {
            po.Osagaia.Stock -= po.Kopurua * dto.Kopurua;
            session.Update(po.Osagaia);
        }

        tx.Commit();
        return Ok(true);
    }


    [HttpPost("itzuli-stock")]
    public IActionResult ItzuliStock([FromBody] StockAldaketaDto dto)
    {
        using var session = _sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        var platera = session.Get<Platerak>(dto.PlaterId);
        if (platera == null)
            return NotFound();

        
        var osagaiak = session.Query<PlaterakOsagaia>()
            .Where(po => po.Platerak.Id == dto.PlaterId)
            .Fetch(po => po.Osagaia)
            .ToList();

       
        platera.Stock += dto.Kopurua;
        session.Update(platera);

        
        foreach (var po in osagaiak)
        {
            po.Osagaia.Stock += po.Kopurua * dto.Kopurua;
            session.Update(po.Osagaia);
        }

        tx.Commit();
        return Ok(true);
    }

}
