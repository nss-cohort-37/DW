using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalkerMVC.Models.ViewModels
{
    public class NeighborhoodViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Walkers { get; set; }
    }
}
