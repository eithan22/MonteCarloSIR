using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarloSIR.Simulador
{
    public class ParametrosSimulacion
    {
        public int TamGrilla { get; }
        public int DiasSimulacion { get; }
        public double Beta { get; }
        public double Gamma { get; }
        public double Mu { get; }
        public int InfectadosIniciales { get; }
        public int Semilla { get; }

        public ParametrosSimulacion(
            int tamGrilla = 1000,
            int diasSimulacion = 365,
            double beta = 0.15,
            double gamma = 0.07,
            double mu = 0.01,
            int infectadosIniciales = 20,
            int semilla = 42)
        {
            TamGrilla = tamGrilla;
            DiasSimulacion = diasSimulacion;
            Beta = beta;
            Gamma = gamma;
            Mu = mu;
            InfectadosIniciales = infectadosIniciales;
            Semilla = semilla;
        }

        public double R0Teorico => Beta / (Gamma + Mu);
    }
}
