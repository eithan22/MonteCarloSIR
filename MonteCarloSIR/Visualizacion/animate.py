"""
Lee los frame_XXX.bin generados por C# y arma un video con Secuencial y
Paralelo lado a lado, para comprobar visualmente que ambos cuentan la
misma historia de la epidemia.
Cada .bin tiene TamGrilla*TamGrilla bytes: 0=Susceptible, 1=Infectado, 2=Recuperado
"""
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.animation as animation
import glob
import os
import re

CARPETA_SECUENCIAL = "../Data/Secuencial"
CARPETA_PARALELO = "../Data/Paralelo"
RUTA_SALIDA = "../Data/sir_comparison.mp4"

COLORES = np.array([
    [255, 255, 255],
    [220, 38, 38],
    [22, 163, 74],
], dtype=np.uint8)


def listar_frames(carpeta):
    archivos = sorted(glob.glob(os.path.join(carpeta, "frame_*.bin")))
    dias = [int(re.search(r"frame_(\d+)\.bin", a).group(1)) for a in archivos]
    return archivos, dias


def cargar_frame(ruta, tam_grilla):
    datos = np.fromfile(ruta, dtype=np.uint8)
    datos = datos.reshape((tam_grilla, tam_grilla))
    return COLORES[datos]


def generar_animacion(tam_grilla, carpeta_sec=CARPETA_SECUENCIAL,
                        carpeta_par=CARPETA_PARALELO, ruta_salida=RUTA_SALIDA, fps=6):
    archivos_sec, dias_sec = listar_frames(carpeta_sec)
    archivos_par, dias_par = listar_frames(carpeta_par)

    n_frames = min(len(archivos_sec), len(archivos_par))
    if n_frames == 0:
        raise RuntimeError(
            "No hay frames .bin. Corre primero las opciones 1 y 3 del menu de C# "
            "antes de generar la animacion."
        )

    fig, (ax1, ax2) = plt.subplots(1, 2, figsize=(10, 5))
    fig.suptitle("Propagacion de la epidemia: Secuencial vs. Paralelo", fontsize=14)

    img1 = ax1.imshow(cargar_frame(archivos_sec[0], tam_grilla))
    ax1.set_title(f"Secuencial - Dia {dias_sec[0]}")
    ax1.axis("off")

    img2 = ax2.imshow(cargar_frame(archivos_par[0], tam_grilla))
    ax2.set_title(f"Paralelo - Dia {dias_par[0]}")
    ax2.axis("off")

    def actualizar(i):
        img1.set_data(cargar_frame(archivos_sec[i], tam_grilla))
        ax1.set_title(f"Secuencial - Dia {dias_sec[i]}")
        img2.set_data(cargar_frame(archivos_par[i], tam_grilla))
        ax2.set_title(f"Paralelo - Dia {dias_par[i]}")
        return img1, img2

    anim = animation.FuncAnimation(fig, actualizar, frames=n_frames, interval=1000 // fps)

    try:
        anim.save(ruta_salida, writer="ffmpeg", fps=fps, dpi=120)
        print(f"Animacion guardada en {ruta_salida}")
    except Exception as e:
        ruta_gif = ruta_salida.replace(".mp4", ".gif")
        print(f"No se pudo usar ffmpeg ({e}). Guardando como GIF...")
        anim.save(ruta_gif, writer="pillow", fps=fps)
        print(f"Animacion guardada en {ruta_gif}")

    plt.close(fig)


if __name__ == "__main__":
    generar_animacion(tam_grilla=1000)