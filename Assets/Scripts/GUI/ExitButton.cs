using UnityEngine;
using System.Collections;

/* Funkcjonalnosc przycisku wyjscia */
public class ExitButton : Button
{
	public override void OnClick()
	{
		Application.Quit();
	}
}
