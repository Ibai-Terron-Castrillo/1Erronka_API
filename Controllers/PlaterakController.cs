using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class PlaterakController : ControllerBase
{
<<<<<<< Updated upstream
	private readonly ISessionFactory _sessionFactory;

	public PlaterakController(ISessionFactory sessionFactory)
	{
		_sessionFactory = sessionFactory;
	}

	// GET: api/platerak
	[HttpGet]
	public IActionResult Get()
	{
		using var session = _sessionFactory.OpenSession();

		// Usando sintaxis de consulta para evitar conflictos con Select
		var platerak = (from p in session.Query<Platerak>()
						select new PlaterakDto
						{
							Id = p.Id,
							Izena = p.Izena,
							Prezioa = p.Prezioa,
							Stock = p.Stock,
							KategoriakId = p.Kategoriak != null ? p.Kategoriak.Id : 0
						}).ToList();

		return Ok(platerak);
	}

	// GET: api/platerak/{id}
	[HttpGet("{id}")]
	public ActionResult<Platerak> Get(int id)
	{
		using (var session = _sessionFactory.OpenSession())
		{
			var platera = session.Query<Platerak>()
				.Fetch(p => p.Kategoriak)
				.FirstOrDefault(p => p.Id == id);

			if (platera == null)
				return NotFound();
			return Ok(platera);
		}
	}

	// POST: api/platerak
	[HttpPost]
	public ActionResult<Platerak> Post([FromBody] Platerak platera)
	{
		using (var session = _sessionFactory.OpenSession())
		using (var transaction = session.BeginTransaction())
		{
			try
			{
				if (string.IsNullOrWhiteSpace(platera.Izena))
					return BadRequest("Izena ezin da hutsik egon");

				if (platera.Prezioa <= 0)
					return BadRequest("Prezioa 0 baino handiagoa izan behar da");

				if (platera.Stock < 0)
					return BadRequest("Stock ezin da negatiboa izan");

				// Verificar y cargar la categoría
				if (platera.Kategoriak == null || platera.Kategoriak.Id <= 0)
					return BadRequest("Kategoria ID baliozkoa izan behar da");

				var kategoriak = session.Get<Kategoriak>(platera.Kategoriak.Id);
				if (kategoriak == null)
					return BadRequest("Kategoria ez da existitzen");

				platera.Kategoriak = kategoriak;

				session.Save(platera);
				transaction.Commit();
				return CreatedAtAction(nameof(Get), new { id = platera.Id }, platera);
			}
			catch (Exception ex)
			{
				transaction.Rollback();
				return StatusCode(500, $"Errorea: {ex.Message}");
			}
		}
	}

	// PUT: api/platerak/{id}
	[HttpPut("{id}")]
	public ActionResult Put(int id, [FromBody] Platerak platera)
	{
		if (id != platera.Id)
			return BadRequest("ID-ak ez datoz bat");

		using (var session = _sessionFactory.OpenSession())
		using (var transaction = session.BeginTransaction())
		{
			try
			{
				var existing = session.Query<Platerak>()
					.Fetch(p => p.Kategoriak)
					.FirstOrDefault(p => p.Id == id);

				if (existing == null)
					return NotFound();

				if (string.IsNullOrWhiteSpace(platera.Izena))
					return BadRequest("Izena ezin da hutsik egon");

				if (platera.Prezioa <= 0)
					return BadRequest("Prezioa 0 baino handiagoa izan behar da");

				if (platera.Stock < 0)
					return BadRequest("Stock ezin da negatiboa izan");

				// Actualizar categoría si es necesario
				if (platera.Kategoriak != null && platera.Kategoriak.Id > 0)
				{
					var kategoriak = session.Get<Kategoriak>(platera.Kategoriak.Id);
					if (kategoriak != null)
						existing.Kategoriak = kategoriak;
				}

				existing.Izena = platera.Izena;
				existing.Prezioa = platera.Prezioa;
				existing.Stock = platera.Stock;

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

	// DELETE: api/platerak/{id}
	[HttpDelete("{id}")]
	public ActionResult Delete(int id)
	{
		using (var session = _sessionFactory.OpenSession())
		using (var transaction = session.BeginTransaction())
		{
			try
			{
				var platera = session.Get<Platerak>(id);
				if (platera == null)
					return NotFound();

				// Verificar si hay relaciones con Platerak_Osagaiak
				var relacionesQuery = "SELECT COUNT(*) FROM Platerak_Osagaiak WHERE platerak_id = :id";
				var relacionesCount = session.CreateSQLQuery(relacionesQuery)
					.SetParameter("id", id)
					.UniqueResult<int>();

				if (relacionesCount > 0)
					return BadRequest($"Ezin da platerra ezabatu, {relacionesCount} osagairekin erlazionatuta dago");

				session.Delete(platera);
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

	// PATCH: api/platerak/{id}/stock
	[HttpPatch("{id}/stock")]
	public ActionResult UpdateStock(int id, [FromBody] StockEguneratuDto update)
	{
		using (var session = _sessionFactory.OpenSession())
		using (var transaction = session.BeginTransaction())
		{
			try
			{
				var platera = session.Get<Platerak>(id);
				if (platera == null)
					return NotFound();

				platera.Stock += update.Kopurua;

				if (platera.Stock < 0)
					return BadRequest("Ezin da stock negatiboa izan");

				session.Update(platera);
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

	// GET: api/platerak/{id}/osagaiak
	[HttpGet("{id}/osagaiak")]
	public ActionResult<IEnumerable<PlaterakOsagaiaDto>> GetOsagaiak(int id)
	{
		using (var session = _sessionFactory.OpenSession())
		{
			var platera = session.Get<Platerak>(id);
			if (platera == null)
				return NotFound();

			// Usando consulta SQL para obtener los osagaiak
			var query = @"
                SELECT 
                    po.id as Id,
                    po.osagaiak_id as OsagaiakId,
                    po.platerak_id as PlaterakId,
                    po.kopurua as Kopurua,
                    o.izena as OsagaiaIzena,
                    o.azken_prezioa as OsagaiaPrezioa
                FROM Platerak_Osagaiak po
                INNER JOIN Osagaiak o ON po.osagaiak_id = o.id
                WHERE po.platerak_id = :platerakId";

			var osagaiak = session.CreateSQLQuery(query)
				.SetParameter("platerakId", id)
				.SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean<PlaterakOsagaiaDto>())
				.List<PlaterakOsagaiaDto>();

			return Ok(osagaiak);
		}
	}

	// GET: api/platerak/search/{term}
	[HttpGet("search/{term}")]
	public ActionResult<IEnumerable<Platerak>> Search(string term)
	{
		using (var session = _sessionFactory.OpenSession())
		{
			var platerak = (from p in session.Query<Platerak>()
							where p.Izena.Contains(term)
							select p).ToList();

			return Ok(platerak);
		}
	}

	// GET: api/platerak/kategoria/{kategoriaId}
	[HttpGet("kategoria/{kategoriaId}")]
	public ActionResult<IEnumerable<Platerak>> GetByKategoria(int kategoriaId)
	{
		using (var session = _sessionFactory.OpenSession())
		{
			var platerak = (from p in session.Query<Platerak>()
							where p.Kategoriak != null && p.Kategoriak.Id == kategoriaId
							select p).ToList();

			return Ok(platerak);
		}
	}

	// GET: api/platerak/stock-gutxi
	[HttpGet("stock-gutxi")]
	public ActionResult<IEnumerable<Platerak>> GetStockGutxi()
	{
		using (var session = _sessionFactory.OpenSession())
		{
			var platerak = (from p in session.Query<Platerak>()
							where p.Stock < 10
							select p).ToList();

			return Ok(platerak);
		}
	}

	// GET: api/platerak/{id}/kostua
	[HttpGet("{id}/kostua")]
	public ActionResult<double> KalkulatuKostua(int id)
	{
		using (var session = _sessionFactory.OpenSession())
		{
			var query = @"
                SELECT SUM(po.kopurua * o.azken_prezioa) as TotalKostua
                FROM Platerak_Osagaiak po
                INNER JOIN Osagaiak o ON po.osagaiak_id = o.id
                WHERE po.platerak_id = :platerakId";

			var result = session.CreateSQLQuery(query)
				.SetParameter("platerakId", id)
				.UniqueResult();

			if (result == null || result == DBNull.Value)
				return Ok(0);

			return Ok(Convert.ToDouble(result));
		}
	}
}

// DTOs
public class PlaterakDto
{
	public int Id { get; set; }
	public string Izena { get; set; }
	public double Prezioa { get; set; }
	public int Stock { get; set; }
	public int KategoriakId { get; set; }
}

public class PlaterakOsagaiaDto
{
	public int Id { get; set; }
	public int OsagaiakId { get; set; }
	public int PlaterakId { get; set; }
	public int Kopurua { get; set; }
	public string OsagaiaIzena { get; set; }
	public double OsagaiaPrezioa { get; set; }
}

public class StockEguneratuDto
{
	public int Kopurua { get; set; }
=======
    private readonly ISessionFactory _sessionFactory;

    public PlaterakController(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    // GET: api/platerak
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
                KategoriakId = p.KategoriakId
            })
            .ToList();

        return Ok(platerak);
    }

    // GET: api/platerak/{id}
    [HttpGet("{id}")]
    public ActionResult<Platerak> Get(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var platera = session.Get<Platerak>(id);
            if (platera == null)
                return NotFound();
            return Ok(platera);
        }
    }

    // POST: api/platerak
    [HttpPost]
    public ActionResult<Platerak> Post([FromBody] Platerak platera)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                if (string.IsNullOrWhiteSpace(platera.Izena))
                    return BadRequest("Izena ezin da hutsik egon");

                if (platera.Prezioa <= 0)
                    return BadRequest("Prezioa 0 baino handiagoa izan behar da");

                if (platera.Stock < 0)
                    return BadRequest("Stock ezin da negatiboa izan");

                session.Save(platera);
                transaction.Commit();
                return CreatedAtAction(nameof(Get), new { id = platera.Id }, platera);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return StatusCode(500, $"Errorea: {ex.Message}");
            }
        }
    }

    // PUT: api/platerak/{id}
    [HttpPut("{id}")]
    public ActionResult Put(int id, [FromBody] Platerak platera)
    {
        if (id != platera.Id)
            return BadRequest("ID-ak ez datoz bat");

        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var existing = session.Get<Platerak>(id);
                if (existing == null)
                    return NotFound();

                if (string.IsNullOrWhiteSpace(platera.Izena))
                    return BadRequest("Izena ezin da hutsik egon");

                if (platera.Prezioa <= 0)
                    return BadRequest("Prezioa 0 baino handiagoa izan behar da");

                if (platera.Stock < 0)
                    return BadRequest("Stock ezin da negatiboa izan");

                existing.Izena = platera.Izena;
                existing.Prezioa = platera.Prezioa;
                existing.Stock = platera.Stock;
                existing.KategoriakId = platera.KategoriakId;

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

    // DELETE: api/platerak/{id}
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var platera = session.Get<Platerak>(id);
                if (platera == null)
                    return NotFound();

                var relacionesCount = session.Query<PlaterakOsagaia>()
                    .Count(po => po.PlaterakId == id);

                if (relacionesCount > 0)
                    return BadRequest($"Ezin da platerra ezabatu, {relacionesCount} osagairekin erlazionatuta dago");

                session.Delete(platera);
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

    // PATCH: api/platerak/{id}/stock
    [HttpPatch("{id}/stock")]
    public ActionResult UpdateStock(int id, [FromBody] StockEguneratuDto update)
    {
        using (var session = _sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var platera = session.Get<Platerak>(id);
                if (platera == null)
                    return NotFound();

                platera.Stock += update.Kopurua;

                if (platera.Stock < 0)
                    return BadRequest("Ezin da stock negatiboa izan");

                session.Update(platera);
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

    // GET: api/platerak/{id}/osagaiak
    [HttpGet("{id}/osagaiak")]
    public ActionResult<IEnumerable<PlaterakOsagaiaDto>> GetOsagaiak(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var platera = session.Get<Platerak>(id);
            if (platera == null)
                return NotFound();

            var osagaiak = session.Query<PlaterakOsagaia>()
                .Where(po => po.PlaterakId == id)
                .Select(po => new PlaterakOsagaiaDto
                {
                    Id = po.Id,
                    OsagaiakId = po.OsagaiakId,
                    PlaterakId = po.PlaterakId,
                    Kopurua = po.Kopurua,
                    OsagaiaIzena = po.Osagaia != null ? po.Osagaia.Izena : "",
                    OsagaiaPrezioa = po.Osagaia != null ? po.Osagaia.AzkenPrezioa : 0
                })
                .ToList();

            return Ok(osagaiak);
        }
    }

    // GET: api/platerak/search/{term}
    [HttpGet("search/{term}")]
    public ActionResult<IEnumerable<Platerak>> Search(string term)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var platerak = session.Query<Platerak>()
                .Where(p => p.Izena.Contains(term))
                .ToList();

            return Ok(platerak);
        }
    }

    // GET: api/platerak/kategoria/{kategoriaId}
    [HttpGet("kategoria/{kategoriaId}")]
    public ActionResult<IEnumerable<Platerak>> GetByKategoria(int kategoriaId)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var platerak = session.Query<Platerak>()
    .Where(p => p.Kategoriak.Id == kategoriaId)
    .Select(p => new PlaterakDto
    {
        Id = p.Id,
        Izena = p.Izena,
        Prezioa = p.Prezioa,
        Stock = p.Stock,
        KategoriakId = p.Kategoriak.Id
    })
    .ToList();


            return Ok(platerak);
        }
    }

    // GET: api/platerak/stock-gutxi
    [HttpGet("stock-gutxi")]
    public ActionResult<IEnumerable<Platerak>> GetStockGutxi()
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var platerak = session.Query<Platerak>()
                .Where(p => p.Stock < 10)
                .ToList();

            return Ok(platerak);
        }
    }

    // GET: api/platerak/{id}/kostua
    [HttpGet("{id}/kostua")]
    public ActionResult<double> KalkulatuKostua(int id)
    {
        using (var session = _sessionFactory.OpenSession())
        {
            var relations = session.Query<PlaterakOsagaia>()
                .Where(po => po.PlaterakId == id)
                .ToList();

            double totalKostua = 0;
            foreach (var relation in relations)
            {
                var osagaia = session.Get<Osagaia>(relation.OsagaiakId);
                if (osagaia != null)
                {
                    totalKostua += relation.Kopurua * osagaia.AzkenPrezioa;
                }
            }

            return Ok(totalKostua);
        }


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

        
        var osagaiak = session.Query<PlaterakOsagaia>()
            .Where(po => po.Platerak.Id == dto.PlaterId)
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

    // DTOs
    public class PlaterakDto
    {
        public int Id { get; set; }
        public string Izena { get; set; }
        public double Prezioa { get; set; }
        public int Stock { get; set; }
        public int KategoriakId { get; set; }
    }

    public class PlaterakOsagaiaDto
    {
        public int Id { get; set; }
        public int OsagaiakId { get; set; }
        public int PlaterakId { get; set; }
        public int Kopurua { get; set; }
        public string OsagaiaIzena { get; set; }
        public double OsagaiaPrezioa { get; set; }
    }

    public class StockEguneratuDto
    {
        public int Kopurua { get; set; }
    }
>>>>>>> Stashed changes
}