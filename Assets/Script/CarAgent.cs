using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class CarAgent : Agent
{
    [Header("Car Settings")]
    public float maxSpeed = 20f;// 70 km/h
    public float accelerationRate = 0.2f; // Controlla la gradualità dell'accelerazione
    private float currentSpeedLimit; // Per gestire il limite di velocità dinamico
    public float acceleration = 50f;
    public float brakingForce = 6f;
    public float turnSpeed = 60f;
    private float speedLimit =8f; // Limite di velocità per la zona "Limite30" (per rimanete sootto i 30)
    private float normalMaxSpeed = 20f;
    private bool isInSpeedLimitZone = false; // Indica se la macchina è nella zona limite
    private bool hasExceededSpeedLimit = false; // Evita penalità ripetute
    private bool isEndingEpisode = false; // Evita chiamate multiple a EndEpisode()
    private bool isTrafficLightGreen = true;

    [Header("Checkpoint Settings")]
    public List<Transform> checkpoints;
    private int currentCheckpointIndex = 0;

    [Header("Checkpoint Directions")]
    public Dictionary<int, float> checkpointDirections = new Dictionary<int, float>
    {
        { 0, 0f },
        { 1, 0f }, // Dal checkpoint 1 al 5 deve andare dritto
        { 2, 0f },
        { 3, 0f },
        { 4, 0f },
        { 5, 0f },
        { 6, 0f },
        { 7, 0f },
        { 8, -1f },
        { 9, -1f },
        { 10, -1f },
        { 11, 0f },
        { 12, 0f },
        { 13, 0f },// Al checkpoint 6 deve girare a sinistra
        { 14, -1f },
        { 15, -1f },
        { 16, 0f },
        { 17, 0f },
        { 19, 0f },
        { 20, 0f },
        { 21, 0f },
        { 22, 0f },
        { 23, 0f },
        { 24, -1f },
        { 25, -1f },
        { 26, 0f },
        { 27, 0f },
        { 28, 0f },
        { 29, -1f },
        { 30, -1f }


    };

    private Rigidbody carRigidbody;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    public override void Initialize()
    {
        carRigidbody = GetComponent<Rigidbody>();
        carRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;
        carRigidbody.velocity = Vector3.zero;
        carRigidbody.angularVelocity = Vector3.zero;
        currentCheckpointIndex = 0;
         // Reset variabili per il nuovo episodio
        hasExceededSpeedLimit = false;
        isInSpeedLimitZone = false;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (actions.DiscreteActions.Length < 2)
        {
            Debug.LogError("Errore: L'array di azioni è più piccolo del previsto!");
            return;
        }

        int steeringAction = actions.DiscreteActions[0];
        int accelerationAction = actions.DiscreteActions[1];

        float steering = 0f;
        if (steeringAction == 0) steering = -1f;
        if (steeringAction == 2) steering = 1f;

        float accelerationInput = (accelerationAction == 1) ? 1f : 0f;
        float brakeInput = (accelerationAction == 0) ? 1f : 0f;

        //  Se il semaforo è rosso, impedisci di accelerare
        if (!isTrafficLightGreen && carRigidbody.velocity.magnitude < 0.1f)
        {
            accelerationInput = 0f; // Non accelera finché non diventa verde
        }
        float lateralVelocity = Vector3.Dot(carRigidbody.velocity, transform.right); // Velocità laterale
        float stabilityReward = 1.0f - Mathf.Clamp01(Mathf.Abs(lateralVelocity) / maxSpeed);

        AddReward(stabilityReward * 0.1f); // Ricompensa se mantiene una traiettoria stabile

        ApplyMovement(steering, accelerationInput, brakeInput);
        if (Mathf.Approximately(currentSpeedLimit, normalMaxSpeed))
        {
        float currentSpeed = carRigidbody.velocity.magnitude;
        // Normalizziamo la velocità in modo che 1 significhi maxSpeed
        float normalizedSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed) / maxSpeed;
        // Il fattore di scala può essere regolato per controllare l’entità della ricompensa
        float speedRewardFactor = (currentSpeed <= speedLimit) ? 0.5f : -1.0f; 
        float speedReward = normalizedSpeed * speedRewardFactor;
        AddReward(speedReward);
        Debug.Log($"Speed reward: Velocità = {currentSpeed}, Normalizzata = {normalizedSpeed}, Ricompensa = {speedReward}");
        }
    }

    private void ApplyMovement(float steering, float accelerate, float brake)
    {
        float turnAngle = steering * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnOffset = Quaternion.Euler(0f, turnAngle, 0f);
        carRigidbody.MoveRotation(carRigidbody.rotation * turnOffset);

        if (accelerate > 0)
        {
            carRigidbody.AddForce(transform.forward * acceleration, ForceMode.Acceleration);
        }
        else if (brake > 0)
        {
            carRigidbody.velocity *= 0.80f;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(carRigidbody.velocity.magnitude / maxSpeed);
        sensor.AddObservation(transform.forward.x);
        sensor.AddObservation(transform.forward.z);
        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.z);
        sensor.AddObservation(carRigidbody.velocity.x);
        // Osservazione sul colore del semaforo (0 = rosso, 1 = verde)
        sensor.AddObservation(isTrafficLightGreen ? 1.0f : 0.0f);
        
    }

public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.A))
        {
            discreteActions[0] = 0;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            discreteActions[0] = 2;
        }
        else
        {
            discreteActions[0] = 1;
        }

        if (Input.GetKey(KeyCode.W))
        {
            discreteActions[1] = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActions[1] = 0;
        }
        else
        {
            discreteActions[1] = 2;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            Debug.Log($"Checkpoint {currentCheckpointIndex} raggiunto");
            
            if (currentCheckpointIndex < checkpoints.Count && other.transform == checkpoints[currentCheckpointIndex])
            {
                AddReward(15.0f);
                Debug.Log("Ricompensa per checkpoint");

                if (checkpointDirections.ContainsKey(currentCheckpointIndex))
                {
                    float requiredSteering = checkpointDirections[currentCheckpointIndex];
                    Debug.Log($"Checkpoint {currentCheckpointIndex}: Direzione richiesta {requiredSteering}");

                    if ((requiredSteering == 0 && Mathf.Abs(carRigidbody.angularVelocity.y) < 0.1f) ||
                        (requiredSteering < 0 && carRigidbody.angularVelocity.y < 0) ||
                        (requiredSteering > 0 && carRigidbody.angularVelocity.y > 0))
                    {
                        AddReward(1.5f);
                        Debug.Log("Sterzata corretta");
                    }
                    else
                    {
                        AddReward(-5.5f);
                        Debug.Log("Sterzata errata");
                    }
                }
                
                currentCheckpointIndex++;
            }
            else
            {
                AddReward(-2.0f);
                Debug.Log("Checkpoint errato - episodio terminato");
                EndEpisode();
            }
        }
        
        if (other.CompareTag("Finish"))
        {
            if (currentCheckpointIndex == checkpoints.Count)
            {
                AddReward(30.0f);
                currentCheckpointIndex=0;
                Debug.Log("Traguardo raggiunto con successo");
            }
            else
            {
                AddReward(-5.0f);
                Debug.Log("Traguardo raggiunto senza completare tutti i checkpoint");
            }
           // EndEpisode();
        }

        if (other.CompareTag("Limite30"))
        {
            isInSpeedLimitZone = true; // Attiva il monitoraggio della velocità
            currentSpeedLimit = speedLimit; // Quando entra in una zona con limite, aggiorna il limite
            Debug.Log("Entrato in zona limite 30 km/h");
            float currentSpeed = carRigidbody.velocity.magnitude;

            if (currentSpeed > speedLimit)
            {
                float penalty = -50; // Penalità più gestibile
                AddReward(penalty);
                Debug.Log($"Superato limite 30 km/h: Velocità = {currentSpeed}, Penalità = {penalty}");
                // Prova a fermare la macchina prima di terminare l'episodio
                carRigidbody.velocity = Vector3.zero;
                carRigidbody.angularVelocity = Vector3.zero;

                Debug.Log("Sto terminando l'episodio con EndEpisode()");
                EndEpisode(); // Termina l'episodio
                // Blocca immediatamente l'accelerazione
                //carRigidbody.velocity = Vector3.ClampMagnitude(carRigidbody.velocity, speedLimit);
                
            }
            else
            {
                // Ricompensa meno aggressiva
                float reward = 25.0f * (currentSpeed / speedLimit);
                AddReward(reward);
                Debug.Log($"Limite 30 km/h rispettato: Velocità = {currentSpeed}, Ricompensa = {reward}");
            }


        }

         if (other.CompareTag("TrafficLightTrigger"))
        {
            // Prova a ottenere il TrafficLightController dalla gerarchia, partendo dal collider.
            TrafficLightContr trafficLight = other.GetComponentInParent<TrafficLightContr>();
            
            if (trafficLight != null)
            {
                string currentColor = trafficLight.GetCurrentColor();
                isTrafficLightGreen = (currentColor == "Green"); // Aggiorna la variabile
                Debug.Log($"[TrafficLightSensor] Colore del semaforo rilevato: {currentColor}");

                if (currentColor == "Red")
                {
                    Debug.Log("[TrafficLightSensor] Semaforo rosso rilevato: applica penalità o comportamenti specifici.");
                    float currentSpeed = carRigidbody.velocity.magnitude;

                    if (currentSpeed > 0.5f) // Se l'auto è in movimento al rosso
                    {
                        AddReward(-20.0f); // Penalità per passaggio col rosso
                        Debug.Log("Penalità per passaggio con il rosso!");
                    }
                    else
                    {
                        AddReward(20.0f); // Ricompensa se l'auto si ferma
                        Debug.Log("Ricompensa per essersi fermata al rosso!");
                    }
                     
                }
                else if (currentColor == "Green")
                {
                
                    Debug.Log("[TrafficLightSensor] Semaforo verde rilevato: procedi.");
                    AddReward(10.0f); // Incentivo a passare quando è verde
                   
                }

            }

        }




    }
        private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Limite30"))
        {
            isInSpeedLimitZone = false; // Disattivo il monitoraggio della velocità
            hasExceededSpeedLimit=false;
            currentSpeedLimit = normalMaxSpeed; // Quando esce dalla zona a limite, ripristina il limite massimo
            Debug.Log("Uscito dalla zona limite 30 km/h");
            Debug.Log($"La velocità torna alla normalità: Velocità = {maxSpeed}");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Offroad"))
        {
            AddReward(-50.0f);
            Debug.Log("Collisione con Offroad - episodio terminato");
            //EndEpisode();
        }
    }
    private void CheckTrafficLight()
    {
        RaycastHit hit;
        float rayDistance = 10f; // Distanza massima del Raycast
        Vector3 rayOrigin = transform.position + Vector3.up * 1.0f; // Leggermente sopra il suolo

        if (Physics.Raycast(rayOrigin, transform.forward, out hit, rayDistance))
        {
            if (hit.collider.CompareTag("TrafficLightTrigger"))
            {
                TrafficLightContr trafficLight = hit.collider.GetComponentInParent<TrafficLightContr>();
                
                if (trafficLight != null)
                {
                    string currentColor = trafficLight.GetCurrentColor();
                    isTrafficLightGreen = (currentColor == "Green");

                    if (currentColor == "Red")
                    {
                        float currentSpeed = carRigidbody.velocity.magnitude;

                        if (currentSpeed > 0.5f)
                        {
                            AddReward(-10.0f); // Penalità per non essersi fermato
                            carRigidbody.velocity *= 0.9f; // Frenata graduale
                        }
                    }
                    else if (currentColor == "Green"&& carRigidbody.velocity.magnitude < 0.1f)
                    {
                        AddReward(10.0f); // Ricompensa per aver rispettato il verde
                    }
                }
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("TrafficLightTrigger"))
        {
            TrafficLightContr trafficLight = other.GetComponentInParent<TrafficLightContr>();

            if (trafficLight != null)
            {
                string currentColor = trafficLight.GetCurrentColor();
                isTrafficLightGreen = (currentColor == "Green");

                if (!isTrafficLightGreen) // Se il semaforo è rosso
                {
                    float currentSpeed = carRigidbody.velocity.magnitude;
                    float distanceToLight = Vector3.Distance(transform.position, other.transform.position);

                    // 🔹 Più l'auto è vicina, più rallenta
                    float slowDownFactor = Mathf.Clamp01(distanceToLight / 10f); // Si riduce gradualmente
                    float newSpeed = currentSpeed * (0.8f - (0.5f * slowDownFactor)); // Maggiore riduzione

                    carRigidbody.velocity = transform.forward * newSpeed; // Applica la nuova velocità

                    Debug.Log($" Semaforo rosso! Rallento: Velocità attuale {currentSpeed} -> {newSpeed} (Distanza: {distanceToLight})");

                    if (newSpeed < 0.1f) // Se è quasi ferma, la blocca completamente
                    {
                        carRigidbody.velocity = Vector3.zero;
                        Debug.Log("🚦 Auto ferma al semaforo rosso.");
                        AddReward(30);
                    }
                }
                else //  Se il semaforo è verde
                {
                    AddReward(40.0f); 
                    Debug.Log(" Semaforo verde! L'auto può ripartire.");
                    carRigidbody.AddForce(transform.forward * acceleration * 1.5f, ForceMode.Acceleration); // Accelerazione più decisa
                }
            }
        }
    }






    void FixedUpdate()
    {
        if (isInSpeedLimitZone) // Se l'auto è nella zona limite
        {
            float currentSpeed = carRigidbody.velocity.magnitude * 3.6f; // Converti la velocità in km/h

            if (currentSpeed > speedLimit) 
            {
                // Penalità progressiva invece di terminare l'episodio subito
                float penalty = -0.1f * (currentSpeed - speedLimit); // Più è fuori limite, maggiore la penalità
                AddReward(penalty);
                Debug.Log($" Superato limite! Velocità: {currentSpeed} km/h, Penalità: {penalty}");

                // Frenata graduale invece di bloccare subito la velocità
                carRigidbody.velocity *= 0.97f; // Riduce gradualmente la velocità
            }
            else 
            {
                // Premia l'agente se sta rispettando il limite
                float reward = 0.05f * (1 - (currentSpeed / speedLimit)); // Più vicino al limite, più ricompensa
                AddReward(reward);
                Debug.Log($"Limite rispettato! Velocità: {currentSpeed} km/h, Ricompensa: {reward}");
            }
        }

             CheckTrafficLight(); // Controlla il semaforo in ogni frame

            if (!isTrafficLightGreen)
            {
            float currentSpeed = carRigidbody.velocity.magnitude;

            if (currentSpeed > 0.1f) // Se si sta ancora muovendo
            {
                carRigidbody.velocity *= 0.90f; // Frenata progressiva
                Debug.Log(" Rallentamento automatico per il semaforo rosso.");
            }
            else
            {
                carRigidbody.velocity = Vector3.zero; // Auto completamente ferma
                Debug.Log("Auto ferma al semaforo rosso.");
        }
        }
        
    }

}



