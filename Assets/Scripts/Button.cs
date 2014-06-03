using System.Reflection;
using UnityEngine;

/* klasa buttona */
public class Button : MonoBehaviour 
{
    /* tekstura (ikona) przycisku */
    public Texture2D icon;
    /* kolejnosc wyswietlania. 
     * Powinna zawierac wartosc od 0 rosnaco */
    public int order;

    /* pozycja i rozmiar okna */
    private Rect rect;
    public float Height { set { rect.height = value; } get { return rect.height; } }
    public float Width { set { rect.width = value; } get { return rect.width; } }
    public float X { set { rect.x = value; } get { return rect.x; } }
    public float Y { set { rect.y = value; } get { return rect.y; } }

    /* nasluchujacy kontroler, ktory bedzie wykonywal operacje po wcisnieciu przycisku */ 
    public Controller listener;
    /* nazwa metody, ktora ma wykonac listener, gdy zostanie nacisniety przycisk */
    public string methodName;
    /* metoda, ktora bedzie wykonywana */
    private MethodInfo method; 

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */

    void Start()
    {
        method = listener.GetType().GetMethod(methodName);
    }

    void OnGUI()
    {
        if(UnityEngine.GUI.Button(new Rect(rect.x, rect.y, rect.width, rect.height), new GUIContent(icon)))
            method.Invoke(listener, null);

    }
}
