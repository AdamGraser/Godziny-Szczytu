using UnityEngine;
using System.Collections;

/* Glowny kontroler. Zawiera rozne tryby dzialania. Od nich zalezy, co bedzie robic klikniecie myszy. */
public abstract class Controller : MonoBehaviour 
{
	/* wszystkie eventy obslugiwane przez pochodne klasy Controller */
	public enum Event 
	{
		None,
		RoadMenuButtonClicked,
		RegionMenuButtonClicked,
		AgentMenuButtonClicked,
		SuperAgentMenuButtonClicked,
		AddRoadButtonClicked,
		RemoveRoadButtonClicked
	}

	/* aktualny tryb pracy, bazujacy na ostatnim evencie*/
	protected Event lastEvent = Event.None;

	/* akcja cyklicznie wykonywana przy konkretnym evencie */
	protected delegate void ActiveAction();
	protected ActiveAction activeAction = null;

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
	
	public abstract void Update();
}
