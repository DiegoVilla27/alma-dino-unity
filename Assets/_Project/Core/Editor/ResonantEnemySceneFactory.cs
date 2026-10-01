#if UNITY_EDITOR
using AlmaDino.Features.Enemies;
using AlmaDino.Features.Player.Controllers;
using UnityEngine;

namespace AlmaDino.Core.Editor
{
    internal sealed class ResonantEnemySceneFactory
    {
        private readonly CaveLevelSceneFactory _f;
        private readonly Transform _root;
        private readonly CrystalEnemyConfigSO _config;
        private readonly PlayerController _player;
        public ResonantEnemySceneFactory(CaveLevelSceneFactory f, Transform root, CrystalEnemyConfigSO config, PlayerController player)
        { _f = f; _root = root; _config = config; _player = player; }

        public void Beetle(string name, Vector2 position, Vector2 bounds)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root, false);
            go.transform.position = position;
            go.AddComponent<BoxCollider2D>().size = new Vector2(1.4f, 0.8f);
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 2.2f;
            body.mass = 2f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var visual = new GameObject("Beetle_Visual");
            visual.transform.SetParent(go.transform, false);
            var shell = _f.Visual("Reflective_Carapace", visual.transform, Vector2.zero, new Vector2(1.4f, 0.7f), new Color(0.88f, 0.67f, 1f), true);
            shell.GetComponent<SpriteRenderer>().sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            for (int i = 0; i < 3; i++)
            {
                var spike = _f.Visual("Diamond_Spike", visual.transform, new Vector2(-0.45f + i * 0.45f, 0.36f),
                    new Vector2(0.25f, 0.25f), new Color(0.85f, 0.95f, 1f), true);
                spike.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                _f.Visual("Beetle_Leg", visual.transform, new Vector2(-0.5f + i * 0.5f, -0.3f),
                    new Vector2(0.1f, 0.3f), new Color(0.9f, 0.4f, 0.05f), true);
            }
            var timerRoot = new GameObject("Flip_Time");
            timerRoot.transform.SetParent(go.transform, false);
            timerRoot.transform.localPosition = new Vector2(0f, 0.7f);
            _f.Visual("Remaining_Flip_Time", timerRoot.transform, Vector2.zero, new Vector2(1.3f, 0.08f), new Color(0.1f, 0.95f, 0.8f), true);
            var beetle = go.AddComponent<CrystalBeetle2D>();
            CaveLevelSceneFactory.Set(beetle, "_config", _config);
            CaveLevelSceneFactory.Set(beetle, "_playerSource", _player);
            CaveLevelSceneFactory.Set(beetle, "_patrolBounds", bounds);
            CaveLevelSceneFactory.Set(beetle, "_visual", visual.transform);
            CaveLevelSceneFactory.Set(beetle, "_shell", shell.GetComponent<SpriteRenderer>());
            CaveLevelSceneFactory.Set(beetle, "_timerBar", timerRoot.transform);
        }

        public void Bat(string name, Vector2 position, float depth)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root, false);
            go.transform.position = position;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1.3f, 0.55f);
            collider.isTrigger = true;
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            var visual = _f.Visual("Bat_Wings", go.transform, Vector2.zero, new Vector2(0.4f, 0.5f), new Color(0.55f, 0.48f, 0.75f), true);
            visual.GetComponent<SpriteRenderer>().sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            for (int side = -1; side <= 1; side += 2)
            {
                var wing = _f.Visual("Bat_Wing", go.transform, new Vector2(side * 0.4f, 0f),
                    new Vector2(0.7f, 0.18f), new Color(0.55f, 0.48f, 0.75f), true);
                wing.transform.localRotation = Quaternion.Euler(0f, 0f, side * 25f);
            }
            var warning = new GameObject("Bat_Warning_Sign");
            warning.transform.SetParent(go.transform, false);
            warning.transform.localPosition = Vector2.up * 0.7f;
            var label = warning.AddComponent<TextMesh>();
            label.text = "!";
            label.fontSize = 40;
            label.characterSize = 0.15f;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(1f, 0.85f, 0.2f);
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            warning.GetComponent<MeshRenderer>().sharedMaterial = label.font.material;
            warning.GetComponent<MeshRenderer>().sortingOrder = 5;
            var bat = go.AddComponent<CaveBat2D>();
            CaveLevelSceneFactory.Set(bat, "_warningSign", warning);
            CaveLevelSceneFactory.Set(bat, "_config", _config);
            CaveLevelSceneFactory.Set(bat, "_playerSource", _player);
            CaveLevelSceneFactory.Set(bat, "_diveDepth", depth);
            CaveLevelSceneFactory.Set(bat, "_visual", visual.GetComponent<SpriteRenderer>());
        }
    }
}
#endif
