using System.Collections.Generic;
using UnityEngine;

namespace JustSail
{
    [DefaultExecutionOrder(-90)]
    public sealed class VoyageAtmosphere : MonoBehaviour
    {
        public SeaSurface sea;
        public Light sun;
        public Camera view;
        public BoatController boat;
        public Material particleMaterial;
        public float hours=15.2f,dayDuration=1440;
        public bool advanceDay=true;
        [Range(0,3)] public int weatherMode=3;
        [Range(0,1)] public float stormStrength=1;
        public bool auroraEnabled=true;
        public int dayIndex;
        public bool AuroraTonight=>dayIndex%3!=1;
        public float AuroraStrength {get;private set;}
        public float Storm {get;private set;}
        public float CloudCover {get;private set;}
        public float Night {get;private set;}
        public Vector2 StormCenter {get;private set;}
        public float StormRadius=285;
        ParticleSystem rain,splash;
        Light beacon;
        readonly List<Material> wetMaterials=new();
        readonly List<Color> dryColors=new();
        Material lantern;
        static Color Hex(string c){ColorUtility.TryParseHtmlString("#"+c,out var v);return v;}
        void Awake()
        {
            if(!sea)sea=FindFirstObjectByType<SeaSurface>();
            if(!boat)boat=FindFirstObjectByType<BoatController>();
            if(!view)view=Camera.main;if(!sun)sun=RenderSettings.sun;
            if(!sea||!boat||!view||!sun){enabled=false;Debug.LogError("Voyage atmosphere references are missing.");return;}
            var clones=new Dictionary<Material,Material>();
            foreach(var renderer in FindObjectsByType<MeshRenderer>())
            {
                if(renderer.gameObject==sea.gameObject||renderer.gameObject.layer==6)continue;
                var originals=renderer.sharedMaterials;var instances=new Material[originals.Length];
                for(int i=0;i<originals.Length;i++)
                {
                    if(!originals[i]||!originals[i].HasProperty("_Smoothness")){instances[i]=originals[i];continue;}
                    if(renderer.name!="Lantern"&&clones.TryGetValue(originals[i],out var shared)){instances[i]=shared;continue;}
                    var m=new Material(originals[i]);instances[i]=m;wetMaterials.Add(m);dryColors.Add(m.GetColor("_BaseColor"));
                    if(renderer.name!="Lantern")clones[originals[i]]=m;
                    if(renderer.name=="Lantern"){lantern=m;lantern.EnableKeyword("_EMISSION");}
                }
                renderer.sharedMaterials=instances;
            }
            var lightHouse=GameObject.Find("Lighthouse");
            if(lightHouse){var g=new GameObject("Night beacon");g.transform.SetParent(lightHouse.transform,false);g.transform.localPosition=new(0,10.7f,0);beacon=g.AddComponent<Light>();beacon.type=LightType.Point;beacon.color=Hex("FFD19A");beacon.range=32;beacon.intensity=3;}
            rain=MakeParticles("Passing rain",view.transform,false);splash=MakeParticles("Bow spray",boat.transform,true);
            SetWeather(weatherMode,true);Apply();
        }
        ParticleSystem MakeParticles(string name,Transform parent,bool bow)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);
            if(bow){go.transform.localPosition=new(0,.3f,2.55f);go.transform.localRotation=Quaternion.Euler(-65,0,0);}
            var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.playOnAwake=false;main.loop=true;main.duration=6;main.simulationSpace=ParticleSystemSimulationSpace.World;
            main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;
            main.startLifetime=bow?.6f:1.9f;main.startSpeed=bow?3:17;main.startSize=bow?.07f:.026f;main.maxParticles=bow?200:3500;
            main.startColor=bow?new Color(.85f,.94f,.9f,.65f):new Color(.7f,.83f,.88f,.3f);main.gravityModifier=bow?.65f:0;
            var emission=ps.emission;emission.rateOverTime=0;
            var shape=ps.shape;shape.shapeType=bow?ParticleSystemShapeType.Cone:ParticleSystemShapeType.Box;shape.radius=.35f;
            if(bow)shape.angle=25;else{shape.scale=new(34,1,34);go.transform.rotation=Quaternion.Euler(90,0,0);}
            var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=particleMaterial;
            renderer.renderMode=bow?ParticleSystemRenderMode.Billboard:ParticleSystemRenderMode.Stretch;
            renderer.lengthScale=3;renderer.velocityScale=.045f;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            var color=ps.colorOverLifetime;color.enabled=true;var g=new Gradient();g.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.15f),new GradientAlphaKey(0,1)});color.color=g;
            return ps;
        }
        public void SetHour(float value){hours=Mathf.Repeat(value,24);Apply();}
        public void SetWeather(int mode,bool immediate=false)
        {
            weatherMode=Mathf.Clamp(mode,0,3);
            if(immediate)Storm=DesiredStorm();
            Apply();
        }
        float DesiredStorm()
        {
            float clock=sea?sea.Clock:0;
            StormCenter=new(180+Mathf.Cos(clock*.0018f)*430,350+Mathf.Sin(clock*.0014f)*390);
            if(weatherMode==2)return stormStrength;
            if(weatherMode!=3||!boat)return 0;
            Vector3 bodyPosition=boat.Body?boat.Body.position:boat.transform.position;
            Vector2 p=new(bodyPosition.x,bodyPosition.z);
            return (1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(StormRadius*.2f,StormRadius,Vector2.Distance(p,StormCenter))))*stormStrength;
        }
        void Update()
        {
            if(advanceDay){float before=hours;hours=Mathf.Repeat(hours+Time.deltaTime*24/Mathf.Max(60,dayDuration),24);if(hours<before)dayIndex++;}
            Storm=Mathf.MoveTowards(Storm,DesiredStorm(),Time.deltaTime/7);
            Apply();
            if(rain)
            {
                rain.transform.position=view.transform.position+Vector3.up*17;
                var e=rain.emission;e.rateOverTime=Storm*1200;
                if(Storm>.01f&&!rain.isPlaying)rain.Play();
                var velocity=rain.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;
                velocity.x=sea.windDirection.x*Storm*4;velocity.z=sea.windDirection.y*Storm*4;
            }
            if(splash){var e=splash.emission;e.rateOverTime=Mathf.Clamp(boat.Speed*boat.BowEntry*16,0,90);if(!splash.isPlaying)splash.Play();}
        }
        public void Apply()
        {
            if(!sun||!sea)return;
            float angle=(hours-6)/24*Mathf.PI*2;
            Vector3 sunDirection=new(Mathf.Cos(angle)*.76f,Mathf.Sin(angle),Mathf.Cos(angle)*.65f);sunDirection.Normalize();
            sunDirection=Quaternion.Euler(0,-175,0)*sunDirection;
            float daylight=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.32f,.18f,sunDirection.y));Night=1-daylight;
            float golden=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.32f,0,sunDirection.y))*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.08f,.55f,sunDirection.y)));
            bool evening=hours>12;
            Color top=Color.Lerp(Hex("142540"),Hex("4695BF"),daylight);
            top=Color.Lerp(top,evening?Hex("607B9E"):Hex("7B9CB4"),golden*.8f);
            Color horizon=Color.Lerp(Hex("304A68"),Hex("B0D8DD"),daylight);
            horizon=Color.Lerp(horizon,evening?Hex("E8AF88"):Hex("F0C9A1"),golden);
            CloudCover=weatherMode==1?.73f:Mathf.Lerp(.38f,.89f,Storm);
            float overcast=weatherMode==1?1:Storm;
            top=Color.Lerp(top,Hex("839BA9"),overcast*.25f*daylight);
            top=Color.Lerp(top,Hex("435B6B"),Storm*.75f);horizon=Color.Lerp(horizon,Hex("82979D"),Storm*.65f);
            Color sunlight=Color.Lerp(Hex("FFF0D5"),Hex("FFB174"),golden*.85f);
            sunlight=Color.Lerp(Hex("B4CEFF"),sunlight,daylight);
            float lightning=Storm>.6f?Mathf.Pow(Mathf.Max(0,Mathf.Sin(sea.Clock*2*Mathf.PI/19)),180)*Storm:0;
            Vector3 keyDirection=daylight>.15f?sunDirection:-sunDirection;keyDirection.y=Mathf.Max(.045f,Mathf.Abs(keyDirection.y));keyDirection.Normalize();
            sun.transform.rotation=Quaternion.LookRotation(-keyDirection,Vector3.forward);
            sun.color=sunlight;sun.intensity=Mathf.Lerp(.26f,1.5f,daylight)*Mathf.Lerp(1,.43f,Storm)*Mathf.Lerp(1,.66f,weatherMode==1?1:0)+lightning*1.8f;
            sun.shadowStrength=Mathf.Lerp(.88f,.55f,overcast);RenderSettings.sun=sun;
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=Color.Lerp(Hex("354E70"),Hex("839BB5"),daylight)*Mathf.Lerp(1,.7f,Storm);
            RenderSettings.ambientEquatorColor=Color.Lerp(Hex("263C55"),Hex("63798A"),daylight);
            RenderSettings.ambientGroundColor=Color.Lerp(Hex("1C2F45"),Hex("454F5B"),daylight);
            AuroraStrength=auroraEnabled&&AuroraTonight&&weatherMode!=1?Mathf.SmoothStep(0,1,Mathf.InverseLerp(.65f,.97f,Night))*(1-Storm)*.85f:0;
            RenderSettings.ambientSkyColor+=new Color(.025f,.065f,.045f)*AuroraStrength;
            RenderSettings.fogColor=horizon;RenderSettings.fogDensity=Mathf.Lerp(.00030f,.0045f,Storm);
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Exponential;
            float windAngle=(24+Mathf.Sin(sea.Clock*.002f)*18+Storm*16)*Mathf.Deg2Rad;
            sea.windDirection=new(Mathf.Cos(windAngle),Mathf.Sin(windAngle));sea.waveScale=Mathf.Lerp(.42f,1.65f,Storm)+(weatherMode==1?.12f:0);
            Shader.SetGlobalVector("_SunDirection",sunDirection);Shader.SetGlobalVector("_MoonDirection",-sunDirection);
            Shader.SetGlobalFloat("_WorldClock",sea.Clock);Shader.SetGlobalFloat("_NightBlend",Night);Shader.SetGlobalFloat("_Lightning",lightning);
            Shader.SetGlobalVector("_WeatherData",new(CloudCover,Storm,daylight,golden));
            Shader.SetGlobalVector("_StormRegion",new(StormCenter.x,StormCenter.y,StormRadius,weatherMode==3?stormStrength:0));
            Shader.SetGlobalVector("_WindData",new(sea.windDirection.x,sea.windDirection.y,Storm,0));
            SetColor("_SkyTop",top);SetColor("_SkyHorizon",horizon);
            SetColor("_CloudLight",Color.Lerp(Hex("70859E"),Color.Lerp(Hex("FFF3D7"),Hex("CBD3D2"),Storm),daylight));
            SetColor("_CloudDark",Color.Lerp(Hex("27394F"),Color.Lerp(Hex("839FAF"),Hex("445967"),Storm),daylight));
            SetColor("_SeaDeep",Color.Lerp(Hex("112F49"),Hex("24547D"),daylight)*Mathf.Lerp(1,.82f,Storm));
            SetColor("_SeaMiddle",Color.Lerp(Hex("193E51"),Hex("3C7395"),daylight)*Mathf.Lerp(1,.9f,Storm));
            SetColor("_SeaShallow",Color.Lerp(Hex("24515B"),Hex("6AA4AB"),daylight));
            SetColor("_SeaFoam",Color.Lerp(Hex("7499B0"),Hex("D7EBDE"),daylight));
            Shader.SetGlobalFloat("_Wetness",Storm);
            Shader.SetGlobalFloat("_AuroraStrength",AuroraStrength);
            Shader.SetGlobalFloat("_HazeDensity",Mathf.Lerp(.00085f,.004f,Storm));
            SetColor("_HazeColor",Color.Lerp(horizon,Hex("B6B0C4"),daylight*golden*.28f));
            for(int i=0;i<wetMaterials.Count;i++){var m=wetMaterials[i];if(!m)continue;m.SetColor("_BaseColor",dryColors[i]*Mathf.Lerp(1,.79f,Storm));m.SetFloat("_Smoothness",Mathf.Lerp(.045f,.36f,Storm));}
            if(lantern)lantern.SetColor("_EmissionColor",Hex("FFD092")*(Night*2));
            if(beacon)beacon.enabled=Night>.2f;
        }
        static void SetColor(string id,Color value)=>Shader.SetGlobalColor(id,value.linear);
        void OnDestroy(){foreach(var m in wetMaterials)if(m)Destroy(m);}
    }
}
