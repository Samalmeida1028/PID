using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    [Header("Box Size")]
    public int length = 100;
    public int width = 100;
    public int height = 50;


    [Header("Simulation Variables")]
    [Range(.02f,.5f)]
    public float timeStep = .02f;
    [Range(1,10)]
    public int timeScale = 1;
    public GameObject PID;
    public GameObject target;
    [Range(10, 3000)]
    public int populationSize;
    
    [Space(20)]
    [Range(0, 1f)]
    public float cullRate;
    [Range(0, 100f)]
    public int iterationTime;

    public float 
    generations = 1,
    showBestThreshold = 2,
    currentTime = 0;

    [Header("Population Stats")]
    [Space(20)]
    public float minimumError = 0;
    public float maximumError = 0;
    public float averageError;
    public float standardDeviation;
    public float averagePGain;
    public float averageIGain;
    public float averageDGain;
    public float PercentBestOnTargetOfPopulation;
    public float currentBestPercent;

    [Header("Debugging Arrays")]
    [Space(20)]
    public GameObject[] PIDControllers;
    public GameObject[] Targets;
    public Vector2[] sortedFitness;

    GameObject prev,current,
    prevT,currentT;
    float minEr = 100000000;
    float maxEr = 0;
    int offSpringLeft;



    void Start()
    {
        GameObject plane = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Plane));
        plane.transform.localScale = new Vector3(length*10,1,width*10);
        plane.GetComponent<MeshRenderer>().material.color = new Color(137,130,85);
        Vector3 p = plane.transform.position;
        plane.transform.position = new Vector3(p.x+length,p.y,p.z+width);
        PIDControllers = new GameObject[populationSize];
        Targets = new GameObject[populationSize];


        
        for(int i = 0; i<populationSize; i++){
            int x = Random.Range(0,length);
            int z = Random.Range(0,width);
            PIDControllers[i] = Instantiate(PID);
            PIDControllers[i].gameObject.GetComponent<Body>().material = new Material(Shader.Find("Standard"));
            PIDControllers[i].gameObject.GetComponent<MeshRenderer>().material.color = Random.ColorHSV(0f, 1f, .5f, 1f, 0.5f, 1f);
            float offset = PIDControllers[i].gameObject.GetComponent<PIDController>().offset;
            PIDControllers[i].gameObject.transform.position = new Vector3(x+offset,10+offset,z+offset);
            Targets[i] = Instantiate(target);
            Targets[i].gameObject.transform.position = new Vector3(x,10,z);
            Targets[i].gameObject.GetComponent<changePos>().upperBound = height;
            Targets[i].gameObject.GetComponent<changePos>().lowerBound = 0;
            Targets[i].gameObject.GetComponent<changePos>().leftBound = 0;
            Targets[i].gameObject.GetComponent<changePos>().rightBound = width;
            Targets[i].gameObject.GetComponent<changePos>().forwardBound = length;
            Targets[i].gameObject.GetComponent<changePos>().backwardBound = 0;
            PIDControllers[i].gameObject.GetComponent<PIDController>().target = Targets[i];
            PIDControllers[i].gameObject.GetComponent<PIDController>().controlled =  PIDControllers[i];
        }
        prev = PIDControllers[0];
        current = PIDControllers[1];
        prevT = Targets[0];
        currentT = Targets[1];
        setRandomatt();
        
    }

    void FixedUpdate()
    {
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = timeStep;
        currentBestPercent=-.0000001F;
        if(currentTime>(iterationTime/showBestThreshold)){
        for(int i = 0; i<populationSize; i++){
            float onPercent = PIDControllers[i].gameObject.GetComponent<PIDController>().timeOn/PIDControllers[i].gameObject.GetComponent<PIDController>().timeOff;
            if(onPercent>=currentBestPercent){
                currentBestPercent=onPercent;
                prev = current;
                current = PIDControllers[i];
                prevT = currentT;
                currentT = Targets[i];
                prev.gameObject.GetComponent<Transform>().localScale = new Vector3(3,3,3);
                current.gameObject.GetComponent<Transform>().localScale = new Vector3(15,15,15);
                prevT.gameObject.GetComponent<Transform>().localScale = new Vector3(2,2,2);
                currentT.gameObject.GetComponent<Transform>().localScale = new Vector3(10,10,10);
            }

        }
        }
        currentTime += Time.fixedDeltaTime;
        if(currentTime>(iterationTime)){
            generations++;
            refresh();
            currentTime = 0;
        }
        
    }

    void setRandomatt(){
        for(int i = 0; i < populationSize; i++){
            PIDControllers[i].gameObject.GetComponent<PIDController>().pGain = Random.Range(0f,200f);
            PIDControllers[i].gameObject.GetComponent<PIDController>().dGain = Random.Range(0f,200f);
            PIDControllers[i].gameObject.GetComponent<PIDController>().iGain = Random.Range(0f,200f);
        }
    }



    void refresh(){
        GameObject[] PIDtemp = new GameObject[populationSize];
       GameObject[] Targettemp = new GameObject[populationSize];
        calculateFitness();
        sortByFitness();
        calculateOffspring();
        standardD();
    (Vector3 [] genelist, Color [] materialList) = haveOffSpring();
;       
       for(int i = 0; i<populationSize; i++)
       {
            int x = Random.Range(0,length);
            int z = Random.Range(0,width);
            PIDtemp[i] = Instantiate(PID);
            float offset = PIDControllers[i].gameObject.GetComponent<PIDController>().offset;
            PIDtemp[i].gameObject.transform.position = new Vector3(x+offset,10+offset,z+offset);
            PIDtemp[i].gameObject.GetComponent<MeshRenderer>().material.color = materialList[i];
            PIDtemp[i].gameObject.GetComponent<PIDController>().pGain = genelist[i].x;
            PIDtemp[i].gameObject.GetComponent<PIDController>().dGain = genelist[i].y;
            PIDtemp[i].gameObject.GetComponent<PIDController>().iGain = genelist[i].z;
            Targettemp[i] = Instantiate(target);
            Targettemp[i].gameObject.transform.position = new Vector3(x,10,z);
            Targettemp[i].gameObject.GetComponent<changePos>().upperBound = height;
            Targettemp[i].gameObject.GetComponent<changePos>().lowerBound = 0;
            Targettemp[i].gameObject.GetComponent<changePos>().leftBound = 0;
            Targettemp[i].gameObject.GetComponent<changePos>().rightBound = width;
            Targettemp[i].gameObject.GetComponent<changePos>().forwardBound = length;
            Targettemp[i].gameObject.GetComponent<changePos>().backwardBound = 0;
            PIDtemp[i].gameObject.GetComponent<PIDController>().target = Targettemp[i];
            PIDtemp[i].gameObject.GetComponent<PIDController>().controlled =  PIDtemp[i];
            if(PIDControllers[i].gameObject.GetComponent<PIDController>().totalError<minEr){
                minEr = PIDControllers[i].gameObject.GetComponent<PIDController>().totalError;
            }
             if(PIDControllers[i].gameObject.GetComponent<PIDController>().totalError>maxEr){
                maxEr = PIDControllers[i].gameObject.GetComponent<PIDController>().totalError;
            }
            GameObject.Destroy(PIDControllers[i]);
            GameObject.Destroy(Targets[i]);
            PIDControllers[i] = PIDtemp[i];
            Targets[i] = Targettemp[i];
        }

        prev = PIDControllers[0];
        current = PIDControllers[1];
        prevT = Targets[0];
        currentT = Targets[1];
        minimumError = minEr/iterationTime;
        maximumError = maxEr/iterationTime;
        maxEr = 0;
        minEr = 1000000000;
    }
   

    (Vector3 [], Color []) haveOffSpring(){
        offSpringLeft = populationSize;
        Vector3 [] geneList = new Vector3[populationSize];
        Color [] materialList = new Color[populationSize];
        for(int i = 0; i < populationSize; i++){
            GameObject parent1 = PIDControllers[(int)sortedFitness[i].y];
            Color material1 =  parent1.gameObject.GetComponent<MeshRenderer>().material.color;
            while((parent1.gameObject.GetComponent<Body>().maxOffspring>0) && offSpringLeft>0){
                int index = Random.Range(0,(int)((populationSize-1)*(1-cullRate)));
                GameObject parent2 = PIDControllers[(int)sortedFitness[index].y];
                if(parent1 != parent2){
                Color material2 =  parent2.gameObject.GetComponent<MeshRenderer>().material.color;
                Vector3 gene1 = new Vector3(parent1.gameObject.GetComponent<PIDController>().pGain,parent1.gameObject.GetComponent<PIDController>().dGain,parent1.gameObject.GetComponent<PIDController>().iGain);
                Vector3 gene2 = new Vector3(parent2.gameObject.GetComponent<PIDController>().pGain,parent2.gameObject.GetComponent<PIDController>().dGain,parent2.gameObject.GetComponent<PIDController>().iGain);
                parent1.gameObject.GetComponent<Body>().maxOffspring--;
                parent2.gameObject.GetComponent<Body>().maxOffspring--;
                (Vector3 childGene, Color childMaterial) = crossOver(gene1,gene2, material1, material2);
                geneList[populationSize-offSpringLeft] = childGene;
                materialList[populationSize-offSpringLeft] = childMaterial;

                offSpringLeft--;
                }
            }
        }
        while(offSpringLeft>0){
            int index = Random.Range(0,(int)((populationSize-1)*(1-cullRate)));
            GameObject parent1 = PIDControllers[(int)sortedFitness[index].y];
            Color material1 =  parent1.gameObject.GetComponent<MeshRenderer>().material.color;
            int index2 = Random.Range(0,(int)((populationSize-1)*(1-cullRate)));
            GameObject parent2 = PIDControllers[(int)sortedFitness[index2].y];
                Color material2 =  parent2.gameObject.GetComponent<MeshRenderer>().material.color;
                Vector3 gene1 = new Vector3(parent1.gameObject.GetComponent<PIDController>().pGain,parent1.gameObject.GetComponent<PIDController>().dGain,parent1.gameObject.GetComponent<PIDController>().iGain);
                Vector3 gene2 = new Vector3(parent2.gameObject.GetComponent<PIDController>().pGain,parent2.gameObject.GetComponent<PIDController>().dGain,parent2.gameObject.GetComponent<PIDController>().iGain);
                (Vector3 childGene, Color childMaterial) = crossOver(gene1,gene2, material1, material2);
                geneList[populationSize-offSpringLeft] = childGene;
                materialList[populationSize-offSpringLeft] = childMaterial;
                offSpringLeft--;
            }
            return (geneList, materialList);
        }

    void calculateFitness(){
        float errorSum = 0;
            averagePGain = 0;
            averageIGain = 0;
            averageDGain = 0;
        for(int i = 0; i<populationSize; i++){
            errorSum += PIDControllers[i].gameObject.GetComponent<PIDController>().totalError;
            averagePGain += PIDControllers[i].gameObject.GetComponent<PIDController>().pGain;
            averageIGain += PIDControllers[i].gameObject.GetComponent<PIDController>().iGain;
            averageDGain += PIDControllers[i].gameObject.GetComponent<PIDController>().dGain;
            float percentage = ((float)PIDControllers[i].gameObject.GetComponent<PIDController>().timeOn/(float)PIDControllers[i].gameObject.GetComponent<PIDController>().timeOff)*100;
            if(percentage>PercentBestOnTargetOfPopulation
    ){
                PercentBestOnTargetOfPopulation
         = percentage;
            }
        }

        averageError = (errorSum/populationSize)/iterationTime;
        averagePGain /= populationSize;
        averageIGain /= populationSize;
        averageDGain /= populationSize;
        for(int i = 0; i<populationSize; i++){
            float fitness = PIDControllers[i].gameObject.GetComponent<PIDController>().totalError/errorSum;
            PIDControllers[i].gameObject.GetComponent<Body>().fitness = fitness;
        }
    }

    void standardD(){
            float temp = 0;
            foreach(GameObject i in PIDControllers){
                temp += (i.gameObject.GetComponent<PIDController>().totalError/iterationTime-averageError)*(i.gameObject.GetComponent<PIDController>().totalError/iterationTime-averageError);
            }
            standardDeviation = Mathf.Sqrt(temp/populationSize);
        }
    void calculateOffspring(){
        for(int i = 0; i<populationSize; i++){
            PIDControllers[i].gameObject.GetComponent<Body>().maxOffspring=(int)Mathf.Ceil((((1/(PIDControllers[i].gameObject.GetComponent<Body>().fitness))/populationSize)));
        }
    }

    void sortByFitness(){
        sortedFitness = new Vector2[populationSize];
        for(int i = 0; i < populationSize; i++){
            sortedFitness[i] = new Vector2(PIDControllers[(int)i].gameObject.GetComponent<Body>().fitness, (float)i);
        }

        for(int i = 0; i< populationSize; i++){

        for(int j = 0; j< populationSize-1; j++){
            float fitnessj = sortedFitness[j].x; 
            int indexj =  (int)sortedFitness[j].y;
            float fitnessjj = sortedFitness[j+1].x; 
            int indexjj =  (int)sortedFitness[j+1].y;

            if (fitnessj>fitnessjj){//>
                sortedFitness[j].x = fitnessjj;
                sortedFitness[j+1].x = fitnessj;
                sortedFitness[j].y = indexjj;
                sortedFitness[j+1].y = indexj;

            }
        }
        }
    }

    (Vector3, Color) crossOver(Vector3 gene1, Vector3 gene2, Color material1, Color material2){
        int p1 = 0;
        int p2 = 0;
        Vector3 childGene = new Vector3();
        Color childMaterial;
            int rand = Random.Range(0,10000);
            if(rand > (2000/(generations/5)) && rand < 5800){
                p1++;
                childGene.x = gene1.x;
            }
            else if (rand < 10000 && rand > 5800){
                p2++;
                childGene.x = gene2.x;
            }
            else{
               childGene.x = Random.Range(.7f,1.4f)*gene1.x*gene2.x/(gene1.x+gene2.x);
            }

            int rand1 = Random.Range(0,10000);
            if(rand1 > (2000/(generations/5)) && rand1 < 5800){
                p1++;
                childGene.y = gene1.y;
            }
            else if (rand1 < 10000 && rand1 > 5800){
                p2++;
                childGene.y = gene2.y;
            }
            else{
               childGene.y = Random.Range(.7f,1.4f)*gene1.y*gene2.y/(gene1.y+gene2.y);
            }

            int rand2 = Random.Range(0,10000);
            if(rand2 > (2000/(generations/5)) && rand2 < 5800){
                p1++;
                childGene.z = gene1.z;
            }
            else if (rand2 < 10000 && rand2 > 5800){
                p2++;
                childGene.z = gene2.z;
            }
            else{
               childGene.z = Random.Range(.7f,1.4f)*gene1.z*gene2.z/(gene1.z+gene2.z);
            }
            if(p1>1){
                float h1,s1,v1;
                Color.RGBToHSV(material1,out h1,out s1, out v1);
                childMaterial = new Color(h1*(Random.Range(.95f,1.05f)), s1, v1);
            }
            if(p2>1){
                float h2,s2,v2;
                Color.RGBToHSV(material2,out h2,out s2, out v2);
                childMaterial = new Color(h2*(Random.Range(.95f,1.05f)), s2, v2);
            }
            else{
                float h1,s1,v1,h2,s2,v2;
                Color.RGBToHSV(material1,out h1,out s1, out v1);
                Color.RGBToHSV(material2,out h2,out s2, out v2);
                float minh = Mathf.Min(h1,h2);
                float maxh = Mathf.Max(h1,h2);
                childMaterial = Random.ColorHSV((minh/1.5f),(maxh*1.5f), 0.5f, 1f);
            }

            return (childGene, childMaterial);
    }
}
