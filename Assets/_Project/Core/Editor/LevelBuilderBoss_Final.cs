#if UNITY_EDITOR
using System.IO;
using System.Linq;
using AlmaDino.Core.Events;
using AlmaDino.Features.Boss.Controllers;
using AlmaDino.Features.Boss.ScriptableObjects;
using AlmaDino.Features.Camera;
using AlmaDino.Features.Environment;
using AlmaDino.Features.Player.Controllers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AlmaDino.Core.Editor
{
    public static class LevelBuilderBoss_Final
    {
        public const string ScenePath = "Assets/Scenes/World_4_Volcano/Boss_Final.unity";
        private static readonly Color Magma = new Color(1f,.27f,.04f);
        [MenuItem("Tools/Alma/Construir Jefe Final - El Rey Ladrón")]
        public static void Build()
        {
            if(EditorApplication.isPlaying) return;
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if(!File.Exists(ScenePath)) AssetDatabase.CopyAsset(LevelBuilder4_4.ScenePath,ScenePath);
            var scene = EditorSceneManager.OpenScene(ScenePath);
            Object.DestroyImmediate(GameObject.Find("--- LEVEL ---"));
            var aura = GameObject.Find("Four_Egg_Aura"); if(aura != null) Object.DestroyImmediate(aura);
            var root = new GameObject("--- LEVEL ---"); var f = new CaveLevelSceneFactory(root.transform);
            var player = GameObject.Find("Alma (Player)").GetComponent<PlayerController>(); player.transform.position = new Vector2(0f,.7f);
            foreach(var ability in new[]{"_doubleJumpUnlocked","_groundPoundUnlocked","_dashUnlocked","_roarUnlocked"}) CaveLevelSceneFactory.Set(player,ability,true);
            CaveLevelSceneFactory.Set(player,"_fallDeathY",-8f); SceneSetupValidator.SetupAlmaVisuals(player.gameObject); SceneSetupValidator.SetupTouchControls();
            var go = new GameObject("Thief_King_Fight"); go.transform.SetParent(root.transform,false); var boss = go.AddComponent<ThiefKingBoss2D>();
            const string configPath = "Assets/_Project/ScriptableObjects/ThiefKingConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<ThiefKingConfigSO>(configPath);
            if(config == null) { config = ScriptableObject.CreateInstance<ThiefKingConfigSO>(); AssetDatabase.CreateAsset(config,configPath); }
            CaveLevelSceneFactory.Set(boss,"_config",config); CaveLevelSceneFactory.Set(boss,"_playerSource",player);
            var pso = new SerializedObject(player); CaveLevelSceneFactory.Set(boss,"_shake",pso.FindProperty("_cameraShakeChannel").objectReferenceValue);
            var arena = new GameObject("Basalt_Combat_Altar"); arena.transform.SetParent(root.transform,false);
            var floor = f.Platform("Arena_Floor",new Vector2(9f,-.4f),new Vector2(28f,.8f));floor.transform.SetParent(arena.transform,true);
            f.Platform("Entrance_Boundary",new Vector2(-5.5f,4f),new Vector2(1f,9f));
            var heat = f.Platform("Charge_Heat_Seal",new Vector2(8f,3.5f),new Vector2(.45f,2f));heat.GetComponent<SpriteRenderer>().color=Magma;
            Mechanism(heat,boss,KingMechanism.HeatSeal);
            var plate = f.Platform("Cracked_Dorsal_Plate",new Vector2(12f,1f),new Vector2(4f,.4f));plate.GetComponent<SpriteRenderer>().color=Color.cyan;
            Mechanism(plate,boss,KingMechanism.WeakPlate);
            f.Visual("Dorsal_Crack",plate.transform,Vector2.zero,new Vector2(.03f,.9f),Color.black,true);
            var king = new GameObject("Thief_King_Visual");king.transform.SetParent(root.transform,false);king.transform.position=new Vector2(15f,1.6f);
            var body=f.Visual("Obsidian_Torso",king.transform,new Vector2(1.5f,.8f),new Vector2(4.3f,3f),new Color(.16f,.16f,.17f),true);
            var skull=f.Visual("Ancient_Skull",king.transform,new Vector2(-1f,1.6f),new Vector2(3.2f,1.6f),new Color(.22f,.22f,.23f),true);
            f.Visual("Jaw",king.transform,new Vector2(-1f,.75f),new Vector2(3.1f,.5f),new Color(.12f,.12f,.13f),true);
            var eye=f.Visual("Crimson_Eye",king.transform,new Vector2(-1.7f,1.85f),new Vector2(.25f,.2f),Color.red,true).GetComponent<SpriteRenderer>();eye.sortingOrder=5;
            for(int i=0;i<6;i++) { var tooth=f.Visual("Ancient_Fang_"+i,king.transform,new Vector2(-2.25f+i*.45f,1f),new Vector2(.12f,.35f),new Color(.9f,.8f,.45f),true); tooth.GetComponent<SpriteRenderer>().sortingOrder=4; }
            for(int i=0;i<5;i++) { var scar=f.Visual("Tectonic_Plate_"+i,king.transform,new Vector2(.1f+i*.65f,2.2f),new Vector2(.35f,.8f),Magma,true);scar.transform.localRotation=Quaternion.Euler(0f,0f,-20f); }
            f.Visual("Left_Claw",king.transform,new Vector2(.3f,-.7f),new Vector2(.8f,1.3f),new Color(.18f,.18f,.19f),true);
            f.Visual("Right_Claw",king.transform,new Vector2(2.8f,-.7f),new Vector2(.8f,1.3f),new Color(.18f,.18f,.19f),true);
            var jaw=f.Visual("Sweeping_Jaw",root.transform,new Vector2(18f,.65f),new Vector2(4.4f,2.2f),new Color(.25f,.2f,.19f),true);
            for(int i=0;i<7;i++) f.Visual("Sweep_Fang_"+i,jaw.transform,new Vector2(-.42f+i*.14f,-.35f),new Vector2(.035f,.23f),new Color(1f,.82f,.45f),true).GetComponent<SpriteRenderer>().sortingOrder=4;
            f.Visual("Jaw_Heat_Trail",jaw.transform,new Vector2(.7f,0f),new Vector2(.4f,.65f),Magma,true);
            jaw.SetActive(false);
            var meteor=f.Visual("Roar_Return_Meteor",root.transform,new Vector2(18f,.9f),new Vector2(1.5f,1.5f),Magma,true);meteor.GetComponent<SpriteRenderer>().sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            meteor.AddComponent<CircleCollider2D>().isTrigger=true; var rb=meteor.AddComponent<Rigidbody2D>();rb.bodyType=RigidbodyType2D.Kinematic;Mechanism(meteor,boss,KingMechanism.Meteor);
            var rock=f.Visual("Meteor_Rain_Rock",root.transform,Vector2.zero,new Vector2(1.3f,1.3f),Magma,true);
            Hazard(jaw,boss); Hazard(rock,boss); Hazard(meteor,boss,true);
            var marker=f.Visual("Meteor_Rain_Warning",root.transform,Vector2.zero,new Vector2(2f,.12f),Color.yellow,true);
            var ascent=new GameObject("Collapse_Ascent");ascent.transform.SetParent(root.transform,false);
            var entry=f.Platform("Collapse_Checkpoint_Ledge",new Vector2(20f,-.4f),new Vector2(6f,.8f));entry.transform.SetParent(ascent.transform,true);
            Step(f,ascent.transform,"Double_Jump_Basalt_1",24f,2.2f,4f);
            Step(f,ascent.transform,"Double_Jump_Basalt_2",29f,4.4f,4f);
            Step(f,ascent.transform,"Steam_Refuge",38f,4.4f,3f);
            var curtain=f.Visual("Descending_Fire_Curtain",ascent.transform,new Vector2(32f,10.9f),new Vector2(1f,3.8f),Magma,true);
            curtain.AddComponent<BoxCollider2D>().isTrigger=true;curtain.AddComponent<HazardTrigger2D>();
            var seal=f.Platform("Steam_Obsidian_Seal",new Vector2(38f,4.6f),new Vector2(2f,.4f));seal.transform.SetParent(ascent.transform,true);Mechanism(seal,boss,KingMechanism.SteamSeal);seal.GetComponent<SpriteRenderer>().color=Color.cyan;
            var vent=f.Platform("Unlocked_Steam_Vent",new Vector2(38f,4.35f),new Vector2(2f,.2f));vent.transform.SetParent(ascent.transform,true);
            CaveLevelSceneFactory.Set(vent.AddComponent<BouncyPlatform2D>(),"_bounceVelocity",config.SteamVelocity);vent.GetComponent<SpriteRenderer>().color=Color.white;vent.SetActive(false);
            Step(f,ascent.transform,"Upper_Steam_Landing",34f,9f,4f);
            var anchor=f.Platform("Stalactite_Anchor",new Vector2(38f,11f),new Vector2(4f,.4f));anchor.transform.SetParent(ascent.transform,true);anchor.GetComponent<SpriteRenderer>().color=Color.yellow;Mechanism(anchor,boss,KingMechanism.Anchor);
            OneWay(anchor);
            OneWay(ascent.transform.Find("Upper_Steam_Landing").gameObject);
            var stalactite=f.Visual("Colossal_Stalactite",root.transform,new Vector2(38f,10.8f),new Vector2(2f,2.5f),new Color(.25f,.25f,.28f),true);
            var lava=f.Visual("Rising_Caldera_Magma",root.transform,new Vector2(28f,-4f),new Vector2(70f,2f),Magma,true);
            Hazard(lava,boss);
            foreach(var platform in ascent.GetComponentsInChildren<BoxCollider2D>())
            {
                if(!platform.name.StartsWith("Double_Jump_Basalt")) continue;
                var crumble=platform.gameObject.AddComponent<CrumblingPlatform2D>();
                CaveLevelSceneFactory.Set(crumble,"_playerSource",player);
                CaveLevelSceneFactory.Set(crumble,"_crumbleDelay",1.5f);
                CaveLevelSceneFactory.Set(crumble,"_shakeIntensity",0f);
            }
            var nest=f.Platform("Protected_Four_Egg_Nest",new Vector2(-2f,5f),new Vector2(3f,.5f));
            Color[] colors={Color.green,Color.cyan,new Color(.7f,.3f,1f),new Color(1f,.1f,.25f)};
            for(int i=0;i<4;i++) { var egg=f.Visual("Protected_Egg_"+i,nest.transform,new Vector2(-.35f+i*.23f,1.1f),new Vector2(.16f,1.2f),colors[i],true);egg.GetComponent<SpriteRenderer>().sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png"); }
            var epilogue=new GameObject("Sunset_Epilogue");epilogue.transform.SetParent(root.transform,false);
            var meadow=f.Platform("Home_Meadow",new Vector2(8f,19.5f),new Vector2(28f,1f));meadow.transform.SetParent(epilogue.transform,true);meadow.GetComponent<SpriteRenderer>().color=new Color(.2f,.55f,.25f);
            f.Visual("Sunset_Sky",epilogue.transform,new Vector2(8f,25f),new Vector2(40f,12f),new Color(.95f,.64f,.37f),true).GetComponent<SpriteRenderer>().sortingOrder=-20;
            for(int i=0;i<4;i++) {
                var baby=f.Visual("Hatchling_"+i,epilogue.transform,new Vector2(5f+i*1.1f,20.35f),new Vector2(.7f,.65f),colors[i],true);
                var babyRenderer=baby.GetComponent<SpriteRenderer>();babyRenderer.sprite=player.transform.Find("Visual").GetComponent<SpriteRenderer>().sprite;
                babyRenderer.color=Color.Lerp(Color.white,colors[i],.4f);babyRenderer.sortingOrder=5;
                baby.transform.localScale=Vector3.one*(.65f/babyRenderer.sprite.bounds.size.y);
                f.Visual("Broken_Shell",epilogue.transform,new Vector2(5f+i*1.1f,20.05f),new Vector2(.85f,.18f),Color.white,true);
            }
            LevelBuilder4_1.Hint(epilogue.transform,"Epilogue_End_Title",new Vector2(7f,24f),"— FIN —\nCUATRO LATIDOS. UN HOGAR.",new Color(.12f,.3f,.16f));
            var hatchlings = new SerializedObject(boss); var array = hatchlings.FindProperty("_hatchlings"); array.arraySize = 4;
            for(int i=0;i<4;i++) array.GetArrayElementAtIndex(i).objectReferenceValue = epilogue.transform.Find("Hatchling_"+i);
            hatchlings.ApplyModifiedPropertiesWithoutUndo();
            epilogue.SetActive(false);ascent.SetActive(false);stalactite.SetActive(false);
            var hint=new GameObject("Final_Boss_Hint");hint.transform.SetParent(root.transform,false);var text=hint.AddComponent<TextMesh>();text.fontSize=40;text.characterSize=.045f;text.anchor=TextAnchor.MiddleCenter;
            foreach(var pair in new[]{ ("_king",(Object)king.transform),("_eye",eye),("_heatSeal",heat),("_plate",plate),("_steamSeal",seal),("_vent",vent),("_anchor",anchor),("_ascent",ascent),("_arena",arena),("_epilogue",epilogue),("_nest",nest),("_jaw",jaw.transform),("_meteor",meteor.transform),("_rainRock",rock.transform),("_rainMarker",marker.transform),("_lava",lava.transform),("_stalactite",stalactite.transform),("_hint",text) }) CaveLevelSceneFactory.Set(boss,pair.Item1,pair.Item2);
            var camera=Camera.main;camera.orthographicSize=6f;camera.backgroundColor=new Color(.045f,.018f,.025f);camera.transform.position=new Vector3(0f,2f,-10f);
            var follow=camera.GetComponent<Camera2DFollow>();follow.SetTarget(player.transform);follow.SetBounds(new Vector2(-1f,-1f),new Vector2(42f,28f));
            CaveLevelSceneFactory.Set(follow,"_offset",new Vector2(0f,1.5f));CaveLevelSceneFactory.Set(follow,"_lookAheadDistance",1.25f);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,ScenePath);
            var scenes=EditorBuildSettings.scenes.ToList();scenes.RemoveAll(s=>s.path==ScenePath);scenes.Add(new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
        }
        private static void OneWay(GameObject go)
        {
            go.GetComponent<Collider2D>().usedByEffector=true;
            var effector=go.AddComponent<PlatformEffector2D>();effector.useOneWay=true;effector.useOneWayGrouping=true;
        }
        private static void Hazard(GameObject go,ThiefKingBoss2D boss,bool meteor=false)
        {
            var collider=go.GetComponent<Collider2D>(); if(collider==null) collider=go.AddComponent<BoxCollider2D>();collider.isTrigger=true;
            var hazard=go.AddComponent<ThiefKingHazard2D>();CaveLevelSceneFactory.Set(hazard,"_boss",boss);CaveLevelSceneFactory.Set(hazard,"_meteor",meteor);
        }
        private static void Step(CaveLevelSceneFactory f,Transform parent,string name,float x,float top,float width)
        { var go=f.Platform(name,new Vector2(x,top-.25f),new Vector2(width,.5f));go.transform.SetParent(parent,true); }
        private static void Mechanism(GameObject go,ThiefKingBoss2D boss,KingMechanism kind)
        { var target=go.AddComponent<ThiefKingMechanism2D>();CaveLevelSceneFactory.Set(target,"_boss",boss);CaveLevelSceneFactory.Set(target,"_kind",kind); }
        [MenuItem("Alma/📂 Cargar Jefe Final")]
        public static void Load() { if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath); }
    }
}
#endif
