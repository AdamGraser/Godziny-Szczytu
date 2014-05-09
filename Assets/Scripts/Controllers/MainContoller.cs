using UnityEngine;
using System.Collections;

/* obsluguje glowne akcje zwiazane z uzytkownikiem (np. uzytkowanie menu) 
 * Tworzy i usuwa inne kontrolery */
public class MainContoller : Controller 
{
	/* prefaby kontrolerow */
	public GameObject roadControllerPrefab;
	public GameObject regionControllerPrefab;
	public GameObject agentControllerPrefab;

	public override Event LastEvent
	{
		/* przyjmuje event oraz - jezeli trzeba - przekazuje go innemu kontrolerowi */
		set
		{
			switch(value)
			{
			case Event.RoadMenuButtonClicked:
				base.LastEvent = value;
				activeAction = RoadMenuButtonAction;
				break;
			case Event.RegionMenuButtonClicked:
				base.LastEvent = value;
				activeAction = RegionMenuButtonAction;
				break;
			case Event.AgentMenuButtonClicked:
				base.LastEvent = value;
				activeAction = AgentMenuButtonAction;
				break;
			default:
				Debug.LogWarning("Przyjeto nieobslugiwane zdarzenie");
				break;
			}
		}

		get
		{
			return base.LastEvent;
		}
	}

	/* aktywny kontroller, ktory przyjmuje aktualnie zdarzenia nieobslugiwane przez MainController */
	private GameObject activeController;

	void Start()
	{
		isActionContinous = false;
	}

	/* ustawia nowy aktywny kontroler. Nie mozna wykorzystywac tej wlasciwosci do odczytywania */
	private GameObject ActiveController
	{
		/* niszczy stary aktywny kontroler i na jego miejsce tworzy nowy */
		set
		{
			if(activeController != null)
				Destroy(activeController);

			activeController = (GameObject)Instantiate(value);
		}
	}

	/* -------------------- WLASCIWE FUNKCJE -------------------- */

	/* obsluguje nacisniecie przycisku RoadMenuButton. 
	 * tworzy i ustawia RoadController jako aktywny.
	 * niszczy inne aktywne kontrolery */
	private void RoadMenuButtonAction()
	{
		ActiveController = roadControllerPrefab;
	}

	/* obsluguje nacisniecie przycisku RegionMenuButton. */
	private void RegionMenuButtonAction()
	{
		ActiveController = regionControllerPrefab;
	}

	/* obsluguje nacisniecie przycisku AgentMenuButton. */
	private void AgentMenuButtonAction()
	{
		ActiveController = agentControllerPrefab;
	}


}
