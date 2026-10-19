using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace JustSail.Editor
{
    public static class REAAdaptation
    {
        [MenuItem("Just Sail/Apply ocean and atmosphere adaptation")]
        public static void ApplyToScene()
        {
            if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Exit Play Mode before adapting the scene.");
            var sea=Object.FindFirstObjectByType<SeaSurface>();var boat=Object.FindFirstObjectByType<BoatController>();var ui=Object.FindFirstObjectByType<SailUI>();
            if(!sea||!boat||!ui)throw new System.InvalidOperationException("Open FirstVoyage before applying the adaptation.");
            var p=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if(!p)throw new System.InvalidOperationException("URP pipeline missing.");
            p.supportsHDR=true;p.supportsCameraDepthTexture=true;p.supportsCameraOpaqueTexture=true;p.shadowCascadeCount=4;p.shadowDistance=100;p.cascade4Split=new(.025f,.083333f,.3f);p.mainLightShadowmapResolution=2048;EditorUtility.SetDirty(p);
            sea.GetComponent<MeshFilter>().sharedMesh=BuildScene.WaterMesh();OceanBake.Bake(sea);
            var atmosphere=Object.FindFirstObjectByType<VoyageAtmosphere>();if(!atmosphere)atmosphere=new GameObject("Living sky and wind").AddComponent<VoyageAtmosphere>();
            atmosphere.sea=sea;atmosphere.boat=boat;atmosphere.sun=RenderSettings.sun;atmosphere.view=Object.FindFirstObjectByType<SailCamera>().GetComponent<Camera>();
            atmosphere.hours=15.2f;atmosphere.dayDuration=1440;atmosphere.advanceDay=true;atmosphere.weatherMode=3;
            const string particlePath="Assets/JustSail/Materials/SeaParticles.mat";
            var particles=AssetDatabase.LoadAssetAtPath<Material>(particlePath);
            if(!particles){var shader=Shader.Find("JustSail/SeaParticles");if(!shader)throw new System.Exception("Particle shader missing.");particles=new Material(shader);AssetDatabase.CreateAsset(particles,particlePath);}
            atmosphere.particleMaterial=particles;ui.atmosphere=atmosphere;
            boat.verticalDamping=2.4f;boat.displacementVolume=.72f;boat.fullSubmergence=.72f;EditorUtility.SetDirty(boat);EditorUtility.SetDirty(ui);EditorUtility.SetDirty(atmosphere);
            var volume=Object.FindFirstObjectByType<Volume>();
            if(volume&&volume.sharedProfile){var profile=volume.sharedProfile;if(profile.TryGet<Bloom>(out var bloom)){bloom.intensity.value=.85f;bloom.threshold.value=.85f;EditorUtility.SetDirty(bloom);}EditorUtility.SetDirty(profile);}
            foreach(var m in Resources.FindObjectsOfTypeAll<Material>())if(AssetDatabase.Contains(m)&&m.shader&&m.shader.name=="Universal Render Pipeline/Lit"){m.enableInstancing=true;EditorUtility.SetDirty(m);}
            atmosphere.SetWeather(3,true);sea.Upload();
            EditorSceneManager.MarkSceneDirty(sea.gameObject.scene);EditorSceneManager.SaveScene(sea.gameObject.scene);AssetDatabase.SaveAssets();
            Directory.CreateDirectory("../Verification/REA");Debug.Log("REA_ADAPTATION_READY");
        }
    }
}
