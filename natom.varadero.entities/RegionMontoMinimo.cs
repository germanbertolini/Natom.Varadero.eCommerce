using System;

namespace natom.varadero.entities
{
    public class RegionMontoMinimo
    {
        public int RegionMontoMinimoId { get; set; }

        public int RegionId { get; set; }
        public Region Region { get; set; }

        public int DiaDeLaSemana { get; set; }
        public decimal MontoMinimo { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}