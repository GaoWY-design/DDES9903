#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LostAndFoundSceneBuilder
{
    const string ScenePath = "Assets/Scenes/LostAndFound.unity";
    const string MatFolder = "Assets/Materials";
    const string TexFolder = "Assets/GeneratedTextures";
    const string FontChinese = "Assets/Fonts/ChineseUIFont.ttf";
    const string FontEnglish = "Assets/Fonts/EnglishUIFont.ttf";

    [MenuItem("Tools/Lost And Found/Build Scene")]
    public static void BuildScene()
    {
        PlayerSettings.productName = "失物招领";
        PlayerSettings.bundleVersion = "0.1";
        PlayerSettings.companyName = "DDES9903";

        EnsureFolders();
        GenerateTextures();
        var mats = CreateMaterials();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.085f, 0.078f, 0.068f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.045f, 0.041f, 0.038f);
        RenderSettings.fogDensity = 0.022f;

        var root = new GameObject("LostAndFoundScene");

        BuildRoom(root.transform, mats);
        BuildLights(root.transform, mats);
        BuildTable(root.transform, mats);
        BuildBook(root.transform, mats);
        BuildWallet(root.transform, mats);
        BuildWallTexts(root.transform);
        BuildPlayer(root.transform);
        BuildExit(root.transform);
        BuildAudio(root.transform);
        var ui = BuildUI();
        BuildSceneSetup(root.transform, ui);

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        ConfigureBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[LostAndFound] Scene built: " + ScenePath +
                  " | Tip: add audio to Assets/Resources/音乐3 if missing.");
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("失物招领",
                "Scene built successfully:\n" + ScenePath +
                "\n\nMenu can rebuild anytime.\nBGM: put clip at Assets/Resources/音乐3",
                "OK");
        }
    }

    static void EnsureFolders()
    {
        Directory.CreateDirectory(MatFolder);
        Directory.CreateDirectory(TexFolder);
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Fonts");
        Directory.CreateDirectory("Assets/Resources");
        Directory.CreateDirectory("Assets/Scripts");
    }

    static void ConfigureBuildSettings()
    {
        var scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };
        EditorBuildSettings.scenes = scenes;
    }

    #region Textures / Materials

    static void GenerateTextures()
    {
        MakeNoiseTex("Wall_Plaster", new Color(0.28f, 0.25f, 0.22f), new Color(0.38f, 0.34f, 0.30f), 0.08f, false);
        MakeNoiseTex("Floor_Concrete", new Color(0.14f, 0.14f, 0.145f), new Color(0.22f, 0.21f, 0.20f), 0.12f, false);
        MakeNoiseTex("Baseboard_Black", new Color(0.04f, 0.035f, 0.03f), new Color(0.08f, 0.07f, 0.06f), 0.04f, false);
        MakeWoodTex("Table_Wood", new Color(0.42f, 0.28f, 0.16f), new Color(0.55f, 0.36f, 0.20f));
        MakeWoodTex("TableLeg_Wood", new Color(0.22f, 0.14f, 0.09f), new Color(0.30f, 0.18f, 0.11f));
        MakeNoiseTex("Book_Navy", new Color(0.08f, 0.12f, 0.28f), new Color(0.12f, 0.18f, 0.38f), 0.15f, true);
        MakeNoiseTex("Book_Pages", new Color(0.90f, 0.86f, 0.76f), new Color(0.95f, 0.92f, 0.84f), 0.05f, false);
        MakeNoiseTex("Wallet_Leather", new Color(0.22f, 0.12f, 0.07f), new Color(0.32f, 0.18f, 0.10f), 0.18f, true);
        MakeVignetteTex("UI_Vignette");
    }

    static void MakeNoiseTex(string name, Color a, Color b, float contrast, bool weave)
    {
        int size = 256;
        var tex = new Texture2D(size, size, TextureFormat.RGB24, true);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n = Mathf.PerlinNoise(x * 0.07f + 12.3f, y * 0.07f + 4.7f);
            float n2 = Mathf.PerlinNoise(x * 0.21f, y * 0.21f);
            float t = Mathf.Clamp01((n * 0.7f + n2 * 0.3f) + (Random.value - 0.5f) * contrast * 0.15f);
            if (weave)
            {
                float wx = Mathf.Abs(Mathf.Sin(x * 0.55f));
                float wy = Mathf.Abs(Mathf.Sin(y * 0.55f));
                t = Mathf.Clamp01(t * 0.75f + wx * wy * 0.35f);
            }
            tex.SetPixel(x, y, Color.Lerp(a, b, t));
        }
        tex.Apply();
        SavePng(tex, TexFolder + "/" + name + ".png");
    }

    static void MakeWoodTex(string name, Color a, Color b)
    {
        int size = 256;
        var tex = new Texture2D(size, size, TextureFormat.RGB24, true);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float grain = Mathf.PerlinNoise(x * 0.02f, y * 0.18f);
            float ring = Mathf.PerlinNoise(x * 0.08f + grain * 2f, 0.5f);
            Color c = Color.Lerp(a, b, grain * 0.65f + ring * 0.35f);
            tex.SetPixel(x, y, c);
        }
        tex.Apply();
        SavePng(tex, TexFolder + "/" + name + ".png");
    }

    static void MakeVignetteTex(string name)
    {
        int size = 512;
        var tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        Vector2 center = new Vector2(0.5f, 0.5f);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float u = x / (float)(size - 1);
            float v = y / (float)(size - 1);
            float d = Vector2.Distance(new Vector2(u, v), center);
            float a = Mathf.SmoothStep(0.35f, 0.98f, d) * 0.78f;
            tex.SetPixel(x, y, new Color(0f, 0f, 0f, a));
        }
        tex.Apply();
        SavePng(tex, TexFolder + "/" + name + ".png");
    }

    static void SavePng(Texture2D tex, string path)
    {
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.sRGBTexture = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.anisoLevel = 4;
            importer.SaveAndReimport();
        }
    }

    struct Mats
    {
        public Material wall, floor, baseboard, tabletop, tableleg, bookCover, bookPages, wallet, downlight;
    }

    static Mats CreateMaterials()
    {
        var m = new Mats
        {
            wall = MakeMat("Wall_DarkWarmGray", "Wall_Plaster", 0.18f, new Vector2(2f, 3f)),
            floor = MakeMat("Floor_DarkConcrete", "Floor_Concrete", 0.34f, new Vector2(2.4f, 3.6f)),
            baseboard = MakeMat("Baseboard_Black", "Baseboard_Black", 0.22f, Vector2.one),
            tabletop = MakeMat("Tabletop_WarmWood", "Table_Wood", 0.25f, new Vector2(1.5f, 1f)),
            tableleg = MakeMat("TableLeg_DarkWood", "TableLeg_Wood", 0.18f, Vector2.one),
            bookCover = MakeMat("BookCover_NavyFabric", "Book_Navy", 0.2f, Vector2.one),
            bookPages = MakeMat("BookPages_Cream", "Book_Pages", 0.12f, Vector2.one),
            wallet = MakeMat("Wallet_DarkBrownLeather", "Wallet_Leather", 0.28f, Vector2.one),
            downlight = MakeEmissive("Downlight_WhiteEmissive", new Color(1f, 0.92f, 0.8f) * 1.5f)
        };
        return m;
    }

    static Material MakeMat(string name, string texName, float smooth, Vector2 tiling)
    {
        string path = MatFolder + "/" + name + ".mat";
        var shader = Shader.Find("Standard");
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        else mat.shader = shader;

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexFolder + "/" + texName + ".png");
        mat.SetTexture("_MainTex", tex);
        mat.SetTextureScale("_MainTex", tiling);
        mat.SetFloat("_Glossiness", smooth);
        mat.SetFloat("_Metallic", 0f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Material MakeEmissive(string name, Color emission)
    {
        string path = MatFolder + "/" + name + ".mat";
        var shader = Shader.Find("Standard");
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_Color", new Color(0.9f, 0.85f, 0.75f));
        mat.EnableKeyword("_EMISSION");
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        mat.SetColor("_EmissionColor", emission);
        mat.SetFloat("_Glossiness", 0.35f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    #endregion

    #region Geometry

    static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, Vector3? euler = null)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        if (euler.HasValue) go.transform.localEulerAngles = euler.Value;
        var r = go.GetComponent<MeshRenderer>();
        if (r != null && mat != null) r.sharedMaterial = mat;
        return go;
    }

    static void BuildRoom(Transform root, Mats mats)
    {
        const float W = 4.8f;
        const float D = 8.0f;
        const float H = 2.8f;
        const float centerZ = -1.55f;
        const float frontZ = 2.45f;
        const float backZ = -5.55f;
        const float doorW = 1.15f;
        float halfW = W * 0.5f;
        float thick = 0.12f;

        var floor = Prim(PrimitiveType.Cube, "Floor", root, new Vector3(0f, -thick * 0.5f, centerZ), new Vector3(W, thick, D), mats.floor);
        var ceiling = Prim(PrimitiveType.Cube, "Ceiling", root, new Vector3(0f, H + thick * 0.5f, centerZ), new Vector3(W, thick, D), mats.wall);

        var walls = new GameObject("Walls");
        walls.transform.SetParent(root, false);

        // Front text wall
        Prim(PrimitiveType.Cube, "FrontWall", walls.transform, new Vector3(0f, H * 0.5f, frontZ), new Vector3(W, H, thick), mats.wall);
        // Side walls
        Prim(PrimitiveType.Cube, "LeftWall", walls.transform, new Vector3(-halfW, H * 0.5f, centerZ), new Vector3(thick, H, D), mats.wall);
        Prim(PrimitiveType.Cube, "RightWall", walls.transform, new Vector3(halfW, H * 0.5f, centerZ), new Vector3(thick, H, D), mats.wall);

        // Back wall with door gap
        float sideW = (W - doorW) * 0.5f;
        float leftX = -halfW + sideW * 0.5f;
        float rightX = halfW - sideW * 0.5f;
        Prim(PrimitiveType.Cube, "BackWall_Left", walls.transform, new Vector3(leftX, H * 0.5f, backZ), new Vector3(sideW, H, thick), mats.wall);
        Prim(PrimitiveType.Cube, "BackWall_Right", walls.transform, new Vector3(rightX, H * 0.5f, backZ), new Vector3(sideW, H, thick), mats.wall);
        float lintelH = 0.35f;
        Prim(PrimitiveType.Cube, "BackWall_ExitLintel", walls.transform, new Vector3(0f, 2.55f, backZ), new Vector3(doorW + 0.05f, lintelH, thick), mats.wall);

        var boards = new GameObject("Baseboards");
        boards.transform.SetParent(root, false);
        float bh = 0.08f;
        float by = bh * 0.5f;
        Prim(PrimitiveType.Cube, "Baseboard_Front", boards.transform, new Vector3(0f, by, frontZ - 0.02f), new Vector3(W - 0.05f, bh, 0.06f), mats.baseboard);
        Prim(PrimitiveType.Cube, "Baseboard_Left", boards.transform, new Vector3(-halfW + 0.02f, by, centerZ), new Vector3(0.06f, bh, D - 0.1f), mats.baseboard);
        Prim(PrimitiveType.Cube, "Baseboard_Right", boards.transform, new Vector3(halfW - 0.02f, by, centerZ), new Vector3(0.06f, bh, D - 0.1f), mats.baseboard);
        Prim(PrimitiveType.Cube, "Baseboard_BackL", boards.transform, new Vector3(leftX, by, backZ + 0.02f), new Vector3(sideW - 0.02f, bh, 0.06f), mats.baseboard);
        Prim(PrimitiveType.Cube, "Baseboard_BackR", boards.transform, new Vector3(rightX, by, backZ + 0.02f), new Vector3(sideW - 0.02f, bh, 0.06f), mats.baseboard);

        // Silence unused warning
        _ = floor;
        _ = ceiling;
    }

    static void BuildLights(Transform root, Mats mats)
    {
        var down = Prim(PrimitiveType.Cylinder, "SmallRoundCeilingDownlight", root,
            new Vector3(0f, 2.775f, 1.58f), new Vector3(0.18f, 0.025f, 0.18f), mats.downlight);
        Object.DestroyImmediate(down.GetComponent<Collider>());

        var main = new GameObject("WarmCentralSpotlight");
        main.transform.SetParent(root, false);
        main.transform.position = new Vector3(0f, 2.68f, 1.58f);
        main.transform.rotation = Quaternion.Euler(72f, 0f, 0f);
        var mainL = main.AddComponent<Light>();
        mainL.type = LightType.Spot;
        mainL.color = new Color(1f, 0.86f, 0.66f);
        mainL.intensity = 5.8f;
        mainL.range = 7f;
        mainL.spotAngle = 84f;
        mainL.innerSpotAngle = 42f;
        mainL.shadows = LightShadows.Soft;
        mainL.shadowStrength = 0.82f;

        CreateTableSpot(root, "WarmTableSoftSpot", new Vector3(0.15f, 2.45f, 0.55f), new Vector3(55f, 10f, 0f), 2.2f, 0.45f);
        CreateTableSpot(root, "WarmTableSoftSpot_Copy", new Vector3(-0.35f, 2.35f, 1.05f), new Vector3(60f, -8f, 0f), 1.1f, 0.35f);

        var bounce = new GameObject("VeryLowWarmBounce");
        bounce.transform.SetParent(root, false);
        bounce.transform.position = new Vector3(0f, 1.6f, -1.45f);
        var bl = bounce.AddComponent<Light>();
        bl.type = LightType.Point;
        bl.color = new Color(0.62f, 0.48f, 0.35f);
        bl.intensity = 0.32f;
        bl.range = 4.8f;
        bl.shadows = LightShadows.None;
    }

    static void CreateTableSpot(Transform root, string name, Vector3 pos, Vector3 euler, float intensity, float shadow)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(euler);
        var l = go.AddComponent<Light>();
        l.type = LightType.Spot;
        l.color = new Color(1f, 0.78f, 0.52f);
        l.intensity = intensity;
        l.range = 4.5f;
        l.spotAngle = 60f;
        l.innerSpotAngle = 24f;
        l.shadows = LightShadows.Soft;
        l.shadowStrength = shadow;
    }

    static void BuildTable(Transform root, Mats mats)
    {
        var table = new GameObject("CleanWoodenTable");
        table.transform.SetParent(root, false);
        table.transform.position = new Vector3(0f, 0f, 0.92f);

        // Top surface: top at y=0.76, thickness 0.12 => center y = 0.70
        Prim(PrimitiveType.Cube, "Tabletop", table.transform, new Vector3(0f, 0.70f, 0f), new Vector3(3.55f, 0.12f, 1.55f), mats.tabletop);
        Prim(PrimitiveType.Cube, "Apron_Front", table.transform, new Vector3(0f, 0.58f, 0.72f), new Vector3(3.4f, 0.1f, 0.06f), mats.tableleg);
        Prim(PrimitiveType.Cube, "Apron_Back", table.transform, new Vector3(0f, 0.58f, -0.72f), new Vector3(3.4f, 0.1f, 0.06f), mats.tableleg);

        float legY = 0.30f;
        float legH = 0.60f;
        Vector3 legScale = new Vector3(0.09f, legH, 0.09f);
        Prim(PrimitiveType.Cube, "Leg_FL", table.transform, new Vector3(-1.55f, legY, 0.62f), legScale, mats.tableleg);
        Prim(PrimitiveType.Cube, "Leg_FR", table.transform, new Vector3(1.55f, legY, 0.62f), legScale, mats.tableleg);
        Prim(PrimitiveType.Cube, "Leg_BL", table.transform, new Vector3(-1.55f, legY, -0.62f), legScale, mats.tableleg);
        Prim(PrimitiveType.Cube, "Leg_BR", table.transform, new Vector3(1.55f, legY, -0.62f), legScale, mats.tableleg);
    }

    static void BuildBook(Transform root, Mats mats)
    {
        var book = new GameObject("BlueHardcoverBook_ShiyuJi");
        book.transform.SetParent(root, false);
        book.transform.position = new Vector3(0f, 0.84f, 0.92f);
        book.transform.rotation = Quaternion.Euler(0f, -8f, 0f);

        Prim(PrimitiveType.Cube, "Cover", book.transform, Vector3.zero, new Vector3(0.28f, 0.045f, 0.36f), mats.bookCover);
        Prim(PrimitiveType.Cube, "Pages", book.transform, new Vector3(0.01f, 0f, 0f), new Vector3(0.24f, 0.038f, 0.33f), mats.bookPages);

        var title = new GameObject("BookTitle_ShiyuJi");
        title.transform.SetParent(book.transform, false);
        title.transform.localPosition = new Vector3(0f, 0.028f, 0f);
        title.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        var tm = title.AddComponent<TextMesh>();
        tm.text = "Shiyu Ji";
        tm.fontSize = 32;
        tm.characterSize = 0.012f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(0.85f, 0.78f, 0.55f);
        var font = AssetDatabase.LoadAssetAtPath<Font>(FontEnglish);
        if (font != null) tm.font = font;
        book.AddComponent<BookInteraction>();

        // Parent collider for raycasts
        var box = book.AddComponent<BoxCollider>();
        box.size = new Vector3(0.3f, 0.06f, 0.38f);
    }

    static void BuildWallet(Transform root, Mats mats)
    {
        var wallet = Prim(PrimitiveType.Cube, "BrownLeatherWallet", root,
            new Vector3(1.209f, 0.831f, 0.82f), new Vector3(0.22f, 0.045f, 0.14f), mats.wallet,
            new Vector3(0f, 18f, 0f));
        wallet.AddComponent<WalletInteraction>();
    }

    static void BuildWallTexts(Transform root)
    {
        CreateWallText(root, "WallText_Chinese_失物招领处", "失物招领处",
            new Vector3(0f, 1.85f, 2.38f), FontChinese, 48, 0.045f, new Color(0.92f, 0.86f, 0.72f));
        CreateWallText(root, "WallText_English_LOST_AND_FOUND", "LOST AND FOUND",
            new Vector3(0f, 1.55f, 2.38f), FontEnglish, 36, 0.028f, new Color(0.78f, 0.72f, 0.60f));
    }

    static void CreateWallText(Transform root, string name, string text, Vector3 pos, string fontPath, int fontSize, float charSize, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.position = pos;
        // TextMesh 默认朝 +Z；玩家在房间内看向前墙，文字需朝向 -Z（面向玩家）
        go.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        // 稍微抬离墙面，避免 z-fighting
        go.transform.position = new Vector3(pos.x, pos.y, pos.z);
        var tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = fontSize;
        tm.characterSize = charSize;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = color;
        var font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        if (font != null)
        {
            tm.font = font;
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null && font.material != null) mr.sharedMaterial = font.material;
        }
    }

    static void BuildPlayer(Transform root)
    {
        // Ensure Player tag
        EnsureTag("Player");

        var player = new GameObject("Player_FirstPerson");
        player.transform.SetParent(root, false);
        player.transform.position = new Vector3(0f, 0.035f, -2.75f);
        player.tag = "Player";

        var cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.28f;
        cc.center = new Vector3(0f, 0.9f, 0f);
        cc.slopeLimit = 45f;
        cc.stepOffset = 0.25f;
        cc.skinWidth = 0.055f;

        var camGo = new GameObject("PlayerCamera");
        camGo.transform.SetParent(player.transform, false);
        camGo.transform.localPosition = new Vector3(0f, 1.58f, 0f);
        var cam = camGo.AddComponent<Camera>();
        cam.fieldOfView = 60f;
        cam.nearClipPlane = 0.03f;
        cam.farClipPlane = 35f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.025f, 0.023f, 0.022f);
        camGo.AddComponent<AudioListener>();

        var fps = player.AddComponent<FirstPersonController>();
        fps.playerCamera = camGo.transform;
        fps.mouseSensitivity = 1.9f;
        fps.moveSpeed = 2.1f;
        fps.gravity = -18f;
        fps.initialPitch = 3f;
    }

    static void EnsureTag(string tag)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (assets == null || assets.Length == 0) return;
        var so = new SerializedObject(assets[0]);
        var tags = so.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        so.ApplyModifiedProperties();
    }

    static void BuildExit(Transform root)
    {
        var exit = new GameObject("ExitTrigger_BehindPlayer");
        exit.transform.SetParent(root, false);
        exit.transform.position = new Vector3(0f, 1f, -5.7f);
        var box = exit.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(1.1f, 2f, 0.55f);
        exit.AddComponent<ExitTrigger>();
    }

    static void BuildAudio(Transform root)
    {
        var go = new GameObject("GameAudio_Music3");
        go.transform.SetParent(root, false);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = true;
        src.loop = true;
        src.spatialBlend = 0f;
        src.volume = 0.55f;

        var clip = Resources.Load<AudioClip>("音乐3");
        if (clip == null)
            clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/音乐3.wav");
        if (clip == null)
            clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/音乐3.ogg");
        src.clip = clip;
        if (clip == null)
            Debug.LogWarning("[LostAndFound] Missing BGM Assets/Resources/音乐3 (wav/ogg). Bootstrap will still create mount point.");
    }

    static UIManager BuildUI()
    {
        var canvasGo = new GameObject("GameUI");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        Font cn = AssetDatabase.LoadAssetAtPath<Font>(FontChinese);
        Font en = AssetDatabase.LoadAssetAtPath<Font>(FontEnglish);
        Font uiFont = en != null ? en : Resources.GetBuiltinResource<Font>("Arial.ttf");
        Font cnFont = cn != null ? cn : uiFont;

        // Vignette
        var vig = CreateUiImage(canvasGo.transform, "CinematicVignette", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetFullStretch(vig.GetComponent<RectTransform>());
        var vigImg = vig.GetComponent<Image>();
        vigImg.raycastTarget = false;
        var vigTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexFolder + "/UI_Vignette.png");
        if (vigTex != null)
        {
            vigImg.sprite = Sprite.Create(vigTex, new Rect(0, 0, vigTex.width, vigTex.height), new Vector2(0.5f, 0.5f));
            vigImg.color = Color.white;
            vigImg.type = Image.Type.Simple;
            vigImg.preserveAspect = false;
        }
        else vigImg.color = new Color(0f, 0f, 0f, 0.35f);

        // Book prompt
        var bookPrompt = CreateUiEmpty(canvasGo.transform, "BookPrompt", new Vector2(0.5f, 0.34f));
        var q = CreateUiText(bookPrompt.transform, "QuestionText", "Is this your book?", uiFont, 28, TextAnchor.MiddleCenter, new Vector2(0f, 40f), new Vector2(560f, 48f));
        q.color = new Color(0.92f, 0.92f, 0.92f, 0.95f);
        var yesBtn = CreateUiImage(bookPrompt.transform, "YesButton_KeyboardE", new Vector2(0f, -18f), new Vector2(138f, 44f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        yesBtn.GetComponent<Image>().color = new Color(0.42f, 0.42f, 0.42f, 0.92f);
        yesBtn.GetComponent<Image>().raycastTarget = false;
        var yesText = CreateUiText(yesBtn.transform, "YesText", "Yes", uiFont, 24, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(138f, 44f));
        yesText.raycastTarget = false;

        // Wallet prompt
        var walletPrompt = CreateUiEmpty(canvasGo.transform, "WalletPrompt", new Vector2(0.5f, 0.34f));
        var wq = CreateUiText(walletPrompt.transform, "WalletQuestionText", "Is this your Wallet?", uiFont, 28, TextAnchor.MiddleCenter, new Vector2(0f, 40f), new Vector2(560f, 48f));
        wq.color = new Color(0.92f, 0.92f, 0.92f, 0.95f);
        var noBtn = CreateUiImage(walletPrompt.transform, "NoButton_KeyboardE", new Vector2(0f, -18f), new Vector2(138f, 44f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        noBtn.GetComponent<Image>().color = new Color(0.42f, 0.42f, 0.42f, 0.92f);
        noBtn.GetComponent<Image>().raycastTarget = false;
        var noText = CreateUiText(noBtn.transform, "NoText", "No", uiFont, 24, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(138f, 44f));
        noText.raycastTarget = false;

        // Game over
        var goOver = CreateUiImage(canvasGo.transform, "GameOverOverlay", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetFullStretch(goOver.GetComponent<RectTransform>());
        var goImg = goOver.GetComponent<Image>();
        goImg.color = new Color(0f, 0f, 0f, 0.76f);
        goImg.raycastTarget = false;
        var goText = CreateUiText(goOver.transform, "GameOverText", "GAME OVER!", uiFont, 78, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(900f, 160f));
        goText.color = Color.white;
        goText.fontStyle = FontStyle.Bold;
        goText.raycastTarget = false;
        // Prefer Chinese font file only where needed; English UI uses English font.
        if (cnFont != null) { /* reserved */ }

        bookPrompt.SetActive(false);
        walletPrompt.SetActive(false);
        goOver.SetActive(false);

        var ui = canvasGo.AddComponent<UIManager>();
        ui.bookPromptRoot = bookPrompt;
        ui.bookQuestionText = q;
        ui.yesButtonImage = yesBtn.GetComponent<Image>();
        ui.yesButtonText = yesText;
        ui.walletPromptRoot = walletPrompt;
        ui.walletQuestionText = wq;
        ui.noButtonImage = noBtn.GetComponent<Image>();
        ui.noButtonText = noText;
        ui.gameOverOverlay = goOver;
        ui.gameOverText = goText;

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        return ui;
    }

    static GameObject CreateUiEmpty(Transform parent, string name, Vector2 anchor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(600f, 140f);
        return go;
    }

    static GameObject CreateUiImage(Transform parent, string name, Vector2 anchoredPos, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.raycastTarget = false;
        return go;
    }

    static Text CreateUiText(Transform parent, string name, string content, Font font, int size, TextAnchor anchor, Vector2 pos, Vector2 dim)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = dim;
        var text = go.AddComponent<Text>();
        text.text = content;
        text.font = font;
        text.fontSize = size;
        text.alignment = anchor;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    static void SetFullStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void BuildSceneSetup(Transform root, UIManager ui)
    {
        var go = new GameObject("SceneSetup");
        go.transform.SetParent(root, false);
        var setup = go.AddComponent<SceneSetup>();
        setup.player = Object.FindObjectOfType<FirstPersonController>();
        setup.book = Object.FindObjectOfType<BookInteraction>();
        setup.wallet = Object.FindObjectOfType<WalletInteraction>();
        setup.exitTrigger = Object.FindObjectOfType<ExitTrigger>();
        setup.uiManager = ui;
        var music = GameObject.Find("GameAudio_Music3");
        if (music != null) setup.musicSource = music.GetComponent<AudioSource>();
    }

    #endregion
}
#endif
