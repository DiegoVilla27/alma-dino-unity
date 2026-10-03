#if UNITY_EDITOR
using System.IO;
using System.Linq;
using AlmaDino.Core.Progression;
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
using UnityEngine.Rendering.Universal;
namespace AlmaDino.Core.Editor
{
    public static class LevelBuilder4_4
    {
        public const string ScenePath="Assets/Scenes/World_4_Volcano/Level_4_4.unity";
        private static readonly Color Red=new Color(.9f,.22f,.27f);
        private static readonly Color Gold=new Color(1f,.73f,.03f);
        [MenuItem("Tools/Alma/Construir Nivel 4-4 - La Antecámara del Fuego")]
        public static void BuildLevel4_4()
        {
            if(EditorApplication.isPlaying) return;
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if(!File.Exists(ScenePath)) AssetDatabase.CopyAsset(LevelBuilder4_3.ScenePath,ScenePath);
            var scene=EditorSceneManager.OpenScene(ScenePath); Object.DestroyImmediate(GameObject.Find("--- LEVEL ---"));
            var oldAura=GameObject.Find("Four_Egg_Aura"); if(oldAura!=null) Object.DestroyImmediate(oldAura);
            var root=new GameObject("--- LEVEL ---"); var f=new CaveLevelSceneFactory(root.transform);
            var player=GameObject.Find("Alma (Player)").GetComponent<PlayerController>(); player.transform.position=new Vector2(0f,.7f);
            CaveLevelSceneFactory.Set(player,"_doubleJumpUnlocked",true); CaveLevelSceneFactory.Set(player,"_groundPoundUnlocked",true);
            CaveLevelSceneFactory.Set(player,"_dashUnlocked",true); CaveLevelSceneFactory.Set(player,"_roarUnlocked",true);
            CaveLevelSceneFactory.Set(player,"_fallDeathY",-8f); SceneSetupValidator.SetupAlmaVisuals(player.gameObject); SceneSetupValidator.SetupTouchControls();
            var combat=new GameObject("Temple_Combat"); combat.transform.SetParent(root.transform,false);
            var fire=new GameObject("Temple_Fire_Currents"); fire.transform.SetParent(root.transform,false);
            BuildDoubleJumpHall(f,root.transform);
            var trials=BuildPoundHall(f,root.transform,player);
            var dash=BuildDashHall(f,root.transform,player,fire.transform); CaveLevelSceneFactory.Set(trials,"_dashGate",dash);
            BuildRoarHall(f,root.transform,player);
            var enemyConfig=AssetDatabase.LoadAssetAtPath<MagmaSalamanderConfigSO>("Assets/_Project/ScriptableObjects/MagmaSalamanderConfig.asset");
            var fireball=AssetDatabase.LoadAssetAtPath<PoisonBubble2D>("Assets/_Project/Prefabs/Projectiles/Projectile_MagmaFireball_Volcano.prefab");
            LevelBuilder4_3.Salamander(f,combat.transform,player,enemyConfig,fireball,0,20f,0f,.8f,false);
            LevelBuilder4_3.Salamander(f,combat.transform,player,enemyConfig,fireball,1,27f,-2f,.4f,false);
            LevelBuilder4_3.Salamander(f,combat.transform,player,enemyConfig,fireball,2,40f,0f,.8f,false);
            f.Checkpoint("Checkpoint_Middle_Trials",35f,0f); f.Checkpoint("Checkpoint_Red_Egg",75f,0f);
            LevelBuilder4_1.Floor(f,"Pedestal_Approach",74f,78f); LevelBuilder4_1.Lava(f,root.transform,"Pedestal_Lava_Moat",78f,86f);
            LevelBuilder4_1.Floor(f,"Obsidian_Sanctuary",86f,104f);
            f.Platform("Entrance_Obsidian_Boundary",new Vector2(-5.5f,3f),new Vector2(1f,8f));
            f.Platform("Sanctuary_Obsidian_Boundary",new Vector2(105f,3f),new Vector2(1f,8f));
            LevelBuilder4_1.Hint(root.transform,"Pedestal_Jump_Hint",new Vector2(75f,2.8f),"TU ÚLTIMO PEQUEÑO\nDOBLE SALTO + DASH →",Red);
            BuildSanctuary(f,root.transform,player,combat,fire);
            var waveGo=new GameObject("Temple_Roar_Wave");waveGo.transform.SetParent(root.transform,false); var wave=waveGo.AddComponent<RoarWaveVisual2D>();
            var line=waveGo.GetComponent<LineRenderer>();line.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
            line.startWidth=.08f;line.endWidth=.08f;line.sortingOrder=8;
            CaveLevelSceneFactory.Set(wave,"_playerSource",player);CaveLevelSceneFactory.Set(wave,"_radius",player.Config.RoarRadius);
            CaveLevelSceneFactory.Set(wave,"_duration",player.Config.RoarDuration);CaveLevelSceneFactory.Set(wave,"_halfAngle",player.Config.RoarHalfAngle);
            var camera=Camera.main;camera.orthographicSize=6f;camera.backgroundColor=new Color(.055f,.018f,.025f);camera.transform.position=new Vector3(0f,2f,-10f);
            var follow=camera.GetComponent<Camera2DFollow>();follow.SetTarget(player.transform);follow.SetBounds(new Vector2(0f,-1.5f),new Vector2(101f,8f));
            CaveLevelSceneFactory.Set(follow,"_offset",new Vector2(0f,1.5f));CaveLevelSceneFactory.Set(follow,"_lookAheadDistance",1.25f);
            LevelBuilder4_1.Atmosphere(f,root.transform,camera.transform);
            Object.DestroyImmediate(root.transform.Find("Volcano_Parallax_0").gameObject);
            Object.DestroyImmediate(root.transform.Find("Volcano_Parallax_2").gameObject);
            var prologue=new GameObject("Antechamber_Prologue");prologue.transform.SetParent(root.transform,false);prologue.transform.position=new Vector2(1f,1f);
            prologue.AddComponent<BoxCollider2D>().isTrigger=true;prologue.AddComponent<NarrativePrologueTrigger>().Configure("LA ANTECÁMARA DEL FUEGO","Cuatro puertas. Cuatro fuerzas.\nDetrás de ellas late el corazón que me falta.",Red,5f);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,ScenePath);
            var scenes=EditorBuildSettings.scenes.ToList();scenes.RemoveAll(s=>s.path==ScenePath);
            scenes.Insert(scenes.FindIndex(s=>s.path==LevelBuilder4_3.ScenePath)+1,new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
        }
        private static void BuildDoubleJumpHall(CaveLevelSceneFactory f,Transform root)
        {
            LevelBuilder4_1.Floor(f,"Hall_Of_Wings",-6f,25f);
            var first=f.Platform("Double_Jump_Column_0",new Vector2(9.5f,1.1f),new Vector2(5f,2.2f));
            var second=f.Platform("Double_Jump_Column_1",new Vector2(15.5f,1.6f),new Vector2(3f,3.2f));
            first.GetComponent<SpriteRenderer>().color=new Color(.09f,.07f,.12f);second.GetComponent<SpriteRenderer>().color=first.GetComponent<SpriteRenderer>().color;
            LevelBuilder4_1.Hint(root,"Wings_Trial_Hint",new Vector2(3.5f,3.5f),"I · ALAS\nDOBLE SALTO A LAS CORNISAS",Color.cyan);
        }
        private static TempleTrialGates2D BuildPoundHall(CaveLevelSceneFactory f,Transform root,PlayerController player)
        {
            f.Platform("Pillar_Refuge",new Vector2(28.5f,-2.75f),new Vector2(7f,1.5f));
            var first=f.BreakableFloor("Seismic_Pillar_0",new Vector2(27f,-.2f),new Vector2(4f,.4f));
            var second=f.BreakableFloor("Seismic_Pillar_1",new Vector2(27f,-1.2f),new Vector2(4f,.4f));
            var gate=f.Visual("Pound_Trial_Gate",root,new Vector2(30.5f,3f),new Vector2(.7f,10f),new Color(.45f,.18f,.12f),true);gate.AddComponent<BoxCollider2D>();
            var status=LevelBuilder4_1.Hint(root,"Pillar_Trial_Status",new Vector2(24f,3.6f),"II · TIERRA\nPISOTÓN ↓ ATRAVIESA LOS DOS PILARES",Gold);
            var go=new GameObject("Temple_Trial_Gates");go.transform.SetParent(root,false);var trials=go.AddComponent<TempleTrialGates2D>();
            CaveLevelSceneFactory.Set(trials,"_playerSource",player);CaveLevelSceneFactory.Set(trials,"_poundGate",gate.GetComponent<Collider2D>());
            CaveLevelSceneFactory.Set(trials,"_poundGateVisual",gate.GetComponent<SpriteRenderer>());CaveLevelSceneFactory.Set(trials,"_poundStatus",status);
            SetReferences(trials,"_pillars",new Object[]{first,second});return trials;
        }
        private static DashBreakableBarrier2D BuildDashHall(CaveLevelSceneFactory f,Transform root,PlayerController player,Transform fire)
        {
            LevelBuilder4_1.Floor(f,"Dash_Trial_Approach",32f,44f); LevelBuilder4_1.Lava(f,root,"Dash_Fire_Moat",44f,52f);
            LevelBuilder4_1.Floor(f,"Roar_Trial_Approach",52f,58f);
            var go=new GameObject("Timed_Fire_Current");go.transform.SetParent(fire,false);go.transform.position=new Vector2(48f,3.1f);
            var col=go.AddComponent<BoxCollider2D>();col.size=new Vector2(6f,1.2f);col.isTrigger=true;
            var flame=f.Visual("Fire_Current",go.transform,Vector2.zero,col.size,Red,true);flame.SetActive(false);
            var vent=f.Visual("Current_Vent",go.transform,new Vector2(3.4f,0f),new Vector2(.5f,1.5f),new Color(.15f,.1f,.08f));
            var status=LevelBuilder4_1.Hint(root,"Dash_Current_Status",new Vector2(43f,4.8f),"PASA",Color.cyan);
            var geyser=go.AddComponent<LavaGeyser2D>();CaveLevelSceneFactory.Set(geyser,"_config",AssetDatabase.LoadAssetAtPath<GeyserConfigSO>("Assets/_Project/ScriptableObjects/GeyserConfig.asset"));
            CaveLevelSceneFactory.Set(geyser,"_playerSource",player);CaveLevelSceneFactory.Set(geyser,"_flamePillarRoot",flame.transform);
            CaveLevelSceneFactory.Set(geyser,"_ventBaseRenderer",vent.GetComponent<SpriteRenderer>());CaveLevelSceneFactory.Set(geyser,"_warningLabel",status);
            var grid=f.Visual("Air_Dash_Trial_Grid",root,new Vector2(50f,2.5f),new Vector2(.6f,7f),new Color(.8f,.26f,.12f),true);grid.AddComponent<BoxCollider2D>();
            var barrier=grid.AddComponent<DashBreakableBarrier2D>();CaveLevelSceneFactory.Set(barrier,"_barrierRenderer",grid.GetComponent<SpriteRenderer>());
            LevelBuilder4_1.Hint(root,"Dash_Trial_Hint",new Vector2(38f,3.6f),"III · IMPULSO\nESPERA PASA · DASH → ROMPE LA REJA",Gold);return barrier;
        }
        private static void BuildRoarHall(CaveLevelSceneFactory f,Transform root,PlayerController player)
        {
            var lava=LevelBuilder4_1.Lava(f,root,"Basalt_Alignment_Lava",58f,74f);
            var boulder=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Resources/Resource_RoarBoulder_Volcano.prefab"));
            boulder.name="Basalt_Alignment_Boulder";boulder.transform.SetParent(root,false);boulder.transform.position=new Vector2(56f,1.8f);
            CaveLevelSceneFactory.Set(boulder.GetComponent<PushableBoulder2D>(),"_playerSource",player);CaveLevelSceneFactory.Set(boulder.GetComponent<PushableBoulder2D>(),"_lava",lava);
            f.Visual("Bridge_Alignment_Marker",root,new Vector2(61f,-.25f),new Vector2(4.2f,.08f),Gold,true);
            LevelBuilder4_1.Hint(root,"Roar_Trial_Hint",new Vector2(54f,4.5f),"IV · VOZ\nROAR → ALINEA EL PUENTE\nAPÓYATE Y ENCADENA DOBLE SALTO + DASH",Color.cyan);
        }
        private static void BuildSanctuary(CaveLevelSceneFactory f,Transform root,PlayerController player,GameObject combat,GameObject fire)
        {
            var portal=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Resources/Resource_LevelExitPortal_Universal.prefab"));
            portal.name="Portal_Exit_To_Boss_Final";portal.transform.SetParent(root,false);portal.transform.position=new Vector2(95f,1.5f);
            var exit=portal.GetComponent<LevelExit2D>();CaveLevelSceneFactory.Set(exit,"_nextSceneName","Boss_Final");
            CaveLevelSceneFactory.Set(exit,"_levelTitle","EL NIDO VUELVE A ESTAR COMPLETO");
            CaveLevelSceneFactory.Set(exit,"_victoryMessage","Los cuatro están a salvo.\nEl Rey Ladrón emerge del cráter. Protege a tus pequeños.");portal.SetActive(false);
            f.Visual("Obsidian_Egg_Pedestal",root,new Vector2(90f,.15f),new Vector2(2.5f,.3f),new Color(.043f,.035f,.04f),true);
            var eggGo=f.Visual("Red_Egg",root,new Vector2(90f,1.1f),new Vector2(.8f,1.1f),Red,true);
            eggGo.GetComponent<SpriteRenderer>().sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");eggGo.AddComponent<CircleCollider2D>().isTrigger=true;
            CaveLevelSceneFactory.Light(eggGo.transform,Vector2.zero,Red,4f,1f);var egg=eggGo.AddComponent<GreenEggRescue2D>();
            egg.Configure(EggType.RedEgg,"¡CUARTO RESCATE: EL HUEVO ROJO!","Los cuatro están aquí.\nMi nido vuelve a estar completo.\n(Cuatro de cuatro rescatados)",Red,exit);
            var guardian=new GameObject("Thief_King_Guardian_Teaser");guardian.transform.SetParent(root,false);guardian.transform.position=new Vector2(93.5f,-2f);
            f.Visual("King_Left_Leg",guardian.transform,new Vector2(-.6f,-.8f),new Vector2(.55f,1.4f),new Color(.12f,.04f,.035f),true);
            f.Visual("King_Right_Leg",guardian.transform,new Vector2(.7f,-.8f),new Vector2(.55f,1.4f),new Color(.12f,.04f,.035f),true);
            f.Visual("King_Torso",guardian.transform,new Vector2(0f,1f),new Vector2(2.5f,2f),new Color(.12f,.04f,.035f),true);
            f.Visual("King_Head",guardian.transform,new Vector2(-.7f,2.5f),new Vector2(2.5f,1.2f),new Color(.23f,.07f,.045f),true);
            f.Visual("King_Jaw",guardian.transform,new Vector2(-1.1f,2.0f),new Vector2(2f,.3f),new Color(.12f,.04f,.035f),true);
            f.Visual("King_Eye",guardian.transform,new Vector2(-1.5f,2.7f),new Vector2(.2f,.15f),Gold,true);
            var scar=f.Visual("King_Scar",guardian.transform,new Vector2(-.4f,2.55f),new Vector2(.04f,.6f),Red,true);scar.transform.localRotation=Quaternion.Euler(0,0,-25);
            for(int i=0;i<5;i++) f.Visual("King_Tooth_"+i,guardian.transform,new Vector2(-1.9f+i*.35f,2.17f),new Vector2(.12f,.18f),Color.white,true);
            guardian.SetActive(false);
            var go=new GameObject("Red_Egg_Sanctuary");go.transform.SetParent(root,false);var sanctuary=go.AddComponent<RedEggSanctuary2D>();
            CaveLevelSceneFactory.Set(sanctuary,"_egg",egg);CaveLevelSceneFactory.Set(sanctuary,"_playerSource",player);CaveLevelSceneFactory.Set(sanctuary,"_combatRoot",combat);
            CaveLevelSceneFactory.Set(sanctuary,"_fireRoot",fire);CaveLevelSceneFactory.Set(sanctuary,"_guardian",guardian);
            var shake=new SerializedObject(player).FindProperty("_cameraShakeChannel").objectReferenceValue;if(shake!=null) CaveLevelSceneFactory.Set(sanctuary,"_shake",shake);
            var visualRoot=(Transform)new SerializedObject(player).FindProperty("_visualRoot").objectReferenceValue;
            var aura=new GameObject("Four_Egg_Aura");aura.transform.SetParent(visualRoot,false);
            Color[] colors={new Color(.2f,1f,.35f),new Color(.2f,.65f,1f),new Color(.65f,.25f,1f),Red};
            var sprites=new Object[4];var lights=new Object[4];
            for(int i=0;i<4;i++)
            {
                Vector2 p=new Vector2(-.28f+(i%2)*.18f,.13f+(i/2)*.22f);
                var e=f.Visual("Carried_Egg_"+(i+1),aura.transform,p,new Vector2(.16f,.21f),colors[i],true);
                e.GetComponent<SpriteRenderer>().sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");e.GetComponent<SpriteRenderer>().sortingOrder=9;
                sprites[i]=e.GetComponent<SpriteRenderer>();CaveLevelSceneFactory.Light(aura.transform,p,colors[i],.7f,.15f);lights[i]=aura.transform.GetChild(aura.transform.childCount-1).GetComponent<Light2D>();
            }
            SetReferences(sanctuary,"_auraEggs",sprites);SetReferences(sanctuary,"_auraLights",lights);
        }
        private static void SetReferences(Object target,string field,Object[] values)
        {
            var so=new SerializedObject(target);var array=so.FindProperty(field);array.arraySize=values.Length;
            for(int i=0;i<values.Length;i++) array.GetArrayElementAtIndex(i).objectReferenceValue=values[i];so.ApplyModifiedPropertiesWithoutUndo();
        }
        [MenuItem("Alma/📂 Cargar Nivel 4-4")]
        public static void LoadLevel() { if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath); }
    }
}
#endif
