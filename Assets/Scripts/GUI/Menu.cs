using UnityEngine;
using System.Collections;

/* Reprezentuje automatyczne menu.
 * Szuka przyciskow, ktore sa dziecmi tego obiektu i wyswietla */
public class Menu : MonoBehaviour
{
	/* miejsce osadzenia menu na ekranie
	 * Mozliwe wartosci:
	 * 1 - gora (dla verticalHandle); prawo (dla horizontalHandle)
	 * 0 - centrum (dla verticalHandle); centrum (dla horizontalHandle)
	 * -1 - dol (dla verticalHandle); lewo (dla horizontalHandle) */
	public int horizontalHandle;
	public int verticalHandle;
	/* pozycja menu */
	public int menuXPosition;
	public int menuYPosition;
	/* czy menu ma miec orientacje pozioma */
	public bool horizontalOrientation;
	/* rozmiary przyciskow */
	public int buttonWidth;
	public int buttonHeight;
	/* odstep miedzy przyciskami */
	public int buttonDistance;
	/* kontroler odbierajacy zdarzenia od przyciskow */
	public Controller listener;

	/* tablice przechowujace informacje nt. przyciskow */
	Button[] buttons;
	Rect[] buttonsRects;

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
		buttonsRects = new Rect[buttons.Length];
		for(int i = 0; i < buttons.Length; ++i)
		{
			if(horizontalOrientation)
				xPos = menuXPosition + buttons[i].order * (buttonWidth + buttonDistance);
			else
				yPos = menuYPosition + buttons[i].order * (buttonHeight + buttonDistance);
			
			/* generowanie pozycji przycisku */
			buttonsRects[i] = new Rect(xPos + xHandle, yPos + yHandle, buttonWidth, buttonHeight);
		}
	}

	/* wyswietlanie przyciskow */
	public virtual void OnGUI()
	{
		for(int i = 0; i < buttons.Length; ++i)
		{
			if(buttons[i].Prepare(buttonsRects[i], listener))
			{
				buttons[i].OnClick();
			}
				
		}
	}
}
