using System;
using System.Collections.Generic;

namespace natom.varadero.entities
{
    public class Region
    {
        public int RegionId { get; set; }
        public string Descripcion { get; set; }
        public DateTime? DeletedAt { get; set; }

        public List<RegionMontoMinimo> MontosMinimos { get; set; }
    }
}