using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace JustSail
{
    // Opt-in evidence capture; normal launches never create this component.
    public sealed class OceanCheck : MonoBehaviour
    {
        readonly List<string> checks=new(),errors=new();
        SeaSurface sea;BoatController boat;VoyageAtmosphere weather;SailUI ui;
        string folder;float minY=100,maxY=-100,maxTilt,seconds;int frames,focusResumes,rainParticlesAtCheck;bool measure,expectedPause,keyboardWasEnabled;
        UnityEngine.InputSystem.Keyboard checkKeyboard;
        float positionError,normalError,inverseError,jacobianMin=100;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Launch(){if(Environment.GetCommandLineArgs().Contains("--just-sail-ocean-check"))new GameObject("Ocean evidence").AddComponent<OceanCheck>();}
        public static void Begin(){new GameObject("Ocean evidence").AddComponent<OceanCheck>();}
        void OnEnable(){Application.logMessageReceived+=Log;}
        void OnDisable(){Application.logMessageReceived-=Log;if(keyboardWasEnabled&&checkKeyboard!=null&&checkKeyboard.added)UnityEngine.InputSystem.InputSystem.EnableDevice(checkKeyboard);}
        void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)errors.Add(message);}
        void Check(bool value,string name){checks.Add((value?"PASS ":"FAIL ")+name);}
        void Update(){if(measure&&ui&&ui.MenuOpen&&!expectedPause){ui.Resume();focusResumes++;}if(measure&&Time.timeScale>0){seconds+=Time.unscaledDeltaTime;frames++;minY=Mathf.Min(minY,boat.Body.position.y);maxY=Mathf.Max(maxY,boat.Body.position.y);maxTilt=Mathf.Max(maxTilt,Vector3.Angle(boat.transform.up,Vector3.up));}}
        IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png"));yield return new WaitForSecondsRealtime(.15f);
        }
        void Parity()
        {
            const int count=192;var compute=Resources.Load<ComputeShader>("OceanParity");
            if(!compute){Check(false,"GPU parity shader is present");return;}
            var inputs=new Vector4[count];var output=new Vector4[count*3];var random=new System.Random(47);
            for(int i=0;i<count;i++)inputs[i]=new Vector4(-220+(float)random.NextDouble()*480,-110+(float)random.NextDouble()*700,i%3*97.23f,0);
            using(var ib=new ComputeBuffer(count,16))using(var ob=new ComputeBuffer(count*3,16))
            {
                ib.SetData(inputs);int k=compute.FindKernel("CSMain");compute.SetBuffer(k,"Inputs",ib);compute.SetBuffer(k,"Outputs",ob);compute.SetInt("Count",count);
                compute.SetTexture(k,"_Seabed",sea.depthMap);compute.SetVector("_DepthBounds",sea.depthBounds);compute.SetFloat("_HasSeabed",1);
                compute.SetVectorArray("_OceanWaves",sea.WaveParameters);compute.SetVectorArray("_OceanShapes",sea.ShapeParameters);
                float originalScale=sea.waveScale;
                foreach(float scale in new[]{.45f,.85f,1.65f})
                {
                    sea.waveScale=scale;compute.SetFloat("_WaveScale",scale);compute.Dispatch(k,(count+63)/64,1,1);ob.GetData(output);
                    for(int i=0;i<count;i++)
                    {
                        var sample=sea.Evaluate(new Vector2(inputs[i].x,inputs[i].y),inputs[i].z);
                        positionError=Mathf.Max(positionError,Vector3.Distance(sample.position,new Vector3(output[i*3].x,output[i*3].y,output[i*3].z)));
                        normalError=Mathf.Max(normalError,Vector3.Distance(sample.normal,new Vector3(output[i*3+1].x,output[i*3+1].y,output[i*3+1].z)));
                        var inverse=sea.SampleSurface(sample.position,inputs[i].z);inverseError=Mathf.Max(inverseError,Vector3.Distance(sample.position,inverse.position));
                        jacobianMin=Mathf.Min(jacobianMin,sample.jacobian);
                    }
                }
                sea.waveScale=originalScale;sea.Upload();
            }
            Check(positionError<.012f&&normalError<.012f,"CPU/GPU position and normal parity across three sea states");
            Check(inverseError<.004f,"World probes invert horizontal Gerstner displacement");Check(jacobianMin>.05f,"Sampled surface remains unfolded");
        }
        IEnumerator Start()
        {
            folder=Path.GetFullPath(Path.Combine(Application.dataPath,Application.isEditor?"../../Verification/REA":"../../../Verification/REA"));Directory.CreateDirectory(folder);
            yield return new WaitForSecondsRealtime(1.5f);
            sea=FindFirstObjectByType<SeaSurface>();boat=FindFirstObjectByType<BoatController>();weather=FindFirstObjectByType<VoyageAtmosphere>();ui=FindFirstObjectByType<SailUI>();
            if(!sea||!boat||!weather||!ui){File.WriteAllText(Path.Combine(folder,"failure.txt"),"Missing scene links");yield break;}
            // Only this explicit QA process owns input and focus while the scripted scenario runs.
            checkKeyboard=UnityEngine.InputSystem.Keyboard.current;keyboardWasEnabled=checkKeyboard?.enabled==true;
            if(keyboardWasEnabled)UnityEngine.InputSystem.InputSystem.DisableDevice(checkKeyboard);
            Screen.SetResolution(1600,900,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.3f);
            Parity();Check(sea.depthMap&&sea.FloorHeight(new Vector2(-105,100))>8&&sea.FloorHeight(new Vector2(0,-65))<-5,"Baked seabed uses actual island colliders");
            var points=(Vector3[])typeof(BoatController).GetField("points",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(boat);Check(points.Length==6,"Six buoyancy probes");
            ui.StartSailing();weather.advanceDay=false;weather.SetWeather(0,true);weather.SetHour(15.2f);boat.throttle=.75f;measure=true;
            yield return new WaitForSecondsRealtime(7);Check(boat.Speed>1,"Sail thrust drives the buoyant boat");yield return Capture("daylight");
            expectedPause=true;ui.Pause();float clock=sea.Clock,hours=weather.hours;Vector3 paused=boat.Body.position;
            yield return new WaitForSecondsRealtime(.5f);Check(Mathf.Abs(clock-sea.Clock)<.001f&&Mathf.Abs(hours-weather.hours)<.001f&&Vector3.Distance(paused,boat.Body.position)<.002f,"Pause freezes waves, day and physics");
            expectedPause=false;ui.Resume();weather.SetHour(23.99f);weather.dayDuration=60;weather.advanceDay=true;yield return new WaitForSecondsRealtime(.3f);
            Check(weather.hours<1,"Day cycle wraps midnight");weather.dayDuration=1440;weather.advanceDay=false;
            Check(Mathf.Abs(weather.dayDuration-1440)<.01f,"Normal day duration is 24 minutes");
            weather.SetHour(6.6f);yield return Capture("dawn");weather.SetHour(18.3f);yield return Capture("sunset");
            weather.SetHour(22.5f);yield return Capture("moonlight");Check(weather.Night>.9f&&-weather.sun.transform.forward.y>0,"Moon takes over as night light");
            weather.SetHour(14);weather.SetWeather(2,true);yield return new WaitForSecondsRealtime(7);
            Check(weather.Storm>.98f&&sea.waveScale>1.5f&&Shader.GetGlobalFloat("_Wetness")>.9f,"Storm drives waves, lighting and wetness");yield return Capture("storm");
            var rain=GameObject.Find("Passing rain").GetComponent<ParticleSystem>();rainParticlesAtCheck=rain.particleCount;Check(rainParticlesAtCheck>20,"Rain particles are alive in storm");
            Vector3 saved=boat.Body.position;boat.Body.position=new Vector3(weather.StormCenter.x,0,weather.StormCenter.y);weather.SetWeather(3,true);Check(weather.Storm>.98f,"Roaming storm has spatial core");
            boat.Body.position=new Vector3(weather.StormCenter.x+weather.StormRadius*2,0,weather.StormCenter.y);weather.SetWeather(3,true);Check(weather.Storm<.01f,"Roaming storm leaves distant water clear");boat.Body.position=saved;
            weather.SetWeather(0,true);weather.SetHour(15.2f);boat.Recover();boat.throttle=.6f;ui.cameraRig.Snap();
            // Real player frames, saved with timestamps for an honest variable-rate motion preview.
            string motion=Path.Combine(folder,"Motion");Directory.CreateDirectory(motion);var times=new List<string>();
            for(int i=0;i<48;i++)
            {
                if(ui.MenuOpen)ui.Resume();yield return new WaitForSecondsRealtime(.08f);yield return new WaitForEndOfFrame();
                var texture=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(motion,i.ToString("D3")+".jpg"),texture.EncodeToJPG(88));Destroy(texture);times.Add(Time.realtimeSinceStartup.ToString("R",System.Globalization.CultureInfo.InvariantCulture));
            }
            File.WriteAllLines(Path.Combine(motion,"timestamps.txt"),times);
            yield return new WaitForSecondsRealtime(7);measure=false;
            Check(minY>-3&&maxY<3&&maxTilt<40,"Clear/storm buoyancy stays bounded");Check(seconds>20&&frames>100,"Real simulation frames advanced");Check(errors.Count==0,"No runtime errors");
            weather.advanceDay=true;weather.SetWeather(3,true);ui.Pause();
            var report=new Report{passed=checks.All(c=>c.StartsWith("PASS"))&&errors.Count==0,checks=checks.ToArray(),errors=errors.ToArray(),cpuGpuPositionError=positionError,cpuGpuNormalError=normalError,inverseError=inverseError,minJacobian=jacobianMin,minY=minY,maxY=maxY,maxTilt=maxTilt,seconds=seconds,frames=frames,averageFps=frames/Mathf.Max(seconds,.001f),rainParticlesAtCheck=rainParticlesAtCheck,focusResumes=focusResumes,gpu=SystemInfo.graphicsDeviceName,resolution=Screen.width+"x"+Screen.height,development=Debug.isDebugBuild};
            File.WriteAllText(Path.Combine(folder,Application.isEditor?"editor-report.json":"windows-report.json"),JsonUtility.ToJson(report,true));
            if(!Application.isEditor)ui.Quit();
        }
        [Serializable]class Report{public bool passed,development;public string[] checks,errors;public float cpuGpuPositionError,cpuGpuNormalError,inverseError,minJacobian,minY,maxY,maxTilt,seconds,averageFps;public int frames,rainParticlesAtCheck,focusResumes;public string gpu,resolution;}
    }
}
