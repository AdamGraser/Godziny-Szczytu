using UnityEngine;
using System.Collections;

/* Glowny kontroler. Zawiera rozne tryby dzialania. Od nich zalezy, co bedzie robic klikniecie myszy. */
public abstract class Controller : MonoBehaviour 
{
	/* wszystkie eventy obslugiwane przez pochodne klasy Controller */
	public enum Event 
	{
		None,
		/* main controller events */
		RoadMenuButtonClicked,
		RegionMenuButtonClicked,
		AgentMenuButtonClicked,
		/* road controller events */
		AddRoadButtonClicked,
		RemoveRoadButtonClicked,
		/* region controller events */
		NeutralRegionButtonClicked,
		UrbanRegionButtonClicked,
		IndustrialRegionButtonClicked,
		/* agent controller events */
		AddAgentDButtonClicked,
		RemoveAgentDButtonClicked,
		AddAgentSButtonClicked,
		RemoveAgentSButtonClicked,
		/* time controlelr events */
		SlowerButtonClicked,
		PauseButtonClicked,
		FasterButtonClicked
	}

	/* aktualny tryb pracy, bazujacy na ostatnim evencie*/
	protected Event lastEvent = Event.None;

	/* akcja cyklicznie wykonywana przy konkretnym evencie */
	protected delegate void ActiveAction();
	protected ActiveAction activeAction = null;

	/* czy akcja jest jednorazowa, czy ciagnie sie przez caly czas?
	 * np. czy uzytkownik chce stawiac serie drog (continous), 
	 * czy moze chce jednym kilknieciem przyspieszyc symulacje (non-continous) */
	protected bool isActionContinous = true;

	public virtual Event LastEvent
	{
		set 
		{
			lastEvent = value;
		}
		get
		{
			return lastEvent;
		}
	}

	/* wykonuj operacje w zaleznosci od ostatniego zgloszonego eventu */
	void Update()
	{
		if(activeAction != null)
		{
			activeAction();
			if(!isActionContinous)
				activeAction = null;
		}
	}
}
