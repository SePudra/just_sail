using UnityEngine;

namespace JustSail
{
    [DefaultExecutionOrder(-80)]
    public sealed class SeaSurface : MonoBehaviour
    {
        public Transform boat;
        public Texture2D depthMap;
        public Vector4 depthBounds;
        public float waveScale=.85f;
        public Vector2 windDirection=new(.94f,.34f);
        // Unity supplies fixed time inside FixedUpdate and scaled render time elsewhere.
        // Accumulating deltaTime in Update made every physics step in a slow frame
        // sample the same stale wave (and produced different motion at 30/120 fps).
        double clockOrigin;
        public float Clock {get=>(float)(Time.timeAsDouble-clockOrigin);private set=>clockOrigin=Time.timeAsDouble-value;}
        public const int WaveCount=6;
        readonly Vector4[] waves=new Vector4[WaveCount], shapes=new Vector4[WaveCount];
        readonly Vector4[] wakes=new Vector4[48], wakeDirections=new Vector4[48];
        readonly float[] lengths={64,31,15,8,4.4f,2.7f};
        readonly float[] amplitudes={.56f,.28f,.14f,.075f,.035f,.018f};
        readonly float[] angles={0,28,-32,52,-18,83};
        readonly float[] steepness={.68f,.62f,.52f,.42f,.28f,.2f};
        int wakeCount;
        float nextWake,previousWakeTime;
        bool wavesConfigured;
        Vector3 previous;
        bool resetWake=true;
        Unity.Collections.NativeArray<float> depthPixels;
        Texture2D sampledDepth;
        public struct SurfaceSample
        {
            public Vector3 position,normal,velocity;
            public float jacobian;
            public Vector2 dx,dz;
        }
        void Awake(){clockOrigin=Time.timeAsDouble;Upload();}
        void ConfigureWaves()
        {
            // Swell keeps its heading. Rotating the entire phase field with each
            // weather update slid distant waves sideways through the world.
            if(wavesConfigured)return;
            wavesConfigured=true;
            var wind=windDirection.sqrMagnitude>.01f?windDirection.normalized:Vector2.right;
            for(int i=0;i<WaveCount;i++)
            {
                float a=angles[i]*Mathf.Deg2Rad;
                Vector2 d=new(wind.x*Mathf.Cos(a)-wind.y*Mathf.Sin(a),wind.x*Mathf.Sin(a)+wind.y*Mathf.Cos(a));
                float k=2*Mathf.PI/lengths[i];
                waves[i]=new(d.x,d.y,amplitudes[i],k);
                shapes[i]=new(steepness[i]/(k*WaveCount),Mathf.Sqrt(9.81f*k),i*1.713f,0);
            }
        }
        public float FloorHeight(Vector2 p)
        {
            if(!depthMap||depthBounds.z<=0||depthBounds.w<=0)return -60;
            Vector2 uv=new((p.x-depthBounds.x)/depthBounds.z,(p.y-depthBounds.y)/depthBounds.w);
            if(uv.x<0||uv.x>1||uv.y<0||uv.y>1)return -60;
            if(sampledDepth!=depthMap){depthPixels=depthMap.GetPixelData<float>(0);sampledDepth=depthMap;}
            // Hardware bilinear coordinates use texel centres; GetPixelBilinear differs at the edges.
            float x=uv.x*depthMap.width-.5f,z=uv.y*depthMap.height-.5f;int ix=Mathf.FloorToInt(x),iz=Mathf.FloorToInt(z);
            float tx=x-ix,tz=z-iz;int x0=Mathf.Clamp(ix,0,depthMap.width-1),x1=Mathf.Clamp(ix+1,0,depthMap.width-1);
            int z0=Mathf.Clamp(iz,0,depthMap.height-1)*depthMap.width,z1=Mathf.Clamp(iz+1,0,depthMap.height-1)*depthMap.width;
            return Mathf.Lerp(Mathf.Lerp(depthPixels[z0+x0],depthPixels[z0+x1],tx),Mathf.Lerp(depthPixels[z1+x0],depthPixels[z1+x1],tx),tz);
        }
        Vector3 Shore(Vector2 p)
        {
            float d=-FloorHeight(p),s=Mathf.Clamp01((d-.6f)/11);
            float fade=.08f+.92f*s*s*(3-2*s);
            float gradient=.92f*6*s*(1-s)/11;
            float x=-(FloorHeight(p+Vector2.right)-FloorHeight(p-Vector2.right))*.5f*gradient;
            float z=-(FloorHeight(p+Vector2.up)-FloorHeight(p-Vector2.up))*.5f*gradient;
            return new(fade,x,z);
        }
        // The GPU evaluates the same parametric surface in OceanWaves.hlsl.
        public SurfaceSample Evaluate(Vector2 p,float time)
        {
            Vector3 shore=Shore(p);float f=shore.x*waveScale,gx=shore.y*waveScale,gz=shore.z*waveScale;
            Vector3 pos=new(p.x,0,p.y),tx=Vector3.right,tz=Vector3.forward,vel=Vector3.zero;
            for(int i=0;i<WaveCount;i++)
            {
                Vector4 w=waves[i],s=shapes[i];float phase=w.w*(w.x*p.x+w.y*p.y)-s.y*time+s.z;
                float sn=Mathf.Sin(phase),cs=Mathf.Cos(phase),h=s.x;
                pos+=new Vector3(h*w.x*f*cs,w.z*f*sn,h*w.y*f*cs);
                tx+=new Vector3(h*w.x*(gx*cs-f*w.w*w.x*sn),w.z*(gx*sn+f*w.w*w.x*cs),h*w.y*(gx*cs-f*w.w*w.x*sn));
                tz+=new Vector3(h*w.x*(gz*cs-f*w.w*w.y*sn),w.z*(gz*sn+f*w.w*w.y*cs),h*w.y*(gz*cs-f*w.w*w.y*sn));
                vel+=new Vector3(h*w.x*f*s.y*sn,-w.z*f*s.y*cs,h*w.y*f*s.y*sn);
            }
            return new SurfaceSample{position=pos,normal=Vector3.Cross(tz,tx).normalized,velocity=vel,jacobian=tx.x*tz.z-tz.x*tx.z,dx=new(tx.x,tx.z),dz=new(tz.x,tz.z)};
        }
        public SurfaceSample SampleSurface(Vector3 world,float time)
        {
            Vector2 target=new(world.x,world.z),p=target;
            // Invert horizontal Gerstner displacement before sampling a world-space probe.
            for(int i=0;i<5;i++)
            {
                var s=Evaluate(p,time);Vector2 e=new(s.position.x-target.x,s.position.z-target.y);
                if(e.sqrMagnitude<.0000001f)break;
                float det=s.dx.x*s.dz.y-s.dz.x*s.dx.y;
                if(Mathf.Abs(det)<.05f){p-=e*.5f;continue;}
                p-=Vector2.ClampMagnitude(new Vector2(s.dz.y*e.x-s.dz.x*e.y,-s.dx.y*e.x+s.dx.x*e.y)/det,8);
            }
            return Evaluate(p,time);
        }
        public SurfaceSample SampleSurface(Vector3 world)=>SampleSurface(world,Clock);
        public Vector4[] WaveParameters=>waves;
        public Vector4[] ShapeParameters=>shapes;
        void Update()
        {
            if(Time.deltaTime>0&&boat)
            {
                for(int i=0;i<wakeCount;i++)wakes[i].z+=Time.deltaTime;
                if(Clock>=nextWake)
                {
                    if(resetWake){previous=boat.position;previousWakeTime=Clock;resetWake=false;}
                    Vector3 move=boat.position-previous;move.y=0;
                    float speed=move.magnitude/Mathf.Max(.001f,Clock-previousWakeTime);
                    if(speed>.3f&&move.sqrMagnitude<100)
                    {
                        for(int i=Mathf.Min(wakeCount,47);i>0;i--){wakes[i]=wakes[i-1];wakeDirections[i]=wakeDirections[i-1];}
                        Vector3 p=boat.position-boat.forward*1.9f;
                        wakes[0]=new(p.x,p.z,0,Mathf.Clamp01(speed/6));
                        Vector3 direction=move.normalized;
                        wakeDirections[0]=new(direction.x,direction.z,speed,0);wakeCount=Mathf.Min(wakeCount+1,48);
                    }
                    previous=boat.position;previousWakeTime=Clock;nextWake=Clock+.18f;
                }
            }
            if(boat)transform.position=new(boat.position.x,0,boat.position.z);
            Upload();
        }
        public void Upload()
        {
            ConfigureWaves();
            Shader.SetGlobalFloat("_SeaTime",Clock);Shader.SetGlobalFloat("_WaveScale",waveScale);
            Shader.SetGlobalVectorArray("_OceanWaves",waves);Shader.SetGlobalVectorArray("_OceanShapes",shapes);
            Shader.SetGlobalTexture("_Seabed",depthMap?depthMap:Texture2D.blackTexture);
            Shader.SetGlobalVector("_DepthBounds",depthBounds);Shader.SetGlobalFloat("_HasSeabed",depthMap?1:0);
            Shader.SetGlobalInt("_WakeCount",wakeCount);Shader.SetGlobalVectorArray("_OceanWake",wakes);Shader.SetGlobalVectorArray("_OceanWakeDirection",wakeDirections);
            if(boat)Shader.SetGlobalVector("_BoatPose",new(boat.position.x,boat.position.z,boat.forward.x,boat.forward.z));
            Shader.SetGlobalFloat("_BoatSpeed",boat&&boat.TryGetComponent<Rigidbody>(out var b)?new Vector2(b.linearVelocity.x,b.linearVelocity.z).magnitude:0);
        }
        public void ClearWake(){wakeCount=0;resetWake=true;}
    }
}
