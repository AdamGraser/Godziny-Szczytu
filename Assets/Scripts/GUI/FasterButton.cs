using UnityEngine;
using System.Collections;

/* Funkcjonalnosc przycisku przyspieszajacego symulacje */
public class FasterButton : Button
{
	public override void OnClick()
	{
		Time.timeScale *= 2;
	}
}
