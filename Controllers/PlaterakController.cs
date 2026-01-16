using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
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
    public IActionResult GetAll()
    {
        using var session = _sessionFactory.OpenSession();

        var platerak = session.Query<Platerak>()
            .Select(p => new PlateraDto
            {
                Id = p.Id,
                Izena = p.Izena,
                Prezioa = p.Prezioa,
                Stock = p.Stock,
                KategoriaId = p.Kategoriak.Id,
                KategoriaIzena = p.Kategoriak.Izena
            })
            .ToList();

        return Ok(platerak);
    }

    // ================= GET BY ID =================
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        using var session = _sessionFactory.OpenSession();

        var platera = session.Query<Platerak>()
            .Where(p => p.Id == id)
            .Select(p => new PlateraDto
            {
                Id = p.Id,
                Izena = p.Izena,
                Prezioa = p.Prezioa,
                Stock = p.Stock,
                KategoriaId = p.Kategoriak.Id,
                KategoriaIzena = p.Kategoriak.Izena
            })
            .FirstOrDefault();

        if (platera == null)
            return NotFound(new { message = "Platera ez da existitzen" });

        return Ok(platera);
    }

    // ================= GET BY KATEGORIA =================
    [HttpGet("kategoria/{kategoriaId}")]
    public IActionResult GetByKategoria(int kategoriaId)
    {
        using var session = _sessionFactory.OpenSession();

        var platerak = session.Query<Platerak>()
            .Where(p => p.Kategoriak.Id == kategoriaId)
            .Select(p => new PlateraDto
            {
                Id = p.Id,
                Izena = p.Izena,
                Prezioa = p.Prezioa,
                Stock = p.Stock,
                KategoriaId = p.Kategoriak.Id,
                KategoriaIzena = p.Kategoriak.Izena
            })
            .ToList();

        return Ok(platerak);
    }

    // ================= CREATE =================
    [HttpPost]
    public IActionResult Create([FromBody] PlateraDto dto)
    {
        using var session = _sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        var kategoria = session.Get<Kategoriak>(dto.KategoriaId);
        if (kategoria == null)
            return BadRequest(new { message = "Kategoria ez da existitzen" });

        var platera = new Platerak
        {
            Izena = dto.Izena,
            Prezioa = dto.Prezioa,
            Stock = dto.Stock,
            Kategoriak = kategoria
        };

        session.Save(platera);
        tx.Commit();

        return Ok(new { message = "Sortuta!", id = platera.Id });
    }

    // ================= UPDATE =================
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] PlateraDto dto)
    {
        using var session = _sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        var existing = session.Get<Platerak>(id);
        if (existing == null)
            return NotFound(new { message = "Platera ez da existitzen" });

        existing.Izena = dto.Izena;
        existing.Prezioa = dto.Prezioa;
        existing.Stock = dto.Stock;

        if (dto.KategoriaId != 0)
        {
            var kategoria = session.Get<Kategoriak>(dto.KategoriaId);
            if (kategoria != null)
                existing.Kategoriak = kategoria;
        }

        session.Update(existing);
        tx.Commit();

        return Ok(new { message = "Eguneratua!" });
    }

    // ================= DELETE =================
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        using var session = _sessionFactory.OpenSession();
        using var tx = session.BeginTransaction();

        var platera = session.Get<Platerak>(id);
        if (platera == null)
            return NotFound(new { message = "Platera ez da existitzen" });

        session.Delete(platera);
        tx.Commit();

        return Ok(new { message = "Platera ezabatuta!" });
    }


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

        // Comprobar ingredientes
        var osagaiak = session.Query<PlaterakOsagaia>()
            .Where(po => po.Platerak.Id == dto.PlaterId)
            .ToList();

        foreach (var po in osagaiak)
        {
            int beharrezkoa = po.Kopurua * dto.Kopurua;

            if (po.Osagaia.Stock < beharrezkoa)
                return BadRequest(false);
        }

        // Restar stock plato
        platera.Stock -= dto.Kopurua;
        session.Update(platera);

        // Restar stock ingredientes
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

        platera.Stock += dto.Kopurua;
        session.Update(platera);

        var osagaiak = session.Query<PlaterakOsagaia>()
            .Where(po => po.Platerak.Id == dto.PlaterId)
            .ToList();

        foreach (var po in osagaiak)
        {
            po.Osagaia.Stock += po.Kopurua * dto.Kopurua;
            session.Update(po.Osagaia);
        }

        tx.Commit();
        return Ok(true);
    }




}
