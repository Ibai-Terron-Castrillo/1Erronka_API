using Microsoft.AspNetCore.Mvc;
using NHibernate;
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
				.Where(p => p.KategoriakId == kategoriaId)
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