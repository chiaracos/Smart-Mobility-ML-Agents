using UnityEngine;
using TMPro;

public class SpeedDisplay : MonoBehaviour
{
    public CarAgent carAgent; // Riferimento alla macchina
    public TextMeshProUGUI speedText; // Riferimento al testo UI
    private float conversionFactor = 3.6f; // Convertitore da m/s a km/h
    private float speedLimit = 31f; // Limite di velocità in km/h

    void Update()
    {
        //Debug.Log("[UI] Sto aggiornando la velocità nel Canvas.");
        if (carAgent != null && speedText != null)
        {
            // Ottieni la velocità attuale e converti in km/h
            float speedKmH = carAgent.GetComponent<Rigidbody>().velocity.magnitude * conversionFactor;
            int speed = (int) speedKmH;
            speedText.text = $"Velocità: {speed} km/h";

            // Cambia colore se supera il limite
            if (speedKmH > speedLimit)
            {
                speedText.color = Color.red; // Rosso se supera il limite
            }
            else
            {
                speedText.color = Color.white; // Verde se è sotto il limite
            }
        }
    }
}
