using UnityEngine;
using System.Collections;

/* Funkcjonalnosc przycisku otwierajacego menu zarzadzania drogami */
public class RoadMenuButton : Button
{
	public override void OnClick()
	{
		Listener.LastEvent = Controller.Event.RoadMenuButtonClicked;
	}
}
