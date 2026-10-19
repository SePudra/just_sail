using UnityEngine;

namespace JustSail
{
    public sealed class TreeBreeze : MonoBehaviour
    {
        public SeaSurface sea;
        Quaternion rest;
        float phase;
        void Awake(){rest=transform.localRotation;phase=transform.position.x*.31f+transform.position.z*.17f;}
        void Update()
        {
            if(!sea)return;
            float strength=Shader.GetGlobalVector("_WindData").z;
            float sway=(Mathf.Sin(sea.Clock*.7f+phase)+Mathf.Sin(sea.Clock*1.37f+phase)*.23f)*Mathf.Lerp(.65f,2.8f,strength);
            transform.localRotation=Quaternion.AngleAxis(sway,new Vector3(sea.windDirection.y,0,-sea.windDirection.x))*rest;
        }
    }
}
