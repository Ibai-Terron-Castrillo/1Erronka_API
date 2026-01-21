using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using API.DTO;

namespace API.Controllers
{
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
				.Fetch(p => p.Kategoriak)
				.Select(p => new PlaterakDto
				{
					Id = p.Id,
					Izena = p.Izena,
					Prezioa = p.Prezioa,
					Stock = p.Stock,
					KategoriaId = p.Kategoriak != null ? p.Kategoriak.Id : 0,
					KategoriaIzena = p.Kategoriak != null ? p.Kategoriak.Izena : "Kategoria gabe"
				})
				.ToList();

			return Ok(platerak);
		}

		// GET: api/platerak/{id}
		[HttpGet("{id}")]
		public ActionResult<PlaterakDto> Get(int id)
		{
			using var session = _sessionFactory.OpenSession();

			var platera = session.Query<Platerak>()
				.Fetch(p => p.Kategoriak)
				.Where(p => p.Id == id)
				.Select(p => new PlaterakDto
				{
					Id = p.Id,
					Izena = p.Izena,
					Prezioa = p.Prezioa,
					Stock = p.Stock,
					KategoriaId = p.Kategoriak != null ? p.Kategoriak.Id : 0,
					KategoriaIzena = p.Kategoriak != null ? p.Kategoriak.Izena : "Kategoria gabe"
				})
				.FirstOrDefault();

			if (platera == null)
				return NotFound();

			return Ok(platera);
		}

		// POST: api/platerak
		[HttpPost]
		public ActionResult<PlaterakDto> Post([FromBody] PlateraPostDto plateraDto)
		{
			using var session = _sessionFactory.OpenSession();
			using var transaction = session.BeginTransaction();

			try
			{
				if (string.IsNullOrWhiteSpace(plateraDto.Izena))
					return BadRequest("Izena ezin da hutsik egon");

				if (plateraDto.Prezioa <= 0)
					return BadRequest("Prezioa 0 baino handiagoa izan behar da");

				if (plateraDto.Stock < 0)
					return BadRequest("Stock ezin da negatiboa izan");

				if (plateraDto.Kategoria == null || plateraDto.Kategoria.Id <= 0)
					return BadRequest("Kategoria ID baliozkoa izan behar da");

				var kategoriak = session.Get<Kategoriak>(plateraDto.Kategoria.Id);
				if (kategoriak == null)
					return BadRequest("Kategoria ez da existitzen");

				var platera = new Platerak
				{
					Izena = plateraDto.Izena,
					Prezioa = plateraDto.Prezioa,
					Stock = plateraDto.Stock,
					Kategoriak = kategoriak
				};

				session.Save(platera);
				transaction.Commit();

				var responseDto = new PlaterakDto
				{
					Id = platera.Id,
					Izena = platera.Izena,
					Prezioa = platera.Prezioa,
					Stock = platera.Stock,
					KategoriaId = kategoriak.Id,
					KategoriaIzena = kategoriak.Izena
				};

				return CreatedAtAction(nameof(Get), new { id = platera.Id }, responseDto);
			}
			catch (Exception ex)
			{
				transaction.Rollback();
				return StatusCode(500, $"Errorea: {ex.Message}");
			}
		}

		// PUT: api/platerak/{id}
		[HttpPut("{id}")]
		public ActionResult Put(int id, [FromBody] JsonElement body)
		{
			using var session = _sessionFactory.OpenSession();
			using var transaction = session.BeginTransaction();

			try
			{
				// 1. Bilatu platera
				var existing = session.Get<Platerak>(id);
				if (existing == null)
				{
					return NotFound(new { error = "Platerra ez da existitzen", id });
				}

				// 2. Datuak eguneratu
				existing.Izena = body.GetProperty("izena").GetString();
				existing.Prezioa = body.GetProperty("prezioa").GetDouble();
				existing.Stock = body.GetProperty("stock").GetInt32();

				// 3. Kategoria eguneratu
				if (body.TryGetProperty("kategoria", out JsonElement kategoriaJson))
				{
					int kategoriaId = kategoriaJson.GetProperty("id").GetInt32();
					if (kategoriaId > 0)
					{
						var kategoriak = session.Get<Kategoriak>(kategoriaId);
						if (kategoriak != null)
						{
							existing.Kategoriak = kategoriak;
						}
					}
				}

				// 4. BALIDAZIOAK
				if (string.IsNullOrWhiteSpace(existing.Izena))
					return BadRequest(new { error = "Izena ezin da hutsik egon" });

				if (existing.Prezioa <= 0)
					return BadRequest(new { error = "Prezioa 0 baino handiagoa izan behar da" });

				if (existing.Stock < 0)
					return BadRequest(new { error = "Stock ezin da negatiboa izan" });

				if (body.TryGetProperty("osagaiak", out JsonElement osagaiakArray))
				{
					// 5.1. Ezabatu erlazio zaharrak
					var deleteQuery = session.CreateSQLQuery(
						"DELETE FROM Platerak_Osagaiak WHERE platerak_id = :platerakId");
					deleteQuery.SetParameter("platerakId", id);
					int deleted = deleteQuery.ExecuteUpdate();
					Console.WriteLine($"{deleted} erlazio zahar ezabatuak");

					// 5.2. Gehitu erlazio berriak
					int added = 0;
					foreach (var osagaiJson in osagaiakArray.EnumerateArray())
					{
						int osagaiId = osagaiJson.GetProperty("id").GetInt32();
						int kopurua = osagaiJson.GetProperty("kopurua").GetInt32();

						var insertQuery = session.CreateSQLQuery(
							"INSERT INTO Platerak_Osagaiak (platerak_id, osagaiak_id, kopurua) " +
							"VALUES (:platerakId, :osagaiakId, :kopurua)");
						insertQuery.SetParameter("platerakId", id);
						insertQuery.SetParameter("osagaiakId", osagaiId);
						insertQuery.SetParameter("kopurua", kopurua);
						insertQuery.ExecuteUpdate();
						added++;
					}
					Console.WriteLine($"{added} osagai berri gehitu dira");
				}

				// 6. Gorde platerra
				session.Update(existing);
				transaction.Commit();

				return Ok(new
				{
					success = true,
					message = "Platerra eta osagaiak eguneratu dira",
					platerraId = id,
					platerraIzena = existing.Izena
				});
			}
			catch (Exception ex)
			{
				transaction.Rollback();
				Console.WriteLine($"Errorea: {ex.Message}");
				return StatusCode(500, new
				{
					error = "Errorea",
					message = ex.Message,
					stackTrace = ex.StackTrace
				});
			}
		}

		// DELETE: api/platerak/{id}
		[HttpDelete("{id}")]
		public ActionResult Delete(int id)
		{
			using var session = _sessionFactory.OpenSession();
			using var transaction = session.BeginTransaction();

			try
			{
				var platera = session.Query<Platerak>()
					.FirstOrDefault(p => p.Id == id);

				if (platera == null)
				{
					return NotFound(new
					{
						error = "Platerra ez da existitzen",
						id = id
					});
				}

				var komandakCount = 0;
				try
				{
					var komandakQuery = "SELECT COUNT(*) FROM Komandak WHERE platerak_id = :id";
					var query = session.CreateSQLQuery(komandakQuery);
					query.SetParameter("id", id);
					komandakCount = Convert.ToInt32(query.UniqueResult());
				}
				catch (Exception sqlEx)
				{
					Console.WriteLine($"Error al contar komandak: {sqlEx.Message}");
				}

				if (komandakCount > 0)
				{
					return BadRequest(new
					{
						error = "Ezin da platerra ezabatu",
						mezua = $"Platerra {komandakCount} komanda/fakturetan agertzen da",
						aukerak = new[]
						{
					"Ezabatu komanda horiek lehenik (/api/platerak/{id}/komandak/all)",
					"Erabili desaktibatu (PATCH /api/platerak/{id}/desaktibatu)",
					"Aldatu stock-a 0-ra (PATCH /api/platerak/{id}/stock)"
				},
						komandaKopurua = komandakCount,
						platerraId = id,
						platerraIzena = platera.Izena
					});
				}

				var relacionesEliminadas = 0;
				try
				{
					var deleteQuery = session.CreateSQLQuery(
						"DELETE FROM Platerak_Osagaiak WHERE platerak_id = :id");
					deleteQuery.SetParameter("id", id);
					relacionesEliminadas = deleteQuery.ExecuteUpdate();
				}
				catch (Exception relEx)
				{
					try
					{
						var hqlDelete = session.CreateQuery(
							"DELETE FROM PlaterakOsagaiak po WHERE po.Platerak.Id = :id");
						hqlDelete.SetParameter("id", id);
						relacionesEliminadas = hqlDelete.ExecuteUpdate();
					}
					catch
					{
						relacionesEliminadas = 0;
					}
				}

				session.Delete(platera);

				transaction.Commit();

				return Ok(new
				{
					mezua = "Platerra ondo ezabatu da",
					platerraId = id,
					platerraIzena = platera.Izena,
					erlazioakEliminadas = relacionesEliminadas,
					data = DateTime.Now
				});
			}
			catch (Exception ex)
			{
				try
				{
					transaction.Rollback();
				}
				catch (Exception rollbackEx)
				{
					Console.WriteLine($"Error: {rollbackEx.Message}");
				}

				return StatusCode(500, new
				{
					error = "Errorea platerra ezabatzerakoan",
					mezua = ex.Message,
					innerError = ex.InnerException?.Message,
					tipoa = ex.GetType().Name,
					platerraId = id,
					gomendioa = "Egiaztatu datu-baseko erlazioak eta murrizketak"
				});
			}
		}

		// PATCH: api/platerak/{id}/stock
		[HttpPatch("{id}/stock")]
		public ActionResult UpdateStock(int id, [FromBody] StockEguneratuDto update)
		{
			using var session = _sessionFactory.OpenSession();
			using var transaction = session.BeginTransaction();

			try
			{
				var platera = session.Query<Platerak>()
					.FirstOrDefault(p => p.Id == id);

				if (platera == null)
					return NotFound();

				platera.Stock += update.Kopurua;

				if (platera.Stock < 0)
					return BadRequest("Ezin da stock negatiboa izan");

				session.Update(platera);
				transaction.Commit();

				var responseDto = new PlaterakDto
				{
					Id = platera.Id,
					Izena = platera.Izena,
					Prezioa = platera.Prezioa,
					Stock = platera.Stock,
					KategoriaId = platera.Kategoriak?.Id ?? 0,
					KategoriaIzena = platera.Kategoriak?.Izena ?? "Kategoria gabe"
				};

				return Ok(responseDto);
			}
			catch (Exception ex)
			{
				transaction.Rollback();
				return StatusCode(500, $"Errorea: {ex.Message}");
			}
		}

		// PATCH: api/platerak/{id}/desaktibatu
		[HttpPatch("{id}/desaktibatu")]
		public ActionResult DesaktibatuPlaterra(int id, [FromBody] PlateraDesaktibatuDto desaktibatuDto = null)
		{
			using var session = _sessionFactory.OpenSession();
			using var transaction = session.BeginTransaction();

			try
			{
				var platera = session.Query<Platerak>()
					.FirstOrDefault(p => p.Id == id);

				if (platera == null)
					return NotFound();

				// Desaktibatu (stock = 0)
				platera.Stock = 0;

				session.Update(platera);
				transaction.Commit();

				var responseDto = new PlaterakDto
				{
					Id = platera.Id,
					Izena = platera.Izena,
					Prezioa = platera.Prezioa,
					Stock = 0,
					KategoriaId = platera.Kategoriak?.Id ?? 0,
					KategoriaIzena = platera.Kategoriak?.Izena ?? "Kategoria gabe"
				};

				return Ok(new
				{
					mezua = "Platerra desaktibatu da (stock = 0)",
					platerra = responseDto,
					arrazoia = desaktibatuDto?.Arrazoia ?? "Eskariaren bidez desaktibatua"
				});
			}
			catch (Exception ex)
			{
				transaction.Rollback();
				return StatusCode(500, $"Errorea: {ex.Message}");
			}
		}

		// GET: api/platerak/{id}/osagaiak
		[HttpGet("{id}/osagaiak")]
		public ActionResult<IEnumerable<PlaterakOsagaiaDto>> GetOsagaiak(int id)
		{
			using var session = _sessionFactory.OpenSession();

			var platera = session.Query<Platerak>()
				.FirstOrDefault(p => p.Id == id);

			if (platera == null)
				return NotFound();

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

		// GET: api/platerak/search/{term}
		[HttpGet("search/{term}")]
		public ActionResult<IEnumerable<PlaterakDto>> Search(string term)
		{
			using var session = _sessionFactory.OpenSession();

			var platerak = session.Query<Platerak>()
				.Where(p => p.Izena.Contains(term))
				.Fetch(p => p.Kategoriak)
				.Select(p => new PlaterakDto
				{
					Id = p.Id,
					Izena = p.Izena,
					Prezioa = p.Prezioa,
					Stock = p.Stock,
					KategoriaId = p.Kategoriak != null ? p.Kategoriak.Id : 0,
					KategoriaIzena = p.Kategoriak != null ? p.Kategoriak.Izena : "Kategoria gabe"
				})
				.ToList();

			return Ok(platerak);
		}

		// GET: api/platerak/kategoria/{kategoriaId}
		[HttpGet("kategoria/{kategoriaId}")]
		public ActionResult<IEnumerable<PlaterakDto>> GetByKategoria(int kategoriaId)
		{
			using var session = _sessionFactory.OpenSession();

			var platerak = session.Query<Platerak>()
				.Where(p => p.Kategoriak != null && p.Kategoriak.Id == kategoriaId)
				.Fetch(p => p.Kategoriak)
				.Select(p => new PlaterakDto
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

		// GET: api/platerak/stock-gutxi
		[HttpGet("stock-gutxi")]
		public ActionResult<IEnumerable<PlaterakDto>> GetStockGutxi()
		{
			using var session = _sessionFactory.OpenSession();

			var platerak = session.Query<Platerak>()
				.Where(p => p.Stock < 10)
				.Fetch(p => p.Kategoriak)
				.Select(p => new PlaterakDto
				{
					Id = p.Id,
					Izena = p.Izena,
					Prezioa = p.Prezioa,
					Stock = p.Stock,
					KategoriaId = p.Kategoriak != null ? p.Kategoriak.Id : 0,
					KategoriaIzena = p.Kategoriak != null ? p.Kategoriak.Izena : "Kategoria gabe"
				})
				.ToList();

			return Ok(platerak);
		}

		// GET: api/platerak/{id}/kostua
		[HttpGet("{id}/kostua")]
		public ActionResult<double> KalkulatuKostua(int id)
		{
			using var session = _sessionFactory.OpenSession();

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

		// GET: api/platerak/{id}/komandak
		[HttpGet("{id}/komandak")]
		public ActionResult<IEnumerable<KomandaInfoDto>> GetKomandakAsociadas(int id)
		{
			using var session = _sessionFactory.OpenSession();

			var query = @"
                SELECT 
                    k.id as Id,
                    k.fakturak_id as FakturaId,
                    k.kopurua as Kopurua,
                    k.totala as Totala,
                    k.egoera as Egoera,
                    k.oharrak as Oharrak,
                    f.totala as FakturaTotala,
                    f.egoera as FakturaEgoera,
                    e.izena as BezeroIzena,
                    e.telefonoa as BezeroTelefonoa
                FROM Komandak k
                INNER JOIN Fakturak f ON k.fakturak_id = f.id
                INNER JOIN Erreserbak e ON f.erreserbak_id = e.id
                WHERE k.platerak_id = :platerakId
                ORDER BY k.id DESC";

			var komandak = session.CreateSQLQuery(query)
				.SetParameter("platerakId", id)
				.SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean<KomandaInfoDto>())
				.List<KomandaInfoDto>();

			return Ok(komandak);
		}

		// GET: api/platerak/{id}/komandak/count
		[HttpGet("{id}/komandak/count")]
		public ActionResult<int> GetKomandakCount(int id)
		{
			using var session = _sessionFactory.OpenSession();

			var query = "SELECT COUNT(*) FROM Komandak WHERE platerak_id = :id";
			var count = session.CreateSQLQuery(query)
				.SetParameter("id", id)
				.UniqueResult<int>();

			return Ok(count);
		}

		// DELETE: api/platerak/{id}/erlazioak-garbitu
		[HttpDelete("{id}/erlazioak-garbitu")]
		public ActionResult ErlazioakGarbitu(int id)
		{
			using var session = _sessionFactory.OpenSession();
			using var transaction = session.BeginTransaction();

			try
			{
				var platera = session.Query<Platerak>()
					.FirstOrDefault(p => p.Id == id);

				if (platera == null)
					return NotFound(new { error = "Platerra ez da existitzen", id = id });

				var deleteRelaciones = session.CreateSQLQuery(
					"DELETE FROM Platerak_Osagaiak WHERE platerak_id = :id")
					.SetParameter("id", id)
					.ExecuteUpdate();

				transaction.Commit();

				return Ok(new
				{
					mezua = "Erlazioak ondo ezabatu dira",
					platerraId = id,
					platerraIzena = platera.Izena,
					erlazioakEliminadas = deleteRelaciones
				});
			}
			catch (Exception ex)
			{
				transaction.Rollback();
				return StatusCode(500, new
				{
					error = "Errorea erlazioak ezabatzerakoan",
					mezua = ex.Message,
					platerraId = id,
					xehetasunak = ex.InnerException?.Message
				});
			}
		}

		// DELETE: api/platerak/{id}/komandak/all
		[HttpDelete("{id}/komandak/all")]
		public ActionResult EliminarKomandak(int id)
		{
			using var session = _sessionFactory.OpenSession();
			using var transaction = session.BeginTransaction();

			try
			{
				var platera = session.Query<Platerak>()
					.FirstOrDefault(p => p.Id == id);

				if (platera == null)
					return NotFound(new { error = "Platerra ez da existitzen", id = id });

				var countQuery = "SELECT COUNT(*) FROM Komandak WHERE platerak_id = :id";
				var komandakCount = session.CreateSQLQuery(countQuery)
					.SetParameter("id", id)
					.UniqueResult<int>();

				if (komandakCount == 0)
					return Ok(new { mezua = "Ez dago komandarik", platerraId = id, count = 0 });

				var deleteQuery = "DELETE FROM Komandak WHERE platerak_id = :id";
				var eliminadas = session.CreateSQLQuery(deleteQuery)
					.SetParameter("id", id)
					.ExecuteUpdate();

				transaction.Commit();

				return Ok(new
				{
					mezua = $"{eliminadas} komanda ezabatu dira",
					platerraId = id,
					platerraIzena = platera.Izena,
					komandakEliminadas = eliminadas,
					komandakOriginales = komandakCount
				});
			}
			catch (Exception ex)
			{
				transaction.Rollback();
				return StatusCode(500, new
				{
					error = "Errorea komandak ezabatzerakoan",
					mezua = ex.Message,
					platerraId = id,
					xehetasunak = ex.InnerException?.Message
				});
			}
		}

		// GET: api/platerak/aktiboak
		[HttpGet("aktiboak")]
		public ActionResult<IEnumerable<PlaterakDto>> GetAktiboak()
		{
			using var session = _sessionFactory.OpenSession();

			var platerak = session.Query<Platerak>()
				.Where(p => p.Stock > 0)
				.Fetch(p => p.Kategoriak)
				.Select(p => new PlaterakDto
				{
					Id = p.Id,
					Izena = p.Izena,
					Prezioa = p.Prezioa,
					Stock = p.Stock,
					KategoriaId = p.Kategoriak != null ? p.Kategoriak.Id : 0,
					KategoriaIzena = p.Kategoriak != null ? p.Kategoriak.Izena : "Kategoria gabe"
				})
				.ToList();

			return Ok(platerak);
		}

		// GET: api/platerak/estadistikak
		[HttpGet("estadistikak")]
		public ActionResult<object> GetEstadistikak()
		{
			using var session = _sessionFactory.OpenSession();

			var estadistikak = new
			{
				TotalPlaterak = session.Query<Platerak>().Count(),
				PlaterakStockGutxi = session.Query<Platerak>().Count(p => p.Stock < 10),
				PlaterakAktiboak = session.Query<Platerak>().Count(p => p.Stock > 0),
				StockTotal = session.Query<Platerak>().Sum(p => p.Stock),
				PrezioBatezBestekoa = session.Query<Platerak>().Average(p => p.Prezioa),
				PrezioMaximoa = session.Query<Platerak>().Max(p => p.Prezioa),
				PrezioMinimoa = session.Query<Platerak>().Min(p => p.Prezioa)
			};

			return Ok(estadistikak);
		}

		// GET: api/platerak/estadistikak/dashboard
		[HttpGet("estadistikak/dashboard")]
		public ActionResult<object> GetDashboardEstadistikak()
		{
			using var session = _sessionFactory.OpenSession();

			var platerak = session.Query<Platerak>().ToList();

			var estadistikak = new
			{
				// Oinarrizko estatistikak
				PlaterakTotalak = platerak.Count,
				PlaterakStockGutxi = platerak.Count(p => p.Stock < 10),
				PlaterakAktiboak = platerak.Count(p => p.Stock > 0),
				StockTotala = platerak.Sum(p => p.Stock),
				PrezioBatezBestekoa = platerak.Average(p => p.Prezioa),

				// Kategoriaka
				Kategoriaka = platerak
					.Where(p => p.Kategoriak != null)
					.GroupBy(p => new { p.Kategoriak.Id, p.Kategoriak.Izena })
					.Select(g => new
					{
						KategoriaId = g.Key.Id,
						KategoriaIzena = g.Key.Izena,
						Kopurua = g.Count(),
						StockTotala = g.Sum(p => p.Stock),
						PrezioBatezBestekoa = g.Average(p => p.Prezioa)
					})
					.ToList(),

				// Top 5 stock handienarekin
				TopStock = platerak
					.OrderByDescending(p => p.Stock)
					.Take(5)
					.Select(p => new
					{
						p.Id,
						p.Izena,
						p.Stock,
						p.Prezioa,
						KategoriaIzena = p.Kategoriak?.Izena ?? "Kategoria gabe"
					})
					.ToList(),

				// Stock gabe dauden platerak
				StockGabe = platerak
					.Where(p => p.Stock == 0)
					.Select(p => new
					{
						p.Id,
						p.Izena,
						KategoriaIzena = p.Kategoriak?.Izena ?? "Kategoria gabe"
					})
					.ToList()
			};

			return Ok(estadistikak);
		}
	}
}