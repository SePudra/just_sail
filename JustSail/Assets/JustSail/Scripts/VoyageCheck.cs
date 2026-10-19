using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace JustSail
{
    // Opt-in standalone acceptance run. Normal launches do not create this component.
    public sealed class VoyageCheck : MonoBehaviour
    {
        readonly List<string> checks=new();readonly List<string> errors=new();
        BoatController boat;SailUI ui;Keyboard keyboard;
        string folder;float elapsed,minY=100,maxY=-100,maxTilt;int frames;bool measure;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Launch(){if(Environment.GetCommandLineArgs().Any(a=>a=="--just-sail-check"||a=="--just-sail-smoke"))new GameObject("Acceptance check").AddComponent<VoyageCheck>();}
        void Update(){if(measure&&Time.timeScale>0){elapsed+=Time.unscaledDeltaTime;frames++;minY=Mathf.Min(minY,boat.transform.position.y);maxY=Mathf.Max(maxY,boat.transform.position.y);maxTilt=Mathf.Max(maxTilt,Vector3.Angle(boat.transform.up,Vector3.up));}}
        void OnEnable(){Application.logMessageReceived+=Log;}
        void OnDisable(){Application.logMessageReceived-=Log;}
        void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)errors.Add(message);}
        void Check(bool pass,string name){checks.Add((pass?"PASS ":"FAIL ")+name);}
        Button FindButton(string label)=>FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.GetComponentInChildren<TMP_Text>().text.StartsWith(label,StringComparison.Ordinal));
        void Press(params Key[] keys){keyboard.MakeCurrent();InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));}
        IEnumerator Capture(string name,int width=1600,int height=900)
        {
            Screen.SetResolution(width,height,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(1);
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png"));yield return new WaitForSecondsRealtime(.3f);
        }
        IEnumerator Start()
        {
            folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Verification/Standalone"));Directory.CreateDirectory(folder);
            yield return new WaitForSecondsRealtime(2);
            boat=FindFirstObjectByType<BoatController>();ui=FindFirstObjectByType<SailUI>();
            if(Environment.GetCommandLineArgs().Contains("--just-sail-smoke"))
            {
                Check(Mathf.Abs(AudioListener.volume-.42f)<.001f,"Volume survives application restart");
                Check(Mathf.Abs(ui.cameraRig.sensitivity-.85f)<.001f,"Camera sensitivity survives application restart");
                var pipeline=UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
                Check(pipeline&&Mathf.Abs(pipeline.renderScale-new[]{.75f,1f,1.15f}[PlayerPrefs.GetInt("quality")])<.001f,"Quality survives application restart");
                Check(Screen.width==PlayerPrefs.GetInt("width")&&Screen.height==PlayerPrefs.GetInt("height"),"Display setting survives application restart");
                Check(!ui.Playing&&ui.MenuOpen,"Normal start opens main menu");
                yield return Capture("release-menu",Screen.width,Screen.height);FindButton("Denize Açıl").onClick.Invoke();
                yield return Capture("release-sailing",Screen.width,Screen.height);ui.Pause();
                Check(ui.Playing&&ui.MenuOpen,"Start and pause after restart");Check(errors.Count==0,"No runtime errors after restart");
                File.WriteAllLines(Path.Combine(folder,"restart-check.txt"),checks.Concat(errors));FindButton("Oyundan Çık").onClick.Invoke();yield break;
            }
            keyboard=InputSystem.AddDevice<Keyboard>();keyboard.MakeCurrent();
            yield return Capture("menu");
            FindButton("Denize Açıl").onClick.Invoke();var initial=boat.transform.position;
            Press(Key.W);yield return new WaitForSecondsRealtime(3);Press();yield return new WaitForSecondsRealtime(4);
            Check(boat.throttle>.8f&&Vector3.Distance(initial,boat.transform.position)>5,"Keyboard forward input and movement");
            float yaw=boat.transform.eulerAngles.y;Press(Key.D);yield return new WaitForSecondsRealtime(2.5f);Press();
            Check(Mathf.Abs(Mathf.DeltaAngle(yaw,boat.transform.eulerAngles.y))>8,"Keyboard steering");
            ui.Pause();var paused=boat.transform.position;float clock=boat.sea.Clock;yield return new WaitForSecondsRealtime(1);
            Check(Vector3.Distance(paused,boat.transform.position)<.001f&&Mathf.Abs(clock-boat.sea.Clock)<.001f,"Pause freezes boat and waves");
            yield return Capture("pause");
            FindButton("Ayarlar").onClick.Invoke();yield return null;
            FindObjectsByType<Slider>(FindObjectsSortMode.None).First(s=>s.name=="Ses slider").value=.42f;
            Check(Mathf.Abs(AudioListener.volume-.42f)<.001f,"Volume setting");
            FindObjectsByType<Slider>(FindObjectsSortMode.None).First(s=>s.name=="Kamera hassasiyeti slider").value=.85f;
            Check(Mathf.Abs(ui.cameraRig.sensitivity-.85f)<.001f,"Camera sensitivity setting");
            FindButton("Görüntü").onClick.Invoke();Check(PlayerPrefs.HasKey("quality"),"Quality setting saved");
            var originalSize=new Vector2Int(Screen.width,Screen.height);var originalMode=Screen.fullScreenMode;
            FindButton("Çözünürlük").onClick.Invoke();yield return new WaitForSecondsRealtime(1);
            Check(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TMP_Text>().text=="Koru","Display confirmation gets keyboard focus");
            FindButton("Geri Al").onClick.Invoke();yield return new WaitForSecondsRealtime(1);
            Check(Screen.width==originalSize.x&&Screen.height==originalSize.y,"Resolution revert");
            FindButton("Ekran").onClick.Invoke();yield return new WaitForSecondsRealtime(1);FindButton("Koru").onClick.Invoke();
            Check(Screen.fullScreenMode!=originalMode&&PlayerPrefs.HasKey("fullscreen"),"Fullscreen change and save");
            FindButton("Ekran").onClick.Invoke();yield return new WaitForSecondsRealtime(13);
            Check(Screen.fullScreenMode!=originalMode,"Display confirmation timeout reverts");
            FindButton("Ekran").onClick.Invoke();yield return new WaitForSecondsRealtime(1);FindButton("Koru").onClick.Invoke();
            yield return Capture("settings-720",1280,720);yield return Capture("settings-1440",2560,1440);yield return Capture("settings-ultrawide",2560,1080);
            FindButton("Geri").onClick.Invoke();Check(ui.MenuOpen&&Time.timeScale==0,"Settings returns to pause");
            FindButton("Tekneyi Kurtar").onClick.Invoke();Check(Vector3.ProjectOnPlane(boat.Body.position-boat.spawn,Vector3.up).magnitude<.1f,"Recovery");
            var mouse=InputSystem.AddDevice<Mouse>();mouse.MakeCurrent();float oldDistance=ui.cameraRig.distance;var oldCamera=ui.cameraRig.transform.position;
            InputSystem.QueueStateEvent(mouse,new MouseState{delta=new Vector2(120,-60),scroll=new Vector2(0,120)}.WithButton(MouseButton.Right));yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState());yield return new WaitForSecondsRealtime(.5f);
            Check(ui.cameraRig.distance<oldDistance&&Vector3.Distance(oldCamera,ui.cameraRig.transform.position)>1,"Mouse orbit and zoom");InputSystem.RemoveDevice(mouse);ui.cameraRig.distance=24;ui.cameraRig.Snap();
            yield return Capture("sailing");
            ui.Pause();FindButton("Ayarlar").onClick.Invoke();FindButton("Işık").onClick.Invoke();FindButton("Geri").onClick.Invoke();ui.Resume();yield return Capture("mist");
            ui.Pause();FindButton("Ayarlar").onClick.Invoke();FindButton("Işık").onClick.Invoke();FindButton("Geri").onClick.Invoke();ui.Resume();yield return Capture("evening");
            ui.Pause();FindButton("Ayarlar").onClick.Invoke();FindButton("Işık").onClick.Invoke();FindButton("Geri").onClick.Invoke();ui.Resume();
            // Drive from the water into the actual southern shore, then test recovery.
            boat.Recover();boat.Body.position=new Vector3(-100,0,40);boat.Body.rotation=Quaternion.identity;boat.throttle=1;boat.Body.linearVelocity=Vector3.forward*5;
            int contacts=boat.Contacts;yield return new WaitForSecondsRealtime(12);
            Check(boat.Contacts>contacts,"Physical shore collision");
            var dock=GameObject.Find("Dock").transform;boat.Recover();var dockStart=dock.TransformPoint(new Vector3(-6,0,3.4f));dockStart.y=0;boat.Body.position=dockStart;boat.Body.rotation=Quaternion.Euler(0,90,0);boat.Body.linearVelocity=Vector3.right*3;boat.throttle=.5f;
            contacts=boat.Contacts;yield return new WaitForSecondsRealtime(10);Check(boat.Contacts>contacts,"Physical dock collision");
            File.WriteAllLines(Path.Combine(folder,"preflight.txt"),checks.Concat(errors));
            if(checks.Any(c=>c.StartsWith("FAIL"))||errors.Count>0){ui.Quit();yield break;}
            boat.Recover();ui.cameraRig.Snap();
            // Ten minutes on open water under the real player loop, at the selected quality.
            measure=true;boat.Body.position=new Vector3(400,0,-250);boat.Body.rotation=Quaternion.Euler(0,90,0);boat.throttle=.55f;boat.controlsEnabled=false;
            float simulationStart=boat.sea.Clock;int openWaterRepositions=0;
            float until=Time.realtimeSinceStartup+900;int testResumes=0;
            QualitySettings.vSyncCount=0;
            while(boat.sea.Clock-simulationStart<600&&Time.realtimeSinceStartup<until){if(ui.MenuOpen){ui.Resume();boat.controlsEnabled=false;testResumes++;}Application.targetFrameRate=elapsed<60?30:elapsed<120?120:60;if(boat.Body.position.x>1100){boat.Body.position=new Vector3(400,0,-250);boat.Body.linearVelocity=Vector3.zero;boat.sea.ClearWake();openWaterRepositions++;}File.WriteAllText(Path.Combine(folder,"heartbeat.txt"),elapsed.ToString("F1")+" seconds; sea time "+(boat.sea.Clock-simulationStart).ToString("F1"));yield return new WaitForSecondsRealtime(10);}
            Application.targetFrameRate=60;
            measure=false;Check(minY>-2&&maxY<2&&maxTilt<35,"Ten minute buoyancy stability");Check(boat.sea.Clock-simulationStart>=600,"Simulation advanced during stability run");Check(errors.Count==0,"No runtime errors");
            ui.Pause();FindButton("Ana Menü").onClick.Invoke();Check(!ui.Playing&&ui.MenuOpen,"Return to main menu");
            PlayerPrefs.Save();var report=new Report{checks=checks.ToArray(),errors=errors.ToArray(),passed=checks.All(c=>c.StartsWith("PASS"))&&errors.Count==0,seconds=elapsed,waveSeconds=boat.sea.Clock-simulationStart,frames=frames,averageFps=frames/Mathf.Max(.001f,elapsed),minY=minY,maxY=maxY,maxTilt=maxTilt,gpu=SystemInfo.graphicsDeviceName,resolution=Screen.width+"x"+Screen.height,quality=PlayerPrefs.GetInt("quality"),development=Debug.isDebugBuild};
            File.WriteAllText(Path.Combine(folder,"report.json"),JsonUtility.ToJson(report,true));File.WriteAllText(Path.Combine(folder,"stability-notes.txt"),"Open-water route reset by test harness "+openWaterRepositions+" times to avoid world boundary; no game recovery used during this stage. Test harness resumed after focus-loss pause "+testResumes+" times. Paused frames excluded; at least 600 seconds of wave simulation required.");
            InputSystem.RemoveDevice(keyboard);FindButton("Çıkış").onClick.Invoke();
        }
        [Serializable]class Report{public bool passed,development;public string[] checks,errors;public float seconds,waveSeconds,averageFps,minY,maxY,maxTilt;public int frames,quality;public string gpu,resolution;}
    }
}
