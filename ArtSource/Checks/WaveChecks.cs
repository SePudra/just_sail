using System;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;
using JustSail;
class WaveChecks
{
 static void Main()
 {
  var sea=(SeaSurface)FormatterServices.GetUninitializedObject(typeof(SeaSurface));
  var flags=BindingFlags.NonPublic|BindingFlags.Instance;
  void Set(string n,object v)=>typeof(SeaSurface).GetField(n,flags).SetValue(sea,v);
  // INITIALIZE_FROM_SEASURFACE_SOURCE
  sea.windDirection=new Vector2(.94f,.34f);
  typeof(SeaSurface).GetMethod("ConfigureWaves",flags).Invoke(sea,null);
  float inverse=0,normalError=0,velocityError=0,minJac=100;
  var random=new System.Random(593);int count=0;
  foreach(float scale in new[]{.42f,.85f,1.65f})
  {
   sea.waveScale=scale;
   for(int i=0;i<700;i++)
   {
    Vector2 p=new Vector2((float)random.NextDouble()*1200-600,(float)random.NextDouble()*1200-600);float t=(float)random.NextDouble()*120;
    var s=sea.Evaluate(p,t);var back=sea.SampleSurface(s.position,t);inverse=Math.Max(inverse,Vector3.Distance(s.position,back.position));minJac=Math.Min(minJac,s.jacobian);
    float e=.02f;var tx=sea.Evaluate(p+Vector2.right*e,t).position-sea.Evaluate(p-Vector2.right*e,t).position;var tz=sea.Evaluate(p+Vector2.up*e,t).position-sea.Evaluate(p-Vector2.up*e,t).position;
    normalError=Math.Max(normalError,Vector3.Distance(s.normal,Vector3.Cross(tz,tx).normalized));
    var velocity=(sea.Evaluate(p,t+e).position-sea.Evaluate(p,t-e).position)/(2*e);velocityError=Math.Max(velocityError,Vector3.Distance(velocity,s.velocity));count++;
   }
  }
  Console.WriteLine($"samples={count} inverse_m={inverse} normal_error={normalError} velocity_error={velocityError} min_jacobian={minJac}");
  if(inverse>.004f||normalError>.01f||velocityError>.02f||minJac<=.05f)Environment.Exit(1);
 }
}
