using System.Reflection;
using UnityEngine;

/**<summary>Klasa reprezentujaca przycisk w menu</summary>*/
public class Button : MonoBehaviour 
{
    /**<summary>Tekstura (ikona) przycisku</summary>*/
    public Texture2D icon;
    /**<summary>Liczba porzadkowa przecyski w menu (liczone od 0)</summary>*/
    public int order;

    /**<summary>Pozycja i rozmiar kontrolki</summary>*/
    private Rect rect;
    /**<summary>Wysokossc kontrolki</summary>*/
    public float Height { set { rect.height = value; } get { return rect.height; } }
    /**<summary>Szerokosc kontrolki</summary>*/
    public float Width { set { rect.width = value; } get { return rect.width; } }
    /**<summary>Pozycja X kontrolki na ekranie</summary>*/
    public float X { set { rect.x = value; } get { return rect.x; } }
    /**<summary>Pozycja Y kontrolki na ekranie</summary>*/
    public float Y { set { rect.y = value; } get { return rect.y; } }

    /**<summary>Nasluchujacy kontroler, ktory bedzie wykonywal operacje po wcisnieciu przycisku</summary>*/ 
    public Controller listener;
    /**<summary>Nazwa metody, ktora ma wykonac listener, gdy zostanie nacisniety przycisk</summary>*/
    public string methodName;
    /**<summary>Metoda, ktora bedzie wykonywana</summary>*/
    private MethodInfo method; 

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */
    /**<summary>Przygotowuje przycisk do dzialania. Funkcja odpalana podczas tworzenia obiektu</summary>*/
    void Start()
    {
        method = listener.GetType().GetMethod(methodName);
    }

    /**<summary>Funkcja rysujaca kontrolke</summary>*/
    void OnGUI()
    {
        if(UnityEngine.GUI.Button(new Rect(rect.x, rect.y, rect.width, rect.height), new GUIContent(icon)))
            method.Invoke(listener, null);

    }
}
