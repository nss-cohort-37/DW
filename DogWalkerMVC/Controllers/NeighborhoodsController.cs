using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DogWalkerMVC.Models;
using DogWalkerMVC.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DogWalkerMVC.Controllers
{
    public class NeighborhoodsController : Controller
    {
        private readonly IConfiguration _config;
        public NeighborhoodsController(IConfiguration config)
        {
            _config = config;
        }
        //COMPUTED PROPERTY FOR THE CONNECTION
        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }


        // GET: Neighborhoods
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT n.Id, n.[Name], COUNT(w.NeighborhoodId) as 'Walkers'
                                        FROM Neighborhood n
                                        LEFT JOIN Walker w
                                        ON	w.NeighborhoodId = n.Id
                                        GROUP BY n.Name, n.Id ";

                    var reader = cmd.ExecuteReader();
                    var walkers = new List<NeighborhoodViewModel>();

                    while (reader.Read())
                    {
                        walkers.Add(new NeighborhoodViewModel()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Walkers = reader.GetInt32(reader.GetOrdinal("Walkers"))
                        });
                    }
                    reader.Close();
                    return View(walkers);
                }
            }
        }

        // GET: Neighborhoods/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Neighborhoods/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Neighborhood neighborhood)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Neighborhood (Name)
                                            OUTPUT INSERTED.Id
                                            VALUES (@name)";

                        cmd.Parameters.Add(new SqlParameter("@name", neighborhood.Name));

                        var id = (int)cmd.ExecuteScalar();
                        neighborhood.Id = id;
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        // GET: Neighborhoods/Edit/5
        public ActionResult Edit(int id)
        {
            var neighborhood = GetNeighborhoodById(id);
            return View(neighborhood);
        }

        // POST: Neighborhoods/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Neighborhood neighborhood)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Neighborhood
                                           SET Name = @name
                                               WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@name", neighborhood.Name));

                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return RedirectToAction(nameof(Index));
                        }
                        throw new Exception("No rows affected");
                    };
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        // GET: Neighborhoods/Delete/5
        public ActionResult Delete(int id)
        {
            var neighborhood = GetNeighborhoodById(id);
            return View(neighborhood);
        }

        // POST: Neighborhoods/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Neighborhood neighborhood)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Dog WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        // TODO: Add delete logic here
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch
            {
                return View(neighborhood);
            }
        }

        private Neighborhood GetNeighborhoodById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT n.Id, n.Name FROM Neighborhood n WHERE n.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();
                    Neighborhood neighborhood = null;

                    if (reader.Read())
                    {
                        neighborhood = new Neighborhood()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        };

                    }
                    reader.Close();
                    return neighborhood;
                }
            }
        }
    }
}