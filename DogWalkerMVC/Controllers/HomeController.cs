using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DogWalkerMVC.Models;
using DogWalkerMVC.Models.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace DogWalkerMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;

        public HomeController(IConfiguration config, ILogger<HomeController> logger)
        {
            _config = config;
            _logger = logger;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        public IActionResult Index()
        {
            var viewModel = new HomepageViewModel();
            var dogsNotWalkedToday = GetDogsNotWalkedToday();
            var topWalkers = GetTopWalkers();

            var totalWalkTimeInMinutes = topWalkers.Sum(w => w.TotalDuration);
            var totalWalkTimeSpan = TimeSpan.FromMinutes(totalWalkTimeInMinutes);
            var totalWalkDisplay = $"{totalWalkTimeSpan.Hours} Hours {totalWalkTimeSpan.Minutes} Minutes";

            viewModel.Dogs = dogsNotWalkedToday;
            viewModel.Today = DateTime.Now;
            viewModel.TopWalkers = topWalkers;
            viewModel.TotalWalkTime = totalWalkDisplay;

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private List<Dog> GetDogsNotWalkedToday()
        {
            using(SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT d.Id, d.[Name], d.Breed, d.Notes, d.OwnerId 
                                        FROM Dog d
                                        LEFT JOIN Walks w
                                        ON d.Id = w.DogId
                                        WHERE w.Date IS NULL 
                                        OR w.[Date] < DATEADD(DAY, -1, GETDATE())";

                    var reader = cmd.ExecuteReader();

                    var dogs = new List<Dog>();

                    while (reader.Read())
                    {
                        dogs.Add(new Dog()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed"))
                        });
                    }
                    reader.Close();
                    return dogs;
                }
            }
        }
    
        private List<TopWalkerViewModel> GetTopWalkers()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT TOP 3 wr.Id, wr.[Name], SUM(wk.Duration) as TotalDuration
                                        FROM Walker wr
                                        LEFT JOIN Walks wk
                                        ON wr.Id = wk.WalkerId
                                        GROUP BY wr.Id, wr.[Name]
                                        ORDER BY TotalDuration DESC";

                    var reader = cmd.ExecuteReader();

                    var walkers = new List<TopWalkerViewModel>();

                    while (reader.Read())
                    {
                        walkers.Add(new TopWalkerViewModel()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            TotalDuration = reader.GetInt32(reader.GetOrdinal("TotalDuration"))
                        });
                    }
                    reader.Close();
                    return walkers;
                }
            }
        }
    }
}
