using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TrafficLightContr: MonoBehaviour
{
    public GameObject redLight;
    public GameObject yellowLight;
    public GameObject greenLight;

    private float timer;
    private LightState currentState;

    public float redDuration = 5.0f;
    public float greenDuration = 10.0f;
    public float yellowDuration = 2.0f;

    public enum LightState { Red, Green, Yellow }

    void Start()
    {
        timer = 0.0f;
        currentState = LightState.Red; // Il semaforo parte da rosso
        UpdateTrafficLight();
    }

    void Update()
    {
        timer += Time.deltaTime;

        switch (currentState)
        {
            case LightState.Red:
                if (timer >= redDuration)
                {
                    ChangeLight(LightState.Green);
                }
                break;

            case LightState.Green:
                if (timer >= greenDuration)
                {
                    ChangeLight(LightState.Yellow);
                }
                break;

            case LightState.Yellow:
                if (timer >= yellowDuration)
                {
                    ChangeLight(LightState.Red);
                }
                break;
        }
    }

    void ChangeLight(LightState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            timer = 0.0f;
            UpdateTrafficLight();
        }
    }

    void UpdateTrafficLight()
    {
        redLight.SetActive(currentState == LightState.Red);
        greenLight.SetActive(currentState == LightState.Green);
        yellowLight.SetActive(currentState == LightState.Yellow);
    }

    public LightState GetCurrentLightState()
    {
        return currentState;
    }

    public string GetCurrentColor()
    {
        return currentState.ToString();
    }
}
