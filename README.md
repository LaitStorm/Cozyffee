# Hito 2: Prototipo Lo-Fi🚧

📝 **Desarrolladoras:** Andry Guevara y Josefina Ulloa E.

📝 **Correos:** aguevara19@uc.cl, joseulloae@uc.cl

---

## Código 🔣

### 👨‍🏫 Stack tecnológico

**Plataforma objetivo:** Standalone VR

**Motor gráfico:** Unity 6000.4.0f1

**Lenguaje:** C# (.NET SDK)

**Librerías/Packages utilizados:** 
- XR Interaction Toolkit 3.6.1, para gestión de interacciones VR
- AI Navigation

**Plugins:**
- XR Devise Simulator, para probar experiencia VR en PC sin casco

**Hardware Objetivo:** Meta Quest 3

**Justificación tecnológica:** Se eligió Unity con el XR Interaction Toolkit por su arquitectura basada en componentes que volvió fácil el desarrollo 

## 💻 Cómo ejecutar proyecto

### Requisitos previos
- Unity Hub y versión de Unity 6000.4.0f1 instalada
- Android Build Support
- SDK de .NET instalado

### Pasos para clonar 
- Ejecutar el comando ``git clone https://github.com/LaitStorm/Cozyffee.git``
- Abrir la carpeta del proyecto en Unity Hub
- Buscar la escena principal ``Assets/Scenes/Cozyffee``

### Ejecución e interacción
- Modo Simulador en PC
    - Teclas para mover la cámara, el jugaodr y mandos se indican en pantalla
    - Para agarrar objetos se apreta la tecla G en el teclado
- Modo Visor VR
    - Uso de palanca en mando izquierdo para mover 
    - Uso de botones laterales de ambos controles para tomar objetos

## Estado actual del proyecto

- Mecanismo de reaparición de instancias de comida tras x segundos de agarrar una instancia
- NCPs se mueven por el ambiente y hablan con texto que aparece sobre ellos

## Referencias de elementos externos
- [How to make a VR Game in Unity 6 Under 60 minutes - Beginner tutorial](https://youtu.be/ofjPCrh0ZIk?si=RWW3guam17HZ_mrY)
- [Playlist de video tutoriales para navegación de NPCs](https://www.youtube.com/playlist?list=PLMNkk-YfpOgOkNiCuFr3TU1QVwsxxJeI9)