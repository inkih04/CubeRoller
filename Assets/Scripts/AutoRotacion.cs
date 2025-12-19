using UnityEngine;

public class AutoRotacion : MonoBehaviour
{
    [Header("Ajustes")]
    public float velocidadGiro = 30f; // Velocidad de rotación
    public Vector3 ejeRotacion = Vector3.up; // Eje Y (hacia arriba)

    void Update()
    {
        // Gira el objeto constantemente
        transform.Rotate(ejeRotacion * velocidadGiro * Time.deltaTime);
    }
}