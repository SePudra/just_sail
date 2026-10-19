using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace JustSail
{
    public sealed class SailCamera : MonoBehaviour
    {
        public Transform target;
        public bool menuOpen=true;
        public float sensitivity=1, distance=40;
        public bool softFocus=false;
        [Min(.05f)] public float heaveSmoothing=.85f;
        [Min(.1f)] public float collisionRadius=.45f;
        float yaw=-25,pitch=9;
        Vector3 velocity,look,lookVelocity;
        float heightVelocity,orbitRelease;
        bool initialized;
        DepthOfField focus;
        public Vector2 Orbit=>new(yaw,pitch);
        void Start()
        {
            softFocus=PlayerPrefs.GetInt("softFocus",0)==1;
            var volume=FindFirstObjectByType<Volume>();
            if(volume&&volume.profile.TryGet<DepthOfField>(out var effect))focus=effect;
        }
        public void ToggleFocus(){softFocus=!softFocus;PlayerPrefs.SetInt("softFocus",softFocus?1:0);}
        public void SetOrbit(float angle,float elevation){yaw=angle;pitch=Mathf.Clamp(elevation,-8,60);Snap();}
        void LateUpdate()
        {
            if(!target)return;
            var mouse=Mouse.current;
            if(!menuOpen&&Keyboard.current?.cKey.wasPressedThisFrame==true)ToggleFocus();
            if(!menuOpen && mouse!=null)
            {
                if(mouse.rightButton.isPressed)
                {var d=mouse.delta.ReadValue();yaw+=d.x*.14f*sensitivity;pitch=Mathf.Clamp(pitch-d.y*.1f*sensitivity,-8,60);orbitRelease=1.5f;}
                else {orbitRelease=Mathf.Max(0,orbitRelease-Time.deltaTime);if(orbitRelease<=0)yaw=Mathf.LerpAngle(yaw,target.eulerAngles.y,1-Mathf.Exp(-Time.deltaTime*.7f));}
                distance=Mathf.Clamp(distance-mouse.scroll.ReadValue().y*.016f,22,55);
            }
            Vector3 targetLook=target.position+Vector3.up*2.6f;
            float dt=Mathf.Min(Time.unscaledDeltaTime,.05f);
            if(!initialized)look=targetLook;
            else
            {
                float y=Mathf.SmoothDamp(look.y,targetLook.y,ref heightVelocity,heaveSmoothing,100,dt);
                look=Vector3.SmoothDamp(look,targetLook,ref lookVelocity,.12f,100,dt);
                look.y=y;
            }
            Vector3 desired=look+Quaternion.Euler(pitch,yaw,0)*new Vector3(0,0,-distance);
            desired.y=Mathf.Max(desired.y,target.position.y+2.1f);
            desired=Constrain(look,desired);
            if(!initialized){transform.position=desired;initialized=true;}
            else transform.position=Constrain(look,Vector3.SmoothDamp(transform.position,desired,ref velocity,.22f,100,dt));
            transform.LookAt(look);
            if(focus){focus.mode.value=softFocus?DepthOfFieldMode.Bokeh:DepthOfFieldMode.Off;focus.focusDistance.value=Vector3.Distance(transform.position,look);}
        }
        Vector3 Constrain(Vector3 origin,Vector3 position)
        {
            Vector3 offset=position-origin;float length=offset.magnitude;
            if(length>.001f&&Physics.SphereCast(origin,collisionRadius,offset/length,out var hit,length,1<<6,QueryTriggerInteraction.Ignore))
                return origin+offset/length*Mathf.Max(.1f,hit.distance-.1f);
            return position;
        }
        public void Snap(){initialized=false;velocity=lookVelocity=Vector3.zero;heightVelocity=0;}
    }
}
