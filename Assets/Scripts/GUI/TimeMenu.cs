using UnityEngine;
using System.Collections;

/* odpowiada za menu zwiazane z czasem symulacji */
public class TimeMenu : Menu
{
	/* rysuje buttony i label */
	public override void OnGUI()
	{
		/* rysuj przyciski */
		base.OnGUI();

		/* rysuj label nad przyciskami */
		string boxText; //tekst wyswietlany
		const int boxWidth = 200;
		const int boxHeight = 30;

		if(Time.timeScale != 0)
			boxText = "Przyspieszenie: x" + Time.timeScale;
		else
			boxText = "Zatrzymano";

		GUI.Box(new Rect((Screen.width - boxWidth) / 2, 
		                 Screen.height + base.menuYPosition - base.buttonDistance - boxHeight, 
			             boxWidth, boxHeight), boxText);
	}
}
