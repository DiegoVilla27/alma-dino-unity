#if UNITY_EDITOR
using AlmaDino.Features.Environment;
using AlmaDino.Features.Player.Controllers;
using UnityEditor;
using UnityEngine;

namespace AlmaDino.Core.Editor
{
    internal sealed class EchoPuzzleSceneFactory
    {
        private readonly CaveLevelSceneFactory _visuals;
        private readonly Transform _root;
        private readonly SeesawConfigSO _config;
        private readonly PlayerController _player;

        public EchoPuzzleSceneFactory(CaveLevelSceneFactory visuals, Transform root, SeesawConfigSO config, PlayerController player)
        {
            _visuals = visuals;
            _root = root;
            _config = config;
            _player = player;
        }

        public RuneSwitch2D Station(string name, float x)
        {
            var board = new GameObject(name);
            board.transform.SetParent(_root, false);
            board.transform.position = new Vector2(x, 1.2f);
            board.AddComponent<BoxCollider2D>().size = new Vector2(6f, 0.5f);
            var body = board.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            _visuals.Visual("Basalt_Plank", board.transform, Vector2.zero, new Vector2(6f, 0.5f), new Color(0.13f, 0.17f, 0.22f));
            _visuals.Visual("Amber_Rune_Rim", board.transform, new Vector2(0f, 0.23f), new Vector2(6f, 0.07f), new Color(0.91f, 0.85f, 0.65f), true);
            _visuals.Visual("Stone_Fulcrum", _root, new Vector2(x, 0.45f), new Vector2(0.7f, 0.9f), new Color(0.3f, 0.35f, 0.39f));
            var seesaw = board.AddComponent<SeesawPlatform2D>();
            CaveLevelSceneFactory.Set(seesaw, "_config", _config);
            CaveLevelSceneFactory.Set(seesaw, "_playerSource", _player);
            var weightObject = _visuals.Visual(name + "_Counterweight", _root, new Vector2(x + 2.3f, 1.85f), Vector2.one * 0.7f, new Color(0.8f, 0.72f, 0.46f));
            weightObject.AddComponent<BoxCollider2D>();
            var weightBody = weightObject.AddComponent<Rigidbody2D>();
            weightBody.bodyType = RigidbodyType2D.Kinematic;
            weightBody.gravityScale = 1.8f;
            weightBody.mass = 4f;
            weightBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            weightBody.interpolation = RigidbodyInterpolation2D.Interpolate;
            var weight = weightObject.AddComponent<CatapultWeight2D>();
            CaveLevelSceneFactory.Set(weight, "_config", _config);
            CaveLevelSceneFactory.Set(weight, "_plank", board.transform);
            CaveLevelSceneFactory.Set(seesaw, "_counterweight", weight);
            var switchObject = _visuals.Visual(name + "_Ceiling_Rune", _root, new Vector2(x + 2.3f, 6f), new Vector2(1.6f, 0.5f), new Color(1f, 0.65f, 0.15f), true);
            switchObject.AddComponent<BoxCollider2D>().isTrigger = true;
            var rune = switchObject.AddComponent<RuneSwitch2D>();
            CaveLevelSceneFactory.Set(rune, "_config", _config);
            CaveLevelSceneFactory.Set(rune, "_indicator", switchObject.GetComponent<SpriteRenderer>());
            CaveLevelSceneFactory.Light(_root, new Vector2(x + 2.3f, 5.8f), new Color(1f, 0.6f, 0.1f), 4f, 0.7f);
            Label(name + "_Pound_Sign", new Vector2(x - 2.3f, 2.9f), "PISOTÓN ↓");
            Label(name + "_Launch_Sign", new Vector2(x + 2.1f, 3.5f), "CORRE →");
            return rune;
        }

        public TimedRuneGate2D Gate(string name, float x, params RuneSwitch2D[] switches)
        {
            var go = _visuals.Visual(name, _root, new Vector2(x, 7f), new Vector2(0.8f, 15f), new Color(0.82f, 0.54f, 0.18f), true);
            go.AddComponent<BoxCollider2D>();
            var gate = go.AddComponent<TimedRuneGate2D>();
            CaveLevelSceneFactory.Set(gate, "_config", _config);
            CaveLevelSceneFactory.Set(gate, "_playerSource", _player);
            CaveLevelSceneFactory.Set(gate, "_visual", go.GetComponent<SpriteRenderer>());
            var timer = _visuals.Visual("Remaining_Time_Bar", go.transform, new Vector2(0.7f, 0f), new Vector2(0.15f, 1f), new Color(0.3f, 1f, 0.55f), true);
            CaveLevelSceneFactory.Set(gate, "_timerBar", timer.transform);
            var serialized = new SerializedObject(gate);
            var array = serialized.FindProperty("_switches");
            array.arraySize = switches.Length;
            for (int i = 0; i < switches.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = switches[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Label(name + "_Time_Sign", new Vector2(x - 0.8f, 3.8f), switches.Length > 1 ? "DOS RUNAS · 4 s" : "4 s →");
            return gate;
        }

        internal void Label(string name, Vector2 position, string text)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root, false);
            go.transform.position = position;
            var label = go.AddComponent<TextMesh>();
            label.text = text;
            label.fontSize = 40;
            label.characterSize = 0.08f;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(0.95f, 0.87f, 0.65f);
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = label.font.material;
            renderer.sortingOrder = 4;
        }
    }
}
#endif
