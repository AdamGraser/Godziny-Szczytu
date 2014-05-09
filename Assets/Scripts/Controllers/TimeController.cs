using UnityEngine;
using System.Collections;

/* obsluguje akcje zwiazane z czasem symulacji */
public class TimeController : Controller 
{
	/* pozycja labela i rozmiar informujacego o predkosci symulacji */
	private Rect label;
	/* text, ktory informuje o predkosci symulacji */
	private string labelText;

	/* predkosc symulacji przed pauza */
	private float simulationSpeed;

	/* w zaleznosci od eventu przydziela odpowiednia akcje wykonywana cyklicznie */
	public override Event LastEvent
	{
		set
		{
			switch(value)
			{
			case Event.SlowerButtonClicked:
				base.LastEvent = value;
				activeAction = SlowerButtonAction;
				break;
			case Event.PauseButtonClicked:
				base.LastEvent = value;
				activeAction = PauseButtonAction;
				break;
			case Event.FasterButtonClicked:
				base.LastEvent = value;
				activeAction = FasterButtonAction;
				break;
			default:
				Debug.LogWarning("Przyjeto nieobslugiwane zdarzenie");
				break;
			}
		}
	}
	
	/* -------------------- WLASCIWE FUNKCJE -------------------- */
	
	/* wypozycjonuj label nad przyciskami */
	void Start()
	{
		isActionContinous = false;

		label = new Rect((Screen.width - 200) / 2, Screen.height - 40 - 30, 200, 30);
		labelText = "Przyspieszenie: x" + Time.timeScale;
		simulationSpeed = 1;
	}

	/* wyswietla label */
	void OnGUI()
	{
		if(Time.timeScale != 0)
			labelText = "Predkosc: x" + Time.timeScale;
		else
			labelText = "Zatrzymano";
		
		GUI.Box(label, labelText);
	}
	
	/* dspowalnia symulacje */
	private void SlowerButtonAction()
	{
		if(Time.timeScale > 0.125)
		{
			Time.timeScale /= 2;
			simulationSpeed = Time.timeScale;
		}
	}
	
	/* pauzuje lub wznawia symulacje */
	private void PauseButtonAction()
	{
		if(Time.timeScale != 0)
			Time.timeScale = 0;
		else
			Time.timeScale = simulationSpeed;
	}
	
	/* przyspiesza symulacje */
	private void FasterButtonAction()
	{
		if(Time.timeScale < 32)
		{
			Time.timeScale *= 2;
			simulationSpeed = Time.timeScale;
		}
	}
	
}
