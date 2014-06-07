using UnityEngine;

/**<summary>Klasa wyswietlajaca podany komunikat</summary>*/
public class Box : MonoBehaviour
{
    /**<summary>Teskt, ktory zostanie wyswietlony</summary>*/
    public string Text { set; get; }

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

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */

    /**<summary>Funkcja rysujaca kontrolke</summary>*/
    protected void OnGUI()
    {
        UnityEngine.GUI.Box(rect, Text);
    }
}
