using UnityEngine;

/**<summary>Reprezentuje menu</summary>*/
public class Menu : MonoBehaviour
{
    /**<summary>Miejsce osedzenia menu w poziomie
     * Mozliwe wartosci:
     * 1 - prawo
     * 0 - centrum
     * -1 -lewo </summary>*/
    public int horizontalHandle;
    /**<summary>Miejsce osedzenia menu w pionie
     * Mozliwe wartosci:
     * 1 - gora 
     * 0 - centrum 
     * -1 - dol </summary>*/
    public int verticalHandle;

    /* pozycja menu */
    /**<summary>Pozycja X na ekranie</summary>*/
    public int menuXPosition;
    /**<summary>Pozycja Y na ekranie</summary>*/
    public int menuYPosition;

    /**<summary>Czy menu ma miec pozioma orientacje</summary>*/
    public bool horizontalOrientation;

    /* rozmiary przyciskow */
    /**<summary>Szerokosc przyciskow-dzieci</summary>*/
    public int buttonWidth;
    /**<summary>Wysokosc przyciskow-dzieci</summary>*/
    public int buttonHeight;

    /**<summary>Odstep miedzy przyciskami</summary>*/
    public int buttonDistance;
    /**<summary>Kontroler nasluchujacy przyciski</summary>*/
    public Controller listener;

    /**<summary>Tablica przechowujaca informacje nt. przyciskow</summary>*/
    Button[] buttons;

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */

    /** <summary>Funkcja przygotowujaca menu. Wywolywana na poczatku istnienia obiektu</summary> */
    public void Start()
    {
        int xPos = menuXPosition, yPos = menuYPosition; //pozycja aktutalnie tworzonego przycisku
        int xHandle = 0, yHandle = 0; //modyfikatory zalezne od punktu osadzenia 
        
        /* ustawienie modyfikatorow */
        switch(verticalHandle)
        {
        case 1: yHandle = 0; break;
        case 0: yHandle = Screen.height / 2; break;
        case -1: yHandle = Screen.height; break;
        }
        switch(horizontalHandle)
        {
        case 1: xHandle = Screen.width; break;
        case 0: xHandle = Screen.width / 2; break;
        case -1: xHandle = 0; break;
        }
        
        buttons = GetComponentsInChildren<Button>();
        for(int i = 0; i < buttons.Length; ++i)
        {
            if(horizontalOrientation)
                xPos = menuXPosition + buttons[i].order * (buttonWidth + buttonDistance);
            else
                yPos = menuYPosition + buttons[i].order * (buttonHeight + buttonDistance);
            
            /* generowanie pozycji przycisku */
            buttons[i].X = xPos + xHandle;
            buttons[i].Y = yPos + yHandle;
            buttons[i].Width = buttonWidth;
            buttons[i].Height = buttonHeight;
        }
    }
}
