using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarloSIR.Simulador.SequentialSim
{
    // Motor de calculo secuencial: aplica las reglas SIR dia por dia, un solo hilo.
    public class MotorSimulacion
    {
        private readonly ParametrosSimulacion _parametros;

        public MotorSimulacion(ParametrosSimulacion parametros)
        {
            _parametros = parametros;
        }

        // Lee de 'actual', escribe en 'siguiente' (doble buffer). Nunca modifica
        // 'actual' mientras la esta leyendo, para que el orden de recorrido no afecte el resultado.
        public EstadisticasDiarias SimularDia(Grilla actual, Grilla siguiente, int dia, int infectadosAyer)
        {
            int susceptibles = 0, infectados = 0, recuperados = 0, nuevosContagios = 0;

            for (int f = 0; f < actual.Tamano; f++)
            {
                for (int c = 0; c < actual.Tamano; c++)
                {
                    EstadoPersona estadoActual = actual.Obtener(f, c);
                    EstadoPersona nuevoEstado = estadoActual;

                    if (estadoActual == EstadoPersona.Susceptible)
                    {
                        int vecinosInfectados = actual.ContarVecinosInfectados(f, c);

                        for (int intento = 0; intento < vecinosInfectados; intento++)
                        {
                            double dado = RandomDeterminista.Siguiente(_parametros.Semilla, dia, f, c, intento);
                            if (dado < _parametros.Beta)
                            {
                                nuevoEstado = EstadoPersona.Infectado;
                                nuevosContagios++;
                                break;
                            }
                        }
                    }
                    else if (estadoActual == EstadoPersona.Infectado)
                    {
                        double dado = RandomDeterminista.Siguiente(_parametros.Semilla, dia, f, c, 100);
                        if (dado < (_parametros.Gamma + _parametros.Mu))
                        {
                            nuevoEstado = EstadoPersona.Recuperado;
                        }
                    }

                    siguiente.Establecer(f, c, nuevoEstado);

                    switch (nuevoEstado)
                    {
                        case EstadoPersona.Susceptible: susceptibles++; break;
                        case EstadoPersona.Infectado: infectados++; break;
                        case EstadoPersona.Recuperado: recuperados++; break;
                    }
                }
            }

            return new EstadisticasDiarias
            {
                Dia = dia,
                Susceptibles = susceptibles,
                Infectados = infectados,
                Recuperados = recuperados,
                NuevosContagiosHoy = nuevosContagios,
                R0Efectivo = infectadosAyer > 0 ? (double)nuevosContagios / infectadosAyer : 0.0
            };
        }
    }

}
