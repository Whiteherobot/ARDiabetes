using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using ARDiabetes;

/// <summary>
/// Setup reproducible del proyecto (se ejecuta por CLI vía -executeMethod ProjectSetup.Build):
///  - Marca el arte de Assets/Imagenes como Sprite.
///  - Crea la escena Scenes/Main.unity con cámara, EventSystem y AppBootstrap (sprites cableados).
///  - Registra la escena en Build Settings y fija PlayerSettings para Android (portrait, bundle id).
/// </summary>
public static class ProjectSetup
{
    const string ScenePath = "Assets/Scenes/Main.unity";

    [MenuItem("ARDiabetes/Rebuild Scene")]
    public static void Build()
    {
        MarcarSpritesEnCarpeta("Assets/Imagenes");

        Directory.CreateDirectory("Assets/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Cámara
        var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camGo.tag = "MainCamera";
        var cam = camGo.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.918f, 0.957f, 0.984f); // #EAF4FB
        cam.orthographic = true;

        // EventSystem
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        // AppBootstrap + sprites
        var go = new GameObject("AppBootstrap");
        var boot = go.AddComponent<AppBootstrap>();
        boot.spLogo        = L("Assets/Imagenes/Diseño/personajes.png");
        boot.spWelcome     = L("Assets/Imagenes/Diseño/Avatar2.png");
        boot.spAvatar1     = L("Assets/Imagenes/Diseño/Avatar1.png");
        boot.spAvatar2     = L("Assets/Imagenes/Diseño/Avatar2.png");
        boot.spAvatar3     = L("Assets/Imagenes/Diseño/Avatar3.png");
        boot.spIconFisio   = L("Assets/Imagenes/Diseño/Botones/Icon_Fisiológico.png");
        boot.spIconNutri   = L("Assets/Imagenes/Diseño/Botones/Icon_nutricional.png");
        boot.spIconClinico = L("Assets/Imagenes/Diseño/Botones/Icon_Clínico.png");
        boot.spIconScan    = L("Assets/Imagenes/Diseño/Botones/btn_scaner.png");
        boot.spIconJuegos  = L("Assets/Imagenes/Diseño/Botones/Icon_juegos 1.png");
        boot.spIconProgreso= L("Assets/Imagenes/UI/ic_chart.png"); // barras (reemplaza la llamita)
        boot.spStar        = L("Assets/Imagenes/Diseño/Botones/Icon_Estrella.png");
        boot.spPancreas    = L("Assets/Imagenes/Diseño/Icon_Pancreas.png");
        boot.pancreasModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Imagenes/QR/Pancreas.fbx");
        boot.platoModel    = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Imagenes/QR/Plato.fbx");
        boot.glucoModel    = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Imagenes/QR/Gluco.fbx");
        if (boot.pancreasModel == null) Debug.LogWarning("[ProjectSetup] Pancreas.fbx no encontrado");
        if (boot.platoModel == null) Debug.LogWarning("[ProjectSetup] Plato.fbx no encontrado");
        if (boot.glucoModel == null) Debug.LogWarning("[ProjectSetup] Gluco.fbx no encontrado");

        boot.icInfo     = L("Assets/Imagenes/UI/ic_info.png");
        boot.icAudio    = L("Assets/Imagenes/UI/ic_audio.png");
        boot.icRotate   = L("Assets/Imagenes/UI/ic_rotate.png");
        boot.icClose    = L("Assets/Imagenes/UI/ic_close.png");
        boot.icCamera   = L("Assets/Imagenes/UI/ic_camera.png");
        boot.icChart    = L("Assets/Imagenes/UI/ic_chart.png");
        boot.icQuestion = L("Assets/Imagenes/UI/ic_question.png");
        boot.icDrop     = L("Assets/Imagenes/UI/ic_drop.png");
        boot.icGear     = L("Assets/Imagenes/UI/ic_gear.png");
        boot.icHome     = L("Assets/Imagenes/UI/ic_home.png");
        boot.icBook     = L("Assets/Imagenes/UI/ic_book.png");
        boot.icPancreas = L("Assets/Imagenes/UI/ic_pancreas.png");
        boot.icPlate    = L("Assets/Imagenes/UI/ic_plate.png");
        boot.icBread    = L("Assets/Imagenes/UI/ic_bread.png");
        boot.icApple    = L("Assets/Imagenes/UI/ic_apple.png");
        boot.icClock    = L("Assets/Imagenes/UI/ic_clock.png");
        boot.icSyringe  = L("Assets/Imagenes/UI/ic_syringe.png");
        boot.icAlert    = L("Assets/Imagenes/UI/ic_alert.png");
        boot.icCalendar = L("Assets/Imagenes/UI/ic_calendar.png");

        boot.narracion = new AudioClip[12];
        for (int i = 0; i < 12; i++)
        {
            boot.narracion[i] = AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Audio/Narracion/narracion_{i}.wav");
            if (boot.narracion[i] == null) Debug.LogWarning($"[ProjectSetup] narracion_{i}.wav no encontrado");
        }

        boot.markersFisio = LoadMarkers("qr_diabetes", "qr_pancreas", "qr_insulina", "qr_funciona");
        boot.markersNutri = LoadMarkers("qr_nutri_plato", "qr_nutri_carbohidratos", "qr_nutri_recomendados", "qr_nutri_habitos");
        boot.markersClinico = LoadMarkers("qr_clinico_insulina", "qr_clinico_sintomas", "qr_clinico_monitoreo", "qr_clinico_cuidados");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        PlayerSettings.companyName = "Endify";
        PlayerSettings.productName = "AR Diabetes Tipo 1";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.useAnimatedAutorotation = true;
        try
        {
            PlayerSettings.SetApplicationIdentifier(
                UnityEditor.Build.NamedBuildTarget.Android, "com.endify.ardiabetes");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        }
        catch (System.Exception e) { Debug.LogWarning("Android settings: " + e.Message); }

        AssetDatabase.SaveAssets();
        Debug.Log("[ProjectSetup] Escena y ajustes generados OK. logo=" + (boot.spLogo != null));
    }

    // Build de validación (Linux) para probar las pantallas en la PC.
    public static void BuildLinux()
    {
        var opts = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = "/tmp/ardiabetes_linux/ARDiabetes.x86_64",
            target = BuildTarget.StandaloneLinux64,
            options = BuildOptions.None
        };
        var report = BuildPipeline.BuildPlayer(opts);
        Debug.Log("[ProjectSetup] Build Linux: " + report.summary.result +
                  " size=" + report.summary.totalSize);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }

    // Build del APK Android.
    public static void BuildAndroid()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        System.IO.Directory.CreateDirectory("/tmp/ardiabetes_apk");
        var opts = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = "/tmp/ardiabetes_apk/ARDiabetes.apk",
            target = BuildTarget.Android,
            options = BuildOptions.None
        };
        var report = BuildPipeline.BuildPlayer(opts);
        Debug.Log("[ProjectSetup] Build Android: " + report.summary.result +
                  " size=" + report.summary.totalSize);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }

    // ---- Preview de las vistas en modo edición ----
    static void Prev(int i)
    {
        var b = UnityEngine.Object.FindFirstObjectByType<AppBootstrap>();
        if (b == null) { Debug.LogError("[Preview] No hay AppBootstrap en la escena. Abre Assets/Scenes/Main.unity."); return; }
        b.EditorPreview(i);
        Debug.Log("[Preview] Mostrando vista " + i + " — mira la pestaña Game.");
    }
    [MenuItem("ARDiabetes/Preview/1 Inicio")] static void Pv0() => Prev(0);
    [MenuItem("ARDiabetes/Preview/2 Bienvenida")] static void Pv1() => Prev(1);
    [MenuItem("ARDiabetes/Preview/3 Perfil")] static void Pv2() => Prev(2);
    [MenuItem("ARDiabetes/Preview/4 Menú")] static void Pv3() => Prev(3);
    [MenuItem("ARDiabetes/Preview/5A Fisiológico/Temas")] static void Pv4() => Prev(4);
    [MenuItem("ARDiabetes/Preview/5A Fisiológico/Detalle")] static void Pv5() => Prev(5);
    [MenuItem("ARDiabetes/Preview/5A Fisiológico/Experiencia AR")] static void Pv6() => Prev(6);
    [MenuItem("ARDiabetes/Preview/5B Nutricional/Temas")] static void Pv7() => Prev(7);
    [MenuItem("ARDiabetes/Preview/5B Nutricional/Detalle")] static void Pv8() => Prev(8);
    [MenuItem("ARDiabetes/Preview/5B Nutricional/Experiencia AR")] static void Pv9() => Prev(9);
    [MenuItem("ARDiabetes/Preview/5C Clínico/Temas")] static void Pv10() => Prev(10);
    [MenuItem("ARDiabetes/Preview/5C Clínico/Detalle")] static void Pv11() => Prev(11);
    [MenuItem("ARDiabetes/Preview/5C Clínico/Experiencia AR")] static void Pv12() => Prev(12);
    [MenuItem("ARDiabetes/Preview/5D Escaneo genérico")] static void Pv13() => Prev(13);
    [MenuItem("ARDiabetes/Preview/Limpiar preview")]
    static void PvClear()
    {
        var b = UnityEngine.Object.FindFirstObjectByType<AppBootstrap>();
        if (b != null) b.EditorClearPreview();
    }

    // Configura el proyecto para ARCore: input handling, OpenGLES3, loader XR, QR legibles.
    public static void SetupAR()
    {
        // 1) Active Input Handling = Both (TrackedPoseDriver del Input System + UI legacy)
        try
        {
            var objs = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (objs.Length > 0)
            {
                var so = new SerializedObject(objs[0]);
                var prop = so.FindProperty("activeInputHandler");
                if (prop != null && prop.intValue != 2) { prop.intValue = 2; so.ApplyModifiedProperties(); Debug.Log("[SetupAR] activeInputHandler=Both"); }
            }
        }
        catch (System.Exception e) { Debug.LogWarning("[SetupAR] input handler: " + e.Message); }

        // 2) Graphics API: solo OpenGLES3 (ARCore no soporta Vulkan)
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });

        // 3) Habilitar loader ARCore
        try
        {
            Directory.CreateDirectory("Assets/XR");
            XRGeneralSettingsPerBuildTarget perBT;
            if (!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out perBT) || perBT == null)
            {
                perBT = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                AssetDatabase.CreateAsset(perBT, "Assets/XR/XRGeneralSettingsPerBuildTarget.asset");
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, perBT, true);
            }
            var settings = perBT.SettingsForBuildTarget(BuildTargetGroup.Android);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<XRGeneralSettings>();
                settings.name = "Android";
                perBT.SetSettingsForBuildTarget(BuildTargetGroup.Android, settings);
                AssetDatabase.AddObjectToAsset(settings, perBT);
            }
            var mgr = settings.AssignedSettings;
            if (mgr == null)
            {
                mgr = ScriptableObject.CreateInstance<XRManagerSettings>();
                mgr.name = "XRManagerSettings";
                settings.AssignedSettings = mgr;
                AssetDatabase.AddObjectToAsset(mgr, perBT);
            }
            bool ok = XRPackageMetadataStore.AssignLoader(mgr, "UnityEngine.XR.ARCore.ARCoreLoader", BuildTargetGroup.Android);
            Debug.Log("[SetupAR] ARCore loader assigned=" + ok);
            EditorUtility.SetDirty(perBT);
        }
        catch (System.Exception e) { Debug.LogWarning("[SetupAR] XR loader: " + e.Message); }

        // 4) QR marcadores legibles (para librería de imágenes en runtime)
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Markers" }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) continue;
            ti.textureType = TextureImporterType.Default;
            ti.isReadable = true;
            ti.mipmapEnabled = false;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[SetupAR] listo");
    }

    public static void PrintARVersions()
    {
        var req = UnityEditor.PackageManager.Client.Search("com.unity.xr.arfoundation");
        while (!req.IsCompleted) System.Threading.Thread.Sleep(100);
        if (req.Status == UnityEditor.PackageManager.StatusCode.Success && req.Result.Length > 0)
        {
            var p = req.Result[0];
            Debug.Log("[ARF] latestCompatible=" + p.versions.latestCompatible);
            Debug.Log("[ARF] compatible=" + string.Join(",", p.versions.compatible));
        }
        else Debug.Log("[ARF] search failed: " + (req.Error != null ? req.Error.message : "?"));
    }

    static Texture2D[] LoadMarkers(params string[] names)
    {
        var arr = new Texture2D[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            arr[i] = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Markers/{names[i]}.png");
            if (arr[i] == null) Debug.LogWarning($"[ProjectSetup] marcador no encontrado: {names[i]}.png");
        }
        return arr;
    }

    static Sprite L(string path)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null) Debug.LogWarning("[ProjectSetup] Sprite no encontrado: " + path);
        return s;
    }

    static void MarcarSpritesEnCarpeta(string folder)
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) continue;
            if (ti.textureType != TextureImporterType.Sprite || ti.spriteImportMode != SpriteImportMode.Single)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.SaveAndReimport();
            }
        }
    }
}
