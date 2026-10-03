#if UNITY_EDITOR
using System.IO;
using System.Linq;
using AlmaDino.Core.Progression;
using AlmaDino.Features.Camera;
using AlmaDino.Features.Enemies;
using AlmaDino.Features.Environment;
using AlmaDino.Features.Player.Controllers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace AlmaDino.Core.Editor
{
    public static class LevelBuilder2_4
    {
        public const string ScenePath = "Assets/Scenes/World_2_Caves/Level_2_4.unity";
        [MenuItem("Tools/Alma/Construir Nivel 2-4 - El Laberinto de Geodas")]
        public static void BuildLevel2_4()
        {
            if (EditorApplication.isPlaying) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(ScenePath)) AssetDatabase.CopyAsset(LevelBuilder2_3.ScenePath, ScenePath);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var previous = GameObject.Find("--- LEVEL ---");
            if (previous != null) Object.DestroyImmediate(previous);
            const string configPath = "Assets/_Project/ScriptableObjects/CrushingCeilingConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<CrushingCeilingConfigSO>(configPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<CrushingCeilingConfigSO>();
                AssetDatabase.CreateAsset(config, configPath); AssetDatabase.SaveAssets();
            }
            var root = new GameObject("--- LEVEL ---");
            var f = new CaveLevelSceneFactory(root.transform);
            var player = GameObject.Find("Alma (Player)").GetComponent<PlayerController>();
            player.transform.position = new Vector2(0f, 0.7f);
            CaveLevelSceneFactory.Set(player,"_doubleJumpUnlocked",true);
            CaveLevelSceneFactory.Set(player,"_groundPoundUnlocked",true);
            CaveLevelSceneFactory.Set(player,"_dashUnlocked",false);
            CaveLevelSceneFactory.Set(player,"_roarUnlocked",false);
            CaveLevelSceneFactory.Set(player,"_fallDeathY",-10f);
            SceneSetupValidator.SetupAlmaVisuals(player.gameObject); SceneSetupValidator.SetupTouchControls();
            f.Platform("Left_Boundary",new Vector2(-3.5f,4f),new Vector2(1f,12f));
            f.Platform("Entrance",new Vector2(4.5f,-0.5f),new Vector2(15f,1f));
            var first = Shelter(f,"First",12f);
            Ceiling(f,root.transform,player,config,"Ceiling_First",new Vector2(14.5f,6f),9f,0.5f);
            f.Platform("Checkpoint_One_Floor",new Vector2(45f,-0.5f),new Vector2(42f,1f));
            f.Checkpoint("Checkpoint_After_First_Ceiling",32f,0f);
            var puzzles = new EchoPuzzleSceneFactory(f,root.transform,
                AssetDatabase.LoadAssetAtPath<SeesawConfigSO>("Assets/_Project/ScriptableObjects/EchoSeesawConfig.asset"),player);
            var rune = puzzles.Station("Seesaw_Pressure",40f);
            var pressureGate = puzzles.Gate("Gate_Pressure",46f,rune);
            CaveLevelSceneFactory.Set(pressureGate, "_allowReturnFromRight", true);
            CaveLevelSceneFactory.Set(pressureGate, "_returnPassageMaxY", 5.2f);
            var returnRamp = f.Platform("Pressure_Return_Ramp", new Vector2(44.5f, 0.65f), new Vector2(4f, 0.3f));
            returnRamp.transform.rotation = Quaternion.Euler(0f, 0f, -30f);
            puzzles.Label("Pressure_Return_Sign", new Vector2(50f, 2.5f), "¿CAÍSTE? VUELVE ←\nREINTENTA EL BALANCÍN");
            Ceiling(f,root.transform,player,config,"Ceiling_Pressure",new Vector2(37.5f,10f),5f,0.6f);
            f.Platform("Pressure_Exit_Ledge",new Vector2(51f,5.7f),new Vector2(10f,1f));
            f.Platform("Pressure_Sealing_Pillar",new Vector2(55.5f,2.6f),new Vector2(1f,5.2f));
            var enemies = new ResonantEnemySceneFactory(f,root.transform,
                AssetDatabase.LoadAssetAtPath<CrystalEnemyConfigSO>("Assets/_Project/ScriptableObjects/CrystalEnemyConfig.asset"),player);
            f.Platform("Beetle_Upper_Seal",new Vector2(58f,8.4f),new Vector2(.7f,12f));
            f.Platform("Beetle_Low_Roof",new Vector2(61.5f,3f),new Vector2(7f,1.2f));
            enemies.Beetle("Beetle_Emergency",new Vector2(61.5f,0.45f),new Vector2(61.2f,61.8f));
            f.Platform("Sanctuary_Approach",new Vector2(69.5f,-0.5f),new Vector2(11f,1f));
            f.Checkpoint("Checkpoint_Before_Blue_Egg",68f,0f);
            var second = Shelter(f,"Final",75f);
            Ceiling(f,root.transform,player,config,"Ceiling_Final",new Vector2(77.5f,6f),9f,0.5f);
            var reset = root.AddComponent<BreakableGroundRespawnReset2D>();
            CaveLevelSceneFactory.Set(reset,"_playerSource",player);
            var serialized = new SerializedObject(reset); var array = serialized.FindProperty("_grounds");
            array.arraySize=2; array.GetArrayElementAtIndex(0).objectReferenceValue=first;
            array.GetArrayElementAtIndex(1).objectReferenceValue=second; serialized.ApplyModifiedPropertiesWithoutUndo();
            f.Platform("Sanctuary_Floor",new Vector2(93.5f,-0.5f),new Vector2(13f,1f));
            f.Platform("Blue_Egg_Pedestal",new Vector2(92f,0.6f),new Vector2(3f,1.2f));
            f.Platform("Right_Boundary",new Vector2(101f,5f),new Vector2(1f,12f));
            f.Hazard("Abyss_Death_Zone",new Vector2(50f,-11f),new Vector2(110f,2f));
            BuildSanctuary(f,root.transform);
            LevelBuilder2_2.BuildAtmosphere(f,root.transform);
            foreach(var renderer in root.GetComponentsInChildren<SpriteRenderer>())
                if(renderer.name=="Gallery_Geode") renderer.color=new Color(0.28f,0.79f,0.89f);
            var camera=Camera.main; camera.orthographicSize=6f;
            var follow=camera.GetComponent<Camera2DFollow>(); follow.SetTarget(player.transform);
            follow.SetBounds(new Vector2(1f,1f),new Vector2(95f,10f));
            CaveLevelSceneFactory.Set(follow,"_offset",new Vector2(0f,2f));
            camera.transform.position=new Vector3(1f,2.7f,-10f);
            foreach(var light in Object.FindObjectsByType<Light2D>())
                if(light.lightType==Light2D.LightType.Global) {light.color=new Color(0.3f,0.75f,1f);light.intensity=0.35f;}
            var intro=new GameObject("Geode_Prologue");intro.transform.SetParent(root.transform);
            intro.AddComponent<BoxCollider2D>().isTrigger=true;
            intro.AddComponent<NarrativePrologueTrigger>().Configure("EL LABERINTO DE GEODAS",
                "El techo avisa antes de caer.\nRompe el suelo claro con POUND y espera en el refugio.\nEl Huevo Azul late al fondo de la cueva.",Color.cyan,7f);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,ScenePath);
            var scenes=EditorBuildSettings.scenes.ToList();scenes.RemoveAll(entry=>entry.path==ScenePath);
            scenes.Insert(scenes.FindIndex(entry=>entry.path==LevelBuilder2_3.ScenePath)+1,new EditorBuildSettingsScene(ScenePath,true));
            EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
        }
        private static BreakableGround2D Shelter(CaveLevelSceneFactory f,string name,float x)
        {
            var slab=f.BreakableFloor(name+"_Refuge_Slab",new Vector2(x+2f,-0.4f),new Vector2(4f,0.8f));
            f.Platform(name+"_Refuge_Floor",new Vector2(x+4f,-3.5f),new Vector2(8f,1f));
            f.Platform(name+"_Upper_Seal",new Vector2(x+4.5f,5.5f),new Vector2(0.7f,13f));
            f.Platform(name+"_Exit_Step",new Vector2(x+9f,-2f),new Vector2(3f,1f));
            f.Platform(name+"_Exit_Step_High",new Vector2(x+12f,-0.5f),new Vector2(3f,1f));
            return slab;
        }
        private static void Ceiling(CaveLevelSceneFactory f,Transform root,PlayerController player,
            CrushingCeilingConfigSO config,string name,Vector2 position,float width,float closedY)
        {
            var go=new GameObject(name);go.transform.SetParent(root,false);go.transform.position=position;
            go.AddComponent<BoxCollider2D>().size=new Vector2(width,1f);
            var body=go.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;
            body.interpolation=RigidbodyInterpolation2D.Interpolate;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            var rock=f.Visual("Crushing_Rock",go.transform,Vector2.zero,new Vector2(width,1f),new Color(0.1f,0.1f,0.12f),true);
            for(int i=0;i<Mathf.FloorToInt(width);i++)
            {
                var spike=f.Visual("Stalactite",go.transform,new Vector2(-width*.5f+.5f+i,-.3f),new Vector2(.4f,.6f),new Color(.56f,.88f,.94f),true);
                spike.transform.localRotation=Quaternion.Euler(0f,0f,45f);
            }
            var label=new GameObject("Ceiling_Warning");label.transform.SetParent(root,false);
            label.transform.position=new Vector2(position.x,closedY+2.8f);
            var text=label.AddComponent<TextMesh>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize=40;text.characterSize=.08f;text.anchor=TextAnchor.MiddleCenter;text.color=Color.white;
            label.GetComponent<MeshRenderer>().sharedMaterial=text.font.material;label.GetComponent<MeshRenderer>().sortingOrder=8;
            var ceiling=go.AddComponent<CrushingCeiling2D>();
            CaveLevelSceneFactory.Set(ceiling,"_config",config);CaveLevelSceneFactory.Set(ceiling,"_playerSource",player);
            CaveLevelSceneFactory.Set(ceiling,"_closedPosition",new Vector2(position.x,closedY));
            CaveLevelSceneFactory.Set(ceiling,"_indicator",rock.GetComponent<SpriteRenderer>());
            CaveLevelSceneFactory.Set(ceiling,"_warningLabel",text);
        }
        private static void BuildSanctuary(CaveLevelSceneFactory f,Transform root)
        {
            var portal=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Resources/Resource_LevelExitPortal_Universal.prefab"));
            portal.name="Portal_Exit_To_Boss_2";portal.transform.SetParent(root,false);portal.transform.position=new Vector2(98f,1.4f);
            var exit=portal.GetComponent<LevelExit2D>();CaveLevelSceneFactory.Set(exit,"_nextSceneName","Boss_2");
            CaveLevelSceneFactory.Set(exit,"_levelTitle","EL HUEVO AZUL ESTÁ A SALVO");
            CaveLevelSceneFactory.Set(exit,"_victoryMessage","La Gran Geoda guarda otra vida.\nEl Armadillo Prehistórico bloquea la salida: prepárate para el próximo combate.");
            portal.SetActive(false);
            var egg=f.Visual("Blue_Egg",root,new Vector2(92f,2f),new Vector2(.7f,.95f),new Color(.28f,.79f,.89f),true);
            egg.GetComponent<SpriteRenderer>().sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            egg.AddComponent<CircleCollider2D>().isTrigger=true;
            CaveLevelSceneFactory.Light(egg.transform,Vector2.zero,Color.cyan,4f,1f);
            var rescue=egg.AddComponent<GreenEggRescue2D>();
            rescue.Configure(EggType.BlueEgg,"¡SEGUNDO RESCATE: EL HUEVO AZUL!",
                "Sentí tu latido contra la piedra fría.\nYa somos dos. No descansaré hasta que estemos los cinco juntos.\n(2 de 4 rescatados)\nEl Armadillo Prehistórico despierta al otro lado...",Color.cyan,exit);
            var guardian=new GameObject("Armadillo_Guardian_Teaser");guardian.transform.SetParent(root,false);
            guardian.transform.position=new Vector2(100f,3.5f);
            var shell=f.Visual("Armored_Shell",guardian.transform,Vector2.zero,new Vector2(3f,2f),new Color(.32f,.4f,.5f),true);
            shell.GetComponent<SpriteRenderer>().sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            for(int i=0;i<5;i++)f.Visual("Armor_Band",guardian.transform,new Vector2(-1f+i*.5f,0f),new Vector2(.07f,1.5f),Color.cyan,true);
            f.Visual("Guardian_Eye",guardian.transform,new Vector2(-1f,.1f),new Vector2(.12f,.12f),Color.white,true);
            guardian.SetActive(false);var sanctuary=root.gameObject.AddComponent<BlueEggSanctuary2D>();
            CaveLevelSceneFactory.Set(sanctuary,"_egg",rescue);CaveLevelSceneFactory.Set(sanctuary,"_armadillo",guardian);
        }
    }
}
#endif
