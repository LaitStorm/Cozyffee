using UnityEngine;
using UnityEngine.UIElements;

public class StartTutorial : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        Button inicio = root.Q<Button>("Inicio_boton");    
        }

    // Update is called once per frame
    void Update()
    {
        
    }
}
