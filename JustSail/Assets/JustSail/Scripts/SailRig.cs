using UnityEngine;

namespace JustSail
{
    public sealed class SailRig : MonoBehaviour
    {
        public SeaSurface sea;
        Transform main,jib;
        Quaternion mainRest,jibRest;
        void Start()
        {
            foreach(var t in GetComponentsInChildren<Transform>())
            {if(t.name=="Mainsail")main=t;if(t.name=="Jib")jib=t;}
            if(main)mainRest=main.localRotation;if(jib)jibRest=jib.localRotation;
        }
        void LateUpdate()
        {
            if(!sea)return;
            float sway=Mathf.Sin(sea.Clock*.7f)*.6f+Mathf.Sin(sea.Clock*1.3f)*.25f;
            Vector3 wind=transform.InverseTransformDirection(new Vector3(sea.windDirection.x,0,sea.windDirection.y));
            sway+=Mathf.Clamp(Mathf.Atan2(wind.x,wind.z)*Mathf.Rad2Deg*.13f,-16,16);
            if(main)main.localRotation=mainRest*Quaternion.Euler(0,sway,0);
            if(jib)jib.localRotation=jibRest*Quaternion.Euler(0,-sway*.7f,0);
        }
    }
}
