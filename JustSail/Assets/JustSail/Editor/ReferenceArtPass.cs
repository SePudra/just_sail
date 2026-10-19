using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object=UnityEngine.Object;
using Random=UnityEngine.Random;

namespace JustSail.Editor
{
    public static class ReferenceArtPass
    {
        const string Root="Assets/JustSail/";
        static readonly Dictionary<string,Material> palette=new();
        static readonly Dictionary<string,string> colors=new()
        {
            {"RefHullIvory","E3D8B7"},{"RefSailLinen","F7ECD1"},{"RefTeak","946044"},{"RefDeck","B98756"},
            {"RefHullRed","863F35"},{"RefVermilion","CD6343"},{"RefInk","243B42"},{"RefRope","C7B48D"},{"RefBrass","C7A15C"},
            {"RefFirDark","2C4A29"},{"RefFir","466B38"},{"RefFirLight","6B8845"},
            {"RefAmberDark","A85F29"},{"RefAmber","D48A34"},{"RefAmberLight","EDB04F"},
            {"RefRustDark","AA4723"},{"RefRust","D96A2E"},{"RefRustLight","EC8D40"},
            {"RefBirchBark","D9DDD2"},{"RefBark","64503F"},{"RefSlate","777E83"},{"RefShrub","556F37"},{"RefShrubTip","839346"},{"RefDriftwood","B6A78C"}
        };
        static Color Hex(string value){ColorUtility.TryParseHtmlString("#"+value,out var c);return c;}
        static T Save<T>(T value,string path) where T:Object
        {
            value.name=Path.GetFileNameWithoutExtension(path);
            var old=AssetDatabase.LoadAssetAtPath<T>(path);
            if(old)
            {
                if(value is Mesh from&&old is Mesh to)
                {
                    // Explicit setters also invalidate the GPU buffers during live art iterations.
                    to.Clear();to.indexFormat=from.indexFormat;to.vertices=from.vertices;to.normals=from.normals;
                    to.colors=from.colors;to.triangles=from.triangles;to.bounds=from.bounds;to.UploadMeshData(false);
                }
                else EditorUtility.CopySerialized(value,old);
                old.name=value.name;Object.DestroyImmediate(value);EditorUtility.SetDirty(old);return old;
            }
            AssetDatabase.CreateAsset(value,path);return value;
        }
        static Material Material(string name)
        {
            name=name.Replace(" (Instance)","");if(palette.TryGetValue(name,out var cached)&&cached)return cached;
            string path=Root+"Materials/Reference/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("JustSail/Crafted"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",Hex(colors.TryGetValue(name,out var hex)?hex:"9B8A71"));m.SetFloat("_Smoothness",.035f);
            m.SetFloat("_Stone",name=="RefSlate"?1:0);
            m.SetFloat("_Foliage",name.Contains("Fir")||name.Contains("Amber")||name.Contains("Rust")||name.Contains("Shrub")?1:0);
            EditorUtility.SetDirty(m);palette[name]=m;return m;
        }
        static GameObject Model(string name,Transform parent,Vector3 position,float scale,float yaw)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Art/Reference/"+name+".fbx");
            if(!prefab)throw new Exception("Missing reference model "+name);
            var obj=new GameObject(name);obj.transform.SetParent(parent,false);
            obj.transform.localPosition=position;obj.transform.localRotation=Quaternion.Euler(0,yaw,0);obj.transform.localScale=Vector3.one*scale;
            // Keep the FBX root's axis conversion and model-origin offset inside the placement transform.
            var imported=(GameObject)PrefabUtility.InstantiatePrefab(prefab);imported.transform.SetParent(obj.transform,false);
            foreach(var renderer in obj.GetComponentsInChildren<MeshRenderer>())
            {
                var mats=renderer.sharedMaterials;for(int i=0;i<mats.Length;i++)mats[i]=Material(mats[i].name);
                renderer.sharedMaterials=mats;
            }
            if(name.Contains("Birch")||name.Contains("Fir")||name.Contains("Shrub"))
            {var breeze=obj.AddComponent<TreeBreeze>();breeze.sea=Object.FindFirstObjectByType<SeaSurface>();}
            return obj;
        }
        internal static float Height(float x,float z,bool tall)
        {
            float noise=(Mathf.PerlinNoise(x*.018f+12,z*.018f+61)-.5f)*2.3f;
            if(tall)
            {
                float first=112-Mathf.Sqrt((x+28)*(x+28)*.85f+(z-32)*(z-32)*1.1f)*.95f;
                float second=95-Mathf.Sqrt((x-35)*(x-35)*1.1f+(z-2)*(z-2)*1.3f)*.9f;
                float third=75-Mathf.Sqrt((x+8)*(x+8)*1.2f+(z+44)*(z+44))*1.05f;
                return Mathf.Max(first,Mathf.Max(second,third))-16+noise*2;
            }
            float hill=35*Mathf.Exp(-((x+30)*(x+30)/5100+(z-23)*(z-23)/4100));
            float shoulder=24*Mathf.Exp(-((x-38)*(x-38)/2500+(z-7)*(z-7)/4100));
            float cove=27*Mathf.Exp(-((x-20)*(x-20)/1550+(z+51)*(z+51)/1600));
            float headland=14*Mathf.Exp(-((x+72)*(x+72)/1000+(z+32)*(z+32)/1400));
            return hill+shoulder+headland-cove-12+noise;
        }
        static void Island(GameObject island,bool tall)
        {
            var root=island.transform;
            for(int i=root.childCount-1;i>=0;i--)
            {var child=root.GetChild(i);if(child.name!="Dock"&&child.name!="Lighthouse")Object.DestroyImmediate(child.gameObject);}
            const int n=140;const float size=350;
            var v=new Vector3[(n+1)*(n+1)];var c=new Color[v.Length];var indices=new int[n*n*6];int k=0;
            for(int z=0;z<=n;z++)for(int x=0;x<=n;x++)
            {
                float px=(float)x/n*size-size/2,pz=(float)z/n*size-size/2;
                float h=Height(px,pz,tall);v[z*(n+1)+x]=new Vector3(px,h,pz);
                float slope=Mathf.Abs(Height(px+1,pz,tall)-h)+Mathf.Abs(Height(px,pz+1,tall)-h);
                float meadow=Mathf.PerlinNoise(px*.016f+3,pz*.016f+7);
                Color grass=Color.Lerp(Hex("FF7D3B"),Hex("FF9957"),meadow);
                Color rock=Color.Lerp(Hex("75808B"),Hex("9CA2A4"),Mathf.InverseLerp(5,80,h));
                float rocky=Mathf.SmoothStep(0,1,Mathf.InverseLerp(tall?.65f:1.4f,tall?1.05f:2.1f,slope));
                if(tall)rocky=Mathf.Max(rocky,Mathf.SmoothStep(0,1,Mathf.InverseLerp(26,58,h))*.88f);
                Color top=Color.Lerp(grass,rock,rocky);
                Color shore=Color.Lerp(Hex("6B797B"),Hex("A69470"),Mathf.SmoothStep(0,1,Mathf.InverseLerp(-3,1.3f,h)));
                c[z*(n+1)+x]=Color.Lerp(shore,top,Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.4f,1.7f,h))).linear;
            }
            for(int z=0;z<n;z++)for(int x=0;x<n;x++)
            {int a=z*(n+1)+x;indices[k++]=a;indices[k++]=a+n+1;indices[k++]=a+1;indices[k++]=a+1;indices[k++]=a+n+1;indices[k++]=a+n+2;}
            var mesh=new Mesh{name=tall?"Reference mountain":"Reference autumn cove",indexFormat=IndexFormat.UInt32};
            mesh.vertices=v;mesh.colors=c;mesh.triangles=indices;mesh.RecalculateNormals();mesh.RecalculateBounds();
            mesh=Save(mesh,Root+"Generated/"+(tall?"ReferenceMountain":"ReferenceCove")+".asset");
            island.GetComponent<MeshFilter>().sharedMesh=mesh;island.GetComponent<MeshCollider>().sharedMesh=mesh;
            island.GetComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/Landscape.mat");
            Random.InitState(tall?114:836);
            var occupied=new List<Vector2>();int limit=tall?150:150;
            for(int i=0,placed=0;i<3500&&placed<limit;i++)
            {
                float x=Random.Range(-115f,115),z=Random.Range(-110f,110),h=Height(x,z,tall);
                if(h<3||h>(tall?62:47))continue;
                float slope=Mathf.Abs(Height(x+1,z,tall)-h)+Mathf.Abs(Height(x,z+1,tall)-h);
                if(slope>(tall?1.55f:1.05f))continue;
                var p=new Vector2(x,z);bool close=false;foreach(var old in occupied)if((old-p).sqrMagnitude<22){close=true;break;}if(close)continue;
                occupied.Add(p);
                float cluster=Mathf.PerlinNoise(x*.021f+17,z*.021f+6);
                bool fir=tall||cluster>.62f;
                string name=fir?"FirRefined":(Random.value>.45f?"BirchGold":"BirchRust");
                float scale=Random.Range(fir?.95f:1.05f,fir?1.75f:1.5f);
                Model(name,root,new Vector3(x,h-.13f,z),scale,Random.Range(0,360));placed++;
            }
            // Authored groups at the entrance: layered firs, white trunks and autumn crowns.
            if(!tall)
            {
                var groups=new[]{new Vector2(-70,-37),new Vector2(-52,-26),new Vector2(-32,-23),new Vector2(57,-32),new Vector2(73,-21)};
                for(int g=0;g<groups.Length;g++)for(int j=0;j<4;j++)
                {
                    float x=groups[g].x+Mathf.Sin(j*2.7f)*4,z=groups[g].y+j*4.5f,h=Height(x,z,false);
                    if(h<2)continue;
                    Model(j==0?"FirRefined":g%2==0?"BirchRust":"BirchGold",root,new Vector3(x,h-.1f,z),j==0?1.55f:1.4f,Random.Range(0,360));
                }
                for(int group=0;group<23;group++)
                {
                    float x=-99+group*8.5f,z=-115;while(Height(x,z,false)<3.5f&&z<75)z+=.5f;
                    if(z>=75)continue;
                    z+=Random.Range(1,5);float h=Height(x,z,false);
                    bool fir=group==7||group==16;string species=fir?"FirRefined":group%3==0?"BirchGold":"BirchRust";
                    Model(species,root,new Vector3(x,h-.1f,z),fir?1.7f:Random.Range(1.3f,1.65f),Random.Range(0,360));
                }
                float pineZ=-110;while(Height(-25,pineZ,false)<2.7f&&pineZ<70)pineZ+=.5f;
                Model("FirRefined",root,new Vector3(-25,Height(-25,pineZ,false),pineZ),1.9f,58);
                for(int group=0;group<16;group++)
                {
                    float x=Random.Range(-90,90),z=Random.Range(-5,90),h=Height(x,z,false);if(h<6)continue;
                    var shelf=Model("SlateBoulder",root,new Vector3(x,h-.25f,z),Random.Range(1.3f,2.2f),Random.Range(0,360));
                    shelf.transform.localScale=Vector3.Scale(shelf.transform.localScale,new Vector3(1.6f,.23f,1.3f));
                }
                float shoreZ=-110;while(Height(53,shoreZ,false)<1&&shoreZ<70)shoreZ+=.5f;
                for(int j=0;j<11;j++)
                {
                    float x=53+Random.Range(-4f,5),z=shoreZ+Random.Range(-1f,2),h=Height(x,z,false);
                    var log=GameObject.CreatePrimitive(PrimitiveType.Cylinder);log.name="Weathered driftwood";Object.DestroyImmediate(log.GetComponent<Collider>());
                    log.transform.SetParent(root,false);log.transform.localPosition=new Vector3(x,h+.17f,z);
                    log.transform.localRotation=Quaternion.Euler(87,Random.Range(45,110),Random.Range(-8,8));
                    log.transform.localScale=new Vector3(.19f,Random.Range(1.3f,2.7f),.19f);log.GetComponent<MeshRenderer>().sharedMaterial=Material("RefDriftwood");
                }
            }
            // Broad slate at the waterline, interspersed rather than a continuous wall.
            for(int i=0;i<330;i++)
            {
                float x=Random.Range(-130f,130),z=Random.Range(-130f,115),h=Height(x,z,tall);
                if(h<-.8f||h>8.5f||Random.value<.34f)continue;
                var rock=Model("SlateBoulder",root,new Vector3(x,h-Random.Range(.9f,1.6f),z),Random.Range(.65f,1.7f),Random.Range(0,360));
                rock.transform.localRotation*=Quaternion.Euler(Random.Range(-9,9),0,Random.Range(-12,12));
                var collider=rock.AddComponent<BoxCollider>();collider.center=new Vector3(0,1.35f,0);collider.size=new Vector3(4.6f,3.2f,3.2f);rock.layer=6;
            }
            if(!tall)
            {
                for(int group=0;group<12;group++)
                {
                    float x=-102+group*18;if(x>8&&x<39)continue;
                    float z=-125;while(Height(x,z,false)<.7f&&z<85)z+=.5f;
                    if(z>=85)continue;
                    for(int j=0;j<3;j++)
                    {
                        float px=x+j*2.4f,pz=z+Mathf.Sin(j*2)*2,h=Height(px,pz,false);
                        var rock=Model("SlateBoulder",root,new Vector3(px,h-1.1f,pz),j==0?2.1f:j==1?1.2f:.65f,Random.Range(0,360));
                        rock.transform.localScale=Vector3.Scale(rock.transform.localScale,new Vector3(1,Random.Range(.55f,.8f),Random.Range(.8f,1.3f)));
                        var collider=rock.AddComponent<BoxCollider>();collider.center=new Vector3(0,1,0);collider.size=new Vector3(4.8f,3.1f,3.2f);rock.layer=6;
                    }
                }
            }
            for(int i=0;i<(tall?70:180);i++)
            {
                float x=Random.Range(-110f,110),z=Random.Range(-100f,100),h=Height(x,z,tall);
                if(h<2.2f||h>35)continue;Model("ShoreShrub",root,new Vector3(x,h-.05f,z),Random.Range(.8f,1.8f),Random.Range(0,360));
            }
            Grass(root,tall);
            if(!tall)
            {
                var dock=GameObject.Find("Dock");float z=-20;while(Height(20,z,false)>-.5f&&z>-115)z-=.5f;
                dock.transform.localPosition=new Vector3(20,0,z-8);dock.transform.localRotation=Quaternion.identity;
            }
            else
            {
                var lighthouse=GameObject.Find("Lighthouse");if(lighthouse)lighthouse.transform.localPosition=new Vector3(-9,Height(-9,0,true),0);
            }
            EditorUtility.SetDirty(island.GetComponent<MeshCollider>());
        }
        static void Grass(Transform parent,bool tall)
        {
            var v=new List<Vector3>();var colors=new List<Color>();var tris=new List<int>();
            int count=tall?35000:200000;
            for(int i=0;i<count;i++)
            {
                float x=Random.Range(-140f,140),z=Random.Range(-130f,130),h=Height(x,z,tall);
                if(h<1.3f||h>(tall?30:42))continue;
                if(Mathf.Abs(Height(x+1,z,tall)-h)+Mathf.Abs(Height(x,z+1,tall)-h)>1.7f)continue;
                float height=Random.Range(.18f,.48f),width=Random.Range(.025f,.075f),a=Random.value*Mathf.PI*2;
                Color baseColor=Color.Lerp(Hex("EE7F3D"),Hex("FF9653"),Random.value).linear;
                for(int j=0;j<3;j++)
                {
                    float angle=a+j*2.1f;var side=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*width;
                    var p=new Vector3(x,h-.04f,z);int k=v.Count;
                    v.Add(p-side);v.Add(p+new Vector3(.11f,height,.07f));v.Add(p+side);
                    // Preserve float alpha .84 / 1.15: Meadow uses it to pin
                    // roots and bend tips, independent of vertex/index order.
                    colors.Add(baseColor*.84f);colors.Add(baseColor*1.15f);colors.Add(baseColor*.84f);tris.Add(k);tris.Add(k+1);tris.Add(k+2);
                }
            }
            var mesh=new Mesh{name="Golden shore grass",indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetColors(colors);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            mesh=Save(mesh,Root+"Generated/"+(tall?"MountainGrass":"CoveGrass")+".asset");
            string path=Root+"Materials/Reference/Grass.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!material){material=new Material(Shader.Find("JustSail/Meadow"));AssetDatabase.CreateAsset(material,path);}
            var go=new GameObject("Golden grass",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);
            go.GetComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;
        }
        [MenuItem("Just Sail/Reference art/Apply autumn shore")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode before applying art.");
            Directory.CreateDirectory(Root+"Materials/Reference");palette.Clear();AssetDatabase.Refresh();
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.path!=Root+"Scenes/FirstVoyage.unity")throw new Exception("Open FirstVoyage before the art pass.");
            var boat=Object.FindFirstObjectByType<BoatController>();
            for(int i=boat.transform.childCount-1;i>=0;i--)
            {var t=boat.transform.GetChild(i);if(t.GetComponentInChildren<MeshFilter>())Object.DestroyImmediate(t.gameObject);}
            Model("SailboatRefined",boat.transform,Vector3.zero,1,0);
            Island(GameObject.Find("Sakin Koy"),false);Island(GameObject.Find("Fener Kayalığı"),true);
            boat.spawn=new Vector3(-70,0,40);boat.transform.position=boat.spawn;boat.transform.rotation=Quaternion.Euler(0,-25,0);
            EditorUtility.SetDirty(boat);
            var camera=Object.FindFirstObjectByType<SailCamera>();camera.distance=40;camera.SetOrbit(-25,5);camera.softFocus=false;
            camera.GetComponent<Camera>().fieldOfView=34;
            if(!camera.GetComponent<ShoreReflection>())camera.gameObject.AddComponent<ShoreReflection>();
            var sea=Object.FindFirstObjectByType<SeaSurface>();sea.gameObject.layer=4;
            var atmosphere=Object.FindFirstObjectByType<VoyageAtmosphere>();atmosphere.hours=8;atmosphere.weatherMode=0;atmosphere.advanceDay=true;
            var volume=Object.FindFirstObjectByType<Volume>();var profile=volume.sharedProfile;
            OverTheHillAdaptation.ConfigureAO();
            var tone=profile.TryGet<Tonemapping>(out var tmap)?tmap:profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.Neutral);
            var grade=profile.TryGet<ColorAdjustments>(out var cad)?cad:profile.Add<ColorAdjustments>(true);grade.contrast.Override(9);grade.saturation.Override(-3);grade.postExposure.Override(.08f);
            if(profile.TryGet<LiftGammaGain>(out var lift))lift.lift.Override(new Vector4(1,1,1,.001f));
            if(profile.TryGet<SplitToning>(out var split)){split.shadows.Override(new Color(.48f,.49f,.52f));split.highlights.Override(new Color(.52f,.505f,.48f));split.balance.Override(0);}
            if(profile.TryGet<DepthOfField>(out var dof)){dof.mode.Override(DepthOfFieldMode.Bokeh);dof.aperture.Override(5.6f);dof.focalLength.Override(45);}
            if(profile.TryGet<Bloom>(out var bloom)){bloom.intensity.Override(.12f);bloom.threshold.Override(1.2f);}
            foreach(var effect in profile.components){if(!AssetDatabase.Contains(effect))AssetDatabase.AddObjectToAsset(effect,profile);EditorUtility.SetDirty(effect);}EditorUtility.SetDirty(profile);
            EditorUtility.SetDirty(camera);EditorUtility.SetDirty(camera.GetComponent<Camera>());EditorUtility.SetDirty(atmosphere);
            Physics.SyncTransforms();OceanBake.Bake(sea);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("REFERENCE_AUTUMN_SHORE_READY");
        }

        // One atomic scene render keeps every art comparison at the same pose and wave time.
        public static string CaptureStudy(string name,float hour=8,float yaw=-25,float pitch=5,float distance=40,string outputDirectory=null)
        {
            if(!EditorApplication.isPlaying)throw new Exception("CaptureStudy requires Play Mode.");
            var ui=Object.FindFirstObjectByType<SailUI>();ui.StartSailing();ui.Pause();
            var body=ui.boat.Body;body.interpolation=RigidbodyInterpolation.None;
            var position=new Vector3(-70,.12f,40);var rotation=Quaternion.Euler(0,-25,0);
            body.position=position;body.rotation=rotation;ui.boat.transform.SetPositionAndRotation(position,rotation);
            body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;ui.boat.throttle=0;Physics.SyncTransforms();
            var sea=ui.boat.sea;typeof(SeaSurface).GetProperty("Clock").GetSetMethod(true).Invoke(sea,new object[]{12f});
            ui.atmosphere.advanceDay=false;ui.atmosphere.SetWeather(0,true);ui.atmosphere.SetHour(hour);sea.ClearWake();sea.Upload();
            ui.cameraRig.softFocus=false;ui.cameraRig.distance=distance;ui.cameraRig.SetOrbit(yaw,pitch);
            typeof(SailCamera).GetMethod("LateUpdate",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(ui.cameraRig,null);
            var cam=ui.cameraRig.GetComponent<Camera>();float aspect=cam.aspect;cam.aspect=16f/9;
            var reflection=cam.GetComponent<ShoreReflection>();if(reflection)reflection.Refresh();
            var target=RenderTexture.GetTemporary(1600,900,24,RenderTextureFormat.ARGB32);var previousTarget=cam.targetTexture;var previousActive=RenderTexture.active;
            string directory=Path.GetFullPath(outputDirectory??"../Verification/VisualStudy");Directory.CreateDirectory(directory);
            Texture2D image=null;string path=Path.Combine(directory,Path.GetFileName(name)+".png");
            try
            {
                var request=new UniversalRenderPipeline.SingleCameraRequest{destination=target};
                RenderPipeline.SubmitRenderRequest(cam,request);RenderTexture.active=target;
                image=new Texture2D(1600,900,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,900),0,0);image.Apply();
                File.WriteAllBytes(path,image.EncodeToPNG());
            }
            finally
            {
                cam.targetTexture=previousTarget;cam.aspect=aspect;RenderTexture.active=previousActive;
                if(image)Object.DestroyImmediate(image);RenderTexture.ReleaseTemporary(target);
            }
            return path;
        }
    }
}
