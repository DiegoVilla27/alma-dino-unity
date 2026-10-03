#if UNITY_EDITOR
using System.IO;
using System.Linq;
using AlmaDino.Features.Camera;
using AlmaDino.Features.Enemies.Controllers;
using AlmaDino.Features.Enemies.ScriptableObjects;
using AlmaDino.Features.Environment;
using AlmaDino.Features.Environment.Controllers;
using AlmaDino.Features.Environment.ScriptableObjects;
using AlmaDino.Features.Player.Controllers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AlmaDino.Core.Editor
{
    public static class LevelBuilder4_3
    {
        public const string ScenePath = "Assets/Scenes/World_4_Volcano/Level_4_3.unity";
        private static readonly Color Magma = new Color(1f, .33f, .02f);
        [MenuItem("Tools/Alma/Construir Nivel 4-3 - La Gran Fractura")]
        public static void BuildLevel4_3()
        {
            if (EditorApplication.isPlaying) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(ScenePath)) AssetDatabase.CopyAsset(LevelBuilder4_2.ScenePath, ScenePath);
            var scene = EditorSceneManager.OpenScene(ScenePath);
            Object.DestroyImmediate(GameObject.Find("--- LEVEL ---"));
            var root = new GameObject("--- LEVEL ---"); var f = new CaveLevelSceneFactory(root.transform);
            var player = GameObject.Find("Alma (Player)").GetComponent<PlayerController>();
            player.transform.position = new Vector2(0f, .7f);
            CaveLevelSceneFactory.Set(player, "_doubleJumpUnlocked", true); CaveLevelSceneFactory.Set(player, "_groundPoundUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_dashUnlocked", true); CaveLevelSceneFactory.Set(player, "_roarUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_fallDeathY", -8f);
            SceneSetupValidator.SetupAlmaVisuals(player.gameObject); SceneSetupValidator.SetupTouchControls();
            LevelBuilder4_1.Floor(f, "Entrance_Refuge", -6f, 10f);
            LevelBuilder4_1.Floor(f, "First_Checkpoint_Refuge", 28f, 42f);
            LevelBuilder4_1.Floor(f, "Second_Checkpoint_Refuge", 64f, 70f);

            f.Platform("Entrance_Basalt_Boundary", new Vector2(-5.5f, 3f), new Vector2(1f, 8f));
            f.Platform("Exit_Basalt_Boundary", new Vector2(115f, 6f), new Vector2(1f, 8f));
            f.Checkpoint("Checkpoint_First_Chain", 32f, 0f); f.Checkpoint("Checkpoint_Final_Fracture", 68f, 0f);
            var config = LoadConfig<FractureConfigSO>("Assets/_Project/ScriptableObjects/FractureConfig.asset");
            Chain(f, root.transform, player, config, 0, 12f);
            BuildBranchingFracture(f, root.transform, player);
            BuildEruptionEscape(f, root.transform, player, config);
            var enemyConfig = LoadConfig<MagmaSalamanderConfigSO>("Assets/_Project/ScriptableObjects/MagmaSalamanderConfig.asset");
            var fireball = FireballPrefab(f, root.transform);
            Salamander(f, root.transform, player, enemyConfig, fireball, 0, 36f);
            Salamander(f, root.transform, player, enemyConfig, fireball, 1, 55.2f, -.6f, .8f, false);
            Salamander(f, root.transform, player, enemyConfig, fireball, 2, 89f, 2.2f, .8f, false);
            LevelBuilder4_1.Hint(root.transform, "Sequence_Hint", new Vector2(5f, 3.1f), "DOBLE SALTO → DASH → PISOTÓN ↓\nEN EL REFUGIO: ROAR → METEORITO", Color.yellow);
            LevelBuilder4_1.Hint(root.transform, "Salamander_Hint", new Vector2(33f, 3.8f), "SALAMANDRAS: AVISAN ANTES DE ESCUPIR\nACÉRCATE Y RUGE PARA ATURDIRLAS", Color.yellow);
            var waveGo = new GameObject("Fracture_Roar_Wave"); waveGo.transform.SetParent(root.transform, false);
            var wave = waveGo.AddComponent<RoarWaveVisual2D>(); var line = waveGo.GetComponent<LineRenderer>();
            line.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
            line.startWidth = .08f; line.endWidth = .08f; line.sortingOrder = 8;
            CaveLevelSceneFactory.Set(wave, "_playerSource", player); CaveLevelSceneFactory.Set(wave, "_radius", player.Config.RoarRadius);
            CaveLevelSceneFactory.Set(wave, "_duration", player.Config.RoarDuration); CaveLevelSceneFactory.Set(wave, "_halfAngle", player.Config.RoarHalfAngle);
            var portal = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Resources/Resource_LevelExitPortal_Universal.prefab"));
            portal.name = "Portal_Exit_To_4_4"; portal.transform.SetParent(root.transform, false); portal.transform.position = new Vector2(110f, 5.3f);
            CaveLevelSceneFactory.Set(portal.GetComponent<LevelExit2D>(), "_nextSceneName", "Level_4_4");
            CaveLevelSceneFactory.Set(portal.GetComponent<LevelExit2D>(), "_levelTitle", "LA GRAN FRACTURA COMPLETADA");
            CaveLevelSceneFactory.Set(portal.GetComponent<LevelExit2D>(), "_victoryMessage", "Salto, impulso, tierra y voz.\nTu último pequeño espera en la cima.");
            var camera = Camera.main; camera.orthographicSize = 6f; camera.backgroundColor = new Color(.17f, .025f, .025f);
            camera.transform.position = new Vector3(0f, 2f, -10f); var follow = camera.GetComponent<Camera2DFollow>();
            follow.SetTarget(player.transform); follow.SetBounds(new Vector2(0f, -1.5f), new Vector2(111f, 10f));
            CaveLevelSceneFactory.Set(follow, "_offset", new Vector2(0f, 1.5f)); CaveLevelSceneFactory.Set(follow, "_lookAheadDistance", 1.25f);
            LevelBuilder4_1.Atmosphere(f, root.transform, camera.transform);
            var prologue = new GameObject("Fracture_Prologue"); prologue.transform.SetParent(root.transform, false); prologue.transform.position = new Vector2(1f, 1f);
            prologue.AddComponent<BoxCollider2D>().isTrigger = true;
            prologue.AddComponent<NarrativePrologueTrigger>().Configure("LA GRAN FRACTURA", "La montaña se rompe. Yo sigo en pie.\nEntre ceniza, cornisas y fuego, encontraré mi propio camino.", Magma, 5f);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = EditorBuildSettings.scenes.ToList(); scenes.RemoveAll(s => s.path == ScenePath);
            scenes.Insert(scenes.FindIndex(s => s.path == LevelBuilder4_2.ScenePath) + 1, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray(); AssetDatabase.SaveAssets();
        }
        private static T LoadConfig<T>(string path) where T : ScriptableObject
        {
            var config = AssetDatabase.LoadAssetAtPath<T>(path);
            if (config == null) { config = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(config, path); }
            EditorUtility.SetDirty(config);
            return config;
        }
        private static void Chain(CaveLevelSceneFactory f, Transform root, PlayerController player, FractureConfigSO config, int index, float edge)
        {
            var launch = f.Platform("Collapsing_Launch_" + index, new Vector2(edge - 1f, -.3f), new Vector2(2f, .6f));
            launch.GetComponent<SpriteRenderer>().color = new Color(.4f, .18f, .13f);
            var crumble = launch.AddComponent<CrumblingPlatform2D>();
            CaveLevelSceneFactory.Set(crumble, "_crumbleDelay", 1f); CaveLevelSceneFactory.Set(crumble, "_shakeIntensity", 0f);
            CaveLevelSceneFactory.Set(crumble, "_playerSource", player);
            LevelBuilder4_1.Lava(f, root, "Fracture_Lava_" + index, edge, edge + 8f);
            f.Hazard("Burning_Spikes_" + index, new Vector2(edge + 4f, .05f), new Vector2(6.5f, .6f));
            foreach (var r in GameObject.Find("Burning_Spikes_" + index).GetComponentsInChildren<SpriteRenderer>()) r.color = Magma;
            float left = edge + 8f; float gateX = left + 8f;
            var lower = f.Platform("Lower_Refuge_" + index, new Vector2(left + 5f, -2.25f), new Vector2(10f, 1.5f));
            lower.GetComponent<SpriteRenderer>().color = new Color(.09f, .09f, .12f);
            var ground = f.BreakableFloor("Pound_Landing_" + index, new Vector2(left + 4f, -.2f), new Vector2(8f, .4f));
            ground.GetComponent<SpriteRenderer>().color = new Color(.6f, .3f, .14f);
            LevelBuilder4_1.Hint(root, "Launch_Hint_" + index, new Vector2(edge - 3f, 2.3f), "ROCA COLAPSANTE\nDOBLE SALTO + DASH →", Color.yellow);
            var gateGo = f.Visual("Meteor_Gate_" + index, root, new Vector2(gateX, 3.5f), new Vector2(.8f, 10f), new Color(.4f, .23f, .15f), true);
            gateGo.AddComponent<BoxCollider2D>(); var gate = gateGo.AddComponent<MeteorImpactGate2D>();
            var meteorGo = new GameObject("Reflectable_Meteor_" + index); meteorGo.transform.SetParent(root, false);
            var body = meteorGo.AddComponent<Rigidbody2D>(); body.gravityScale = 0f; body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            meteorGo.AddComponent<CircleCollider2D>().radius = .35f; meteorGo.GetComponent<CircleCollider2D>().isTrigger = true;
            var visual = f.Visual("Meteor_Core", meteorGo.transform, Vector2.zero, Vector2.one * .7f, Magma, true);
            visual.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            f.Visual("Meteor_Tail", meteorGo.transform, new Vector2(.45f, .25f), new Vector2(.9f, .12f), Color.yellow, true);
            var meteor = meteorGo.AddComponent<ReflectableMeteor2D>(); CaveLevelSceneFactory.Set(meteor, "_visual", visual.GetComponent<SpriteRenderer>());
            meteorGo.SetActive(false);
            var warning = f.Visual("Meteor_Warning_" + index, root, new Vector2(left + 5f, .5f), Vector2.one * .8f, new Color(1f, .8f, .05f, .4f), true);
            warning.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            warning.SetActive(false);
            var status = LevelBuilder4_1.Hint(root, "Meteor_Status_" + index, new Vector2(left + 3f, -1.1f), "PISOTÓN ↓ / METEORITO →", Color.yellow);
            CaveLevelSceneFactory.Set(gate, "_config", config); CaveLevelSceneFactory.Set(gate, "_playerSource", player);
            CaveLevelSceneFactory.Set(gate, "_ground", ground); CaveLevelSceneFactory.Set(gate, "_meteor", meteor);
            CaveLevelSceneFactory.Set(gate, "_visual", gateGo.GetComponent<SpriteRenderer>()); CaveLevelSceneFactory.Set(gate, "_warningMarker", warning.transform);
            CaveLevelSceneFactory.Set(gate, "_status", status); CaveLevelSceneFactory.Set(gate, "_chamberLeft", left);
            var jetGo = new GameObject("Landing_Flame_Jet_" + index); jetGo.transform.SetParent(root, false); jetGo.transform.position = new Vector2(left + 4f, 1f);
            var col = jetGo.AddComponent<BoxCollider2D>(); col.size = new Vector2(8f, .7f); col.isTrigger = true;
            var flame = f.Visual("Horizontal_Flame", jetGo.transform, Vector2.zero, col.size, Magma, true);
            flame.SetActive(false);
            var jetStatus = LevelBuilder4_1.Hint(root, "Jet_Status_" + index, new Vector2(left + 3f, 2.5f), "PISOTÓN ↓ AL REFUGIO", Color.cyan);
            var jet = jetGo.AddComponent<FractureFlameJet2D>(); CaveLevelSceneFactory.Set(jet, "_config", config); CaveLevelSceneFactory.Set(jet, "_playerSource", player);
            CaveLevelSceneFactory.Set(jet, "_flame", flame); CaveLevelSceneFactory.Set(jet, "_status", jetStatus); CaveLevelSceneFactory.Set(jet, "_landingBounds", new Vector2(left, gateX));
            CaveLevelSceneFactory.Light(root, new Vector2(left + 4f, -.7f), Magma, 5f, .7f);
        }
        private static GameObject Ledge(CaveLevelSceneFactory f, Transform root, PlayerController player, string name, float left, float right, float top, bool crumbling = false)
        {
            var go=f.Platform(name,new Vector2((left+right)*.5f,top-.2f),new Vector2(right-left,.4f));
            go.GetComponent<SpriteRenderer>().color=new Color(.16f,.2f,.23f);
            go.transform.GetChild(0).GetComponent<SpriteRenderer>().color=new Color(.35f,.8f,.85f);
            if(crumbling)
            {
                var crumble=go.AddComponent<CrumblingPlatform2D>();
                CaveLevelSceneFactory.Set(crumble,"_playerSource",player); CaveLevelSceneFactory.Set(crumble,"_crumbleDelay",1.5f);
                CaveLevelSceneFactory.Set(crumble,"_shakeIntensity",0f);
            }
            return go;
        }
        private static void BuildBranchingFracture(CaveLevelSceneFactory f, Transform root, PlayerController player)
        {
            var lava=LevelBuilder4_1.Lava(f,root,"Branching_Deep_Lava",42f,64f);
            lava.transform.position=new Vector2(53f,-3.4f);
            Ledge(f,root,player,"Upper_Crumbling_0",44f,48f,2.2f,true);
            Ledge(f,root,player,"Upper_Crumbling_1",51f,55f,2.8f,true);
            Ledge(f,root,player,"Upper_Crumbling_2",58f,62f,2.8f,true);
            Ledge(f,root,player,"Lower_Stone_0",44f,50f,-.6f);
            Ledge(f,root,player,"Lower_Stone_1",52f,59f,-.6f);
            Ledge(f,root,player,"Lower_Stone_2",61f,64f,-.6f);
            foreach(float x in new[]{47f,57.5f})
            {
                var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Traps/Trap_FireGeyser_Volcano.prefab"));
                go.name="Branch_Steam_"+x; go.transform.SetParent(root,false);go.transform.position=new Vector2(x,.2f);
                go.GetComponent<BoxCollider2D>().size=new Vector2(1.2f,1.6f);
                go.transform.Find("Hot_Steam").localScale=new Vector3(1.2f,1.6f,1f);
                go.transform.Find("Vent_Base").localPosition=new Vector2(0f,-.65f);
                go.transform.Find("Steam_State").localPosition=new Vector2(0f,1.1f);
                CaveLevelSceneFactory.Set(go.GetComponent<LavaGeyser2D>(),"_playerSource",player);
            }
            LevelBuilder4_1.Hint(root,"Branch_Choice_Hint",new Vector2(40f,4.1f),"ELIGE TU CAMINO\nARRIBA: CORNISAS QUE CEDEN\nABAJO: VAPOR Y SALAMANDRA",Color.cyan);
            for(int i=0;i<3;i++)
            {
                float top=i==0?2.2f:2.8f;
                f.Visual("Fractured_Hanging_Chain_"+i,root,new Vector2(46f+i*7f,(top+8.6f)*.5f),new Vector2(.07f,8.6f-top),new Color(.24f,.35f,.38f));
            }
        }
        private static void BuildEruptionEscape(CaveLevelSceneFactory f, Transform root, PlayerController player, FractureConfigSO config)
        {
            var seal=f.BreakableFloor("Final_Eruption_Seal",new Vector2(72f,-.2f),new Vector2(4f,.4f));
            seal.GetComponent<SpriteRenderer>().color=new Color(.85f,.24f,.04f);
            f.Platform("Eruption_Lower_Refuge",new Vector2(72.5f,-2.25f),new Vector2(5f,1.5f));
            Ledge(f,root,player,"Escape_Step_0",75f,79f,-.2f);
            Ledge(f,root,player,"Escape_Step_1",81f,85f,1f);
            Ledge(f,root,player,"Escape_Step_2",87f,91f,2.2f,true);
            Ledge(f,root,player,"Escape_Dash_Launch",93f,97f,3.2f,true);
            var exit=f.Platform("Escape_Final_Refuge",new Vector2(109.5f,3.05f),new Vector2(9f,1.5f));
            exit.GetComponent<SpriteRenderer>().color=new Color(.11f,.13f,.18f);
            var barrier=f.Visual("Eruption_Seal_Barrier",root,new Vector2(74.5f,3.5f),new Vector2(.6f,10f),new Color(.5f,.15f,.04f),true);
            barrier.AddComponent<BoxCollider2D>();
            var go=new GameObject("Fracture_Eruption");go.transform.SetParent(root,false);go.transform.position=new Vector2(91f,config.EruptionInitialHeight-10f);
            var rb=go.AddComponent<Rigidbody2D>();rb.bodyType=RigidbodyType2D.Kinematic;
            var collider=go.AddComponent<BoxCollider2D>();collider.size=new Vector2(46f,20f);collider.isTrigger=true;
            f.Visual("Rising_Magma",go.transform,Vector2.zero,collider.size,Magma,true);
            f.Visual("Magma_Surface",go.transform,new Vector2(0f,9.9f),new Vector2(46f,.2f),Color.yellow,true);
            var status=LevelBuilder4_1.Hint(root,"Eruption_Status",new Vector2(70f,2.8f),"PISOTÓN ↓ ROMPE EL SELLO",Color.yellow);
            CaveLevelSceneFactory.Light(root,new Vector2(72f,1f),Magma,7f,.15f);
            var glow=root.GetChild(root.childCount-1).GetComponent<UnityEngine.Rendering.Universal.Light2D>();
            var eruption=go.AddComponent<FractureEruption2D>(); CaveLevelSceneFactory.Set(eruption,"_config",config);
            CaveLevelSceneFactory.Set(eruption,"_playerSource",player);CaveLevelSceneFactory.Set(eruption,"_seal",seal);
            CaveLevelSceneFactory.Set(eruption,"_barrier",barrier.GetComponent<Collider2D>());CaveLevelSceneFactory.Set(eruption,"_barrierVisual",barrier.GetComponent<SpriteRenderer>());
            CaveLevelSceneFactory.Set(eruption,"_status",status); CaveLevelSceneFactory.Set(eruption,"_glow",glow);
            var shake=new SerializedObject(player).FindProperty("_cameraShakeChannel").objectReferenceValue;
            if(shake!=null) CaveLevelSceneFactory.Set(eruption,"_shake",shake);
            LevelBuilder4_1.Hint(root,"Escape_Final_Dash_Hint",new Vector2(95f,5.2f),"ÚLTIMA FRACTURA\nDOBLE SALTO + DASH →",Color.yellow);
        }
        private static PoisonBubble2D FireballPrefab(CaveLevelSceneFactory f, Transform root)
        {
            var go = new GameObject("Magma_Fireball"); go.transform.SetParent(root, false);
            var rb = go.AddComponent<Rigidbody2D>(); rb.gravityScale = 0f; rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            go.AddComponent<CircleCollider2D>().radius = .2f; go.GetComponent<CircleCollider2D>().isTrigger = true;
            var visual = f.Visual("Fireball", go.transform, Vector2.zero, Vector2.one * .4f, Magma, true);
            visual.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            go.AddComponent<PoisonBubble2D>(); go.SetActive(false);
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/_Project/Prefabs/Projectiles/Projectile_MagmaFireball_Volcano.prefab");
            Object.DestroyImmediate(go); return prefab.GetComponent<PoisonBubble2D>();
        }
        internal static void Salamander(CaveLevelSceneFactory f, Transform root, PlayerController player, MagmaSalamanderConfigSO config, PoisonBubble2D fireball, int index, float x, float floorTop = 0f, float patrolWidth = 3f, bool climbingWall = true)
        {
            if (climbingWall) f.Platform("Salamander_Climbing_Wall_" + index, new Vector2(x + 3.6f, .85f), new Vector2(.4f, 1.7f));
            if (climbingWall) LevelBuilder4_1.Hint(root, "Climbing_Wall_Hint_" + index, new Vector2(x + 3.6f, 3.1f), "DOBLE SALTO →", Color.cyan);
            var go = new GameObject("Magma_Salamander_" + index); go.transform.SetParent(root, false); go.transform.position = new Vector2(x, floorTop + .7f);
            var body = go.AddComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic; body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            go.AddComponent<BoxCollider2D>().size = new Vector2(1.1f, .7f); go.GetComponent<BoxCollider2D>().isTrigger = true;
            var visual = f.Visual("Salamander_Body", go.transform, Vector2.zero, new Vector2(1.2f, .65f), Magma, true);
            f.Visual("Salamander_Tail", go.transform, new Vector2(-.7f, 0f), new Vector2(.5f, .15f), new Color(.5f, .08f, .02f), true);
            f.Visual("Salamander_Eye", visual.transform, new Vector2(.3f, .2f), Vector2.one * .12f, Color.yellow, true);
            var status = LevelBuilder4_1.Hint(go.transform, "Salamander_Status", new Vector2(x, floorTop + 1.8f), "ROAR → ATURDE", Color.yellow);
            var enemy = go.AddComponent<MagmaSalamander2D>(); CaveLevelSceneFactory.Set(enemy, "_config", config);
            CaveLevelSceneFactory.Set(enemy, "_playerSource", player); CaveLevelSceneFactory.Set(enemy, "_fireballPrefab", fireball);
            CaveLevelSceneFactory.Set(enemy, "_visual", visual.GetComponent<SpriteRenderer>()); CaveLevelSceneFactory.Set(enemy, "_status", status);
            var so = new SerializedObject(enemy); var route = so.FindProperty("_route"); route.arraySize = 4;
            Vector2[] points = {new Vector2(x,floorTop+.7f),new Vector2(x+patrolWidth,floorTop+.7f),new Vector2(x+patrolWidth,floorTop+(climbingWall?2.2f:.7f)),new Vector2(x,floorTop+.7f)};
            for (int i=0;i<points.Length;i++) route.GetArrayElementAtIndex(i).vector2Value = points[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        [MenuItem("Alma/📂 Cargar Nivel 4-3")]
        public static void LoadLevel() { if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath); }
    }
}
#endif
