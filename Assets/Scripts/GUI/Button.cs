using UnityEngine;
using System.Collections;

/* klasa buttona */
public class Button : MonoBehaviour 
{
	/* tekstura (ikona) przycisku */
	public Texture2D icon;
    /* kolejnosc wyswietlania. 
     * Powinna zawierac wartosc od 0 rosnaco */
    public int order;

	/* nasluchujacy kontroler, ktory bedzie wykonywal operacje po wcisnieciu przycisku */ 
	private Controller listener;
	public Controller Listener {set; get;}

	/* typ eventu (jego nazwa), ktora bedzie przesylana do listenera */
	public Controller.Event eventName;

	/* Rysuje przycisk. 
	 * rect - pozycja i wymiary przycisku. 
	 * listener - kontroller nasluchujacy nacisniecie przycisku 
	 * Zwraca true, jesli wszystko pojdzie dobrze lub false, gdy cos nawali */
	public bool Prepare(Rect rect, Controller listener)
	{
		Listener = listener;
		return GUI.Button(new Rect(rect.x, rect.y, rect.width, rect.height), new GUIContent(icon));
	}

	/* Akcja, ktora zostanie wykonana po nacisnieciu przycisku */
	virtual public void OnClick()
	{
		Listener.LastEvent = eventName;
	}
}
