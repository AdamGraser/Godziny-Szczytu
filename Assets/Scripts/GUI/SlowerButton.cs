using UnityEngine;
using System.Collections;

/* Funkcjonalnosc przycisku spowalaniajacego czas symulacji */
public class SlowerButton : Button
{
	public override void OnClick()
	{
		Time.timeScale /= 2;
	}
}
