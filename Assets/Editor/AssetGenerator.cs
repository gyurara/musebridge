using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ScriptableObject / Prefab / Sprite / Tag / Layer / 오디오클립 일괄 생성
/// BridgeRhythm → Generate All Assets 한 번이면 끝
/// </summary>
public static class AssetGenerator
{
    private const string SpriteFolder   = "Assets/Sprites";
    private const string PrefabFolder   = "Assets/Prefabs";
    private const string SoFolder       = "Assets/ScriptableObjects";
    private const string AudioFolder    = "Assets/Audio";
    private const string ResourcesFolder = "Assets/Resources";

    private const string SquareSpritePath = SpriteFolder + "/Square.png";
    private const string RegistryPath    = ResourcesFolder + "/InstrumentRegistry.asset";

    // ──────────────────────────────────────────────────────────
    // 메뉴
    // ──────────────────────────────────────────────────────────

    [MenuItem("BridgeRhythm/Generate All Assets", priority = 0)]
    public static void GenerateAll()
    {
        EnsureFolders();
        EnsureTagsAndLayers();
        EnsureSquareSprite();

        var instruments = EnsureAllInstruments();
        EnsureInstrumentRegistry(instruments);
        EnsureStages(instruments);

        EnsureBridgePiecePrefab();
        EnsurePlayerPrefab();
        EnsureInstrumentButtonPrefab();
        EnsureGoalPrefab();
        EnsureGroundPrefab();
        EnsureInstrumentPickupPrefab(instruments.Length > 1 ? instruments[1] : null);
        EnsureMovingObstaclePrefab();
        EnsureFallingObstaclePrefab();
        EnsureWindZonePrefab();
        EnsureSpikePrefab();
        EnsureStageTabPrefab();
        EnsureRecordEntryPrefab();
        EnsureBouncerPrefab();
        EnsureGravityZonePrefab();
        EnsureTimedGatePrefab();
        EnsureLaserPrefab();
        EnsureIceZonePrefab();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[AssetGenerator] 완료. 악기 {instruments.Length}종 + 스테이지 15개 + 프리팹 일괄 생성.");
    }

    [MenuItem("BridgeRhythm/Generate ScriptableObjects")]
    public static void GenerateSOOnly()
    {
        EnsureFolders();
        var inst = EnsureAllInstruments();
        EnsureInstrumentRegistry(inst);
        EnsureStages(inst);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("BridgeRhythm/Generate Prefabs")]
    public static void GeneratePrefabsOnly()
    {
        EnsureFolders();
        EnsureTagsAndLayers();
        EnsureSquareSprite();
        EnsureBridgePiecePrefab();
        EnsurePlayerPrefab();
        EnsureInstrumentButtonPrefab();
        EnsureGoalPrefab();
        EnsureGroundPrefab();
        EnsureMovingObstaclePrefab();
        EnsureFallingObstaclePrefab();
        EnsureWindZonePrefab();
        EnsureSpikePrefab();
        EnsureBouncerPrefab();
        EnsureGravityZonePrefab();
        EnsureTimedGatePrefab();
        EnsureLaserPrefab();
        EnsureIceZonePrefab();
        EnsureStageTabPrefab();
        EnsureRecordEntryPrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("BridgeRhythm/Regenerate Audio Clips")]
    public static void RegenerateAudio()
    {
        EnsureFolders();
        var instruments = EnsureAllInstruments();
        EnsureInstrumentRegistry(instruments);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // ──────────────────────────────────────────────────────────
    // 폴더 / 태그 / 레이어 / 스프라이트
    // ──────────────────────────────────────────────────────────

    private static void EnsureFolders()
    {
        CreateFolder("Assets", "Sprites");
        CreateFolder("Assets", "Prefabs");
        CreateFolder("Assets", "ScriptableObjects");
        CreateFolder("Assets", "Audio");
        CreateFolder("Assets", "Resources");
    }

    private static void CreateFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
            AssetDatabase.CreateFolder(parent, name);
    }

    private static void EnsureTagsAndLayers()
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        AddTag(tagManager, "Player");
        AddTag(tagManager, "Goal");
        AddTag(tagManager, "DeathZone");
        AddTag(tagManager, "Ground");
        AddLayer(tagManager, "Ground");
        tagManager.ApplyModifiedProperties();
    }

    private static void AddTag(SerializedObject tm, string tag)
    {
        var p = tm.FindProperty("tags");
        for (int i = 0; i < p.arraySize; i++)
            if (p.GetArrayElementAtIndex(i).stringValue == tag) return;
        p.InsertArrayElementAtIndex(p.arraySize);
        p.GetArrayElementAtIndex(p.arraySize - 1).stringValue = tag;
    }

    private static void AddLayer(SerializedObject tm, string layer)
    {
        var p = tm.FindProperty("layers");
        for (int i = 8; i < p.arraySize; i++)
        {
            var slot = p.GetArrayElementAtIndex(i);
            if (slot.stringValue == layer) return;
            if (string.IsNullOrEmpty(slot.stringValue)) { slot.stringValue = layer; return; }
        }
    }

    private static void EnsureSquareSprite()
    {
        if (!File.Exists(SquareSpritePath))
        {
            var tex = new Texture2D(32, 32);
            var px = new Color32[32 * 32];
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(px); tex.Apply();
            File.WriteAllBytes(SquareSpritePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(SquareSpritePath, ImportAssetOptions.ForceUpdate);
        }
        var importer = AssetImporter.GetAtPath(SquareSpritePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32f;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }
    }

    private static Sprite LoadSquareSprite() => AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);

    // ──────────────────────────────────────────────────────────
    // 악기 정의 (21종)
    // ──────────────────────────────────────────────────────────

    private struct InstrumentDef
    {
        public string id;
        public string displayName;
        public KeyCode key;
        public BridgeShapeType shape;
        public Color color;
        public ProceduralAudioGenerator.InstrumentVoice voice;
    }

    private static InstrumentDef[] GetInstrumentDefs() => new[]
    {
        new InstrumentDef {
            id = "piano", displayName = "Piano", key = KeyCode.Alpha1,
            shape = BridgeShapeType.Straight, color = new Color(0.95f, 0.95f, 0.95f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.Sine,
                duration = 0.6f, attack = 0.005f, decay = 2.5f, octaveShift = 0, noiseMix = 0 }
        },
        new InstrumentDef {
            id = "guitar", displayName = "Guitar", key = KeyCode.Alpha2,
            shape = BridgeShapeType.Curved, color = new Color(0.55f, 0.3f, 0.12f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.Pluck,
                duration = 0.8f, attack = 0.003f, decay = 2.0f, octaveShift = 0, noiseMix = 0.02f }
        },
        new InstrumentDef {
            id = "drum", displayName = "Drum", key = KeyCode.Alpha3,
            shape = BridgeShapeType.Zigzag, color = new Color(0.25f, 0.25f, 0.3f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.Noise,
                duration = 0.25f, attack = 0.001f, decay = 3.5f, octaveShift = -2f, noiseMix = 0.8f }
        },
        new InstrumentDef {
            id = "flute", displayName = "Flute", key = KeyCode.Alpha4,
            shape = BridgeShapeType.Straight, color = new Color(0.7f, 0.85f, 1f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.Breath,
                duration = 0.7f, attack = 0.08f, decay = 1.2f, octaveShift = 1f, noiseMix = 0.05f }
        },
        new InstrumentDef {
            id = "violin", displayName = "Violin", key = KeyCode.Alpha5,
            shape = BridgeShapeType.Curved, color = new Color(0.75f, 0.25f, 0.25f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.Sawtooth,
                duration = 0.9f, attack = 0.06f, decay = 1.5f, octaveShift = 0, noiseMix = 0.02f }
        },
        new InstrumentDef {
            id = "bass", displayName = "Bass", key = KeyCode.Alpha6,
            shape = BridgeShapeType.Stepped, color = new Color(0.3f, 0.2f, 0.5f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.Triangle,
                duration = 0.7f, attack = 0.01f, decay = 2.0f, octaveShift = -1f, noiseMix = 0 }
        },
        new InstrumentDef {
            id = "xylophone", displayName = "Xylophone", key = KeyCode.Alpha7,
            shape = BridgeShapeType.Zigzag, color = new Color(1f, 0.85f, 0.2f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.Square,
                duration = 0.4f, attack = 0.002f, decay = 3.0f, octaveShift = 1f, noiseMix = 0 }
        },
        new InstrumentDef {
            id = "bell", displayName = "Bell", key = KeyCode.Alpha8,
            shape = BridgeShapeType.Straight, color = new Color(1f, 0.75f, 0.3f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.Bell,
                duration = 1.2f, attack = 0.005f, decay = 1.8f, octaveShift = 1f, noiseMix = 0 }
        },
        new InstrumentDef {
            id = "trumpet", displayName = "Trumpet", key = KeyCode.Alpha9,
            shape = BridgeShapeType.Bouncy, color = new Color(0.9f, 0.7f, 0.1f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.Brass,
                duration = 0.6f, attack = 0.02f, decay = 2.0f, octaveShift = 0, noiseMix = 0.01f }
        },
        new InstrumentDef {
            id = "saxophone", displayName = "Saxophone", key = KeyCode.Alpha0,
            shape = BridgeShapeType.Curved, color = new Color(0.85f, 0.65f, 0.15f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.Sawtooth,
                duration = 0.8f, attack = 0.04f, decay = 1.8f, octaveShift = 0, noiseMix = 0.04f }
        },
        new InstrumentDef {
            id = "harp", displayName = "Harp", key = KeyCode.Q,
            shape = BridgeShapeType.Slippery, color = new Color(0.85f, 0.75f, 0.95f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.StringPad,
                duration = 1.0f, attack = 0.01f, decay = 1.5f, octaveShift = 1f, noiseMix = 0 }
        },
        new InstrumentDef {
            id = "organ", displayName = "Organ", key = KeyCode.W,
            shape = BridgeShapeType.Wide, color = new Color(0.4f, 0.25f, 0.15f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.Organ,
                duration = 1.0f, attack = 0.02f, decay = 1.0f, octaveShift = 0, noiseMix = 0 }
        },
        new InstrumentDef {
            id = "synth", displayName = "Synth", key = KeyCode.E,
            shape = BridgeShapeType.Bouncy, color = new Color(0f, 0.9f, 0.9f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.SynthPad,
                duration = 0.7f, attack = 0.01f, decay = 2.0f, octaveShift = 0, noiseMix = 0.01f }
        },
        new InstrumentDef {
            id = "marimba", displayName = "Marimba", key = KeyCode.R,
            shape = BridgeShapeType.Zigzag, color = new Color(0.7f, 0.45f, 0.2f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.Marimba,
                duration = 0.5f, attack = 0.003f, decay = 2.8f, octaveShift = 0, noiseMix = 0 }
        },
        new InstrumentDef {
            id = "clarinet", displayName = "Clarinet", key = KeyCode.T,
            shape = BridgeShapeType.Straight, color = new Color(0.15f, 0.15f, 0.15f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.Square,
                duration = 0.7f, attack = 0.05f, decay = 1.5f, octaveShift = 0, noiseMix = 0.03f }
        },
        new InstrumentDef {
            id = "harmonica", displayName = "Harmonica", key = KeyCode.Y,
            shape = BridgeShapeType.Slippery, color = new Color(0.6f, 0.6f, 0.65f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.Harmonica,
                duration = 0.6f, attack = 0.03f, decay = 1.8f, octaveShift = 0, noiseMix = 0.06f }
        },
        new InstrumentDef {
            id = "banjo", displayName = "Banjo", key = KeyCode.U,
            shape = BridgeShapeType.Curved, color = new Color(0.8f, 0.65f, 0.3f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.Sitar,
                duration = 0.5f, attack = 0.002f, decay = 2.5f, octaveShift = 0, noiseMix = 0.03f }
        },
        new InstrumentDef {
            id = "cello", displayName = "Cello", key = KeyCode.I,
            shape = BridgeShapeType.Curved, color = new Color(0.5f, 0.2f, 0.1f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.StringPad,
                duration = 1.0f, attack = 0.1f, decay = 1.2f, octaveShift = -1f, noiseMix = 0.02f }
        },
        new InstrumentDef {
            id = "tuba", displayName = "Tuba", key = KeyCode.O,
            shape = BridgeShapeType.Stepped, color = new Color(0.6f, 0.5f, 0.2f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.Brass,
                duration = 0.8f, attack = 0.03f, decay = 1.5f, octaveShift = -1f, noiseMix = 0.01f }
        },
        new InstrumentDef {
            id = "accordion", displayName = "Accordion", key = KeyCode.P,
            shape = BridgeShapeType.Wide, color = new Color(0.8f, 0.15f, 0.15f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.Accordion,
                duration = 0.8f, attack = 0.04f, decay = 1.2f, octaveShift = 0, noiseMix = 0.02f }
        },
        new InstrumentDef {
            id = "kalimba", displayName = "Kalimba", key = KeyCode.F,
            shape = BridgeShapeType.Straight, color = new Color(0.6f, 0.85f, 0.5f),
            voice = new ProceduralAudioGenerator.InstrumentVoice {
                waveform = ProceduralAudioGenerator.Waveform.Kalimba,
                duration = 0.8f, attack = 0.003f, decay = 2.2f, octaveShift = 1f, noiseMix = 0 }
        },
    };

    private static InstrumentData[] EnsureAllInstruments()
    {
        var defs = GetInstrumentDefs();
        var result = new InstrumentData[defs.Length];
        for (int i = 0; i < defs.Length; i++)
            result[i] = EnsureInstrument(defs[i]);
        return result;
    }

    private static InstrumentData EnsureInstrument(InstrumentDef d)
    {
        string path = $"{SoFolder}/{d.displayName}.asset";
        string audioFolder = $"{AudioFolder}/{d.id}";

        var clips = ProceduralAudioGenerator.EnsureNoteClips(d.id, d.voice, audioFolder);

        var asset = AssetDatabase.LoadAssetAtPath<InstrumentData>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<InstrumentData>();
            AssetDatabase.CreateAsset(asset, path);
        }
        asset.instrumentName = d.displayName;
        asset.activationKey  = d.key;
        asset.shapeType      = d.shape;
        asset.bridgeWidth    = 2f;
        asset.bridgeHeight   = 0.3f;
        asset.bridgeColor    = d.color;
        asset.bridgeSprite   = LoadSquareSprite();
        asset.noteClips      = clips;
        asset.volume         = 0.8f;
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static void EnsureInstrumentRegistry(InstrumentData[] instruments)
    {
        var registry = AssetDatabase.LoadAssetAtPath<InstrumentRegistry>(RegistryPath);
        if (registry == null)
        {
            registry = ScriptableObject.CreateInstance<InstrumentRegistry>();
            AssetDatabase.CreateAsset(registry, RegistryPath);
        }
        registry.instruments = instruments;
        EditorUtility.SetDirty(registry);
    }

    // ──────────────────────────────────────────────────────────
    // 스테이지 (15개) — stageIndex 0-based로 통일
    // ──────────────────────────────────────────────────────────

    private static void EnsureStages(InstrumentData[] instruments)
    {
        InstrumentData Inst(string n)
        {
            foreach (var i in instruments) if (i.instrumentName == n) return i;
            return null;
        }
        var piano     = Inst("Piano");
        var guitar    = Inst("Guitar");
        var drum      = Inst("Drum");
        var flute     = Inst("Flute");
        var violin    = Inst("Violin");
        var bass      = Inst("Bass");
        var xylo      = Inst("Xylophone");
        var bell      = Inst("Bell");
        var trumpet   = Inst("Trumpet");
        var sax       = Inst("Saxophone");
        var harp      = Inst("Harp");
        var organ     = Inst("Organ");
        var synth     = Inst("Synth");
        var marimba   = Inst("Marimba");
        var clarinet  = Inst("Clarinet");
        var harmonica = Inst("Harmonica");
        var banjo     = Inst("Banjo");
        var cello     = Inst("Cello");
        var tuba      = Inst("Tuba");
        var accordion = Inst("Accordion");
        var kalimba   = Inst("Kalimba");

        var stages = new List<StageData>();

        // stageIndex = 0-based (0~14). 파일명 편의상 1-based 유지.
        stages.Add(Stage(1,  "Tutorial - First Note", "피아노 하나로 간단한 다리를 만들어 건너보세요.",
            3f,   20f, 30, 5,  12, new[] { Pickup(guitar, 0, -1.5f) }, new ObstaclePlacement[0]));

        stages.Add(Stage(2,  "Strings Attached", "기타의 곡선 다리를 활용해 구덩이를 건너세요.",
            3.2f, 22f, 30, 7,  15,
            new[] { Pickup(drum, 3, -1.5f) },
            new[] { Obstacle(ObstacleKind.Moving, new Vector2(0, -1.5f), Vector2.one, Vector2.right, 2f, 2f) }));

        stages.Add(Stage(3,  "Wind of Change", "바람이 부는 구간을 피해 다리를 놓으세요.",
            3.5f, 24f, 32, 10, 18,
            new[] { Pickup(flute, -2, -1.5f) },
            new[] { Wind(new Vector2(0, 0), new Vector2(4, 3), new Vector2(4f, 0)) }));

        stages.Add(Stage(4,  "Crumbling Path", "밟으면 무너지는 발판 위를 재빨리 건너세요.",
            3.8f, 26f, 34, 12, 20,
            new[] { Pickup(violin, 2, -0.5f) },
            new[] {
                Obstacle(ObstacleKind.Falling, new Vector2(-3, -1.5f), Vector2.one),
                Obstacle(ObstacleKind.Falling, new Vector2(3,  -1.5f), Vector2.one),
            }));

        stages.Add(Stage(5,  "Spiked Rhythm", "날카로운 가시 위로 다리를 지어야 합니다.",
            4f,   28f, 36, 14, 22,
            new[] { Pickup(bass, 0, -1f), Pickup(trumpet, -5, 0f) },
            new[] { Spike(new Vector2(-4, -2f)), Spike(new Vector2(0, -2f)), Spike(new Vector2(4, -2f)) }));

        stages.Add(Stage(6,  "Bounce House", "트램폴린을 활용해 높은 곳으로 점프하세요!",
            4f,   26f, 36, 14, 22,
            new[] { Pickup(sax, -2, 1.5f) },
            new[] {
                Bouncer(new Vector2(-3, -1.8f), 18f),
                Bouncer(new Vector2( 3, -1.8f), 18f),
                Spike(new Vector2(0, -2f)),
            }));

        stages.Add(Stage(7,  "Moving Melody", "좌우로 움직이는 장애물을 피해 다리를 놓으세요.",
            4.2f, 28f, 38, 16, 24,
            new[] { Pickup(xylo, -3, 1f), Pickup(harp, 4, 0f) },
            new[] {
                Obstacle(ObstacleKind.Moving, new Vector2(-3, 0.5f), Vector2.one, Vector2.up, 2f, 2.5f),
                Obstacle(ObstacleKind.Moving, new Vector2( 3, 0.5f), Vector2.one, Vector2.up, 2f, 2.5f),
            }));

        stages.Add(Stage(8,  "Gravity Shift", "중력이 뒤집히는 영역을 조심하세요!",
            4.3f, 30f, 38, 16, 24,
            new[] { Pickup(organ, 0, 1f), Pickup(bell, -4, 0.5f) },
            new[] {
                GravityFlip(new Vector2(-2, 0), new Vector2(4, 4)),
                Spike(new Vector2(4, -2f)),
                Spike(new Vector2(4,  3f)),
            }));

        stages.Add(Stage(9,  "Ticking Gates", "주기적으로 열리고 닫히는 문을 타이밍에 맞춰 통과하세요.",
            4.5f, 30f, 40, 18, 26,
            new[] { Pickup(synth, 2, 1f) },
            new[] {
                TimedGate(new Vector2(-4, 0), new Vector2(0.5f, 3f), 2f, 2f),
                TimedGate(new Vector2( 0, 0), new Vector2(0.5f, 3f), 2f, 2f),
                TimedGate(new Vector2( 4, 0), new Vector2(0.5f, 3f), 2f, 2f),
            }));

        stages.Add(Stage(10, "Laser Symphony", "레이저 빔 사이로 안전한 다리를 지으세요.",
            4.5f, 32f, 40, 18, 26,
            new[] { Pickup(marimba, -3, 0f), Pickup(clarinet, 3, 0f) },
            new[] {
                Laser(new Vector2(-5, 0.5f), 1.5f, 2f, 6f),
                Laser(new Vector2( 0,-0.5f), 1.5f, 2f, 6f),
                Laser(new Vector2( 5, 0.5f), 1.5f, 2f, 6f),
            }));

        stages.Add(Stage(11, "Frozen Melody", "미끄러운 얼음 위에서 균형을 잡으며 건너세요.",
            4.8f, 32f, 40, 18, 26,
            new[] { Pickup(harmonica, -2, 0.5f), Pickup(banjo, 4, -1f) },
            new[] {
                Ice(new Vector2(-3, -1.8f), new Vector2(6, 0.5f)),
                Ice(new Vector2( 3, -1.8f), new Vector2(6, 0.5f)),
                Wind(new Vector2(0, 0), new Vector2(3, 3), new Vector2(3f, 0)),
            }));

        stages.Add(Stage(12, "Symphony of Traps", "바운서 + 레이저 + 바람. 모든 기믹이 합쳐집니다.",
            5f,   34f, 42, 20, 28,
            new[] { Pickup(cello, 0, 1.5f) },
            new[] {
                Bouncer(new Vector2(-5, -1.8f), 16f),
                Laser(new Vector2(-2, 0.5f), 1f, 2.5f, 5f),
                Wind(new Vector2(2, 0), new Vector2(3, 3), new Vector2(5f, 0)),
                Spike(new Vector2(5, -2f)),
            }));

        stages.Add(Stage(13, "Gravity Maze", "중력 반전 + 시한문 + 빙판. 미로를 돌파하세요.",
            5f,   34f, 42, 20, 28,
            new[] { Pickup(tuba, -4, 0f), Pickup(accordion, 4, 0f) },
            new[] {
                GravityFlip(new Vector2(-3, 0), new Vector2(3, 4)),
                TimedGate(new Vector2(0, 0), new Vector2(0.5f, 3f), 1.5f, 1.5f),
                Ice(new Vector2(3, -1.8f), new Vector2(4, 0.5f)),
                Obstacle(ObstacleKind.Falling, new Vector2(5, -1.5f), Vector2.one),
            }));

        stages.Add(Stage(14, "Tight Tempo", "좁은 공간, 빠른 판정선. 집중력 총동원!",
            5.5f, 34f, 42, 22, 30,
            new[] { Pickup(kalimba, 0, 1f) },
            new[] {
                Obstacle(ObstacleKind.Moving, new Vector2(-4, -0.5f), Vector2.one, Vector2.right, 3f, 3.5f),
                Laser(new Vector2(-1, 0.5f), 1f, 1.5f, 5f),
                TimedGate(new Vector2(2, 0), new Vector2(0.5f, 3f), 1.5f, 1.5f),
                Obstacle(ObstacleKind.Moving, new Vector2(5, -0.5f), Vector2.one, Vector2.up, 2f, 3f),
                Spike(new Vector2(0, -2f)),
            }));

        stages.Add(Stage(15, "Grand Finale", "21가지 악기로 당신만의 피날레를 완성하세요!",
            5.5f, 38f, 48, 24, 34,
            new PickupPlacement[0],
            new[] {
                Bouncer(new Vector2(-8, -1.8f), 20f),
                GravityFlip(new Vector2(-5, 0), new Vector2(3, 3)),
                Laser(new Vector2(-2, 0.5f), 1f, 2f, 5f),
                Obstacle(ObstacleKind.Falling, new Vector2(0, -1.5f), Vector2.one),
                TimedGate(new Vector2(2, 0), new Vector2(0.5f, 3f), 1.5f, 1.5f),
                Wind(new Vector2(4, 0), new Vector2(3, 3), new Vector2(-5f, 0)),
                Ice(new Vector2(6, -1.8f), new Vector2(4, 0.5f)),
                Spike(new Vector2(8, -2f)),
                Obstacle(ObstacleKind.Moving, new Vector2(9, -0.5f), Vector2.one, Vector2.up, 2f, 3f),
            }));

        foreach (var s in stages) EditorUtility.SetDirty(s);
    }

    // ── 장애물 헬퍼 ────────────────────────────────────────────

    private static ObstaclePlacement Bouncer(Vector2 pos, float force)
        => new ObstaclePlacement { kind = ObstacleKind.Bouncer, position = pos, size = new Vector2(1.5f, 0.3f), bounceForce = force };

    private static ObstaclePlacement GravityFlip(Vector2 pos, Vector2 size)
        => new ObstaclePlacement { kind = ObstacleKind.GravityFlip, position = pos, size = size };

    private static ObstaclePlacement TimedGate(Vector2 pos, Vector2 size, float openTime, float closeTime)
        => new ObstaclePlacement { kind = ObstacleKind.TimedGate, position = pos, size = size, gateOpenTime = openTime, gateCloseTime = closeTime };

    private static ObstaclePlacement Laser(Vector2 pos, float onTime, float offTime, float length)
        => new ObstaclePlacement { kind = ObstacleKind.Laser, position = pos, size = new Vector2(length, 0.15f), laserOnTime = onTime, laserOffTime = offTime, laserLength = length };

    private static ObstaclePlacement Ice(Vector2 pos, Vector2 size)
        => new ObstaclePlacement { kind = ObstacleKind.Ice, position = pos, size = size };

    private static StageData Stage(int fileIdx, string name, string desc,
        float speed, float width, int maxPieces, int perfect, int good,
        PickupPlacement[] pickups, ObstaclePlacement[] obstacles)
    {
        string path = $"{SoFolder}/Stage{fileIdx}.asset";
        var s = AssetDatabase.LoadAssetAtPath<StageData>(path);
        if (s == null)
        {
            s = ScriptableObject.CreateInstance<StageData>();
            AssetDatabase.CreateAsset(s, path);
        }
        // [FIX] stageIndex를 0-based로 저장 (GameManager.CurrentStageIndex와 일치)
        s.stageIndex      = fileIdx - 1;
        s.stageName       = name;
        s.description     = desc;
        s.rhythmLineSpeed = speed;
        s.stageWidth      = width;
        s.maxBridgePieces = maxPieces;
        s.perfectThreshold = perfect;
        s.goodThreshold   = good;
        s.startGroundX    = -width / 2f - 1f;
        s.endGroundX      = width  / 2f + 1f;
        s.groundY         = -2f;
        s.goalY           = 0.5f;
        s.gravityScale    = 1f;
        s.pickups         = new List<PickupPlacement>(pickups);
        s.obstacles       = new List<ObstaclePlacement>(obstacles);
        return s;
    }

    private static PickupPlacement Pickup(InstrumentData inst, float x, float y)
        => new PickupPlacement { instrument = inst, position = new Vector2(x, y) };

    private static ObstaclePlacement Obstacle(ObstacleKind kind, Vector2 pos, Vector2 size)
        => new ObstaclePlacement { kind = kind, position = pos, size = size };

    private static ObstaclePlacement Obstacle(ObstacleKind kind, Vector2 pos, Vector2 size,
        Vector2 dir, float dist, float speed)
        => new ObstaclePlacement { kind = kind, position = pos, size = size,
            moveDirection = dir, moveDistance = dist, moveSpeed = speed };

    private static ObstaclePlacement Wind(Vector2 pos, Vector2 size, Vector2 force)
        => new ObstaclePlacement { kind = ObstacleKind.Wind, position = pos, size = size, windForce = force };

    private static ObstaclePlacement Spike(Vector2 pos)
        => new ObstaclePlacement { kind = ObstacleKind.Spike, position = pos, size = new Vector2(0.8f, 0.4f) };

    // ──────────────────────────────────────────────────────────
    // 프리팹들
    // ──────────────────────────────────────────────────────────

    private static void EnsureBridgePiecePrefab()
    {
        var go = new GameObject("BridgePiece");
        // [FIX] 다리 조각을 Ground 레이어로 설정 → 플레이어 groundCheck가 인식함
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer >= 0) go.layer = groundLayer;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSquareSprite();
        sr.sortingOrder = 1;
        go.AddComponent<BoxCollider2D>();
        go.AddComponent<BridgePiece>();
        PrefabUtility.SaveAsPrefabAsset(go, PrefabFolder + "/BridgePiece.prefab");
        Object.DestroyImmediate(go);
    }

    private static void EnsurePlayerPrefab()
    {
        var go = new GameObject("Player");
        go.tag = "Player";
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSquareSprite();
        sr.color = new Color(0.25f, 0.7f, 1f);
        sr.sortingOrder = 5;
        go.transform.localScale = new Vector3(0.8f, 1.2f, 1f);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        // [FIX] 기본 drag 설정 — IceZone 종료 시 0.5f로 복원하는 쌍과 맞춤
        rb.drag = 0.5f;
        go.AddComponent<BoxCollider2D>();

        var gc = new GameObject("GroundCheck");
        gc.transform.SetParent(go.transform, false);
        gc.transform.localPosition = new Vector3(0, -0.5f, 0);

        var ctrl = go.AddComponent<PlayerController>();
        int groundLayer = LayerMask.NameToLayer("Ground");
        int mask = groundLayer >= 0 ? (1 << groundLayer) : 0;
        var so = new SerializedObject(ctrl);
        so.FindProperty("groundCheck").objectReferenceValue = gc.transform;
        so.FindProperty("groundLayer").intValue = mask;
        so.FindProperty("groundCheckRadius").floatValue = 0.15f;
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(go, PrefabFolder + "/Player.prefab");
        Object.DestroyImmediate(go);
    }

    private static void EnsureInstrumentButtonPrefab()
    {
        var root = new GameObject("InstrumentSidebarButton", typeof(RectTransform));
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 70);
        var bg = root.AddComponent<Image>();
        bg.color = new Color(0.9f, 0.9f, 0.9f, 0.85f);
        root.AddComponent<Button>();

        var icon = CreateUIImage(root, "Icon", new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(0, 0.5f), new Vector2(10, 0), new Vector2(50, 50));
        icon.color = Color.white;
        icon.preserveAspect = true;

        var nameTmp = CreateUIText(root, "NameLabel", "Piano", 22, new Vector2(0, 0.5f), new Vector2(1, 1),
            new Vector2(70, 0), new Vector2(-5, -2));
        nameTmp.alignment = TextAlignmentOptions.Left;

        var keyTmp = CreateUIText(root, "KeyLabel", "[Alpha1]", 16, new Vector2(0, 0), new Vector2(1, 0.5f),
            new Vector2(70, 2), new Vector2(-5, 0));
        keyTmp.alignment = TextAlignmentOptions.Left;

        var sidebar = root.AddComponent<InstrumentSidebarButton>();
        var so = new SerializedObject(sidebar);
        so.FindProperty("backgroundImage").objectReferenceValue = bg;
        so.FindProperty("iconImage").objectReferenceValue = icon;
        so.FindProperty("nameLabel").objectReferenceValue = nameTmp;
        so.FindProperty("keyLabel").objectReferenceValue = keyTmp;
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(root, PrefabFolder + "/InstrumentSidebarButton.prefab");
        Object.DestroyImmediate(root);
    }

    private static void EnsureGoalPrefab()
    {
        var go = new GameObject("Goal"); go.tag = "Goal";
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSquareSprite();
        sr.color = new Color(0.2f, 1f, 0.3f, 0.8f);
        sr.sortingOrder = 2;
        go.transform.localScale = new Vector3(1f, 2.5f, 1f);
        var c = go.AddComponent<BoxCollider2D>(); c.isTrigger = true;
        go.AddComponent<GoalTrigger>();
        PrefabUtility.SaveAsPrefabAsset(go, PrefabFolder + "/Goal.prefab");
        Object.DestroyImmediate(go);
    }

    private static void EnsureGroundPrefab()
    {
        var go = new GameObject("Ground");
        int gl = LayerMask.NameToLayer("Ground");
        if (gl >= 0) go.layer = gl;
        go.tag = "Ground";
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSquareSprite();
        sr.color = new Color(0.35f, 0.25f, 0.15f);
        go.transform.localScale = new Vector3(5f, 1f, 1f);
        go.AddComponent<BoxCollider2D>();
        PrefabUtility.SaveAsPrefabAsset(go, PrefabFolder + "/Ground.prefab");
        Object.DestroyImmediate(go);
    }

    private static void EnsureInstrumentPickupPrefab(InstrumentData defaultInst)
    {
        var go = new GameObject("InstrumentPickup");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSquareSprite();
        sr.color = new Color(1f, 0.85f, 0.2f);
        sr.sortingOrder = 3;
        go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        var c = go.AddComponent<BoxCollider2D>(); c.isTrigger = true;
        var pickup = go.AddComponent<InstrumentPickup>();
        if (defaultInst != null)
        {
            var so = new SerializedObject(pickup);
            so.FindProperty("instrumentData").objectReferenceValue = defaultInst;
            so.ApplyModifiedProperties();
        }
        PrefabUtility.SaveAsPrefabAsset(go, PrefabFolder + "/InstrumentPickup.prefab");
        Object.DestroyImmediate(go);
    }

    private static void EnsureMovingObstaclePrefab()
    {
        var go = new GameObject("MovingObstacle");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSquareSprite();
        sr.color = new Color(0.7f, 0.2f, 0.2f);
        sr.sortingOrder = 2;
        var c = go.AddComponent<BoxCollider2D>(); c.isTrigger = true;
        go.AddComponent<MovingObstacle>();
        PrefabUtility.SaveAsPrefabAsset(go, PrefabFolder + "/MovingObstacle.prefab");
        Object.DestroyImmediate(go);
    }

    private static void EnsureFallingObstaclePrefab()
    {
        var go = new GameObject("FallingObstacle");
        int gl = LayerMask.NameToLayer("Ground"); if (gl >= 0) go.layer = gl;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSquareSprite();
        sr.color = new Color(0.5f, 0.3f, 0.15f);
        sr.sortingOrder = 2;
        // 물리 충돌용 콜라이더 (Ground 레이어로 플레이어가 위에 설 수 있음)
        go.AddComponent<BoxCollider2D>();
        // 트리거: 플레이어가 위에 올라가는 것을 감지
        var triggerCol = go.AddComponent<BoxCollider2D>();
        triggerCol.isTrigger = true;
        triggerCol.size = new Vector2(1f, 2f);
        triggerCol.offset = new Vector2(0, 1f);
        // [FIX] Rigidbody2D를 Kinematic으로 시작 — FallingObstacle.cs의 Awake와 일치
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        go.AddComponent<FallingObstacle>();
        PrefabUtility.SaveAsPrefabAsset(go, PrefabFolder + "/FallingObstacle.prefab");
        Object.DestroyImmediate(go);
    }

    private static void EnsureWindZonePrefab()
    {
        var go = new GameObject("WindZone");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSquareSprite();
        sr.color = new Color(0.5f, 0.85f, 1f, 0.3f);
        sr.sortingOrder = 2;
        var c = go.AddComponent<BoxCollider2D>(); c.isTrigger = true;
        go.AddComponent<WindZoneObstacle>();
        PrefabUtility.SaveAsPrefabAsset(go, PrefabFolder + "/WindZone.prefab");
        Object.DestroyImmediate(go);
    }

    private static void EnsureSpikePrefab()
    {
        var go = new GameObject("Spike");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSquareSprite();
        sr.color = new Color(0.7f, 0.1f, 0.1f);
        sr.sortingOrder = 2;
        var c = go.AddComponent<BoxCollider2D>(); c.isTrigger = true;
        go.AddComponent<SpikeObstacle>();
        PrefabUtility.SaveAsPrefabAsset(go, PrefabFolder + "/Spike.prefab");
        Object.DestroyImmediate(go);
    }

    private static void EnsureStageTabPrefab()
    {
        var go = new GameObject("StageTab", typeof(RectTransform));
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 50);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.9f, 0.9f, 0.9f, 0.9f);
        go.AddComponent<Button>();
        CreateUIText(go, "Label", "Stage 1", 22, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        PrefabUtility.SaveAsPrefabAsset(go, PrefabFolder + "/StageTab.prefab");
        Object.DestroyImmediate(go);
    }

    private static void EnsureRecordEntryPrefab()
    {
        var go = new GameObject("RecordEntry", typeof(RectTransform));
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 80);
        var img = go.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.85f);
        go.AddComponent<Button>();
        CreateUIText(go, "Label", "record", 20, Vector2.zero, Vector2.one, new Vector2(10, 5), new Vector2(-10, -5));
        PrefabUtility.SaveAsPrefabAsset(go, PrefabFolder + "/RecordEntry.prefab");
        Object.DestroyImmediate(go);
    }

    private static void EnsureBouncerPrefab()
    {
        var go = new GameObject("Bouncer");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSquareSprite();
        sr.color = new Color(0.2f, 0.9f, 0.3f);
        sr.sortingOrder = 2;
        go.transform.localScale = new Vector3(1.5f, 0.3f, 1f);
        // [NOTE] BouncerObstacle은 OnCollisionEnter2D + OnTriggerEnter2D 둘 다 처리하므로
        // 일반 콜라이더(물리 충돌)와 트리거(진입 감지) 모두 필요
        var normalCol = go.AddComponent<BoxCollider2D>();
        var triggerCol = go.AddComponent<BoxCollider2D>();
        triggerCol.isTrigger = true;
        go.AddComponent<BouncerObstacle>();
        PrefabUtility.SaveAsPrefabAsset(go, PrefabFolder + "/Bouncer.prefab");
        Object.DestroyImmediate(go);
    }

    private static void EnsureGravityZonePrefab()
    {
        var go = new GameObject("GravityZone");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSquareSprite();
        sr.color = new Color(0.6f, 0.2f, 0.8f, 0.3f);
        sr.sortingOrder = 2;
        var c = go.AddComponent<BoxCollider2D>(); c.isTrigger = true;
        go.AddComponent<GravityZoneObstacle>();
        PrefabUtility.SaveAsPrefabAsset(go, PrefabFolder + "/GravityZone.prefab");
        Object.DestroyImmediate(go);
    }

    private static void EnsureTimedGatePrefab()
    {
        var go = new GameObject("TimedGate");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSquareSprite();
        sr.color = new Color(0.4f, 0.4f, 0.5f);
        sr.sortingOrder = 2;
        // TimedGate는 isTrigger=false 일반 콜라이더로 물리 막음
        go.AddComponent<BoxCollider2D>();
        go.AddComponent<TimedGateObstacle>();
        PrefabUtility.SaveAsPrefabAsset(go, PrefabFolder + "/TimedGate.prefab");
        Object.DestroyImmediate(go);
    }

    private static void EnsureLaserPrefab()
    {
        var go = new GameObject("Laser");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSquareSprite();
        sr.color = new Color(1f, 0.1f, 0.1f, 0.8f);
        sr.sortingOrder = 3;
        var c = go.AddComponent<BoxCollider2D>(); c.isTrigger = true;
        go.AddComponent<LaserObstacle>();
        PrefabUtility.SaveAsPrefabAsset(go, PrefabFolder + "/Laser.prefab");
        Object.DestroyImmediate(go);
    }

    private static void EnsureIceZonePrefab()
    {
        var go = new GameObject("IceZone");
        int gl = LayerMask.NameToLayer("Ground"); if (gl >= 0) go.layer = gl;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSquareSprite();
        sr.color = new Color(0.7f, 0.9f, 1f, 0.5f);
        sr.sortingOrder = 2;
        // 물리 콜라이더: 플레이어가 위에 설 수 있음
        go.AddComponent<BoxCollider2D>();
        // 트리거: 진입/퇴장 감지
        var tc = go.AddComponent<BoxCollider2D>();
        tc.isTrigger = true;
        tc.size = new Vector2(1f, 2f);
        tc.offset = new Vector2(0, 1f);
        go.AddComponent<IceZoneObstacle>();
        PrefabUtility.SaveAsPrefabAsset(go, PrefabFolder + "/IceZone.prefab");
        Object.DestroyImmediate(go);
    }

    // ──────────────────────────────────────────────────────────
    // UI 헬퍼
    // ──────────────────────────────────────────────────────────

    private static Image CreateUIImage(GameObject parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos; rt.sizeDelta = size;
        return go.AddComponent<Image>();
    }

    private static TextMeshProUGUI CreateUIText(GameObject parent, string name, string text, int fontSize,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;
        return tmp;
    }
}
