using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class Helper {
        public static class Render
        {
            public static void ChangeMaterialFull(Transform tr, Material mat)
            {
                MeshRenderer[] childrenModels = tr.GetComponentsInChildren<MeshRenderer>();
                foreach (var model in childrenModels)
                {
                    Material[] newMaterials = new Material[model.materials.Length];
                    for (int i = 0; i < model.materials.Length; i++)
                    {
                        newMaterials[i] = mat;
                    }
                    model.materials = newMaterials;
                }
            }
            
            public static void ChangeMaterialFull(List<Transform> trList, Material mat)
            {
                foreach (var tr in trList)
                {
                    MeshRenderer[] childrenModels = tr.GetComponentsInChildren<MeshRenderer>();
                    
                    foreach (var model in childrenModels)
                    {
                        Material[] newMaterials = new Material[model.materials.Length];
                        for (int i = 0; i < model.materials.Length; i++)
                        {
                            newMaterials[i] = mat;
                        }
                        model.materials = newMaterials;
                    }
                }
            }
        }
        public static class Layers
        {
            public static void SetLayer(Transform tr, int layer)
            {
                tr.gameObject.layer = layer;
                foreach (Transform child in tr)
                {
                    SetLayer(child, layer);
                }
            }
            
            public static void SetLayer(Transform tr, string layer)
            {
                // почему-то норма меняет только так (с LayerMask.GetMask не работает)
                int layerIndex = LayerMask.NameToLayer(layer);
                if (layerIndex == -1)
                {
                    Debug.LogWarning($"Layer '{layer}' does not exist.");
                    return;
                }

                tr.gameObject.layer = layerIndex;
                foreach (Transform child in tr)
                {
                    SetLayer(child, layer);
                }
            }
            
            public static bool IsLayersEqual(string layer1ByName, int layer2ByInt)
            {
                LayerMask layer1 = LayerMask.GetMask(layer1ByName);
                LayerMask layer2 = 1 << layer2ByInt;

                return (layer1 & layer2) != 0;
            }
            
            public static bool IsLayersEqual(int layer1ByInt, string layer2ByName)
            {
                LayerMask layer1 = 1 << layer1ByInt;
                LayerMask layer2 = LayerMask.GetMask(layer2ByName);

                return (layer1 & layer2) != 0;
            }
        
            public static bool IsLayersEqual(string layer1ByName, string layer2ByName)
            {
                LayerMask layer1 = LayerMask.GetMask(layer1ByName);
                LayerMask layer2 = LayerMask.GetMask(layer2ByName);

                return (layer1 & layer2) != 0;
            }
        
            public static bool IsLayersEqual(int layer1ByInt, int layer2ByInt)
            {
                LayerMask layer1 = 1 << layer1ByInt;
                LayerMask layer2 = 1 << layer2ByInt;

                return (layer1 & layer2) != 0;
            }
        }

        
    }
}
