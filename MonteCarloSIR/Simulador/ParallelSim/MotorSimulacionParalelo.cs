using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarloSIR.Simulador.ParallelSim
{
    // Version paralela: divide la grilla en franjas de filas (una por hilo).
    // Cada hilo copia las filas frontera de sus vecinos (ghost-cells) antes
    // de procesar, y al final se reducen (suman) las estadisticas de todos los hilos.
    public class MotorSimulacionParalelo
    {
        private readonly ParametrosSimulacion _parametros;
        private readonly int _gradoParalelismo;

        public MotorSimulacionParalelo(ParametrosSimulacion parametros, int gradoParalelismo)
        {
            _parametros = parametros;
            _gradoParalelismo = gradoParalelismo;
        }

        // "Canasta" de conteo privada de cada hilo.
        private class EstadisticasLocales
        {
            public int Susceptibles;
            public int Infectados;
            public int Recuperados;
            public int NuevosContagios;
        }

        public EstadisticasDiarias SimularDia(Grilla actual, Grilla siguiente, int dia, int infectadosAyer)
        {
            int n = actual.Tamano;
            int numBloques = _gradoParalelismo;
            int filasPorBloque = (n + numBloques - 1) / numBloques;

            int totalS = 0, totalI = 0, totalR = 0, totalNuevos = 0;
            object candado = new object();

            var opciones = new ParallelOptions { MaxDegreeOfParallelism = _gradoParalelismo };

            Parallel.For<EstadisticasLocales>(
                0, numBloques, opciones,
                () => new EstadisticasLocales(),
                (b, estadoBucle, local) =>
                {
                    int filaInicio = b * filasPorBloque;
                    int filaFin = Math.Min(filaInicio + filasPorBloque, n);
                    if (filaInicio >= filaFin) return local;

                    // === GHOST CELLS ===
                    // Copiamos las filas justo arriba y abajo de nuestro bloque,
                    // que pertenecen a bloques procesados por otros hilos.
                    int filaGhostNorte = (filaInicio - 1 + n) % n;
                    int filaGhostSur = filaFin % n;
                    EstadoPersona[] ghostNorte = actual.ObtenerFila(filaGhostNorte);
                    EstadoPersona[] ghostSur = actual.ObtenerFila(filaGhostSur);

                    for (int f = filaInicio; f < filaFin; f++)
                    {
                        for (int c = 0; c < n; c++)
                        {
                            EstadoPersona estadoActual = actual.Obtener(f, c);
                            EstadoPersona nuevoEstado = estadoActual;

                            if (estadoActual == EstadoPersona.Susceptible)
                            {
                                int vecinosInfectados = ContarVecinosConGhost(
                                    actual, f, c, filaInicio, filaFin, ghostNorte, ghostSur, n);

                                for (int intento = 0; intento < vecinosInfectados; intento++)
                                {
                                    double dado = RandomDeterminista.Siguiente(_parametros.Semilla, dia, f, c, intento);
                                    if (dado < _parametros.Beta)
                                    {
                                        nuevoEstado = EstadoPersona.Infectado;
                                        local.NuevosContagios++;
                                        break;
                                    }
                                }
                            }
                            else if (estadoActual == EstadoPersona.Infectado)
                            {
                                double dado = RandomDeterminista.Siguiente(_parametros.Semilla, dia, f, c, 100);
                                if (dado < (_parametros.Gamma + _parametros.Mu))
                                    nuevoEstado = EstadoPersona.Recuperado;
                            }

                            siguiente.Establecer(f, c, nuevoEstado);

                            switch (nuevoEstado)
                            {
                                case EstadoPersona.Susceptible: local.Susceptibles++; break;
                                case EstadoPersona.Infectado: local.Infectados++; break;
                                case EstadoPersona.Recuperado: local.Recuperados++; break;
                            }
                        }
                    }

                    return local;
                },
                local =>
                {
                    lock (candado)
                    {
                        totalS += local.Susceptibles;
                        totalI += local.Infectados;
                        totalR += local.Recuperados;
                        totalNuevos += local.NuevosContagios;
                    }
                });

            return new EstadisticasDiarias
            {
                Dia = dia,
                Susceptibles = totalS,
                Infectados = totalI,
                Recuperados = totalR,
                NuevosContagiosHoy = totalNuevos,
                R0Efectivo = infectadosAyer > 0 ? (double)totalNuevos / infectadosAyer : 0.0
            };
        }

        private static int ContarVecinosConGhost(
            Grilla actual, int f, int c, int filaInicio, int filaFin,
            EstadoPersona[] ghostNorte, EstadoPersona[] ghostSur, int n)
        {
            int contador = 0;
            int oeste = (c - 1 + n) % n;
            int este = (c + 1) % n;

            EstadoPersona norte = (f == filaInicio) ? ghostNorte[c] : actual.Obtener(f - 1, c);
            EstadoPersona sur = (f == filaFin - 1) ? ghostSur[c] : actual.Obtener(f + 1, c);

            if (norte == EstadoPersona.Infectado) contador++;
            if (sur == EstadoPersona.Infectado) contador++;
            if (actual.Obtener(f, oeste) == EstadoPersona.Infectado) contador++;
            if (actual.Obtener(f, este) == EstadoPersona.Infectado) contador++;

            return contador;
        }
    }
}
