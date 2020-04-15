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
    public class WalkersController : Controller
    {
        private readonly IConfiguration _config;
        public WalkersController(IConfiguration config)
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


        // GET: Walkers
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT wr.Id, wr.[Name], wr.NeighborhoodId, n.[Name] as 'NeighborhoodName'
                                        FROM Walker wr
                                        LEFT JOIN Neighborhood n 
                                        ON n.Id = wr.NeighborhoodId ";

                    var reader = cmd.ExecuteReader();
                    var walkers = new List<WalkerViewModel>();

                    while (reader.Read())
                    {
                        walkers.Add(new WalkerViewModel()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))
                        });

                        if (!reader.IsDBNull(reader.GetInt32(reader.GetOrdinal("NeighborhoodId")))
                        {
                            walkers.NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"));
                        }
                        else
                        {
                            walkers.NeighborhoodId = null;
                        }
                    }

                    reader.Close();
                    return View(walkers);
                }
            }
        }

        // GET: Walkers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Walkers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Walker walker)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Walker (Name)
                                            OUTPUT INSERTED.Id
                                            VALUES (@name)";

                        cmd.Parameters.Add(new SqlParameter("@name", walker.Name));

                        var id = (int)cmd.ExecuteScalar();
                        walker.Id = id;
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        // GET: Walkers/Edit/5
        public ActionResult Edit(int id)
        {
            var walker = GetWalkerById(id);
            return View(walker);
        }

        // POST: Walkers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Walker walker)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Walker
                                           SET Name = @name
                                               WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@name", walker.Name));

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

        // GET: Walkers/Delete/5
        public ActionResult Delete(int id)
        {
            var walker = GetWalkerById(id);
            return View(walker);
        }

        // POST: Walkers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Walker walker)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Walker WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        // TODO: Add delete logic here
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch
            {
                return View(walker);
            }
        }

        private Walker GetWalkerById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT wr.Id, wr.[Name], NeighborhoodId, FROM Walker wr WHERE wr.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();
                    Walker walker = null;

                    if (reader.Read())
                    {
                        walker = new Walker()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))
                        };

                    }
                    reader.Close();
                    return walker;
                }
            }
        }
    }
}