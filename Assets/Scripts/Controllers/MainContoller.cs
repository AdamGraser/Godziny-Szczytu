using UnityEngine;
using System.Collections;

/* obsluguje glowne akcje zwiazane z uzytkownikiem (np. uzytkowanie menu) 
 * Tworzy i usuwa inne kontrolery */
public class MainContoller : Controller 
{
	/* prefaby kontrolerow */
	public GameObject roadControllerPrefab;
	public GameObject regionControllerPrefab;
	public GameObject AgentControllerPrefab;
	public GameObject SuperAgentControllerPrefab;

	public override Event LastEvent
	{
		/* przyjmuje event oraz - jezeli trzeba - przekazuje go innemu kontrolerowi */
		set
		{
			switch(value)
			{
			case Event.RoadMenuButtonClicked:
				base.LastEvent = value;
				RoadMenuButtonAction();
				break;
			case Event.RegionMenuButtonClicked:
				base.LastEvent = value;
				RegionMenuButtonAction();
				break;
			case Event.AgentMenuButtonClicked:
				base.LastEvent = value;
				AgentMenuButtonAction();
				break;
			case Event.SuperAgentMenuButtonClicked:
				base.LastEvent = value;
				SuperAgentMenuButtonAction();
				break;
			default:
				if(activeController != null)
					activeController.LastEvent = value;
				break;
			}
		}

		get
		{
			return base.LastEvent;
		}
	}

	/* aktywny kontroller, ktory przyjmuje aktualnie zdarzenia nieobslugiwane przez MainController */
	private Controller activeController;

	/* ustawia nowy aktywny kontroler. Nie mozna wykorzystywac tej wlasciwosci do odczytywania */
	private GameObject ActiveController
	{
		/* niszczy stary aktywny kontroler i na jego miejsce tworzy nowy */
		set
		{
			if(activeController != null)
				Destroy(activeController);

			GameObject controller = (GameObject)Instantiate(value);
			activeController = controller.GetComponent<Controller>();
		}
	}

	/* -------------------- WLASCIWE FUNKCJE -------------------- */
		
	/* wykonuj operacje w zaleznosci od ostatniego zgloszonego eventu.
	 * aktualnie nie ma zadnych akcji, ktore trzeba by wykonywac cyklicznie */
	public override void Update()
	{
	}

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

	}

	/* obsluguje nacisniecie przycisku AgentMenuButton. */
	private void AgentMenuButtonAction()
	{
		
	}

	/* obsluguje nacisniecie przycisku SuperAgentMenuButton. */
	private void SuperAgentMenuButtonAction()
	{
		
	}

}
