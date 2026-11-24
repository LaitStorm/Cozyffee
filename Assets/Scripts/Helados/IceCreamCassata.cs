using UnityEngine;

public class IceCreamCassata : MonoBehaviour
{
    [Tooltip("Nombre identificador del sabor (ej: Vainilla, Chocolate, Menta).")]
    public string flavorName = "Vainilla";

    [Tooltip("Prefab del scoop (bola de helado) correspondiente a este sabor.")]
    public GameObject scoopPrefab;
}