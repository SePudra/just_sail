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
        [Header("Hull response")]
        [Min(0)] public float lateralResistance=1.8f;
        [Min(0)] public float rudderResponse=3.5f;
        [Range(0,12)] public float sailHeel=5.5f;
        public Vector3 spawn=new Vector3(0,0,-65);
        public Rigidbody Body {get;private set;}
        public float Speed => new Vector2(Body.linearVelocity.x,Body.linearVelocity.z).magnitude;
        public float Rudder {get;private set;}
        public int Contacts {get;private set;}
        readonly Vector3[] points={new(-.57f,-.32f,1.65f),new(.57f,-.32f,1.65f),new(-.72f,-.32f,0),new(.72f,-.32f,0),new(-.57f,-.32f,-1.6f),new(.57f,-.32f,-1.6f)};
        public float SubmergedFraction {get;private set;}
        public float BowEntry {get;private set;}
        float rudderTarget;

        void Awake()
        {
            Body=GetComponent<Rigidbody>();Body.mass=320;Body.centerOfMass=new Vector3(0,-.35f,0);
            Body.interpolation=RigidbodyInterpolation.Interpolate;
            Body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            Body.angularDamping=.65f;Body.linearDamping=.025f;Body.maxAngularVelocity=2.5f;
        }
        void Update()
        {
            var k=Keyboard.current;
            if(!controlsEnabled || k==null){rudderTarget=0;return;}
            throttle=Mathf.Clamp01(throttle+((k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0))*Time.deltaTime*.42f);
            rudderTarget=(k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0);
        }
        void FixedUpdate()
        {
            if(!sea)return;
            Rudder=Mathf.MoveTowards(Rudder,rudderTarget,rudderResponse*Time.fixedDeltaTime);
            float submerged=0;BowEntry=0;
            Vector3 current=Vector3.zero,surfaceNormal=Vector3.zero;
            for(int i=0;i<points.Length;i++)
            {
                Vector3 p=Body.position+Body.rotation*points[i];var water=sea.SampleSurface(p);
                float depth=water.position.y-p.y;if(depth<=0)continue;
                float ratio=Mathf.Clamp01(depth/Mathf.Max(.05f,fullSubmergence));submerged+=ratio;
                current+=water.velocity*ratio;surfaceNormal+=water.normal*ratio;
                Vector3 relative=Body.GetPointVelocity(p)-water.velocity;
                float gravity=Physics.gravity.magnitude;
                float lift=waterDensity*(displacementVolume/points.Length)*gravity*ratio;
                float damping=relative.y*verticalDamping*Body.mass/points.Length*Mathf.Clamp01(depth/.15f);
                // Submerged water drag must oppose rising as well as falling motion.
                float cap=Body.mass*gravity/points.Length*2.8f;
                float force=Mathf.Clamp(lift-damping,-cap*.5f,cap);
                Body.AddForceAtPosition(Vector3.up*force,p,ForceMode.Force);
                if(i<2)BowEntry=Mathf.Max(BowEntry,Mathf.Max(0,-relative.y)*ratio);
            }
            SubmergedFraction=submerged/points.Length;
            if(submerged>.001f){current/=submerged;surfaceNormal/=submerged;}
            Vector3 velocity=Body.linearVelocity-current;
            Vector3 f=Vector3.ProjectOnPlane(Body.rotation*Vector3.forward,Vector3.up).normalized;
            Vector3 r=Vector3.Cross(Vector3.up,f);
            float forward=Vector3.Dot(velocity,f), lateral=Vector3.Dot(velocity,r);
            Vector3 windVector=new(sea.windDirection.x,0,sea.windDirection.y);
            float wind=.65f+.35f*Mathf.Clamp01((Vector3.Dot(f,windVector.normalized)+1)*.5f);
            float wet=Mathf.Clamp01(SubmergedFraction*2.3f);
            Body.AddForce((f*(throttle*thrust*wind-forward*Mathf.Abs(forward)*.03f)-r*lateral*lateralResistance)*wet,ForceMode.Acceleration);
            float steer=Rudder*steering*Mathf.Lerp(.25f,1,Mathf.Clamp01(Mathf.Abs(forward)/3));
            Body.AddTorque(Vector3.up*(steer-Body.angularVelocity.y*1.55f)*wet,ForceMode.Acceleration);
            // Sail load and turning heel remain small; the hull, not the camera,
            // follows the swell. No yaw damping is hidden in the righting torque.
            float heel=(-Vector3.Dot(r,windVector)*throttle+Rudder*Mathf.Clamp01(Mathf.Abs(forward)/5)*.35f)*sailHeel;
            Vector3 upright=Vector3.Slerp(Vector3.up,surfaceNormal.sqrMagnitude>.01f?surfaceNormal.normalized:Vector3.up,.2f);
            upright=Quaternion.AngleAxis(heel,f)*upright;
            Vector3 tiltVelocity=Vector3.ProjectOnPlane(Body.angularVelocity,Vector3.up);
            Body.AddTorque((Vector3.Cross(Body.rotation*Vector3.up,upright)*2.5f-tiltVelocity*.65f)*wet,ForceMode.Acceleration);
            if(transform.position.y < -8 || new Vector2(transform.position.x,transform.position.z).magnitude>1800) Recover();
        }
        public void Recover()
        {
            var mooring=GetComponent<MooringLine>();if(mooring)mooring.Detach();
            Vector3 safe=spawn;safe.y=sea.SampleSurface(spawn).position.y+.05f;
            Body.position=safe;Body.rotation=Quaternion.Euler(0,-25,0);
            Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;throttle=0;Rudder=0;rudderTarget=0;sea.ClearWake();
        }
        void OnCollisionEnter(Collision collision){Contacts++;}
    }
}
