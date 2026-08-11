using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarloSIR.Simulador
{
    public class EstadisticasDiarias
    {
        public int Dia { get; set; }
        public int Susceptibles { get; set; }
        public int Infectados { get; set; }
        public int Recuperados { get; set; }
        public int NuevosContagiosHoy { get; set; }
        public double R0Efectivo { get; set; }
        public int Total => Susceptibles + Infectados + Recuperados;
    }
}
