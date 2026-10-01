#if UNITY_EDITOR
using System;
using AlmaDino.Core.Interfaces;
using AlmaDino.Features.Environment;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace AlmaDino.Core.Editor
{
    internal sealed class CaveLevelSceneFactory
    {
        private readonly Transform _root;
        private readonly Sprite _square;
        private readonly Sprite _circle;
        private readonly Material _lit;
        private readonly Material _unlit;
        private static readonly Color Stone = new Color(0.18f, 0.24f, 0.37f);
        public static readonly Color Crystal = new Color(0.28f, 0.79f, 0.89f);

        public CaveLevelSceneFactory(Transform root)
        {
            _root = root;
            _square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Square.png");
            _circle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            const string materials = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/";
            _lit = AssetDatabase.LoadAssetAtPath<Material>(materials + "Sprite-Lit-Default.mat");
            _unlit = AssetDatabase.LoadAssetAtPath<Material>(materials + "Sprite-Unlit-Default.mat");
        }

        public GameObject Platform(string name, Vector2 center, Vector2 size)
        {
            var go = Visual(name, _root, center, size, Stone);
            go.AddComponent<BoxCollider2D>();
            Visual("Surface_Rim", go.transform, new Vector2(0f, 0.47f),
                new Vector2(1f, 0.06f), new Color(0.39f, 0.48f, 0.61f));
            return go;
        }

        public BreakableGround2D BreakableFloor(string name, Vector2 center, Vector2 size)
        {
            var go = Platform(name, center, size);
            go.GetComponent<SpriteRenderer>().color = new Color(0.67f, 0.71f, 0.74f);
            for (int i = 0; i < 4; i++)
            {
                var crack = Visual("Crack_" + i, go.transform, new Vector2(-0.35f + i * 0.23f, 0f),
                    new Vector2(0.018f, 0.8f), new Color(0.06f, 0.1f, 0.2f));
                crack.transform.localRotation = Quaternion.Euler(0f, 0f, i % 2 == 0 ? 22f : -22f);
            }
            return go.AddComponent<BreakableGround2D>();
        }

        public void Hazard(string name, Vector2 center, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root);
            go.transform.position = center;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = size;
            collider.isTrigger = true;
            go.AddComponent<HazardTrigger2D>();
            int count = Mathf.CeilToInt(size.x / 0.7f);
            for (int i = 0; i < count; i++)
            {
                var shard = Visual("Sharp_Crystal_" + i, go.transform,
                    new Vector2(-size.x * 0.5f + 0.35f + i * 0.7f, 0f),
                    new Vector2(0.5f, size.y), new Color(0.77f, 0.39f, 0.63f), true);
                shard.transform.localRotation = Quaternion.Euler(0f, 0f, 16f);
            }
        }

        public void Checkpoint(string name, float x, float floorTop)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root);
            go.transform.position = new Vector2(x, floorTop + 0.7f);
            var trigger = go.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.7f, 1.4f);
            Visual("Stone_Nest", go.transform, new Vector2(0f, -0.5f), new Vector2(1.8f, 0.35f), Stone);
            var ember = Visual("Checkpoint_Ember", go.transform, Vector2.zero,
                new Vector2(0.35f, 0.45f), Color.white, true);
            ember.GetComponent<SpriteRenderer>().sprite = _circle;
            Set(go.AddComponent<Checkpoint2D>(), "_indicatorRenderer", ember.GetComponent<SpriteRenderer>());
            Light(go.transform, new Vector2(0f, 0.2f), new Color(1f, 0.75f, 0.3f), 2.5f, 0.6f);
        }

        public void Altar(Vector2 position)
        {
            const string path = "Assets/_Project/Prefabs/Universal/Ability_Relic_Altar.prefab";
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            go.name = "Altar_Seismic_Geode";
            go.transform.SetParent(_root);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.85f;
            var relic = go.GetComponent<AbilityRelic2D>();
            Set(relic, "_abilityToUnlock", AbilityType.GroundPound);
            Set(relic, "_relicTitle", "PISOTÓN SÍSMICO");
            Set(relic, "_loreDescription", "Tu amor maternal adquiere la fuerza de la tierra.\nEn el aire, pulsa ABAJO o POUND para quebrar los suelos agrietados.");
            Set(relic, "_glowColor", Crystal);
            foreach (var renderer in go.GetComponentsInChildren<SpriteRenderer>()) renderer.color = Crystal;
            Light(_root, position, Crystal, 3.5f, 0.9f);
        }

        public GameObject Visual(string name, Transform parent, Vector2 position, Vector2 size, Color color, bool unlit = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = _square;
            renderer.sharedMaterial = unlit ? _unlit : _lit;
            renderer.color = color;
            renderer.sortingOrder = parent == _root ? 0 : 1;
            return go;
        }

        public static void Light(Transform parent, Vector2 position, Color color, float radius, float intensity)
        {
            var go = new GameObject("Crystal_Light");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.pointLightOuterRadius = radius;
            light.pointLightInnerRadius = radius * 0.15f;
        }

        public static void Set(Object target, string name, object value)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(name);
            if (property == null) throw new ArgumentException("Missing serialized field: " + name);
            switch (value)
            {
                case bool flag: property.boolValue = flag; break;
                case float number: property.floatValue = number; break;
                case string text: property.stringValue = text; break;
                case Vector2 vector: property.vector2Value = vector; break;
                case Color color: property.colorValue = color; break;
                case Enum enumValue: property.enumValueIndex = Convert.ToInt32(enumValue); break;
                case Object reference: property.objectReferenceValue = reference; break;
                default: throw new ArgumentException("Unsupported serialized value for " + name);
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
