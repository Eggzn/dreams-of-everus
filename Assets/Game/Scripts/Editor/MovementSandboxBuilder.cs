using System.IO;
using System.Linq;
using DreamsOfEverus.Game;
using DreamsOfEverus.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DreamsOfEverus.EditorTools
{
    /// <summary>
    /// Menu: Dreams of Everus > Build Movement Sandbox.
    /// Creates the Milestone 1 graybox scene from scratch. The room layout is a PLACEHOLDER:
    /// after building, move and resize the cubes freely in the Scene view.
    /// Rebuilding replaces the scene but keeps the character data asset and its values.
    /// </summary>
    public static class MovementSandboxBuilder
    {
        private const string ScenePath = "Assets/Game/Scenes/MovementSandbox.unity";
        private const string CharacterDataPath = "Assets/Game/Data/BunCharacterData.asset";
        private const string ControlsPath = "Assets/Game/Input/GameplayControls.inputactions";
        private const string MaterialFolder = "Assets/Game/Art/Graybox";

        [MenuItem("Dreams of Everus/Build Movement Sandbox")]
        private static void BuildFromMenu()
        {
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog(
                    "Rebuild Movement Sandbox?",
                    "This replaces the MovementSandbox scene, so any changes you made to the room will be lost.\n\nBun's character data is kept.",
                    "Rebuild", "Cancel"))
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Build();
        }

        /// <summary>Builds without asking. Also used from the command line (-executeMethod).</summary>
        public static void Build()
        {
            // Make the new scene first: opening a scene unloads assets nothing uses yet,
            // which would empty any references to assets loaded before it.
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            EnsureFolder("Assets/Game/Scenes");
            EnsureFolder("Assets/Game/Data");
            EnsureFolder(MaterialFolder);

            CharacterData bunData = LoadOrCreateCharacterData();
            var controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);

            Material solid = LoadOrCreateMaterial("Graybox_Solid", new Color(0.55f, 0.55f, 0.58f));
            Material passThrough = LoadOrCreateMaterial("Graybox_PassThrough", new Color(0.35f, 0.6f, 0.9f));
            Material fallZone = LoadOrCreateMaterial("Graybox_FallZone", new Color(0.6f, 0.12f, 0.12f));
            Material bunMaterial = LoadOrCreateMaterial("Placeholder_Bun", new Color(1f, 0.72f, 0.3f));

            if (controls == null) Debug.LogError($"Movement Sandbox: controls not found at {ControlsPath}");

            // ----- Room (PLACEHOLDER layout, in world units) -----
            var level = new GameObject("Level").transform;

            // Floor in two pieces with a 4-unit gap between x = -1 and x = 3. Floor top is y = 0.
            CreateBox("Floor Left", level, new Vector2(-7.5f, -0.5f), new Vector2(13f, 1f), LevelBlockKind.Solid, solid);
            CreateBox("Floor Right", level, new Vector2(8.5f, -0.5f), new Vector2(11f, 1f), LevelBlockKind.Solid, solid);

            CreateBox("Wall Left", level, new Vector2(-14.5f, 6f), new Vector2(1f, 14f), LevelBlockKind.Solid, solid);
            CreateBox("Wall Right", level, new Vector2(14.5f, 6f), new Vector2(1f, 14f), LevelBlockKind.Solid, solid);

            CreateBox("Platform Low (Solid)", level, new Vector2(-9f, 2.25f), new Vector2(4f, 0.5f), LevelBlockKind.Solid, solid);
            CreateBox("Platform Mid (Pass Through)", level, new Vector2(-3f, 4f), new Vector2(4f, 0.3f), LevelBlockKind.PassThrough, passThrough);
            CreateBox("Platform Mid (Pass Through) 2", level, new Vector2(5f, 3f), new Vector2(4f, 0.3f), LevelBlockKind.PassThrough, passThrough);
            CreateBox("Platform High (Solid)", level, new Vector2(10f, 6.25f), new Vector2(4f, 0.5f), LevelBlockKind.Solid, solid);

            // Below the whole room, so anything that drops through the gap lands in it.
            CreateBox("Fall Zone", level, new Vector2(0f, -9f), new Vector2(32f, 4f), LevelBlockKind.FallZone, fallZone);

            var respawn = new GameObject("Respawn Point", typeof(RespawnPoint));
            respawn.transform.position = new Vector3(-6f, 0f, 0f);

            // ----- Simulation -----
            var simulationObject = new GameObject("Simulation", typeof(SimulationRunner), typeof(PlayerInputReader));
            var runner = simulationObject.GetComponent<SimulationRunner>();
            SetField(runner, "playerCharacter", bunData);
            SetField(simulationObject.GetComponent<PlayerInputReader>(), "controls", controls);

            // ----- Bun (placeholder capsule) -----
            var bun = new GameObject("Bun", typeof(FighterView));
            bun.transform.position = respawn.transform.position;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body (Placeholder)";
            Object.DestroyImmediate(body.GetComponent<Collider>()); // no Unity physics in gameplay
            body.GetComponent<Renderer>().sharedMaterial = bunMaterial;
            body.transform.SetParent(bun.transform, false);
            body.transform.localScale = new Vector3(bunData.bodyWidth, bunData.bodyHeight * 0.5f, bunData.bodyWidth);
            body.transform.localPosition = new Vector3(0f, bunData.bodyHeight * 0.5f, 0f);

            var view = bun.GetComponent<FighterView>();
            SetField(view, "runner", runner);
            SetField(view, "body", body.transform);

            // ----- Camera: fixed overview for now; the follow camera comes in a later step -----
            GameObject cameraObject = GameObject.FindWithTag("MainCamera");
            if (cameraObject != null)
            {
                cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 4f, -24f), Quaternion.identity);
                cameraObject.GetComponent<Camera>().fieldOfView = 50f;
            }

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            AddSceneToBuildSettings();
            AssetDatabase.SaveAssets();

            Debug.Log($"Movement Sandbox built: {ScenePath}");
        }

        private static GameObject CreateBox(string name, Transform parent, Vector2 center, Vector2 size, LevelBlockKind kind, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            Object.DestroyImmediate(box.GetComponent<Collider>()); // collision comes from LevelBox, not Unity physics
            box.GetComponent<Renderer>().sharedMaterial = material;
            box.transform.SetParent(parent, false);
            box.transform.position = new Vector3(center.x, center.y, 0f);
            box.transform.localScale = new Vector3(size.x, size.y, 2f); // depth is visual only

            var levelBox = box.AddComponent<LevelBox>();
            var serialized = new SerializedObject(levelBox);
            serialized.FindProperty("kind").enumValueIndex = (int)kind;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return box;
        }

        private static void SetField(Object target, string fieldName, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(fieldName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static CharacterData LoadOrCreateCharacterData()
        {
            var data = AssetDatabase.LoadAssetAtPath<CharacterData>(CharacterDataPath);
            if (data != null) return data;

            data = ScriptableObject.CreateInstance<CharacterData>();
            AssetDatabase.CreateAsset(data, CharacterDataPath);
            return data;
        }

        private static Material LoadOrCreateMaterial(string name, Color color)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;

            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static void AddSceneToBuildSettings()
        {
            if (EditorBuildSettings.scenes.Any(s => s.path == ScenePath)) return;
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
