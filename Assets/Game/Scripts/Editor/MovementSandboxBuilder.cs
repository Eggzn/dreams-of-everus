using System.IO;
using System.Linq;
using DreamsOfEverus.Game;
using DreamsOfEverus.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

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
        private const string InputFeelPath = "Assets/Game/Data/InputFeel.asset";
        private const string LightAttackPath = "Assets/Game/Data/BunLightAttack.asset";
        private const string FollowCameraPath = "Assets/Game/Data/FollowCamera.asset";
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

            CharacterData bunData = LoadOrCreateAsset<CharacterData>(CharacterDataPath);
            InputFeelData inputFeel = LoadOrCreateAsset<InputFeelData>(InputFeelPath);
            AttackData bunLightAttack = LoadOrCreateAsset<AttackData>(LightAttackPath);
            if (bunData.lightAttack == null)
            {
                bunData.lightAttack = bunLightAttack;
                EditorUtility.SetDirty(bunData);
            }
            var controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);

            Material solid = LoadOrCreateMaterial("Graybox_Solid", new Color(0.55f, 0.55f, 0.58f));
            Material passThrough = LoadOrCreateMaterial("Graybox_PassThrough", new Color(0.35f, 0.6f, 0.9f));
            Material fallZone = LoadOrCreateMaterial("Graybox_FallZone", new Color(0.6f, 0.12f, 0.12f));
            Material bunMaterial = LoadOrCreateMaterial("Placeholder_Bun", new Color(1f, 0.72f, 0.3f));
            Material dummyMaterial = LoadOrCreateMaterial("Placeholder_Dummy", new Color(0.85f, 0.85f, 0.8f));
            Material hitboxMaterial = LoadOrCreateSeeThroughMaterial("Debug_Hitbox", new Color(1f, 0.1f, 0.1f, 0.45f));

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

            // Crawl tunnel: a low solid ceiling 1.3 units above the floor. Too low to stand under, high enough to crawl.
            CreateBox("Crawl Tunnel Ceiling", level, new Vector2(10.5f, 2.15f), new Vector2(4f, 1.7f), LevelBlockKind.Solid, solid);

            // Below the whole room, so anything that drops through the gap lands in it.
            CreateBox("Fall Zone", level, new Vector2(0f, -9f), new Vector2(32f, 4f), LevelBlockKind.FallZone, fallZone);

            var respawn = new GameObject("Respawn Point", typeof(RespawnPoint));
            respawn.transform.position = new Vector3(-6f, 0f, 0f);

            // ----- Simulation -----
            var simulationObject = new GameObject("Simulation", typeof(SimulationRunner), typeof(PlayerInputReader), typeof(DebugHud));
            var runner = simulationObject.GetComponent<SimulationRunner>();
            SetField(runner, "playerCharacter", bunData);
            SetField(runner, "inputFeel", inputFeel);
            SetField(simulationObject.GetComponent<DebugHud>(), "runner", runner);
            SetField(simulationObject.GetComponent<PlayerInputReader>(), "controls", controls);
            SetField(simulationObject.GetComponent<PlayerInputReader>(), "inputFeel", inputFeel);

            // ----- Bun (placeholder capsule) -----
            var bun = new GameObject("Bun", typeof(FighterView));
            bun.transform.position = respawn.transform.position;

            // Visual turns to show facing; body and nose turn with it.
            var visual = new GameObject("Visual").transform;
            visual.SetParent(bun.transform, false);

            GameObject body = CreateVisualPrimitive(PrimitiveType.Capsule, "Body (Placeholder)", visual, bunMaterial);
            body.transform.localScale = new Vector3(bunData.bodyWidth, bunData.bodyHeight * 0.5f, bunData.bodyWidth);
            body.transform.localPosition = new Vector3(0f, bunData.bodyHeight * 0.5f, 0f);

            // A small block on the front of the face, so you can see which way Bun is facing.
            GameObject nose = CreateVisualPrimitive(PrimitiveType.Cube, "Nose (Facing)", visual, solid);
            nose.transform.localScale = new Vector3(0.3f, 0.2f, 0.2f);
            nose.transform.localPosition = new Vector3(bunData.bodyWidth * 0.5f, bunData.bodyHeight * 0.75f, 0f);

            var view = bun.GetComponent<FighterView>();
            SetField(view, "runner", runner);
            SetField(view, "visualRoot", visual);
            SetField(view, "body", body.transform);
            SetField(view, "nose", nose.transform);

            // ----- Training dummy (stands to Bun's right, within a few steps) -----
            var dummy = new GameObject("Training Dummy", typeof(TrainingDummy), typeof(TrainingDummyView));
            dummy.transform.position = new Vector3(-3f, 0f, 0f);
            var dummyData = dummy.GetComponent<TrainingDummy>();

            GameObject dummyBody = CreateVisualPrimitive(PrimitiveType.Cube, "Body (Placeholder)", dummy.transform, dummyMaterial);
            dummyBody.transform.localScale = new Vector3(dummyData.hurtboxWidth, dummyData.hurtboxHeight, 0.6f);
            dummyBody.transform.localPosition = new Vector3(0f, dummyData.hurtboxHeight * 0.5f, 0f);

            var dummyView = dummy.GetComponent<TrainingDummyView>();
            SetField(dummyView, "runner", runner);
            SetField(dummyView, "body", dummyBody.transform);
            SetField(dummyView, "bodyRenderer", dummyBody.GetComponent<Renderer>());

            // ----- Hitbox display (red, see-through, only while a hitbox is out) -----
            var hitboxDisplay = new GameObject("Hitbox Display", typeof(HitboxView));
            GameObject hitboxBox = CreateVisualPrimitive(PrimitiveType.Cube, "Hitbox", hitboxDisplay.transform, hitboxMaterial);
            SetField(hitboxDisplay.GetComponent<HitboxView>(), "runner", runner);
            SetField(hitboxDisplay.GetComponent<HitboxView>(), "box", hitboxBox.GetComponent<Renderer>());

            // ----- Camera: side-on perspective, follows Bun -----
            GameObject cameraObject = GameObject.FindWithTag("MainCamera");
            if (cameraObject != null)
            {
                FollowCameraData cameraSettings = LoadOrCreateAsset<FollowCameraData>(FollowCameraPath);
                var follow = cameraObject.AddComponent<FollowCamera>();
                SetField(follow, "runner", runner);
                SetField(follow, "target", bun.transform);
                SetField(follow, "settings", cameraSettings);

                // Start where the camera will be when play begins, so the Scene and Game views match.
                cameraObject.transform.SetPositionAndRotation(
                    FollowCamera.DesiredPosition(bun.transform.position, cameraSettings.lookAheadDistance, cameraSettings),
                    Quaternion.Euler(cameraSettings.pitch, 0f, 0f));
                cameraObject.GetComponent<Camera>().fieldOfView = cameraSettings.fieldOfView;
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

        private static GameObject CreateVisualPrimitive(PrimitiveType type, string name, Transform parent, Material material)
        {
            GameObject shape = GameObject.CreatePrimitive(type);
            shape.name = name;
            Object.DestroyImmediate(shape.GetComponent<Collider>()); // no Unity physics in gameplay
            shape.GetComponent<Renderer>().sharedMaterial = material;
            shape.transform.SetParent(parent, false);
            return shape;
        }

        /// <summary>Loads a data asset, or creates it with default values if it doesn't exist. Existing values are kept.</summary>
        private static T LoadOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        /// <summary>An unlit, see-through material (URP Unlit set to Transparent), for debug boxes like hitboxes.</summary>
        private static Material LoadOrCreateSeeThroughMaterial(string name, Color color)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;

            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Surface", 1f); // Transparent
            material.SetFloat("_Blend", 0f);   // Alpha blending
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            AssetDatabase.CreateAsset(material, path);
            return material;
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
