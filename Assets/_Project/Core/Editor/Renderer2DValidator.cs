#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AlmaDino.Core.Editor
{
    [InitializeOnLoad]
    public static class Renderer2DValidator
    {
        static Renderer2DValidator()
        {
            EditorApplication.delayCall += Validate2DRenderer;
        }

        private static void Validate2DRenderer()
        {
            var rendererData = AssetDatabase.LoadAssetAtPath<Renderer2DData>("Assets/Settings/URP2D_Renderer.asset");
            if (rendererData != null)
            {
                if (rendererData.lightBlendStyles == null || rendererData.lightBlendStyles.Length == 0)
                {
                    Debug.Log("[Renderer2DValidator] Inicializando LightBlendStyles en URP2D_Renderer...");
                    var so = new SerializedObject(rendererData);
                    var prop = so.FindProperty("m_LightBlendStyles");
                    if (prop != null)
                    {
                        prop.arraySize = 4;
                        // 0: Multiply
                        prop.GetArrayElementAtIndex(0).FindPropertyRelative("name").stringValue = "Multiply";
                        prop.GetArrayElementAtIndex(0).FindPropertyRelative("blendMode").enumValueIndex = 1;
                        prop.GetArrayElementAtIndex(0).FindPropertyRelative("maskTextureChannel").enumValueIndex = 0;

                        // 1: Additive
                        prop.GetArrayElementAtIndex(1).FindPropertyRelative("name").stringValue = "Additive";
                        prop.GetArrayElementAtIndex(1).FindPropertyRelative("blendMode").enumValueIndex = 0;
                        prop.GetArrayElementAtIndex(1).FindPropertyRelative("maskTextureChannel").enumValueIndex = 0;

                        // 2: Multiply with Mask
                        prop.GetArrayElementAtIndex(2).FindPropertyRelative("name").stringValue = "Multiply with Mask";
                        prop.GetArrayElementAtIndex(2).FindPropertyRelative("blendMode").enumValueIndex = 1;
                        prop.GetArrayElementAtIndex(2).FindPropertyRelative("maskTextureChannel").enumValueIndex = 1;

                        // 3: Additive with Mask
                        prop.GetArrayElementAtIndex(3).FindPropertyRelative("name").stringValue = "Additive with Mask";
                        prop.GetArrayElementAtIndex(3).FindPropertyRelative("blendMode").enumValueIndex = 0;
                        prop.GetArrayElementAtIndex(3).FindPropertyRelative("maskTextureChannel").enumValueIndex = 1;

                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(rendererData);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        Debug.Log("[Renderer2DValidator] LightBlendStyles configurados con éxito.");
                    }
                }
            }
        }
    }
}
#endif
