# Simulación Monte-Carlo de Epidemias — Modelo SIR

Simulación de una epidemia sobre una grilla de 1000×1000 personas (1,000,000 de individuos) usando el modelo SIR (Susceptible–Infectado–Recuperado), en dos versiones: **secuencial** y **paralela** (con ghost-cells), corridas durante 365 días.

**Autor:** Eithan — Matrícula 2024-1869 — ITLA

## Estructura
MonteCarloSIR/
├── Simulador/
│ ├── SequentialSim/ -> Motor secuencial
│ ├── ParallelSim/ -> Motor paralelo + benchmark de scaling
│ ├── EstadoPersona.cs, Grilla.cs, ParametrosSimulacion.cs,
│ │ EstadisticasDiarias.cs, RandomDeterminista.cs (compartidos)
│ └── Program.cs
├── Visualization/ -> Scripts Python (gráfica + animación)
├── Resultados/ -> CSV, gráfica de speed-up, animación
└── informe.md


## Requisitos

- .NET 8.0 SDK
- Python 3.9+ con `numpy`, `pandas`, `matplotlib`

## Cómo correrlo

1. Ejecutar con F5 en Visual Studio.
2. Menú: `2`/`4` validan el modelo (secuencial/paralelo, caso pequeño) · `1`/`3` corren la simulación completa · `5` corre el benchmark de scaling.
3. Desde `Visualization/`:
```bash
   python validar_identicos.py
   python plot_scaling.py
   python animate.py
```

## Modelo

Cada persona está en uno de tres estados: **susceptible**, **infectada** o **recuperada** (estado final). Una persona susceptible se contagia según una probabilidad β por cada vecino infectado (vecindad de Von Neumann: N, S, E, O). Una infectada se resuelve (recupera o muere) con probabilidad γ + μ por día. La grilla es **toroidal**: los bordes se conectan, así todas las personas tienen siempre 4 vecinos.

| Parámetro | Valor |
|---|---|
| β (contagio) | 0.15 |
| γ (recuperación) | 0.07 |
| μ (muerte) | 0.01 |
| R0 teórico | ≈1.88 |

## Paralelización

La grilla se divide en franjas de filas, una por hilo (`Parallel.For`). Cada hilo copia las filas frontera de sus vecinos (**ghost-cells**) antes de procesar su bloque. El generador de números aleatorios es **determinístico por celda** (depende de posición y día, no del orden de ejecución), lo que garantiza que secuencial y paralelo den el mismo resultado. Las estadísticas se combinan con `Parallel.For<TLocal>` (map-reduce).

## Resultados

Validación (20×20, 30 días): secuencial y paralelo dieron resultados **idénticos**.

Simulación completa (1000×1000, 365 días, máquina de 16 núcleos):

| Hilos | Tiempo (ms) | Speed-up |
|---|---|---|
| 1 | 15,136 | 1.00× |
| 2 | 6,355 | 2.38× |
| 4 | 3,420 | 4.43× |
| 8 | 2,555 | 5.93× |

![Gráfica de speed-up](Resultados/speedup_graph.png)

![Animación secuencial vs. paralelo](Resultados/sir_comparison.gif)

Con 2 y 4 hilos el speed-up superó al ideal (superlineal), por mejor uso de caché al trabajar con bloques más pequeños. Con 8 hilos la eficiencia baja a 74%, consistente con la ley de Amdahl y el uso de núcleos lógicos (hyperthreading).

