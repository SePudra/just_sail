using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace JustSail.Editor
{
    public static class OceanBake
    {
        private const string AssetPath = "Assets/JustSail/Generated/Seabed.asset";
        private const int Resolution = 512;
        private const float Margin = 200f;
        private const float Floor = -60f;

        public static void Bake(SeaSurface sea)
        {
            if (sea == null) throw new ArgumentNullException(nameof(sea));

            MeshCollider[] islands = UnityEngine.Object.FindObjectsByType<MeshCollider>(FindObjectsSortMode.None);
            int count = 0;
            for (int i = 0; i < islands.Length; i++)
            {
                MeshCollider collider = islands[i];
                if (collider.gameObject.layer == 6 && collider.gameObject.scene == sea.gameObject.scene &&
                    collider.enabled && collider.sharedMesh != null)
                    islands[count++] = collider;
            }
            if (count != 2) throw new InvalidOperationException($"Expected two layer-6 island MeshColliders in the sea scene; found {count}.");

            Physics.SyncTransforms();

            Bounds combined = islands[0].bounds;
            combined.Encapsulate(islands[1].bounds);
            float minX = combined.min.x - Margin;
            float minZ = combined.min.z - Margin;
            float width = combined.size.x + Margin * 2f;
            float height = combined.size.z + Margin * 2f;
            float rayY = combined.max.y + 1f;
            float rayDistance = rayY - combined.min.y + 1f;
            var pixels = new float[Resolution * Resolution];

            for (int z = 0; z < Resolution; z++)
            {
                float worldZ = minZ + (z + 0.5f) * (height / Resolution);
                for (int x = 0; x < Resolution; x++)
                {
                    float worldX = minX + (x + 0.5f) * (width / Resolution);
                    float topY = Floor;
                    bool hitAny = false;
                    float nearest = rayDistance;
                    var ray = new Ray(new Vector3(worldX, rayY, worldZ), Vector3.down);

                    for (int i = 0; i < 2; i++)
                    {
                        MeshCollider collider = islands[i];
                        Bounds b = collider.bounds;
                        if (worldX < b.min.x || worldX > b.max.x || worldZ < b.min.z || worldZ > b.max.z)
                            continue;
                        if (collider.Raycast(ray, out RaycastHit hit, nearest))
                        {
                            nearest = hit.distance;
                            topY = hit.point.y;
                            hitAny = true;
                        }
                    }
                    pixels[z * Resolution + x] = hitAny ? topY : Floor;
                }
            }

            Directory.CreateDirectory("Assets/JustSail/Generated");
            UnityEngine.Object existing = AssetDatabase.LoadMainAssetAtPath(AssetPath);
            if (existing != null && !(existing is Texture2D))
                throw new InvalidOperationException($"{AssetPath} is occupied by a non-texture asset.");

            var baked = new Texture2D(Resolution, Resolution, TextureFormat.RFloat, false, true)
            {
                name = "Seabed",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 0
            };
            try
            {
                baked.SetPixelData(pixels, 0);
                baked.Apply(false, false);

                if (existing is Texture2D current)
                {
                    EditorUtility.CopySerialized(baked, current);
                    EditorUtility.SetDirty(current);
                    UnityEngine.Object.DestroyImmediate(baked);
                    baked = current;
                }
                else
                {
                    AssetDatabase.CreateAsset(baked, AssetPath);
                }

                sea.depthMap = baked;
                sea.depthBounds = new Vector4(minX, minZ, width, height);
                EditorUtility.SetDirty(sea);
                EditorSceneManager.MarkSceneDirty(sea.gameObject.scene);
                AssetDatabase.SaveAssets();
            }
            catch
            {
                if (baked != null && !AssetDatabase.Contains(baked))
                    UnityEngine.Object.DestroyImmediate(baked);
                throw;
            }
        }
    }
}
