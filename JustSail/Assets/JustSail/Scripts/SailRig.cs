using UnityEngine;

namespace JustSail
{
    public sealed class SailRig : MonoBehaviour
    {
        public SeaSurface sea;
        Transform main,jib;
        Quaternion mainRest,jibRest;
        Rigidbody body;
        float trim,trimVelocity;
        void Start()
        {
            foreach(var t in GetComponentsInChildren<Transform>())
            {if(t.name=="Mainsail")main=t;if(t.name=="Jib")jib=t;}
            if(main)mainRest=main.localRotation;if(jib)jibRest=jib.localRotation;
            body=GetComponent<Rigidbody>();
        }
        void LateUpdate()
        {
            if(!sea)return;
            float sway=Mathf.Sin(sea.Clock*.7f)*.6f+Mathf.Sin(sea.Clock*1.3f)*.25f;
            Vector3 apparent=new Vector3(sea.windDirection.x,0,sea.windDirection.y)*7-(body?body.linearVelocity:Vector3.zero);
            Vector3 wind=transform.InverseTransformDirection(apparent.normalized);
            float desired=Mathf.Clamp(wind.x*18,-16,16);
            if(Time.deltaTime>0)trim=Mathf.SmoothDamp(trim,desired,ref trimVelocity,.8f,30,Time.deltaTime);
            // FBX carries Blender's axis conversion. Rotate around the boat's
            // up axis expressed in the parent frame, not the imported local Y.
            if(main)main.localRotation=Quaternion.AngleAxis(trim+sway,main.parent.InverseTransformDirection(transform.up))*mainRest;
            // The jib turns about its sloping forestay, keeping tack and head
            // attached; a vertical rotation would pull it off the bow fitting.
            Vector3 forestay=transform.TransformDirection(new Vector3(0,4.34f,-1.86f).normalized);
            if(jib)jib.localRotation=Quaternion.AngleAxis((trim+sway)*.3f,jib.parent.InverseTransformDirection(forestay))*jibRest;
        }
    }
}
