using UnityEngine;
using UnityEngine.InputSystem;

namespace JustSail
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class BoatController : MonoBehaviour
    {
        public SeaSurface sea;
        public float throttle;
        public bool controlsEnabled;
        public float thrust=1.4f, steering=1.7f;
        public float waterDensity=1000, displacementVolume=.72f, fullSubmergence=.72f, verticalDamping=2.4f;
        public Vector3 spawn=new Vector3(0,0,-65);
        public Rigidbody Body {get;private set;}
        public float Speed => new Vector2(Body.linearVelocity.x,Body.linearVelocity.z).magnitude;
        public float Rudder {get;private set;}
        public int Contacts {get;private set;}
        readonly Vector3[] points={new(-.57f,-.32f,1.65f),new(.57f,-.32f,1.65f),new(-.72f,-.32f,0),new(.72f,-.32f,0),new(-.57f,-.32f,-1.6f),new(.57f,-.32f,-1.6f)};
        public float SubmergedFraction {get;private set;}
        public float BowEntry {get;private set;}

        void Awake()
        {
            Body=GetComponent<Rigidbody>();Body.mass=320;Body.centerOfMass=new Vector3(0,-.35f,0);
            Body.interpolation=RigidbodyInterpolation.Interpolate;
            Body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            Body.angularDamping=1.7f;Body.linearDamping=.025f;Body.maxAngularVelocity=2.5f;
        }
        void Update()
        {
            var k=Keyboard.current;
            if(!controlsEnabled || k==null){Rudder=0;return;}
            throttle=Mathf.Clamp01(throttle+((k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0))*Time.deltaTime*.42f);
            Rudder=(k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0);
        }
        void FixedUpdate()
        {
            if(!sea)return;
            float submerged=0;BowEntry=0;
            for(int i=0;i<points.Length;i++)
            {
                Vector3 p=Body.position+Body.rotation*points[i];var water=sea.SampleSurface(p);
                float depth=water.position.y-p.y;if(depth<=0)continue;
                float ratio=Mathf.Clamp01(depth/fullSubmergence);submerged+=ratio;
                Vector3 relative=Body.GetPointVelocity(p)-water.velocity;
                float lift=waterDensity*(displacementVolume/points.Length)*9.81f*ratio;
                float damping=relative.y*verticalDamping*Body.mass/points.Length*Mathf.Clamp01(depth/.15f);
                float force=Mathf.Clamp(lift-damping,0,Body.mass*9.81f/points.Length*2.8f);
                Body.AddForceAtPosition(Vector3.up*force,p,ForceMode.Force);
                Body.AddForce(new Vector3(water.velocity.x,0,water.velocity.z)*(.14f*ratio/points.Length),ForceMode.Acceleration);
                if(i<2)BowEntry=Mathf.Max(BowEntry,Mathf.Max(0,-relative.y)*ratio);
            }
            SubmergedFraction=submerged/points.Length;
            Vector3 velocity=Body.linearVelocity;
            Vector3 f=Vector3.ProjectOnPlane(transform.forward,Vector3.up).normalized;
            Vector3 r=Vector3.Cross(Vector3.up,f);
            float forward=Vector3.Dot(velocity,f), lateral=Vector3.Dot(velocity,r);
            Vector3 windVector=new(sea.windDirection.x,0,sea.windDirection.y);
            float wind=.65f+.35f*Mathf.Clamp01((Vector3.Dot(f,windVector.normalized)+1)*.5f);
            float wet=Mathf.Clamp01(SubmergedFraction*2.3f);
            Body.AddForce((f*(throttle*thrust*wind-forward*Mathf.Abs(forward)*.03f)-r*lateral*1.4f)*wet,ForceMode.Acceleration);
            Body.AddTorque(Vector3.up*(Rudder*steering*Mathf.Lerp(.3f,1,Mathf.Clamp01(Speed/3)))-Vector3.up*Body.angularVelocity.y*.5f,ForceMode.Acceleration);
            // A gentle righting torque supplements buoyancy at large roll angles.
            Body.AddTorque(Vector3.Cross(transform.up,Vector3.up)*2.5f*wet,ForceMode.Acceleration);
            if(transform.position.y < -8 || new Vector2(transform.position.x,transform.position.z).magnitude>1800) Recover();
        }
        public void Recover()
        {
            var mooring=GetComponent<MooringLine>();if(mooring)mooring.Detach();
            Vector3 safe=spawn;safe.y=sea.SampleSurface(spawn).position.y+.05f;
            Body.position=safe;Body.rotation=Quaternion.Euler(0,-25,0);
            Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;throttle=0;sea.ClearWake();
        }
        void OnCollisionEnter(Collision collision){Contacts++;}
    }
}
