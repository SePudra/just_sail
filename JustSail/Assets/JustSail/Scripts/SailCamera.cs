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
        float yaw=-25,pitch=5;
        Vector3 velocity;
        bool initialized;
        DepthOfField focus;
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
                {var d=mouse.delta.ReadValue();yaw+=d.x*.14f*sensitivity;pitch=Mathf.Clamp(pitch-d.y*.1f*sensitivity,-8,60);}
                else yaw=Mathf.LerpAngle(yaw,target.eulerAngles.y,1-Mathf.Exp(-Time.deltaTime*.7f));
                distance=Mathf.Clamp(distance-mouse.scroll.ReadValue().y*.016f,22,55);
            }
            Vector3 look=target.position+Vector3.up*2.6f;
            Vector3 desired=look+Quaternion.Euler(pitch,yaw,0)*new Vector3(0,0,-distance);
            desired.y=Mathf.Max(desired.y,target.position.y+2.1f);
            if(Physics.Linecast(look,desired,out RaycastHit hit,1<<6))desired=hit.point+hit.normal*.8f;
            if(!initialized){transform.position=desired;initialized=true;}
            else transform.position=Vector3.SmoothDamp(transform.position,desired,ref velocity,.22f,100,Time.unscaledDeltaTime);
            transform.LookAt(look);
            if(focus){focus.mode.value=softFocus?DepthOfFieldMode.Bokeh:DepthOfFieldMode.Off;focus.focusDistance.value=Vector3.Distance(transform.position,look);}
        }
        public void Snap(){initialized=false;velocity=Vector3.zero;}
    }
}
