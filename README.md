# AR Diabetes Tipo 1 — App educativa

App de realidad aumentada para enseñar diabetes tipo 1 a niños/adolescentes (5-15 años).
El usuario escanea páginas de un libro físico (QR) y aparecen modelos 3D interactivos con
narración, mini-juegos y seguimiento de progreso.

> Producto: Endify. Este repo es solo la app (Unity); el proceso de trabajo, decisiones y
> capturas de pantalla completas del avance quedan documentadas aparte (ver sección
> [Estado del proyecto](#estado-del-proyecto-2026-07-08)).

---

## Requisitos para abrir el proyecto

| Herramienta | Versión |
|---|---|
| **Unity Editor** | `6000.5.2f1` (Unity 6), instalado vía Unity Hub |
| **Módulo Android Build Support** | con *OpenJDK* y *Android SDK & NDK Tools* marcados |
| Sistema operativo probado | Linux (CachyOS), debería funcionar igual en Windows/Mac |

El proyecto **no trae `Library/`** (se regenera sola al abrir, tarda unos minutos la primera vez
mientras Unity importa todos los assets).

---

## Cómo clonar y abrir

```bash
git clone <url-del-repo> ARDiabetes
```

1. Abrir **Unity Hub** → **Add** → **Add project from disk** → seleccionar la carpeta `ARDiabetes`.
2. Abrir con el editor **6000.5.2f1** (instálalo desde Unity Hub si no lo tienes).
3. Esperar a que Unity importe todos los assets y resuelva los paquetes (`Packages/manifest.json`
   ya lista todo: AR Foundation, ARCore, TextMeshPro, Input System, etc. — se descargan solos).
4. Abrir la escena `Assets/Scenes/Main.unity` si no se abre sola.
5. Dar **Play** y cambiar a la pestaña **Game** para ver la app funcionando.

> **Nota clave:** casi toda la UI se construye **por código en runtime**
> (`Assets/Scripts/AppBootstrap.cs`), no hay prefabs de pantallas para arrastrar. En modo
> edición (sin Play) la escena se ve vacía — es normal. Para inspeccionar una pantalla sin
> ejecutar, usar el menú **`ARDiabetes ▸ Preview ▸ <pantalla>`** (se ve en la pestaña Game).

---

## Cómo compilar y desplegar a un dispositivo Android

### Opción A — Desde el Editor (recomendada para uso normal)
1. **File ▸ Build Profiles** ▸ pestaña **Android** ▸ **Switch Platform** (la primera vez tarda).
2. Conectar el dispositivo por USB con **Depuración USB** activada (Ajustes ▸ Opciones de
   desarrollador). Aceptar el diálogo "¿Permitir depuración USB?" en el dispositivo.
3. Seleccionar el dispositivo en **Run Device** ▸ botón **Build And Run**.

### Opción B — Por línea de comandos (la que se usó durante el desarrollo)
Útil para automatizar / no depender de tener el Editor con GUI abierto.

```bash
UNITY=/ruta/a/Unity/Hub/Editor/6000.5.2f1/Editor/Unity
PROJ=/ruta/a/ARDiabetes

# 1) Aplica ajustes del proyecto (orientación, escena, sprites) — solo si cambiaste algo de setup
$UNITY -batchmode -quit -projectPath "$PROJ" -executeMethod ProjectSetup.Build

# 2) Compila el APK (queda en /tmp/ardiabetes_apk/ARDiabetes.apk)
$UNITY -batchmode -quit -projectPath "$PROJ" -buildTarget Android \
  -executeMethod ProjectSetup.BuildAndroid

# 3) Instala en el dispositivo conectado (reemplaza si ya existe, concede permisos)
adb install -r -g /tmp/ardiabetes_apk/ARDiabetes.apk
adb shell monkey -p com.endify.ardiabetes -c android.intent.category.LAUNCHER 1
```

`adb` está incluido en el editor: `Editor/Data/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb`
(no hace falta instalar el SDK de Android aparte).

### Validar cambios de UI en la PC sin un dispositivo (Linux)
```bash
$UNITY -batchmode -quit -projectPath "$PROJ" -executeMethod ProjectSetup.BuildLinux
# corre el binario resultante con ARCAP=1 para que tome screenshots automáticos de cada pantalla
ARCAP=1 ARCAP_DIR=/tmp/caps /tmp/ardiabetes_linux/ARDiabetes.x86_64 -screen-width 1080 -screen-height 1920
```

---

## Estructura del proyecto

```
ARDiabetes/
├── Assets/
│   ├── Scripts/            # TODA la lógica y las pantallas (ver tabla abajo)
│   ├── Scenes/Main.unity   # única escena (Canvas vacío + AppBootstrap + cámara + EventSystem)
│   ├── Editor/
│   │   └── ProjectSetup.cs # setup reproducible + build por CLI + menú de Preview
│   ├── Audio/Narracion/    # 4 narraciones (una por tema del Libro Fisiológico), voz TTS
│   ├── Imagenes/
│   │   ├── Diseño/         # arte del prototipo original (avatares, iconos, botones)
│   │   ├── UI/             # iconos propios generados (info, audio, rotar, cámara, home…)
│   │   └── QR/             # Pancreas.fbx (modelo 3D real, extraído del prototipo)
│   ├── Markers/            # PNG de los QR (copia para la librería de imágenes AR en runtime)
│   ├── XR/                 # config de ARCore / XR generada por ProjectSetup.SetupAR
│   └── TextMesh Pro/       # paquete de fuentes TMP
├── Marcadores/              # QR IMPRIMIBLES (4 tarjetas + hoja A4) para probar el escaneo AR
├── Packages/manifest.json  # dependencias (AR Foundation 6.5, ARCore 6.5, TMP, Input System…)
├── ProjectSettings/
└── README.md               # este archivo
```

### Scripts (`Assets/Scripts/`)

| Script | Rol |
|---|---|
| `AppBootstrap.cs` | El corazón de la app. Construye **todas** las pantallas por código (Inicio, Bienvenida, Perfil, Menú, Libro Fisiológico: Temas/Detalle/AR). Un método `Build...()` por pantalla. |
| `UIKit.cs` | Mini design-system: paleta de colores, generador de sprites (rounded-rect, círculos, degradados) y fábricas de widgets (botones, cajas, texto, sombras) + helpers de layout responsivo por fracciones (`Frac`, `Cell`). |
| `AppState.cs` | Estado persistente vía `PlayerPrefs` (edad elegida, estrellas, nivel, racha). |
| `Carousel.cs` | Carrusel de onboarding: swipe con snap, rubber-band, auto-avance, puntos indicadores. |
| `Appear.cs` | Animación de entrada (fade + slide) para tarjetas/tiles, con delay para efecto escalonado. |
| `PressEffect.cs` | Feedback táctil (escala el botón al presionar). |
| `SafeArea.cs` | Ajusta la UI al área segura del dispositivo (notch / barra de estado). |
| `ModelViewer.cs` | Visor 3D de respaldo: renderiza el modelo a una `RenderTexture` con cámara y luces propias (se usa cuando no hay AR real disponible). |
| `Spinner.cs` | Gira el modelo 3D sobre su eje. |
| `ARController.cs` | Rig de AR real (ARCore): `ARSession` + `XROrigin` + `ARCameraManager`/`Background` + `ARTrackedImageManager`. Crea la librería de imágenes en runtime desde los QR y coloca el modelo sobre el marcador detectado. Si el dispositivo no es compatible, cae automáticamente al `ModelViewer`. |

---

## Estado del proyecto (2026-07-08)

### Completado y verificado en dispositivo real
- **Pantallas 1-4** (Inicio, Bienvenida con carrusel, Selección de perfil, Menú principal) —
  pulidas, responsivas (portrait **y** landscape con layouts propios, no reutilizados entre sí),
  con transiciones (fade+slide), animaciones de entrada escalonadas, fondo con blobs decorativos,
  header enriquecido (avatar, nivel, racha, barra de progreso) y **barra de navegación inferior**
  (Inicio / Progreso / Config / Ayuda) fija.
- **Libro Fisiológico completo:**
  - **Temas** (4 temas con icono + color de acento propio, tarjeta simplificada tipo la de Perfil).
  - **Detalle** (ilustración, descripción, botón **Escuchar** con narración TTS real por tema).
  - **Experiencia AR**: visor 3D del páncreas (modelo real extraído del prototipo original) con
    controles circulares (Info / Audio / Girar / Cerrar / Foto).
- **Audio:** narración en español (4 clips, generados con gTTS ante la falta de API key de
  ElevenLabs) — confirmado funcionando en dispositivo.
- **Iconos propios:** generados con Python/PIL (no dependen del arte del prototipo original).
- **QR de prueba:** 4 marcadores + hoja A4 imprimible en `Marcadores/`.
- **AR real (ARCore) integrado:** `ARController` con image tracking desde los QR generados.
  Confirmado funcionando (sesión inicializa y detecta) en un **Infinix NOTE 40 Pro** (dispositivo
  ARCore-certificado). Con fallback automático al visor 3D si el dispositivo no es compatible.

### Caveat conocido de AR en dispositivos NO certificados por ARCore
En un dispositivo que Google no certifica para ARCore (verificado con una tablet genérica),
ARCore puede mostrar **una sola vez** su propio diálogo nativo *"This application requires the
latest version of Google Play Services for AR"* al entrar a la Experiencia AR — esto lo dispara
el SDK de Google internamente (no es un bug de esta app) apenas se habilita el componente
`ARSession`. Se intentó suprimirlo manteniendo `ARSession` apagado hasta confirmar
compatibilidad, pero eso **cuelga indefinidamente** la comprobación de disponibilidad (nada
sondea el resultado nativo async sin el componente activo) — arriesgando el flujo real en
dispositivos sí compatibles, así que se revirtió: se prioriza que el AR funcione correctamente
en hardware compatible. En un dispositivo certificado (lista oficial de Google "ARCore supported
devices") no aparece ningún diálogo y la cámara en vivo + tracking funcionan directo.

### Pendiente
- Pantallas **5B (Libro Nutricional)**, **5C (Libro Clínico)**, **6 (Juegos y Retos)**,
  **7 (Progreso)**, **8 (Configuración)** — hoy muestran un *toast* "Próximamente".
- Icono/skin propios para el mini-juego y progreso reales (hoy son solo el tile del menú).
- Probar el escaneo real del QR con imagen impresa (marcador) en un dispositivo ARCore.

---

## Dependencias (`Packages/manifest.json`)
- `com.unity.xr.arfoundation` **6.5.0**
- `com.unity.xr.arcore` **6.5.0**
- `com.unity.ugui` (uGUI + TextMeshPro)
- `com.unity.inputsystem` (requerido por AR Foundation, Active Input Handling = *Both*)
- `com.unity.xr.management` / `com.unity.xr.core-utils` (dependencias de AR Foundation)

Todas se resuelven solas al abrir el proyecto (necesita internet la primera vez).

## Configuración del Player relevante
- Orientación: **Auto-rotación** (portrait + landscape ambos sentidos).
- Android **Graphics API: solo OpenGLES3** (ARCore no soporta Vulkan).
- `minSdkVersion`: Android API 26.
- Bundle ID: `com.endify.ardiabetes`.
