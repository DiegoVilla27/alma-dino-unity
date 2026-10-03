using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using AlmaDino.Shared.Visuals;
using UnityEditor;
using UnityEngine;

namespace AlmaDino.Core.Editor
{
    internal static class PrefabStructure
    {
        internal static string Signature(GameObject root)
        {
            var text = new StringBuilder();
            Append(root.transform, text, true);
            return text.ToString();
        }

        private static void Append(Transform node, StringBuilder text, bool root)
        {
            text.Append('[');
            if (!root) text.Append(Regex.Replace(Regex.Replace(node.name, @"__Part\d+$", ""), @"\d+", "#"));
            foreach (var component in node.GetComponents<Component>())
            {
                if (component == null) throw new System.InvalidOperationException($"Missing script: {node.name}");
                if (component is PrefabSprite2D) continue;
                text.Append('|').Append(component.GetType().FullName);
                if (component is Collider2D collider) text.Append(collider.isTrigger ? ":trigger" : ":solid");
            }
            foreach (Transform child in node) Append(child, text, false);
            text.Append(']');
        }

        internal static void AddSpriteSlots(GameObject root)
        {
            foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                var slot = renderer.GetComponent<PrefabSprite2D>();
                if (slot == null) slot = renderer.gameObject.AddComponent<PrefabSprite2D>();
                slot.Initialize();
            }
        }

        internal static Dictionary<GameObject, string> MakeNamesUnique(GameObject root)
        {
            var originals = new Dictionary<GameObject, string>();
            foreach (var parent in root.GetComponentsInChildren<Transform>(true))
            foreach (var siblings in parent.Cast<Transform>().GroupBy(t => t.name).Where(g => g.Count() > 1))
            {
                int index = 0;
                foreach (var child in siblings)
                {
                    originals.Add(child.gameObject, child.name);
                    child.name += "__Part" + index++;
                }
            }
            return originals;
        }

        internal static string Category(GameObject root)
        {
            var scripts = root.GetComponents<MonoBehaviour>().Where(c => c != null && c is not PrefabSprite2D).ToArray();
            var names = scripts.Select(c => c.GetType().Name).ToArray();
            if (names.Contains("PlayerController")) return "Player";
            if (names.Any(n => n is "PoisonBubble2D" or "RollingFruitProjectile2D" or "BossFallingCrystal2D" or "ReflectableMeteor2D")) return "Projectiles";
            if (names.Any(n => n is "GiantMonkeyBoss2D" or "PrehistoricArmadilloBoss2D" or "PterodactylBoss2D" or "ThiefKingBoss2D")) return "Bosses";
            if (names.Any(n => n is "CarnivorousPlant2D" or "CrystalBeetle2D" or "CaveBat2D" or "PoisonToad2D" or "MagmaSalamander2D")) return "Enemies";
            if (names.Any(n => n is "HazardTrigger2D" or "CrushingCeiling2D" or "LavaGeyser2D" or "RisingHazardFloor2D" or "FractureFlameJet2D" or "FractureEruption2D" or "BellFlameDoor2D" or "ThiefKingHazard2D")) return "Traps";
            if (names.Any(n => n is "NarrativePrologueTrigger" or "ThiefMonkeyTeaser2D" or "RoarWaveVisual2D" or "ParallaxLayer2D")) return "Narrative";
            if (scripts.Length > 0 && scripts.All(c => c.GetType().Namespace?.StartsWith("AlmaDino") != true)) return "Narrative";
            if (scripts.Length > 0 || root.GetComponentsInChildren<Collider2D>(true).Length > 0) return "Resources";
            return "Narrative";
        }

        internal static string Label(GameObject root)
        {
            var behaviour = root.GetComponents<MonoBehaviour>().FirstOrDefault(c => c != null && c is not PrefabSprite2D);
            var label = behaviour != null ? behaviour.GetType().Name : root.name;
            var prefix = Category(root) switch
            {
                "Enemies" => "Enemy", "Traps" => "Trap", "Projectiles" => "Projectile",
                "Player" => "Player", "Bosses" => "Boss", "Narrative" => "Scenery", _ => "Resource"
            };
            return prefix + "_" + Regex.Replace(Regex.Replace(label, @"[^a-zA-Z0-9_]+", "_"), "_+", "_").Trim('_') + "_Universal";
        }

        internal static string AssemblyLabel(GameObject root)
        {
            var behaviour = root.GetComponents<MonoBehaviour>().FirstOrDefault(c => c != null && c is not PrefabSprite2D);
            if (behaviour != null) return behaviour.GetType().Name;
            if (root.GetComponent<PlatformEffector2D>() != null) return "OneWayPlatform";
            if (root.GetComponent<Collider2D>() != null) return "SolidPlatform";
            if (root.GetComponent<SpriteRenderer>() != null && root.transform.childCount == 0) return "Decoration";
            return Regex.Replace(root.name, @"[^a-zA-Z_]+", "").Trim('_');
        }

        internal static string UniquePath(string folder, string label)
        {
            var path = folder + "/" + label + ".prefab";
            int variant = 1;
            while (System.IO.File.Exists(path)) path = folder + "/" + label + "_Variant" + variant++ + ".prefab";
            return path;
        }

        internal static GameObject SceneTarget(Object value)
        {
            if (value == null || EditorUtility.IsPersistent(value)) return null;
            return value is Component component ? component.gameObject : value as GameObject;
        }
    }
}
