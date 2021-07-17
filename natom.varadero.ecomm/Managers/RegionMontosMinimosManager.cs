using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace natom.varadero.ecomm.Managers
{
    public class RegionMontosMinimosManager
    {
        private DbEcommerceContext db = new DbEcommerceContext();

        public int GetMontosMinimosCount()
        {
            return this.db.RegionesMontosMinimos.Where(m => m.DeletedAt == null).Count();
        }

        public IEnumerable<RegionMontoMinimo> GetMontosMinimos()
        {
            return this.db.RegionesMontosMinimos.Include(r => r.Region).Where(m => m.DeletedAt == null);
        }

        public void Grabar(RegionMontoMinimo montoMinimo)
        {
            if (montoMinimo.RegionMontoMinimoId == 0)
            {
                if (db.RegionesMontosMinimos.Any(r => r.DeletedAt == null && r.RegionId == montoMinimo.RegionId && r.DiaDeLaSemana == montoMinimo.DiaDeLaSemana))
                {
                    throw new Exception("Ya existe un monto mínimo para Región y Día de la semana indicados.");
                }

                db.RegionesMontosMinimos.Add(montoMinimo);
            }
            else
            {
                if (db.RegionesMontosMinimos.Any(r => r.DeletedAt == null && r.RegionId == montoMinimo.RegionId && r.DiaDeLaSemana == montoMinimo.DiaDeLaSemana && r.RegionMontoMinimoId != montoMinimo.RegionMontoMinimoId))
                {
                    throw new Exception("Ya existe un monto mínimo para Región y Día de la semana indicados.");
                }

                var montoMinimoDB = db.RegionesMontosMinimos.Find(montoMinimo.RegionMontoMinimoId);
                db.Entry(montoMinimoDB).State = System.Data.Entity.EntityState.Modified;
                montoMinimoDB.RegionId = montoMinimo.RegionId;
                montoMinimoDB.DiaDeLaSemana = montoMinimo.DiaDeLaSemana;
                montoMinimoDB.MontoMinimo = montoMinimo.MontoMinimo;
            }
            db.SaveChanges();
        }

        public RegionMontoMinimo GetMontoMinimo(int id)
        {
            return db.RegionesMontosMinimos.Find(id);
        }

        public void EliminarMontoMinimo(int id)
        {
            var obj = db.RegionesMontosMinimos.Find(id);
            db.Entry(obj).State = EntityState.Modified;
            obj.DeletedAt = DateTime.Now;
            db.SaveChanges();
        }

        public RegionMontoMinimo ObtenerMontoMinimoParaHoy(int regionId)
        {
            int diaDeLaSemanaHoy = (int)DateTime.Now.DayOfWeek;
            return db.RegionesMontosMinimos.FirstOrDefault(r => r.DeletedAt == null
                                                                && r.RegionId == regionId
                                                                && r.DiaDeLaSemana == diaDeLaSemanaHoy);
        }
    }
}