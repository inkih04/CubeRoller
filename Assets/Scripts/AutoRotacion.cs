using UnityEngine;

public class AutoRotacion : MonoBehaviour
{
    [Header("Ajustes")]
    public float velocidadGiro = 30f; 
    public Vector3 ejeRotacion = Vector3.up; 

    void Update()
    {
        // Gira el objeto constantemente
        transform.Rotate(ejeRotacion * velocidadGiro * Time.deltaTime);
    }
}