using UnityEngine;
using System.Collections;

/* Funkcjonalnosc przycisku pauzujacego i wznawiajacego symulacje */
public class PauseButton : Button
{
	/* szybkosc symulacji przed pauza */
	private float simulationSpeedBefore;

	public override void OnClick()
	{
		if(Time.timeScale != 0)
		{
			simulationSpeedBefore = Time.timeScale;
			Time.timeScale = 0;
		}
		else
			Time.timeScale = simulationSpeedBefore;

	}
}
