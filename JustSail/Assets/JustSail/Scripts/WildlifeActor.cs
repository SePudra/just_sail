using UnityEngine;

namespace JustSail
{
    public sealed class WildlifeActor : MonoBehaviour
    {
        public bool dolphin;
        public float phase;
        public SeaSurface sea;
        public BoatController boat;
        public Transform leftWing,rightWing;
        Vector3 home,previous;
        public string Subject=>dolphin?"Yunus":"Martı";
        void Start(){home=transform.position;previous=home;}
        void Update()
        {
            if(!sea||!boat)return;
            float t=sea.Clock;
            Vector3 position;
            if(dolphin)
            {
                Vector3 forward=Vector3.ProjectOnPlane(boat.transform.forward,Vector3.up).normalized;
                Vector3 side=Vector3.Cross(Vector3.up,forward);
                Vector3 center=Vector3.Distance(boat.transform.position,home)<170&&boat.Speed>.65f?boat.transform.position+forward*10:home;
                position=center+side*(Mathf.Sin(t*.32f+phase)*5)+forward*Mathf.Cos(t*.22f+phase)*4;
                if(sea.FloorHeight(new Vector2(position.x,position.z))>-3)position=home;
                float jump=Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*.65f+phase)),5)*2;
                position.y=sea.SampleSurface(position).position.y-1.05f+jump;
            }
            else
            {
                float angle=t*.12f+phase;
                position=home+new Vector3(Mathf.Cos(angle)*24,Mathf.Sin(t*.35f+phase)*1.4f,Mathf.Sin(angle)*18);
                Vector3 away=position-boat.transform.position;away.y=0;
                if(away.magnitude<15)position+=away.normalized*(15-away.magnitude)*.8f;
                float flap=Mathf.Sin(t*3.8f+phase)*18;
                if(leftWing)leftWing.localRotation=Quaternion.Euler(0,0,flap);
                if(rightWing)rightWing.localRotation=Quaternion.Euler(0,0,-flap);
            }
            Vector3 direction=position-previous;
            if(direction.sqrMagnitude>.00001f&&Time.deltaTime>0)
            {
                direction.y=Mathf.Clamp(direction.y,-.3f,.3f);
                transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(direction),1-Mathf.Exp(-Time.deltaTime*4));
            }
            transform.position=position;previous=position;
        }
    }
}
