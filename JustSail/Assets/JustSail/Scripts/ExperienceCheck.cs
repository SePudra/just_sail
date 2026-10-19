using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace JustSail
{
    // Opt-in verification uses an isolated photo album, never the player's photographs.
    public sealed class ExperienceCheck : MonoBehaviour
    {
        readonly List<string> checks=new(),errors=new();
        string folder;
        SailUI ui;VoyageAtmosphere weather;VoyageJournal journal;SeaSurface sea;BoatController boat;
        float seconds;int frames;bool measuring;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Launch(){if(Environment.GetCommandLineArgs().Contains("--just-sail-experience-check"))Begin();}
        public static void Begin(){new GameObject("Diorama verification").AddComponent<ExperienceCheck>();}
        void OnEnable(){Application.logMessageReceived+=Log;}
        void OnDisable(){Application.logMessageReceived-=Log;}
        void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)errors.Add(message);}
        void Check(bool value,string name){checks.Add((value?"PASS ":"FAIL ")+name);}
        void Update(){if(measuring&&Time.timeScale>0){seconds+=Time.unscaledDeltaTime;frames++;}}
        IEnumerator Capture(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png"));yield return new WaitForSecondsRealtime(.2f);}
        IEnumerator Start()
        {
            folder=Path.GetFullPath(Path.Combine(Application.dataPath,Application.isEditor?"../../Verification/OverTheHill":"../../../Verification/OverTheHill"));Directory.CreateDirectory(folder);
            yield return new WaitForSecondsRealtime(1.5f);
            ui=FindFirstObjectByType<SailUI>();weather=FindFirstObjectByType<VoyageAtmosphere>();journal=FindFirstObjectByType<VoyageJournal>();sea=FindFirstObjectByType<SeaSurface>();boat=FindFirstObjectByType<BoatController>();
            if(!ui||!weather||!journal||!sea||!boat){File.WriteAllText(Path.Combine(folder,"failure.txt"),"Scene links missing");yield break;}
            Screen.SetResolution(1600,900,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.3f);
            int originalQuality=PlayerPrefs.GetInt("quality",1);bool originalFocus=ui.cameraRig.softFocus;
            var cycleQuality=typeof(SailUI).GetMethod("CycleQuality",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            for(int i=0;i<3&&PlayerPrefs.GetInt("quality",1)!=2;i++)cycleQuality.Invoke(ui,null);
            ui.cameraRig.softFocus=true;
            var camera=ui.cameraRig.GetComponent<Camera>();ui.StartSailing();measuring=true;weather.advanceDay=false;weather.SetWeather(0,true);weather.SetHour(15.2f);ui.cameraRig.SetOrbit(-12,22);boat.throttle=.65f;
            Check(Mathf.Abs(camera.fieldOfView-34)<.01f&&ui.cameraRig.distance>=14,"Telephoto diorama camera");
            var volume=FindFirstObjectByType<Volume>();var profile=volume.profile;
            Check(profile.TryGet<DepthOfField>(out var dof)&&dof.mode.value==DepthOfFieldMode.Bokeh&&profile.Has<SplitToning>()&&profile.Has<LiftGammaGain>(),"Focus and analog grading are active");
            bool focus=ui.cameraRig.softFocus;ui.cameraRig.ToggleFocus();Check(ui.cameraRig.softFocus!=focus,"Soft focus can be disabled");ui.cameraRig.ToggleFocus();
            var animals=FindObjectsByType<WildlifeActor>(FindObjectsSortMode.None);var trees=FindObjectsByType<TreeBreeze>(FindObjectsSortMode.None);
            Check(animals.Count(a=>a.dolphin)==3&&animals.Count(a=>!a.dolphin)==9,"Nine gulls and three dolphins");Check(trees.Length>=50,"Island trees respond to wind");
            Vector3 bird=animals.First(a=>!a.dolphin).transform.position;Quaternion tree=trees[0].transform.rotation;
            yield return new WaitForSecondsRealtime(5);
            Check(Vector3.Distance(bird,animals.First(a=>!a.dolphin).transform.position)>.2f&&Quaternion.Angle(tree,trees[0].transform.rotation)>.02f,"Wildlife and vegetation really animate");
            Check(dof.focusDistance.value>12&&dof.focusDistance.value<60,"Focus follows actual camera distance");yield return Capture("daylight");
            weather.SetHour(6.6f);yield return Capture("dawn");weather.SetHour(18);yield return Capture("golden-hour");
            weather.SetHour(22.5f);ui.cameraRig.SetOrbit(-12,-8);yield return new WaitForSecondsRealtime(.2f);Check(weather.AuroraStrength>.7f,"Clear first night has aurora");yield return Capture("aurora");
            weather.dayIndex=1;weather.Apply();Check(weather.AuroraStrength==0,"Aurora has nights off");weather.dayIndex=0;weather.SetWeather(2,true);Check(weather.AuroraStrength==0,"Storm hides aurora");
            ui.cameraRig.SetOrbit(-12,22);weather.SetHour(14);yield return new WaitForSecondsRealtime(2);yield return Capture("storm");
            weather.SetWeather(0,true);weather.SetHour(18);ui.cameraRig.SetOrbit(-12,22);
            journal.UseStorage(Path.Combine(folder,"AlbumCheck"));int before=journal.PhotoCount;journal.SetPhotoMode(true);yield return Capture("viewfinder");journal.TakePhoto();yield return new WaitForSecondsRealtime(.5f);
            Check(journal.PhotoCount==before+1&&File.Exists(journal.LastPhotoPath)&&new FileInfo(journal.LastPhotoPath).Length>2000,"Photograph saves a real JPEG and caption");
            yield return Capture("photo-card");journal.SetPhotoMode(false);journal.OpenAlbum();
            Check(journal.AlbumOpen&&ui.ExternalPanelOpen&&Time.timeScale==0,"Album pauses sailing with a dedicated overlay");yield return Capture("album");
            bool fits=true;foreach(var button in FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None)){var corners=new Vector3[4];button.GetComponent<RectTransform>().GetWorldCorners(corners);foreach(var corner in corners)fits&=corner.x>=-1&&corner.y>=-1&&corner.x<=Screen.width+1&&corner.y<=Screen.height+1;}
            Check(fits,"Album buttons fit the actual screen");
            var card=FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).First(b=>b.name=="Fotoğraf kartı");card.onClick.Invoke();yield return Capture("photo-detail");
            Check(FindObjectsByType<UnityEngine.UI.RawImage>(FindObjectsSortMode.None).Any(image=>image.name=="Büyük fotoğraf"&&image.texture),"Photo card opens the full image");
            int saved=journal.PhotoCount;journal.UseStorage(journal.StoragePath);Check(journal.PhotoCount==saved,"Photo index reloads from disk");journal.CloseAlbum();
            Check(!ui.MenuOpen&&Time.timeScale==1,"Closing album resumes sailing");
            var mooring=FindFirstObjectByType<MooringLine>();weather.SetHour(15.2f);boat.throttle=0;var near=mooring.bollard.position+new Vector3(5,0,-5);near.y=sea.SampleSurface(near).position.y;
            boat.Body.position=near;boat.Body.rotation=Quaternion.identity;boat.Body.linearVelocity=Vector3.zero;boat.Body.angularVelocity=Vector3.zero;sea.ClearWake();ui.cameraRig.distance=26;ui.cameraRig.SetOrbit(65,30);
            yield return new WaitForSecondsRealtime(1.5f);Check(mooring.Attach(),"Slow approach can attach a physical mooring line");
            yield return new WaitForSecondsRealtime(2);Check(mooring.Moored&&boat.GetComponent<SpringJoint>()&&boat.Speed<3&&Vector3.Distance(mooring.Bow,mooring.bollard.position)<15,"Mooring remains bounded while boat floats");yield return Capture("mooring");
            weather.SetHour(22);int day=weather.dayIndex;journal.WaitUntilMorning();yield return new WaitForSecondsRealtime(3.3f);Check(weather.hours>6.5f&&weather.hours<7&&weather.dayIndex==day+1,"Moored rest advances the sky and night schedule to morning");
            boat.Recover();yield return null;Check(!mooring.Moored&&boat.GetComponent<SpringJoint>()==null,"Recovery releases the mooring joint");
            ui.cameraRig.distance=16;ui.cameraRig.SetOrbit(-12,22);weather.SetHour(15.2f);boat.throttle=.6f;
            var motion=Path.Combine(folder,"Motion");Directory.CreateDirectory(motion);var times=new List<string>();
            for(int i=0;i<48;i++){if(ui.MenuOpen)ui.Resume();yield return new WaitForSecondsRealtime(.08f);yield return new WaitForEndOfFrame();var image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(motion,i.ToString("D3")+".jpg"),image.EncodeToJPG(88));Destroy(image);times.Add(Time.realtimeSinceStartup.ToString("R",System.Globalization.CultureInfo.InvariantCulture));}
            File.WriteAllLines(Path.Combine(motion,"timestamps.txt"),times);measuring=false;
            Check(frames>300&&seconds>15,"Real simulation frames advanced");Check(errors.Count==0,"No runtime errors");
            var pipeline=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            var report=new Report{passed=checks.All(c=>c.StartsWith("PASS"))&&errors.Count==0,checks=checks.ToArray(),errors=errors.ToArray(),seconds=seconds,frames=frames,averageFps=frames/Mathf.Max(.001f,seconds),gpu=SystemInfo.graphicsDeviceName,resolution=Screen.width+"x"+Screen.height,renderScale=pipeline.renderScale,softFocus=ui.cameraRig.softFocus,development=Debug.isDebugBuild};
            File.WriteAllText(Path.Combine(folder,Application.isEditor?"editor-report.json":"windows-report.json"),JsonUtility.ToJson(report,true));
            for(int i=0;i<3&&PlayerPrefs.GetInt("quality",1)!=originalQuality;i++)cycleQuality.Invoke(ui,null);
            if(ui.cameraRig.softFocus!=originalFocus)ui.cameraRig.ToggleFocus();
            weather.advanceDay=true;ui.Pause();if(!Application.isEditor)ui.Quit();
        }
        [Serializable]class Report{public bool passed,development,softFocus;public string[] checks,errors;public float seconds,averageFps,renderScale;public int frames;public string gpu,resolution;}
    }
}
