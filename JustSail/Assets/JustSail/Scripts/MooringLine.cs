using UnityEngine;
using UnityEngine.InputSystem;

namespace JustSail
{
    public sealed class MooringLine : MonoBehaviour
    {
        public BoatController boat;
        public SailUI ui;
        public Transform bollard;
        public Material ropeMaterial;
        public bool Moored=>joint;
        public bool CanAttach=>boat&&bollard&&boat.Speed<2.2f&&Vector3.Distance(Bow,bollard.position)<12;
        public Vector3 Bow=>boat.transform.TransformPoint(new Vector3(0,.8f,1.9f));
        SpringJoint joint;
        LineRenderer line;
        void Start()
        {
            var go=new GameObject("Palamar");go.transform.SetParent(transform,false);line=go.AddComponent<LineRenderer>();
            line.sharedMaterial=ropeMaterial;line.positionCount=20;line.startWidth=line.endWidth=.045f;
            line.useWorldSpace=true;line.generateLightingData=true;line.numCapVertices=2;line.enabled=false;
        }
        void Update()
        {
            if(!ui.MenuOpen&&Keyboard.current?.eKey.wasPressedThisFrame==true){if(Moored)Detach();else Attach();}
            if(Moored&&Vector3.Distance(Bow,bollard.position)>30)Detach();
            if(!line)return;line.enabled=Moored;
            if(!Moored)return;
            for(int i=0;i<line.positionCount;i++)
            {
                float t=(float)i/(line.positionCount-1);Vector3 p=Vector3.Lerp(Bow,bollard.position,t);
                float slack=Mathf.Clamp(joint.maxDistance-Vector3.Distance(Bow,bollard.position)+.45f,.1f,1.1f);
                p.y-=Mathf.Sin(t*Mathf.PI)*slack;line.SetPosition(i,p);
            }
        }
        void FixedUpdate(){if(joint){joint.maxDistance=Mathf.MoveTowards(joint.maxDistance,3.8f,Time.fixedDeltaTime*.3f);boat.throttle=0;}}
        public bool Attach()
        {
            if(Moored||!CanAttach)return false;
            boat.throttle=0;joint=boat.gameObject.AddComponent<SpringJoint>();joint.autoConfigureConnectedAnchor=false;
            joint.anchor=new Vector3(0,.8f,1.9f);joint.connectedAnchor=bollard.position;
            joint.minDistance=0;joint.maxDistance=Mathf.Max(3.8f,Vector3.Distance(Bow,bollard.position));
            joint.spring=250;joint.damper=180;joint.tolerance=.05f;joint.enableCollision=true;
            return true;
        }
        public void Detach(){if(joint){joint.spring=0;joint.damper=0;Destroy(joint);joint=null;}if(line)line.enabled=false;}
    }
}
