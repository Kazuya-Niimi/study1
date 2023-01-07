using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SpawnObjectToPlane : MonoBehaviour
{
    // Start is called before the first frame update

    // public GameObject cubePrefab;
    // GameObject spawnedObj;
    // ARRaycastManager  arRaycastManager;


    private float[] dirX = new float[] {1,0,-1,0,0.5f,-0.5f,0.5f,-0.5f,0.25f,-0.25f,0.25f,-0.25f,0.75f,-0.75f,0.75f,-0.75f};
    private float[] dirZ = new float[] {0,1,0,-1,0.5f,-0.5f,-0.5f,0.5f,0.75f,0.75f,-0.75f,-0.75f,0.25f,0.25f,-0.25f,-0.25f};
    
    private int height = 15;
    private int numDir = 16;
    private int numCube_a;
    public int rndDir;
    public int rndHeight;
    public int sumCube;
    public int cLine;


    private float scale = 0.25f;
    private float mag = 0;
    private float cDis = 0;
    private float timeBreath = 0.0f;
    private float timeJump = 0.0f;
    private float velocityJump = 10f;
    private float gravity = 9.8f;
    private float arousal = 0.3f;
    private float valence = 0.5f;

    public float cubeDis;
    public float distance;
    public float leave;
    public float centerRad;
    public bool isMove;
    public bool isJump;
    public float parentCircleDis;

    public GameObject Main;
    public GameObject Scren;
    public GameObject ParentGPos;
    public GameObject centerPoint;
    public GameObject circleStartPoint;
    public GameObject ParentLPos;
    public GameObject cubeParent;
    

    public Vector3 cCube;
    public Vector3 conCube;
    public Vector3 direction;
    public Vector3 MainDir;
    public Vector3 MainPos;
    public Vector3 cubeParentDir;
    public Vector3 targetScrenPos;
    public Vector3 ScrenDir;
    public Vector3 targetParentGPos;
    public Vector3 targetParentLPos;
    public Vector3 targetCubeParentPos;
    

    public Quaternion camerarotate;


    public GameObject[]  cube; 
    public Vector3[] basePos;
    public Vector3[] targetPosA;
    public Vector3[] targetPosV;
    public Vector3[] targetPosB;
    public Vector3[] targetPosM;
    public Vector3[] targetPosJ;
    public Vector3[] targetPos;
    public Vector3[] tmpPos;


    // List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {

        // arRaycastManager = GetComponent<ARRaycastManager>();

        Camera.main.transform.position = new Vector3(0,0,0);


        distance = 20f;
        isJump = false;
        isMove = false;
        leave = -5f*valence + 5f;
        cLine = height/2 + height%2;
        sumCube = (cLine-1)*(numDir*cLine - numDir + 2) + 1;
        rndDir = Random.Range(1,numDir);
        rndHeight = Random.Range(0,height);

        Main = new GameObject("Main");
        Scren = new GameObject("Scren");
        ParentGPos = new GameObject("ParentGPos");
        ParentLPos = new GameObject("ParentLPos");
        cubeParent = new GameObject("cubeParent");
        centerPoint = new GameObject("centerPoint");
        circleStartPoint = new GameObject("circleStartPoint");

        centerPoint.transform.parent = Main.transform;
        circleStartPoint.transform.parent = Main.transform;
        ParentGPos.transform.parent = Scren.transform;
        ParentLPos.transform.parent = ParentGPos.transform;

        cube = new GameObject[sumCube]; 
        basePos = new Vector3[sumCube];
        targetPosA = new Vector3[sumCube];
        targetPosV = new Vector3[sumCube];
        targetPosB = new Vector3[sumCube];
        targetPosM = new Vector3[sumCube];
        targetPosJ = new Vector3[sumCube];
        targetPos = new Vector3[sumCube];
        tmpPos = new Vector3[sumCube];



        

        // cube
        for(int i= 0; i< sumCube;i++){
            cube[i]  = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube[i].transform.localScale = new Vector3(scale, scale, scale);
            cube[i].transform.parent = cubeParent.transform;
            cube[i].GetComponent<Renderer>().material.color = Color.white;
        }
        


        for(int i = 1;i < cLine + 1;i++){
            cube[numCube(i)].transform.position = new Vector3(0,i - cLine,0) * 0.5f;
            for(int j = 0; j < numDir*(i-1); j++){
                cube[numCube(i)+ 1 +  j].transform.position = new Vector3((j/numDir+1)*dirX[j%numDir],i - cLine,(j/numDir+1)*dirZ[j%numDir]) * 0.5f;
            }
        }

        for(int i = 1; i < cLine; i++){
            cube[sumCube - numCube(i+1) ].transform.position = new Vector3(0,height - i + 1 - cLine,0) * 0.5f;
            for(int j = 0; j < numDir*(i-1); j++){
                cube[sumCube - numCube(i+1) + 1 + j].transform.position = new Vector3((j/numDir+1)*dirX[j%numDir],height - i + 1 - cLine,(j/numDir+1)*dirZ[j%numDir]) * 0.5f;
            }
        }

        //Main
        MainPos = Camera.main.gameObject.transform.position;
        MainDir = Camera.main.gameObject.transform.forward;

        //Scren
        ScrenDir = FuncDir(Vector3.zero,MainDir);
        Scren.transform.position = ScrenDir*distance;       
        
        //ParentGPos
        targetParentGPos = FuncParentGPos(distance,-(Mathf.PI/2)*valence + Mathf.PI/2);
        ParentGPos.transform.localPosition = targetParentGPos;

        //ParentPos
        cubeParent.transform.position = ParentLPos.transform.position;

        // CircleDis
        parentCircleDis = FuncCubeDis(Scren.transform.position,ParentGPos.transform.position);


        for(int i = 0;i < sumCube;i++){
            basePos[i] = cube[i].transform.localPosition;
            tmpPos[i] = cube[i].transform.localPosition;
            targetPosA[i] = new Vector3(0,0,0);
            targetPosV[i] = new Vector3(0,0,0);
            targetPosJ[i] = new Vector3(0,0,0);
        }

        

    }

    // Update is called once per frame
    void Update()
    {
        // format

        for(int i=0;i<sumCube;i++){
            tmpPos[i] = basePos[i] + (cube[numCube(cLine)].transform.localPosition - basePos[numCube(cLine)]);
        }
        

        if(timeBreath > 1000*Mathf.PI){
            timeBreath = 0.0f;
        }
        timeBreath += Time.deltaTime*1.5f;

        //Camera
        MainDir = Camera.main.gameObject.transform.forward;
        
        //Scren
        targetScrenPos = MainDir*distance;

        //centerPoint
        //circleStartPoint
        //ParentGPos

        if((FuncCubeDis(targetScrenPos,ParentGPos.transform.position) < parentCircleDis && valence < 0.7f) || (FuncCubeDis(targetScrenPos,ParentGPos.transform.position) > parentCircleDis && valence > 0.7f)){
            centerPoint.transform.localPosition = new Vector3(0,0,distance*FuncCenterPoint(MainPos,targetScrenPos,ParentGPos.transform.position));
            circleStartPoint.transform.localPosition = new Vector3(1f,0,distance*FuncCenterPoint(MainPos,targetScrenPos,ParentGPos.transform.position));
            centerRad = FuncCubeRad(centerPoint.transform.position,circleStartPoint.transform.position,ParentGPos.transform.position);
            if(ParentGPos.transform.position.y < Scren.transform.position.y){
                centerRad *= -1f;
            }
            // centerRad = CalculateRoundHalfUp(centerRad,3);
            // Debug.Log("centerPoint" + centerPoint.transform.position);
            // Debug.Log("circleStartPoint" + circleStartPoint.transform.position);
            // Debug.Log("ParentGPos" + ParentGPos.transform.position);
            // Debug.Log("centerRad" + centerRad);

            targetParentGPos = new Vector3(distance*Mathf.Sin(-(Mathf.PI/2)*valence + Mathf.PI/2)*Mathf.Cos(centerRad),distance*Mathf.Sin(-(Mathf.PI/2)*valence + Mathf.PI/2)*Mathf.Sin(centerRad),-20f + distance*Mathf.Cos(-(Mathf.PI/2)*valence + Mathf.PI/2));
            // targetParentGPos = new Vector3(distance*Mathf.Sin(-(Mathf.PI/2)*valence + Mathf.PI/2)*Mathf.Cos(centerRad),0,-20f + distance*Mathf.Cos(-(Mathf.PI/2)*valence + Mathf.PI/2));
        }
        
        // ParentLPos

            // if(Input.touchCount  > 0){
            //     Touch touch = Input.GetTouch(0);
            //     if(touch.phase == TouchPhase.Began){
            //         isJump = true;
            //     }
            // }        
            // if(isJump){
            //     timeJump += Time.deltaTime;
            //     FuncJump(timeJump);
            // }
            // if(!isJump){
            //     timeJump = 0.0f;
            // }

        // if(Input.touchCount > 0){
        //     Touch touch = Input.GetTouch(0);
        //     if(touch.phase == TouchPhase.Began){
        //         isJump = true;
        //         isMove = true;
        //     }
        // }
        // if(Input.GetKey(KeyCode.Return)){
        //     isJump = true;
        //     isMove = true;
        // }


        // if(isJump){
        //     targetParentLPos = FuncParentLPos(leave); 
        // }
        // isJump = false;
        // if(ParentLPos.transform.localPosition == targetParentLPos){
        //     isMove = false;
        // }

        //cubeParent
        targetCubeParentPos = ParentLPos.transform.position;

        //cube
        FuncBreath(timeBreath);
        // FuncValence(valence);
        FuncArousal(arousal);

        Main.transform.position = Camera.main.gameObject.transform.position;
        Main.transform.rotation = Camera.main.gameObject.transform.rotation;

        if((FuncCubeDis(targetScrenPos,ParentGPos.transform.position) < parentCircleDis && valence < 0.7f) || (FuncCubeDis(targetScrenPos,ParentGPos.transform.position) > parentCircleDis && valence > 0.7f)){
            Scren.transform.position = targetScrenPos;
            Scren.transform.rotation = Camera.main.gameObject.transform.rotation;

            ParentGPos.transform.localPosition = targetParentGPos;    
        }
        Debug.Log(FuncCubeDis(targetScrenPos,ParentGPos.transform.position));

        

        // Debug.Log(FuncCubeDis(Scren.transform.position,ParentGPos.transform.position));
        // Debug.Log(parentCircleDis);


        cubeParent.transform.position = Vector3.MoveTowards(cubeParent.transform.position,targetCubeParentPos, 0.5f*Mathf.Pow(valence,2) + 0.05f);
        cubeParent.transform.rotation = Camera.main.gameObject.transform.rotation;


        for(int i=0; i<sumCube;i++){

            // targetPos[i] = targetParentPos[i];
            // cube[i].transform.position = Vector3.MoveTowards(cube[i].transform.position,targetPos[i],1/(Time.deltaTime*10));


            // targetPos[i] = targetPos[i]  +targetPosV[i];
            targetPos[i] = tmpPos[i]  + targetPosA[i];
            // targetPos[i] = basePos[i]  + targetPosA[i] +targetPosV[i];
            // cube[i].transform.position = Vector3.MoveTowards(cube[i].transform.position,targetPos[i],1/(Time.deltaTime*10));

            if(isMove){
                ParentLPos.transform.localPosition = Vector3.MoveTowards(ParentLPos.transform.localPosition,targetParentLPos,0.1f*valence + 0.2f);        
            }
            else{
                targetPos[i]  += targetPosB[i];
                cube[i].transform.localPosition = Vector3.MoveTowards(cube[i].transform.localPosition,targetPos[i],1/(Time.deltaTime*10));
            }
        }

        rotateCamera();
    }





    private int numCube(int Line){
        numCube_a = 0;
        if(Line == 1){
            numCube_a = 0;
        }
        else{
            for(int i=1;i<Line;i++){
                numCube_a = numCube_a + numDir*i - numDir + 1;
            }
        }
        return numCube_a;
    }
    


    private void FuncArousal(float value_arousal){
        if(value_arousal < 0){      
            for(int i=1; i < cLine;i++){
                for(int j=0;j<numDir*(i-1);j++){
                    cCube = tmpPos[numCube(cLine)];

                    for(int k=numCube(i) + 1 +j;k<sumCube - numCube(i+1) +2+j; k = k + sumCube - numCube(i+1) -(numCube(i) + 1 +j) + 1 +j){

                        mag =  Mathf.Ceil(Mathf.Abs(tmpPos[k].x - cCube.x) + Mathf.Abs(tmpPos[k].y - cCube.y) + Mathf.Abs(tmpPos[k].z - cCube.z)) /(Mathf.Sqrt(Mathf.Pow(tmpPos[k].x - cCube.x,2)+Mathf.Pow(tmpPos[k].y - cCube.y,2)+Mathf.Pow(tmpPos[k].z - cCube.z,2)));

                        targetPosA[k] = new Vector3( -value_arousal*(cCube.x + mag*(tmpPos[k].x - cCube.x) - tmpPos[k].x), -value_arousal*(cCube.y + mag*(tmpPos[k].y - cCube.y) - tmpPos[k].y) , -value_arousal*(cCube.z + mag*(tmpPos[k].z - cCube.z) - tmpPos[k].z));
                    }
                }
            }
        }
        else{
            for(int i=1; i< cLine;i++){
                for(int j=0;j<numDir*(i-1);j++){
                    for(int k=numCube(i) + 1 +j;k<sumCube - numCube(i+1)+2+j; k = k + sumCube - numCube(i+1) -(numCube(i) + 1 +j) + 1 +j){
                        if(k < numCube(cLine)){
                            cCube = new Vector3(tmpPos[(cLine -2)*((cLine-1)*numDir/2 + numDir + 1) + j%numDir + 2].x, tmpPos[0].y,  tmpPos[(cLine -2)*((cLine-1)*numDir/2 + numDir + 1) + j%numDir + 2 ].z );
                        }
                        else{
                            cCube = new Vector3(tmpPos[(cLine -2)*((cLine-1)*numDir/2 + numDir + 1) + j%numDir  + 2 ].x, tmpPos[sumCube-1].y,  tmpPos[(cLine -2)*((cLine-1)*numDir/2 + numDir + 1) + j%numDir + 2].z );            
                        }                        

                        cDis = Mathf.Sqrt(Mathf.Pow(tmpPos[k].x - cCube.x,2) + Mathf.Pow(tmpPos[k].y - cCube.y,2) + Mathf.Pow(tmpPos[k].z - cCube.z,2));

                        if(cDis == Mathf.Ceil(cDis)){
                            mag = (Mathf.Floor(cDis) + 1)/cDis;
                        }
                        else{
                            mag = (Mathf.Ceil(cDis))/cDis;
                        }
                        targetPosA[k] = new Vector3(value_arousal*(cCube.x + mag*(tmpPos[k].x - cCube.x) - tmpPos[k].x),value_arousal*(cCube.y + mag*(tmpPos[k].y - cCube.y) - tmpPos[k].y),value_arousal*(cCube.z + mag*(tmpPos[k].z - cCube.z) - tmpPos[k].z));                       
                    }
                }
            }
        }

    }

    private void FuncValence(float value_valence){
        cCube = tmpPos[numCube(cLine)];
         //arm

        for(int i=0;i<sumCube;i++){
            targetPosV[i] = new Vector3(0, -valence*0.1f*(tmpPos[i].y - cCube.y) +  (Mathf.Pow(tmpPos[i].x-cCube.x,2)+(Mathf.Pow(tmpPos[i].z -cCube.z,2)))*(valence)*0.1f,0);
        }

        //normal
        
            // for(int i=0;i<numCube(cLine);i++){
            //     targetPosV[i] = new Vector3(0, 0.5f*valence*(tmpPos[i].y - cCube.y),0);
            //     targetPosV[sumCube - i -1] = new Vector3(0,-0.5f*valence*(tmpPos[sumCube-i-1].y - cCube.y),0);
            // }

        //height

            // for(int i=0;i<sumCube;i++){
            //     targetPosV[i] = 0.3f*valence*(tmpPos[i] - cCube);
            //     targetPosV[i].x = -targetPosV[i].x;
            //     targetPosV[i].z = -targetPosV[i].z;
            // }
    }

    private void FuncJump(float timeJump){
        if(velocityJump*timeJump - gravity*Mathf.Pow(timeJump,2) < 0){
            isJump = false;
            for(int i=0;i<sumCube;i++){
                targetPosJ[i] = new Vector3(0,0,0);
            }
        }
        else{
            for(int i=0;i<sumCube;i++){
                targetPosJ[i] = new Vector3(0,velocityJump*timeJump - gravity*Mathf.Pow(timeJump,2),0);
            }
        }

    }

    private void FuncBreath(float timeBreath){
        for(int i=0; i< sumCube;i++){
            if(arousal < 0){
                mag =  0.06f*FuncCubeDis(tmpPos[numCube(cLine)],tmpPos[i])*Mathf.Sin(timeBreath*(0.25f*Mathf.Pow(arousal + 1,2) + 0.75f));
            }
            else{
                mag =  0.06f*FuncCubeDis(tmpPos[numCube(cLine)],tmpPos[i])*Mathf.Sin(timeBreath*(6f*Mathf.Pow(arousal,2) + 1f));
            }
            targetPosB[i] = FuncDir(tmpPos[i],tmpPos[numCube(cLine)])*mag;
        }
    }

    
    private float FuncCubeDis(Vector3 pointP,Vector3 pointQ){
        cubeDis = Mathf.Sqrt(Mathf.Pow(pointP.x - pointQ.x,2) + Mathf.Pow(pointP.y - pointQ.y,2) + Mathf.Pow(pointP.z - pointQ.z,2));
        return  cubeDis;
    }

    private Vector3 FuncDir(Vector3 pointStart, Vector3 pointEnd){
        direction = (pointEnd - pointStart)*FuncCubeDis(pointStart,pointEnd);
        return direction;
    }

    private float FuncCubeRad(Vector3 pointC,Vector3 pointP,Vector3 pointQ){
        Vector3 pointCP;
        Vector3 pointCQ;
        float DisA = 0;
        float DisB = 0;
        float innerProduct = 0;
        float rad = 0;

        pointCP = pointP - pointC;
        pointCQ = pointQ - pointC;
        DisA = FuncCubeDis(pointC,pointP);
        DisB = FuncCubeDis(pointC,pointQ);
        innerProduct = pointCP.x*pointCQ.x + pointCP.y*pointCQ.y + pointCP.z*pointCQ.z;
        rad = Mathf.Acos(innerProduct/(DisA*DisB));

        return rad;

    }





    private Vector3 FuncParentGPos(float rad, float theta){
        float Pointx;
        float Pointy;
        float Pointz;
        Vector3 Point;

        Pointx = Random.Range(-rad*Mathf.Sin(theta),rad*Mathf.Sin(theta));
        Pointy = Mathf.Sqrt(Mathf.Pow(rad * Mathf.Sin(theta),2) - Mathf.Pow(Pointx,2));
        if(Random.Range(-1f,1f) < 0){
            Pointy *= -1;
        }
        Pointz = rad * Mathf.Cos(theta);
        
        Point = new Vector3(Pointx,0,-20f + Pointz);

        return Point;
    }

    private float FuncCenterPoint(Vector3 pointP,Vector3 pointQ,Vector3 pointR){
        float dotNumer = 0;
        float dotDenom = 0;
        float Point;

        dotNumer = Vector3.Dot(pointQ - pointP,pointR - pointP);
        dotDenom = Vector3.Dot(pointQ - pointP,pointQ - pointP);

        Point = dotNumer/dotDenom;

        return Point;
    }

    private Vector3 FuncParentLPos(float rad){
        float Pointx = 0f;
        float Pointy = 0f;
        float Pointz = 0f;
        Vector3 Point;

        do{
            Pointx  = rad;
            Pointz = Random.Range(distance,Mathf.Sqrt(Mathf.Pow(10.0f*rad,2)*(1 - Mathf.Pow(Pointx/rad,2))));
            Pointy = Mathf.Sqrt(Mathf.Pow(2.5f*rad,2)*(1 - Mathf.Pow(Pointx/rad,2) - Mathf.Pow(Pointz/(rad*10.0f),2)));
            if(Random.Range(-1f,1f) < 0){
                Pointy *= -1;
            }
        }
        while(float.IsNaN(Pointy));

        Point = new Vector3(Pointx,Pointy,Pointz);
        return Point;
    }

    private float CalculateRoundHalfUp(float value, int digitsNum) {
        return float.Parse(value.ToString("F" + digitsNum.ToString()));
    }

    private void rotateCamera()
    {
        Vector3 angle = new Vector3(Input.GetAxis("Mouse X") * 2.0f,Input.GetAxis("Mouse Y") * 2.0f, 0);      
        Camera.main.gameObject.transform.RotateAround(Vector3.zero, Vector3.up, angle.x);
    }

    
}
