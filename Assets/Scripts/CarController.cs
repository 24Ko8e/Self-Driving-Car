using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NeuralNetwork))]

public class CarController : MonoBehaviour
{
    private Vector3 startPosition, startRotation;
    private NeuralNetwork network;
    [Range(-1f, 1f)]
    public float a, t;
    public float timeSinceStart = 0f;

    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultiplier = 1.4f;
    public float averageSpeedMultiplier = 0.2f;
    public float sensorMultiplier = 0.1f;

    [Header("Network Options")]
    public int layers = 1;
    public int neurons = 10;


    private Vector3 lastPosition;
    private float totalDistancetravelled;
    private float avgSpeed;

    private float aSensor, bSensor, cSensor;

    private Vector3 inp;

    // Start is called before the first frame update
    void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        network = GetComponent<NeuralNetwork>();
    }

    public void resetWithNetwork(NeuralNetwork net)
    {
        network = net;
        reset();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        inputSensors();
        lastPosition = transform.position;

        (a, t) = network.runNetwork(aSensor, bSensor, cSensor);

        moveCar(a, t);
        timeSinceStart += Time.deltaTime;
        calculateFitness();
        //a = 0;
        //t = 0;
    }

    public void reset()
    {
        timeSinceStart = 0f;
        totalDistancetravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        death();
    }

    void death()
    {
        GameObject.FindObjectOfType<geneticAlgorithm>().death(overallFitness, network);
    }

    void calculateFitness()
    {
        totalDistancetravelled += Vector3.Distance(transform.position, lastPosition);
        avgSpeed = totalDistancetravelled / timeSinceStart;

        overallFitness = (totalDistancetravelled * distanceMultiplier) + (avgSpeed * averageSpeedMultiplier) + (((aSensor + bSensor + cSensor) / 3) * sensorMultiplier);

        if (timeSinceStart > 20 && overallFitness < 40)
        {
            death();
        }
        /*if (overallFitness >= 1000)
        {
            death();
        }*/
    }

    private void inputSensors()
    {
        Vector3 a = (transform.forward + transform.right);
        Vector3 b = (transform.forward);
        Vector3 c = (transform.forward - transform.right);

        Ray r = new Ray(transform.position, a);
        RaycastHit hit;

        if(Physics.Raycast(r, out hit))
        {
            aSensor = hit.distance / 20;        // Change denominator value if using a different track. The sensors values are supposed to be between 0 and 1
            Debug.DrawLine(r.origin, hit.point, Color.red);             // or slightly above 1
        }

        r.direction = b;
        if (Physics.Raycast(r, out hit))
        {
            bSensor = hit.distance / 20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = c;
        if (Physics.Raycast(r, out hit))
        {
            cSensor = hit.distance / 20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }
    }

    public void moveCar(float v, float h)
    {
        inp = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, v * 11.4f), 0.02f);
        inp = transform.TransformDirection(inp);
        transform.position += inp;

        transform.eulerAngles += new Vector3(0, (h * 90.0f) * 0.02f, 0);
    }
}
