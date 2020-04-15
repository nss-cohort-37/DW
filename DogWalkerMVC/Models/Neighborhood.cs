﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalkerMVC.Models
{
    public class Neighborhood
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Walker> Walker { get; set; }
    }
}
