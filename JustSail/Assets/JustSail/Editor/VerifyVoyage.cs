using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace JustSail.Editor
{
    // Runs against the real scene/player loop, not a copy of the movement equations.
    public static class VerifyVoyage
    {
        static BoatController boat;static SailUI ui;static Keyboard keyboard;
        static double started,phaseAt;static int phase,startFrame;static Vector3 startPos,pausedPos;static float pausedClock,yaw;
        static readonly List<string> checks=new();static float minY,maxY,maxRoll;static bool active;
        public static void Begin()
        {
            if(!EditorApplication.isPlaying)throw new Exception("Enter Play Mode first");
            if(active)throw new Exception("Verification already active");
            boat=UnityEngine.Object.FindFirstObjectByType<BoatController>();ui=UnityEngine.Object.FindFirstObjectByType<SailUI>();
            if(!boat||!ui)throw new Exception("Scene components missing");
            checks.Clear();keyboard=InputSystem.AddDevice<Keyboard>();keyboard.MakeCurrent();
            boat.Recover();ui.StartSailing();startPos=boat.transform.position;startFrame=Time.frameCount;
            started=phaseAt=EditorApplication.timeSinceStartup;phase=0;minY=100;maxY=-100;maxRoll=0;active=true;
            EditorApplication.update+=Tick;
        }
        static void Assert(bool condition,string name){if(!condition)throw new Exception(name);checks.Add(name);}
        static void Key(params Key[] keys){keyboard.MakeCurrent();InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));}
        static Button FindButton(string label)=>UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.GetComponentInChildren<TMP_Text>().text.StartsWith(label,StringComparison.Ordinal));
        static void Next(){phase++;phaseAt=EditorApplication.timeSinceStartup;}
        static void Tick()
        {
            try
            {
                if(!EditorApplication.isPlaying)throw new Exception("Play Mode ended");
                double elapsed=EditorApplication.timeSinceStartup-phaseAt;
                minY=Mathf.Min(minY,boat.transform.position.y);maxY=Mathf.Max(maxY,boat.transform.position.y);
                maxRoll=Mathf.Max(maxRoll,Vector3.Angle(boat.transform.up,Vector3.up));
                if(phase==0){Key(elapsed<3?new[]{UnityEngine.InputSystem.Key.W}:Array.Empty<Key>());if(elapsed>8){Assert(Vector3.Distance(startPos,boat.transform.position)>4,"W input accelerates and moves boat");Assert(boat.Speed>1,"Forward speed develops");yaw=boat.transform.eulerAngles.y;Next();}}
                else if(phase==1){Key(UnityEngine.InputSystem.Key.D);if(elapsed>2.5){Assert(Mathf.Abs(Mathf.DeltaAngle(yaw,boat.transform.eulerAngles.y))>8,"D input turns boat");Key();ui.Pause();pausedPos=boat.transform.position;pausedClock=boat.sea.Clock;Next();}}
                else if(phase==2 && elapsed>1.3){Assert(Vector3.Distance(pausedPos,boat.transform.position)<.001f,"Pause freezes physics");Assert(Mathf.Abs(pausedClock-boat.sea.Clock)<.001f,"Pause freezes wave time");FindButton("Ayarlar").onClick.Invoke();Next();}
                else if(phase==3 && elapsed>.5)
                {
                    var sliders=UnityEngine.Object.FindObjectsByType<Slider>(FindObjectsSortMode.None);
                    sliders.First(s=>s.name=="Ses slider").value=.42f;Assert(Mathf.Abs(AudioListener.volume-.42f)<.001f,"Volume control changes listener");
                    sliders.First(s=>s.name=="Kamera hassasiyeti slider").value=.85f;Assert(Mathf.Abs(ui.cameraRig.sensitivity-.85f)<.001f,"Camera sensitivity control works");
                    FindButton("Görüntü").onClick.Invoke();Assert(PlayerPrefs.HasKey("quality"),"Quality selection persists");
                    FindButton("Işık").onClick.Invoke();Assert(ui.WeatherIndex==1,"Atmosphere selection works");
                    FindButton("Geri").onClick.Invoke();Assert(ui.MenuOpen&&Time.timeScale==0,"Settings returns to paused game");
                    FindButton("Tekneyi Kurtar").onClick.Invoke();Assert(Vector3.ProjectOnPlane(boat.Body.position-boat.spawn,Vector3.up).magnitude<.1f,"Recovery returns to safe water");Next();
                }
                else if(phase==4 && elapsed>5)
                {
                    Assert(Time.frameCount>startFrame+50,"Real player frames advanced");Assert(minY>-2&&maxY<2,"Buoyancy height stays bounded");Assert(maxRoll<35,"Roll and pitch stay bounded");
                    Assert(Physics.Raycast(new Vector3(-105,120,100),Vector3.down,200,1<<6),"Island collider exists and is queryable");
                    ui.Pause();FindButton("Ana Menü").onClick.Invoke();Assert(!ui.Playing&&ui.MenuOpen,"Return to main menu works");
                    Finish(null);
                }
            }
            catch(Exception ex){Finish(ex.ToString());}
        }
        static void Finish(string error)
        {
            EditorApplication.update-=Tick;active=false;if(keyboard!=null){InputSystem.RemoveDevice(keyboard);keyboard=null;}
            Directory.CreateDirectory("../Verification");
            var report=new Report{passed=error==null,error=error,checks=checks.ToArray(),seconds=(float)(EditorApplication.timeSinceStartup-started),frames=Time.frameCount-startFrame,minY=minY,maxY=maxY,maxTilt=maxRoll,device=SystemInfo.graphicsDeviceName};
            File.WriteAllText("../Verification/editor-run.json",JsonUtility.ToJson(report,true));Debug.Log("JUST_SAIL_VERIFICATION "+(error??"PASSED"));
        }
        [Serializable]class Report{public bool passed;public string error;public string[] checks;public float seconds,minY,maxY,maxTilt;public int frames;public string device;}
    }
}
