using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace JustSail
{
    // A single sea plane: the reflected landscape is softened by the water shader.
    [DefaultExecutionOrder(220)]
    public sealed class ShoreReflection : MonoBehaviour
    {
        public Camera source;
        public int resolution=1024;
        Camera reflection;
        RenderTexture texture;
        readonly UniversalRenderPipeline.SingleCameraRequest request=new();
        void OnEnable()
        {
            if(!source)source=GetComponent<Camera>();
            var go=new GameObject("Shore reflection camera"){hideFlags=HideFlags.HideAndDontSave};
            reflection=go.AddComponent<Camera>();reflection.enabled=false;
            var data=reflection.GetUniversalAdditionalCameraData();data.renderPostProcessing=false;
            data.renderShadows=true;data.requiresColorTexture=false;data.requiresDepthTexture=false;
            texture=new RenderTexture(resolution,resolution,24,RenderTextureFormat.ARGBHalf){name="Autumn shore reflection",useMipMap=true,autoGenerateMips=true,filterMode=FilterMode.Trilinear};
            texture.Create();request.destination=texture;
        }
        void LateUpdate()=>Refresh();
        public void Refresh()
        {
            if(!source||!reflection||!texture||source.transform.position.y<.15f){Shader.SetGlobalFloat("_ShoreReflectionAvailable",0);return;}
            reflection.CopyFrom(source);reflection.enabled=false;reflection.targetTexture=texture;
            // CopyFrom preserves automatic aspect; the square target would then narrow the frustum to 1:1.
            reflection.aspect=source.aspect;
            reflection.fieldOfView=source.fieldOfView+4;reflection.ResetProjectionMatrix();
            reflection.cullingMask=source.cullingMask&~(1<<4);reflection.allowMSAA=false;
            reflection.useOcclusionCulling=false;
            var data=reflection.GetUniversalAdditionalCameraData();data.renderPostProcessing=false;data.renderShadows=true;
            data.requiresColorTexture=false;data.requiresDepthTexture=false;
            var mirror=Matrix4x4.identity;mirror.m11=-1;
            var pos=source.transform.position;pos.y=-pos.y;reflection.transform.position=pos;
            reflection.transform.rotation=Quaternion.Euler(-source.transform.eulerAngles.x,source.transform.eulerAngles.y,0);
            reflection.worldToCameraMatrix=source.worldToCameraMatrix*mirror;
            Vector3 point=reflection.worldToCameraMatrix.MultiplyPoint(new Vector3(0,.045f,0));
            Vector3 normal=reflection.worldToCameraMatrix.MultiplyVector(Vector3.up).normalized;
            var plane=new Vector4(normal.x,normal.y,normal.z,-Vector3.Dot(point,normal));
            reflection.projectionMatrix=reflection.CalculateObliqueMatrix(plane);
            var previous=GL.invertCulling;
            try{GL.invertCulling=!previous;RenderPipeline.SubmitRenderRequest(reflection,request);}
            finally{GL.invertCulling=previous;}
            Shader.SetGlobalTexture("_ShoreReflection",texture);
            Shader.SetGlobalMatrix("_ShoreReflectionVP",GL.GetGPUProjectionMatrix(reflection.projectionMatrix,true)*reflection.worldToCameraMatrix);
            Shader.SetGlobalFloat("_ShoreReflectionAvailable",1);
        }
        void OnDisable()
        {
            Shader.SetGlobalFloat("_ShoreReflectionAvailable",0);
            if(texture){texture.Release();Destroy(texture);}
            if(reflection)Destroy(reflection.gameObject);
        }
    }
}
