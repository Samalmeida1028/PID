using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changePos : MonoBehaviour
{
    Vector3 position;
    public int count = 0;
    public bool playerControlled = false;
    public int turnLength = 100;
    int turn;
    public float step;
    int MoveDirectionZ = 0;
    int MoveDirectionX = 0;
    int MoveDirectionY = 0;
    public int upperBound;
    public int lowerBound;
    public int leftBound;
    public int rightBound;
    public int forwardBound;
    public int backwardBound;
    
    // Start is called before the first frame update
    void Start()
    {

        turn = turnLength;
            MoveDirectionX = 1;
            MoveDirectionZ = 1;
            MoveDirectionY = 0;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        count++;
        if(playerControlled){
            controlPos();
        }
        else{
            if(count>turn){
            turn = Random.Range(0,turnLength);
            MoveDirectionX = Random.Range(-1,2);
            MoveDirectionY = Random.Range(-0,0);
            MoveDirectionZ = Random.Range(-1,2);
            count = 0;
            }
            if(!(gameObject.transform.position.y>upperBound)&&
            !(gameObject.transform.position.y<lowerBound)&&
            !(gameObject.transform.position.z<leftBound)&&
            !(gameObject.transform.position.z>rightBound)&&
            !(gameObject.transform.position.x>forwardBound)&&
            !(gameObject.transform.position.x<backwardBound)){
            gameObject.transform.Translate(new Vector3(MoveDirectionX*step,MoveDirectionY*step,MoveDirectionZ*step));}
            else{
                if((gameObject.transform.position.y>upperBound)&&MoveDirectionY>0) MoveDirectionY = -MoveDirectionY;
                if((gameObject.transform.position.y<lowerBound)&&MoveDirectionY<0) MoveDirectionY = -MoveDirectionY;
                if((gameObject.transform.position.z>leftBound)&&MoveDirectionZ>0) MoveDirectionZ = -MoveDirectionZ;
                if((gameObject.transform.position.z<rightBound)&&MoveDirectionZ<0) MoveDirectionZ = -MoveDirectionZ;
                if((gameObject.transform.position.x>forwardBound)&&MoveDirectionX>0) MoveDirectionX = -MoveDirectionX;
                if((gameObject.transform.position.x<backwardBound)&&MoveDirectionX<0) MoveDirectionX = -MoveDirectionX;
                gameObject.transform.Translate(new Vector3(MoveDirectionX*step,MoveDirectionY*step,MoveDirectionZ*step));
            }
        }
        
    }

    void controlPos(){

        if(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.D)){
        MoveDirectionZ = Input.GetKey(KeyCode.D) ? 1 : -1;
        }
        else{
            MoveDirectionZ = 0;
        }
        if(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.S)){
        MoveDirectionX = Input.GetKey(KeyCode.W) ? 1 : -1;
        }
        else{
            MoveDirectionX = 0;
        }
        if(Input.GetKey(KeyCode.I)||Input.GetKey(KeyCode.K)){
        MoveDirectionY = Input.GetKey(KeyCode.I) ? 1 : -1;
        }
        else{
            MoveDirectionY = 0;
        }


        gameObject.transform.Translate(new Vector3(-MoveDirectionX*step,MoveDirectionY*step,MoveDirectionZ*step));



    }
}
