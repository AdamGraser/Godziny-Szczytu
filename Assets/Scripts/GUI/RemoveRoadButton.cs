using UnityEngine;
using System.Collections;

/* Funkcjonalnosc przycisku dodajacego droge */
public class RemoveRoadButton : Button
{
	public override void OnClick()
	{
		Debug.Log("Ikona: " + icon.ToString());
		Listener.LastEvent = Controller.Event.RemoveRoadButtonClicked;
	}
}
