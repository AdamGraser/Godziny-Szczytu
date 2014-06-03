using UnityEngine;

/* klasa wyswietlajaca podany komunikat*/
public class Box : MonoBehaviour
{
    public string Text { set; get; } //tekst, ktory bedzie wyswietlany

    /* pozycja i rozmiar okna */
    protected Rect rect;
    public float Height { set { rect.height = value; } get { return rect.height; } }
    public float Width { set { rect.width = value; } get { return rect.width; } }
    public float X { set { rect.x = value; } get { return rect.x; } }
    public float Y { set { rect.y = value; } get { return rect.y; } }

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */

    protected void OnGUI()
    {
        UnityEngine.GUI.Box(rect, Text);
    }
}
