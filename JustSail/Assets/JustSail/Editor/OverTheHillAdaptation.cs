using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace JustSail.Editor
{
    public static class OverTheHillAdaptation
    {
        const string Root="Assets/JustSail/";
        static Color Hex(string code){ColorUtility.TryParseHtmlString("#"+code,out var c);return c;}
        static T Effect<T>(VolumeProfile profile) where T:VolumeComponent
        {if(!profile.TryGet<T>(out var effect))effect=profile.Add<T>(true);return effect;}
        [MenuItem("Just Sail/Apply Over the Hill adaptation")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Exit Play Mode first.");
            var boat=Object.FindFirstObjectByType<BoatController>();var sea=Object.FindFirstObjectByType<SeaSurface>();var ui=Object.FindFirstObjectByType<SailUI>();
            if(!boat||!sea||!ui){EditorSceneManager.OpenScene("Assets/JustSail/Scenes/FirstVoyage.unity");boat=Object.FindFirstObjectByType<BoatController>();sea=Object.FindFirstObjectByType<SeaSurface>();ui=Object.FindFirstObjectByType<SailUI>();}
            if(!boat||!sea||!ui)throw new System.Exception("Open FirstVoyage first.");
            var rig=Object.FindFirstObjectByType<SailCamera>();rig.distance=16;rig.SetOrbit(-12,22);rig.GetComponent<Camera>().fieldOfView=34;EditorUtility.SetDirty(rig);EditorUtility.SetDirty(rig.GetComponent<Camera>());
            var camera=rig.GetComponent<Camera>();var cameraData=camera.GetUniversalAdditionalCameraData();camera.allowHDR=true;cameraData.renderPostProcessing=true;cameraData.requiresDepthTexture=true;EditorUtility.SetDirty(cameraData);
            var volume=Object.FindFirstObjectByType<Volume>();if(!volume||!volume.sharedProfile)throw new System.Exception("Atmosphere profile missing.");var profile=volume.sharedProfile;
            Effect<Tonemapping>(profile).mode.Override(TonemappingMode.ACES);
            var color=Effect<ColorAdjustments>(profile);color.contrast.Override(32);color.saturation.Override(18);color.postExposure.Override(.15f);
            var split=Effect<SplitToning>(profile);split.shadows.Override(new Color(.46f,.49f,.535f));split.highlights.Override(new Color(.54f,.515f,.47f));split.balance.Override(8);
            var lift=Effect<LiftGammaGain>(profile);lift.lift.Override(new Vector4(1,1.015f,1.04f,.018f));
            var dof=Effect<DepthOfField>(profile);dof.mode.Override(DepthOfFieldMode.Bokeh);dof.focusDistance.Override(16);dof.focalLength.Override(65);dof.aperture.Override(1.8f);dof.bladeCount.Override(6);
            var bloom=Effect<Bloom>(profile);bloom.intensity.Override(.85f);bloom.threshold.Override(.85f);bloom.scatter.Override(.65f);
            var vignette=Effect<Vignette>(profile);vignette.intensity.Override(.065f);vignette.smoothness.Override(.55f);vignette.color.Override(Hex("263A48"));
            foreach(var effect in profile.components){if(!AssetDatabase.Contains(effect))AssetDatabase.AddObjectToAsset(effect,profile);EditorUtility.SetDirty(effect);}EditorUtility.SetDirty(profile);
            ConfigureAO();
            foreach(string guid in AssetDatabase.FindAssets("t:Material",new[]{Root+"Materials"}))
            {
                var mat=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if(mat.shader.name=="Universal Render Pipeline/Lit"){mat.shader=Shader.Find("JustSail/Crafted");EditorUtility.SetDirty(mat);}
            }
            foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))if(t.name=="StonePine")
            {var breeze=t.GetComponent<TreeBreeze>();if(!breeze)breeze=t.gameObject.AddComponent<TreeBreeze>();breeze.sea=sea;EditorUtility.SetDirty(breeze);}
            var dock=GameObject.Find("Dock");if(!dock)throw new System.Exception("Dock missing.");
            var bollard=dock.transform.Find("Palamar noktası");if(!bollard){bollard=new GameObject("Palamar noktası").transform;bollard.SetParent(dock.transform,false);bollard.localPosition=new Vector3(1,1.7f,0);}
            var mooring=boat.GetComponent<MooringLine>();if(!mooring)mooring=boat.gameObject.AddComponent<MooringLine>();mooring.boat=boat;mooring.ui=ui;mooring.bollard=bollard;mooring.ropeMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/Rope.mat");EditorUtility.SetDirty(mooring);
            var journal=Object.FindFirstObjectByType<VoyageJournal>();if(!journal)journal=new GameObject("Voyage journal").AddComponent<VoyageJournal>();journal.ui=ui;journal.mooring=mooring;journal.weather=Object.FindFirstObjectByType<VoyageAtmosphere>();EditorUtility.SetDirty(journal);
            CreateWildlife(sea,boat);
            ui.font.TryAddCharacters("…");EditorUtility.SetDirty(ui.font);
            Directory.CreateDirectory("../Verification/OverTheHill");
            EditorSceneManager.MarkSceneDirty(boat.gameObject.scene);EditorSceneManager.SaveScene(boat.gameObject.scene);AssetDatabase.SaveAssets();
            Debug.Log("OVER_THE_HILL_ADAPTATION_READY");
        }
        internal static void ConfigureAO()
        {
            var data=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/PC_Renderer.asset");if(!data)throw new System.Exception("PC renderer missing.");
            ScreenSpaceAmbientOcclusion ao=null;foreach(var feature in data.rendererFeatures)if(feature is ScreenSpaceAmbientOcclusion existing)ao=existing;
            if(!ao){ao=ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();ao.name="Soft contact shadows";AssetDatabase.AddObjectToAsset(ao,data);data.rendererFeatures.Add(ao);}
            var serialized=new SerializedObject(ao);var settings=serialized.FindProperty("m_Settings");
            settings.FindPropertyRelative("AfterOpaque").boolValue=true;settings.FindPropertyRelative("Source").enumValueIndex=0;
            settings.FindPropertyRelative("Downsample").boolValue=false;settings.FindPropertyRelative("Intensity").floatValue=.3f;
            settings.FindPropertyRelative("Radius").floatValue=.2f;settings.FindPropertyRelative("Falloff").floatValue=120;
            settings.FindPropertyRelative("DirectLightingStrength").floatValue=.1f;
            serialized.ApplyModifiedPropertiesWithoutUndo();ao.SetActive(true);EditorUtility.SetDirty(ao);data.SetDirty();EditorUtility.SetDirty(data);
        }
        static Material Material(string name,string color)
        {
            string path=Root+"Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!mat){mat=new Material(Shader.Find("JustSail/Crafted"));AssetDatabase.CreateAsset(mat,path);}mat.SetColor("_BaseColor",Hex(color));mat.SetFloat("_Smoothness",.04f);EditorUtility.SetDirty(mat);return mat;
        }
        static Mesh Mesh(string name,Vector3[] vertices,int[] triangles)
        {
            var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();
            string path=Root+"Generated/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);EditorUtility.SetDirty(old);return old;}AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        static Transform Part(string name,Transform parent,Mesh mesh,Material material,Vector3 scale)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);go.transform.localScale=scale;go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=material;return go.transform;
        }
        static void CreateWildlife(SeaSurface sea,BoatController boat)
        {
            var old=GameObject.Find("Sea life");if(old)Object.DestroyImmediate(old);var root=new GameObject("Sea life").transform;
            Material cream=Material("GullIvory","E7E2CC"),wing=Material("GullWing","6C7A7D"),dolphin=Material("DolphinBlue","668F96");
            var bird=Mesh("GullBody",new[]{new Vector3(0,0,.5f),new Vector3(0,.16f,.1f),new Vector3(-.13f,0,0),new Vector3(.13f,0,0),new Vector3(0,-.1f,0),new Vector3(0,0,-.45f)},new[]{0,2,1,0,1,3,0,4,2,0,3,4,5,1,2,5,3,1,5,2,4,5,4,3});
            var wingMesh=Mesh("GullWing",new[]{Vector3.zero,new Vector3(1.15f,.08f,-.15f),new Vector3(.7f,0,-.4f),new Vector3(.13f,0,-.25f)},new[]{0,1,2,0,2,3});
            for(int i=0;i<9;i++)
            {
                var go=new GameObject("Martı "+(i+1));go.transform.SetParent(root,false);go.transform.position=i<6?new Vector3(-94,9+i*.6f,15):new Vector3(145,24,485);
                var animal=go.AddComponent<WildlifeActor>();animal.sea=sea;animal.boat=boat;animal.phase=i*2.1f;
                Part("Body",go.transform,bird,cream,Vector3.one);
                animal.leftWing=Part("Left wing",go.transform,wingMesh,wing,new Vector3(-1,1,1));animal.rightWing=Part("Right wing",go.transform,wingMesh,wing,Vector3.one);
            }
            var vertices=new List<Vector3>();var triangles=new List<int>();
            float[] zs={-1.5f,-.9f,0,.7f,1.4f,1.8f},widths={.035f,.2f,.36f,.27f,.13f,.015f};
            for(int z=0;z<zs.Length;z++)for(int j=0;j<8;j++){float a=j*Mathf.PI/4;vertices.Add(new Vector3(Mathf.Cos(a)*widths[z],Mathf.Sin(a)*widths[z]*.75f,zs[z]));}
            for(int z=0;z<zs.Length-1;z++)for(int j=0;j<8;j++){int a=z*8+j,b=z*8+(j+1)%8,c=a+8,d=b+8;triangles.AddRange(new[]{a,b,c,b,d,c});}
            var body=Mesh("DolphinBody",vertices.ToArray(),triangles.ToArray());
            var fins=Mesh("DolphinFins",new[]{new Vector3(0,.2f,-.4f),new Vector3(0,.85f,-.3f),new Vector3(0,.25f,.35f),new Vector3(-.2f,0,-.2f),new Vector3(-.85f,-.2f,-.65f),new Vector3(-.25f,-.1f,.25f),new Vector3(.2f,0,-.2f),new Vector3(.85f,-.2f,-.65f),new Vector3(.25f,-.1f,.25f),new Vector3(0,0,-1.25f),new Vector3(-.65f,.04f,-1.7f),new Vector3(0,0,-1.55f),new Vector3(.65f,.04f,-1.7f)},new[]{0,1,2,3,4,5,6,8,7,9,10,11,9,11,12});
            for(int i=0;i<3;i++){var go=new GameObject("Yunus "+(i+1));go.transform.SetParent(root,false);go.transform.position=new Vector3(-6+i*5,0,-45+i*4);var animal=go.AddComponent<WildlifeActor>();animal.dolphin=true;animal.phase=i*2.2f;animal.sea=sea;animal.boat=boat;Part("Body",go.transform,body,dolphin,Vector3.one);Part("Fins",go.transform,fins,dolphin,Vector3.one);}
        }
    }
}
