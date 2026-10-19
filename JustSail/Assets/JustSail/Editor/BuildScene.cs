using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace JustSail.Editor
{
    public static class BuildScene
    {
        const string Root="Assets/JustSail/";
        static Material land;
        static readonly Dictionary<string,Material> materials=new();
        static Color Hex(string c){ColorUtility.TryParseHtmlString("#"+c,out var result);return result;}

        [MenuItem("Just Sail/Create sailing scene")]
        public static void Create()
        {
            Directory.CreateDirectory(Root+"Scenes");Directory.CreateDirectory(Root+"Materials");Directory.CreateDirectory(Root+"Generated");
            AssetDatabase.Refresh();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            PlayerSettings.companyName="Just Sail";PlayerSettings.productName="Just Sail";
            PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;
            PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.runInBackground=true;
            PlayerSettings.colorSpace=ColorSpace.Linear;Time.fixedDeltaTime=.02f;
            var urp=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if(!urp)throw new System.Exception("URP template pipeline is missing.");
            urp.supportsCameraDepthTexture=true;urp.supportsCameraOpaqueTexture=true;urp.msaaSampleCount=4;
            urp.shadowDistance=220;urp.renderScale=1;EditorUtility.SetDirty(urp);
            land=NewMaterial("Landscape","JustSail/Landscape");
            var seaMat=NewMaterial("Sea","JustSail/Sea");seaMat.SetColor("_DeepColor",Hex("245B70"));seaMat.SetColor("_ShallowColor",Hex("58BDB5"));seaMat.SetColor("_FoamColor",Hex("E4F0DF"));
            var sky=NewMaterial("Sky","JustSail/Sky");RenderSettings.skybox=sky;
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Exponential;RenderSettings.ambientMode=AmbientMode.Trilight;
            var sunGo=new GameObject("Afternoon sun");var sun=sunGo.AddComponent<Light>();sun.type=LightType.Directional;sun.shadows=LightShadows.Soft;sun.shadowStrength=.68f;sun.shadowBias=.06f;RenderSettings.sun=sun;
            var ocean=new GameObject("The sea",typeof(MeshFilter),typeof(MeshRenderer),typeof(SeaSurface));
            ocean.GetComponent<MeshFilter>().sharedMesh=WaterMesh();ocean.GetComponent<MeshRenderer>().sharedMaterial=seaMat;
            ocean.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;
            var sea=ocean.GetComponent<SeaSurface>();
            Island(new Vector3(-105,0,100),false);Island(new Vector3(145,0,490),true);
            var boatGo=new GameObject("Your boat",typeof(Rigidbody),typeof(BoatController));boatGo.transform.position=new(0,0,-65);boatGo.transform.rotation=Quaternion.Euler(0,-12,0);
            var visual=Model("Sailboat",boatGo.transform,Vector3.zero,Vector3.one);
            // FBX is authored in Blender at metres. The importer handles unit conversion.
            var hull=boatGo.AddComponent<BoxCollider>();hull.center=new(0,.14f,0);hull.size=new(1.65f,.92f,4.65f);
            var boat=boatGo.GetComponent<BoatController>();boat.sea=sea;sea.boat=boatGo.transform;
            boatGo.AddComponent<SailRig>().sea=sea;
            var camGo=new GameObject("Sailing camera",typeof(Camera),typeof(AudioListener),typeof(SailCamera));camGo.tag="MainCamera";
            var cam=camGo.GetComponent<Camera>();cam.fieldOfView=53;cam.nearClipPlane=.15f;cam.farClipPlane=1800;cam.allowHDR=true;cam.backgroundColor=Hex("B6CFCC");
            var data=cam.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;data.requiresDepthTexture=true;data.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            var rig=camGo.GetComponent<SailCamera>();rig.target=boatGo.transform;
            var volumeGo=new GameObject("Atmosphere",typeof(Volume));var vol=volumeGo.GetComponent<Volume>();vol.isGlobal=true;
            var profile=ScriptableObject.CreateInstance<VolumeProfile>();var tone=profile.Add<Tonemapping>(true);tone.mode.value=TonemappingMode.Neutral;
            var bloom=profile.Add<Bloom>(true);bloom.intensity.value=.12f;bloom.threshold.value=1.2f;
            profile=SaveAsset(profile,Root+"Settings/Atmosphere.asset");
            foreach(var component in profile.components)if(!AssetDatabase.Contains(component))AssetDatabase.AddObjectToAsset(component,profile);
            vol.sharedProfile=profile;
            var audio=new GameObject("Sea ambience").AddComponent<SeaAudio>();audio.boat=boatGo.GetComponent<Rigidbody>();audio.dock=GameObject.Find("Dock")?.transform;
            var ui=new GameObject("Voyage",typeof(SailUI)).GetComponent<SailUI>();ui.boat=boat;ui.cameraRig=rig;ui.ambience=audio;ui.sun=sun;
            var sourceFont=AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
            if(!sourceFont)throw new System.Exception("Import TMP Essential Resources first.");
            string fontPath=Root+"UI/SailingFont.asset";Directory.CreateDirectory(Root+"UI");
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            if(!font){font=TMP_FontAsset.CreateFontAsset(sourceFont);AssetDatabase.CreateAsset(font,fontPath);AssetDatabase.AddObjectToAsset(font.material,font);foreach(var atlas in font.atlasTextures)AssetDatabase.AddObjectToAsset(atlas,font);}
            font.TryAddCharacters("çğıİöşüÇĞÖŞÜâÂ×›");ui.font=font;EditorUtility.SetDirty(font);
            Shader.SetGlobalVector("_SeaTint",Vector3.one);
            sun.transform.rotation=Quaternion.Euler(34,-38,0);sun.color=new(1,.91f,.75f);sun.intensity=1.15f;
            RenderSettings.fogColor=Hex("A6C4BA");RenderSettings.fogDensity=.00135f;RenderSettings.ambientSkyColor=new(.74f,.82f,.77f);RenderSettings.ambientEquatorColor=new(.45f,.56f,.52f);RenderSettings.ambientGroundColor=new(.3f,.34f,.3f);
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),Root+"Scenes/FirstVoyage.unity");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Root+"Scenes/FirstVoyage.unity",true)};
            AssetDatabase.SaveAssets();REAAdaptation.ApplyToScene();OverTheHillAdaptation.Apply();ReferenceArtPass.Apply();Debug.Log("JUST_SAIL_SCENE_READY");
        }

        static Material NewMaterial(string name,string shader)
        {
            var mat=new Material(Shader.Find(shader)??throw new System.Exception("Missing shader "+shader)){name=name};
            return SaveAsset(mat,Root+"Materials/"+name+".mat");
        }
        static T SaveAsset<T>(T obj,string path) where T:Object
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));var old=AssetDatabase.LoadAssetAtPath<Object>(path);
            if(old){EditorUtility.CopySerialized(obj,old);Object.DestroyImmediate(obj);return (T)old;}else AssetDatabase.CreateAsset(obj,path);return obj;
        }
        static Material Palette(string name)
        {
            name=name.Replace(" (Instance)","");if(materials.TryGetValue(name,out var existing)&&existing)return existing;
            var colors=new Dictionary<string,string>{{"SailIvory","F0E6CB"},{"Teak","986B4B"},{"HullCream","D9CDB0"},{"Terracotta","B9674B"},{"DeepTeal","254B50"},{"Rope","C1AB7F"},{"Pine","6E8260"},{"PineLight","80916A"},{"Bark","655944"},{"Limestone","C4BA9D"},{"FadedRed","AC6652"},{"Iron","354746"}};
            var mat=NewMaterial(name,"Universal Render Pipeline/Lit");mat.SetColor("_BaseColor",Hex(colors.TryGetValue(name,out string c)?c:"D7C6A3"));mat.SetFloat("_Smoothness",.05f);mat.SetFloat("_Cull",0);materials[name]=mat;return mat;
        }
        static GameObject Model(string name,Transform parent,Vector3 position,Vector3 scale)
        {
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Art/"+name+".fbx");
            if(!asset)throw new System.Exception("Missing model "+name);
            var go=(GameObject)PrefabUtility.InstantiatePrefab(asset);go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;
            foreach(var r in go.GetComponentsInChildren<Renderer>()){var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)mats[i]=Palette(mats[i].name);r.sharedMaterials=mats;}
            return go;
        }
        internal static Mesh WaterMesh()
        {
            int n=400;float size=4400;var v=new Vector3[(n+1)*(n+1)];var tris=new int[n*n*6];int t=0;
            float Grid(int i){float u=(float)i/n*2-1;return Mathf.Sign(u)*Mathf.Pow(Mathf.Abs(u),2.2f)*size/2;}
            for(int z=0;z<=n;z++)for(int x=0;x<=n;x++)v[z*(n+1)+x]=new(Grid(x),0,Grid(z));
            for(int z=0;z<n;z++)for(int x=0;x<n;x++){int a=z*(n+1)+x;tris[t++]=a;tris[t++]=a+n+1;tris[t++]=a+1;tris[t++]=a+1;tris[t++]=a+n+1;tris[t++]=a+n+2;}
            var mesh=new Mesh{name="Wide sea",indexFormat=IndexFormat.UInt32};mesh.vertices=v;mesh.triangles=tris;mesh.RecalculateNormals();mesh.bounds=new Bounds(Vector3.zero,new Vector3(4500,30,4500));SaveAsset(mesh,Root+"Generated/Sea.asset");return AssetDatabase.LoadAssetAtPath<Mesh>(Root+"Generated/Sea.asset");
        }
        static float Height(float x,float z,bool tall)
        {
            float noise=(Mathf.PerlinNoise(x*.035f+44,z*.035f+12)-.5f)*3;
            if(tall){float a=x+z*.33f;return 53*Mathf.Exp(-((a+9)*(a+9)/1900+(z-4)*(z-4)/3000))+26*Mathf.Exp(-((x-25)*(x-25)/750+(z+18)*(z+18)/1200))-12+noise*1.7f;}
            float a1=32*Mathf.Exp(-((x+18)*(x+18)/3700+(z-12)*(z-12)/3000));
            float a2=21*Mathf.Exp(-((x-39)*(x-39)/1300+(z+7)*(z+7)/2600));
            float bay=22*Mathf.Exp(-((x-5)*(x-5)/1050+(z+44)*(z+44)/1400));
            float headland=11*Mathf.Exp(-((x+53)*(x+53)/650+(z+28)*(z+28)/900));
            return a1+a2+headland-bay-10+noise;
        }
        static void Island(Vector3 center,bool tall)
        {
            string name=tall?"Fener Kayalığı":"Sakin Koy";var go=new GameObject(name);go.transform.position=center;go.layer=6;
            int n=66;float size=260;var vertices=new List<Vector3>();var colors=new List<Color>();var triangles=new List<int>();
            for(int z=0;z<n;z++)for(int x=0;x<n;x++)
            {
                Vector3 P(int xx,int zz){float px=(float)xx/n*size-size/2,pz=(float)zz/n*size-size/2;if(xx>0&&xx<n&&zz>0&&zz<n){px+=Mathf.Sin(xx*17.1f+zz*31.7f)*.85f;pz+=Mathf.Cos(xx*13.7f-zz*19.3f)*.85f;}return new(px,Height(px,pz,tall),pz);}
                Vector3 a=P(x,z),b=P(x,z+1),c=P(x+1,z),d=P(x+1,z+1);
                void Face(Vector3 p,Vector3 q,Vector3 r){int k=vertices.Count;vertices.Add(p);vertices.Add(q);vertices.Add(r);triangles.Add(k);triangles.Add(k+1);triangles.Add(k+2);float h=(p.y+q.y+r.y)/3;float slope=Vector3.Dot(Vector3.Cross(q-p,r-p).normalized,Vector3.up);Color grass=Color.Lerp(Hex("74866C"),Hex("A6AD7E"),Mathf.InverseLerp(3,32,h));Color rock=Color.Lerp(Hex("ADAAA0"),Hex("D8CAB0"),Mathf.InverseLerp(0,62,h));Color land=Color.Lerp(rock,grass,Mathf.SmoothStep(0,1,Mathf.InverseLerp(.55f,.9f,slope))*(tall?.28f:1));Color col=Color.Lerp(Hex("E2D0A8"),land,Mathf.SmoothStep(0,1,Mathf.InverseLerp(1,5,h)));float variation=Mathf.PerlinNoise(p.x*.035f+17,p.z*.035f+2);col*=.97f+.055f*variation;col=col.linear;colors.Add(col);colors.Add(col);colors.Add(col);}
                Face(a,b,c);Face(c,b,d);
            }
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();SaveAsset(mesh,Root+"Generated/"+(tall?"RockIsland":"CoveIsland")+".asset");
            mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"Generated/"+(tall?"RockIsland":"CoveIsland")+".asset");
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=land;go.AddComponent<MeshCollider>().sharedMesh=mesh;
            Random.InitState(tall?541:81);
            int trees=tall?9:65;
            for(int i=0,placed=0;i<500&&placed<trees;i++)
            {
                float x=Random.Range(-85f,85f),z=Random.Range(-85f,85f),h=Height(x,z,tall);
                if(h<4||h>(tall?28:31))continue;
                float slope=Mathf.Abs(Height(x+1,z,tall)-h)+Mathf.Abs(Height(x,z+1,tall)-h);if(slope>.9f)continue;
                float s=Random.Range(1.2f,2.1f);var tree=Model("StonePine",go.transform,new(x,h-.2f,z),Vector3.one*s);tree.transform.localRotation=Quaternion.Euler(0,Random.Range(0,360),0);placed++;
            }
            for(int i=0;i<36;i++)
            {
                float x=Random.Range(-90f,90f),z=Random.Range(-95f,95f),h=Height(x,z,tall);if(h<-.9f||h>24)continue;
                var rock=Model("LimestoneRock",go.transform,new(x,h,z),Vector3.one*Random.Range(1,3.8f));rock.transform.localRotation=Quaternion.Euler(Random.Range(-15,15),Random.Range(0,360),Random.Range(-10,10));
            }
            if(tall){float h=Height(-5,0,true);Model("Lighthouse",go.transform,new(-5,h,0),Vector3.one);}
            else
            {
                // The dock faces out of the low southern bay; collider is intentionally simple.
                float z=-30;while(Height(5,z,false)>-.3f&&z>-100)z-=1;
                var dock=Model("Dock",go.transform,new(5,0,z-4),new(1.5f,1,1.7f));dock.layer=6;
                var col=dock.AddComponent<BoxCollider>();col.center=new(0,.3f,3.52f);col.size=new(2.5f,1.6f,7.4f);
            }
        }

        [MenuItem("Just Sail/Build Windows")]
        public static void WindowsBuild()
        {
            var path=Path.GetFullPath("../Builds/JustSail/JustSail.exe");Directory.CreateDirectory(Path.GetDirectoryName(path));
            var report=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,path,BuildTarget.StandaloneWindows64,BuildOptions.None);
            Debug.Log("JUST_SAIL_BUILD "+report.summary.result+" "+report.summary.totalErrors);
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Windows build failed");
        }
    }
}
