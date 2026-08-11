using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarloSIR.Simulador
{
    // Los 3 estados posibles de cada persona en el modelo SIR.
    public enum EstadoPersona : byte
    {
        Susceptible = 0,
        Infectado = 1,
        Recuperado = 2
    }
}
