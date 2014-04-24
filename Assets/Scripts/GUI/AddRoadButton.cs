using UnityEngine;
using System.Collections;

/* Funkcjonalnosc przycisku dodajacego droge */
public class AddRoadButton : Button
{
	public override void OnClick()
	{
		Listener.LastEvent = Controller.Event.AddRoadButtonClicked;
	}
}
