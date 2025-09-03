using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Assegna qui la macchina (il CarAgent)
    public Vector3 offset = new Vector3(0, 5, -10); // Offset per posizionare la camera dietro e sopra la macchina
    public float smoothSpeed = 0.125f; // Velocità di interpolazione per uno spostamento fluido

    void LateUpdate()
    {
        // Calcola la posizione desiderata
        Vector3 desiredPosition = target.position + offset;
        // Interpola tra la posizione attuale e quella desiderata per ottenere un movimento fluido
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
        // Fai in modo che la camera guardi sempre il target
        transform.LookAt(target);
    }
}
