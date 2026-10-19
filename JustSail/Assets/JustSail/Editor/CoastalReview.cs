using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace JustSail.Editor
{
    // Explicit review tools. Never rebuilds the scene or changes saved player preferences.
    public static class CoastalReview
    {
        [MenuItem("Just Sail/Review/Capture coastal art")]
        public static void Capture()
        {
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Open FirstVoyage and enter Play Mode first.");
            var ui=UnityEngine.Object.FindAnyObjectByType<SailUI>();
            if(!ui||!ui.Playing||ui.ExternalPanelOpen)throw new InvalidOperationException("Start sailing and close the journal before capturing.");
            var boat=ui.boat;var body=boat.Body;var sea=boat.sea;var weather=ui.atmosphere;var rig=ui.cameraRig;
            Vector3 position=body.position,velocity=body.linearVelocity,angular=body.angularVelocity;
            Quaternion rotation=body.rotation;var interpolation=body.interpolation;
            float throttle=boat.throttle,hour=weather.hours,clock=sea.Clock,distance=rig.distance;
            int mode=weather.weatherMode;bool advance=weather.advanceDay,focus=rig.softFocus,paused=ui.MenuOpen;
            Vector2 orbit=rig.Orbit;
            string folder=Path.GetFullPath("../Verification/CoastalPass/Unity");
            try
            {
                ReferenceArtPass.CaptureStudy("01-morning",8,-25,9,40,folder);
                ReferenceArtPass.CaptureStudy("02-midday",13,-25,9,40,folder);
                ReferenceArtPass.CaptureStudy("03-evening",18.3f,-25,9,40,folder);
                ReferenceArtPass.CaptureStudy("04-moonlight",22.5f,-25,9,40,folder);
                ReferenceArtPass.CaptureStudy("05-rig-close",8,-70,12,22,folder);
                File.WriteAllText(Path.Combine(folder,"capture.txt"),
                    "Actual Unity camera renders; 1600x900; fixed wave time 12s; clear weather; focus off.\n"+
                    "Unity "+Application.unityVersion+"; "+SystemInfo.graphicsDeviceName+"\n"+
                    "Inspect hull waterline, sail/boom attachment, shore foam, distant silhouettes and unclipped mast.\n");
                Debug.Log("COASTAL_ART_CAPTURE_READY "+folder);
            }
            finally
            {
                body.position=position;body.rotation=rotation;boat.transform.SetPositionAndRotation(position,rotation);
                body.linearVelocity=velocity;body.angularVelocity=angular;body.interpolation=interpolation;boat.throttle=throttle;
                typeof(SeaSurface).GetProperty("Clock").GetSetMethod(true).Invoke(sea,new object[]{clock});
                weather.advanceDay=advance;weather.SetHour(hour);weather.SetWeather(mode,true);sea.ClearWake();sea.Upload();
                rig.softFocus=focus;rig.distance=distance;rig.SetOrbit(orbit.x,orbit.y);Physics.SyncTransforms();
                if(paused)ui.Pause();else ui.Resume();
            }
        }

        [MenuItem("Just Sail/Review/Run ocean and buoyancy checks")]
        public static void CheckOcean()
        {
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Enter Play Mode first.");
            if(UnityEngine.Object.FindAnyObjectByType<OceanCheck>())throw new InvalidOperationException("An ocean check is already present. Restart Play Mode to run another.");
            OceanCheck.Begin();
        }
    }
}
