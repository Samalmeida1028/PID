using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PIDController : MonoBehaviour
{
public GameObject controlled;
public GameObject target;
Rigidbody RB;
public Vector3 Proportional;
public Vector3 Derivative;
public Vector3 Integral;
public float IntegralBound;
public Vector3 position;
public Vector3 lastposition;
bool kick = false;

Vector3 targetvector;
public float dGain = 5;
public float pGain = 1;
public float iGain = 1;
public float offset = .5F;
public float clamp = 10;
public float timeOn = 0;
public float timeOff = 0;
public double threshold = .5; 

public float totalError;
    void Start()
    {
        RB = controlled.GetComponent<Rigidbody>();
        lastposition = new Vector3(0,0,0);
        position = controlled.transform.position;
        targetvector = target.transform.position;
        targetvector.y = targetvector.y + offset;
    }

    void FixedUpdate()
    {
        if(Vector3.Magnitude(targetvector-position)<threshold){
            timeOn+=1;
        }
        timeOff+=1;

        totalError += Vector3.Magnitude(targetvector - position);
        targetvector = target.transform.position;
        targetvector.y = targetvector.y + offset;
        lastposition = position;
        position = controlled.transform.position;
        Vector3 F = PID(Time.deltaTime, position, lastposition, targetvector);
        RB.AddForce(F);
    }




    Vector3 PID(float dt, Vector3 position, Vector3 lastposition, Vector3 targetPosition)
    {
        Vector3 change = lastposition - position;
        Proportional = (targetPosition - position)*pGain;
        Derivative = ((change)/dt)*dGain;
        Integral =Integral + (targetPosition-position)*dt*iGain;
        Integral = ClampedVector(Integral,clamp);

        if(kick){
        return Proportional + Integral + Derivative;
        }
        else{
        kick = true;
        return Proportional + Integral;

        }
    }


    Vector3 ClampedVector(Vector3 vector, float clamp){
        vector.x = Mathf.Clamp(vector.x,-clamp/2,clamp/2);
        vector.y = Mathf.Clamp(vector.y,-clamp,clamp);
        vector.z = Mathf.Clamp(vector.z,-clamp/2,clamp/2);
        return vector;

    }
}
