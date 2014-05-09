using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/* obsluguje akcje zwiazane z dodawaniem i usuwaniem agentow */
public class AgentController : Controller 
{
	/* prefaby obiektow oraz pola przechowujace referencje ich obiektow */
	/* podmenu do zarzadzania agentami */
	public GameObject agentMenuPrefab;
	private GameObject agentMenu = null;
	/* agent D i agent S */
	public GameObject agentDPrefab;
	public GameObject agentSPrefab;

	private Map map;
	/* czy mozna wykonac akcje? potrzebne, gdy potrzebujemy informacje od uzytkownika */
	private bool isActionActive;

	/* GUI */
	/* pozycja i rozmiar napisu informujacego */
	Rect BoxRect;
	/* pozycja i rozmiar tekstowego inputu */
	Rect TextFieldRect;
	/* pozycja i rozmiar przycisku zatwierdzenia */
	Rect ButtonRect;
	/* wartosc wpisana przez uzytkownika */
	private string input;
	
	/* w zaleznosci od eventu przydziela odpowiednia akcje wykonywana cyklicznie */
	public override Event LastEvent
	{
		set
		{
			switch(value)
			{
			case Event.AddAgentDButtonClicked:
				base.LastEvent = value;
				activeAction = AddAgentDButtonAction;
				break;
			case Event.RemoveAgentDButtonClicked:
				base.LastEvent = value;
				activeAction = RemoveAgentDButtonAction;
				break;
			case Event.AddAgentSButtonClicked:
				base.LastEvent = value;
				activeAction = AddAgentSButtonAction;
				break;
			case Event.RemoveAgentSButtonClicked:
				base.LastEvent = value;
				activeAction = RemoveAgentSButtonAction;
				break;
			default:
				Debug.LogWarning("Przyjeto nieobslugiwane zdarzenie");
				break;
			}
		}
	}
	
	/* -------------------- WLASCIWE FUNKCJE -------------------- */
	
	/* wygeneruj grid oraz podmenu */
	public void Start()
	{
		agentMenu = (GameObject)Instantiate(agentMenuPrefab);
		Menu menu = agentMenu.GetComponent<Menu>();
		menu.listener = this;

		map = (Map)GameObject.FindGameObjectWithTag("Map").GetComponent("Map");
		input = "";

		isActionContinous = true;
		isActionActive = false;

		BoxRect = new Rect((Screen.width - 400) / 2, (Screen.height - 100) / 2, 400, 30);
		TextFieldRect = new Rect((Screen.width - 50) / 2, (Screen.height - 100) / 2 + 35, 50, 30);
		ButtonRect = new Rect((Screen.width - 50) / 2, (Screen.height - 100) / 2 + 70, 50, 30);

	}
	
	void OnGUI()
	{
		GUIStyle style = new GUIStyle();
		isActionActive = false;

		if(activeAction == AddAgentDButtonAction)
		{
			style.alignment = TextAnchor.MiddleCenter;
			GUI.Box(BoxRect, "Type number of Agents D to be added:");
			input = GUI.TextField(TextFieldRect, input);

			//jezeli zatwierdzono enterem
			if(GUI.Button(ButtonRect, "OK"))
			{
				AddAgentDButtonAction();
				isActionContinous = false;
				isActionActive = true;
            }
		}
	}

	/* niszczy grid i menu */
	public void OnDestroy()
	{
		GameObject.Destroy(agentMenu);
	}
	
	/* dodaje zwykluch agentow */
	private void AddAgentDButtonAction()
	{
        if(isActionActive)
		{
			int count = 0; //liczba agentow do dodania
			
			try
			{
				count = int.Parse(input);
				
				List<Vector2> urbanCrosses = map.GetCrossroadListWithRegion(Region.Urban);
				List<Vector2> industrialCrosses = map.GetCrossroadListWithRegion(Region.Industrial);

				Debug.Log("uCrosses: " + urbanCrosses.Count + " iCrosses: " + industrialCrosses.Count);

				if(industrialCrosses.Count > 0 && urbanCrosses.Count > 0)
				{
					for(int i = 0; i < count; ++i)
					{
						AddAgentD(urbanCrosses[UnityEngine.Random.Range(0, urbanCrosses.Count - 1)],
						          industrialCrosses[UnityEngine.Random.Range(0, industrialCrosses.Count - 1)]);					
					}
				}
			}
			catch(ArgumentNullException)
			{
				Debug.LogWarning("Nie udalo sie sparsowac int: inptu == null");
            }
            catch(FormatException)
            {
                Debug.LogWarning("Nie udalo sie sparsowac int: zly format");
            }
            catch(OverflowException)
            {
                Debug.LogWarning("Nie udalo sie sparsowac int: przepelnienie");
            }
        }
    }
    
    /* usuwa zwyklego agenta */
	private void RemoveAgentDButtonAction()
	{
		
	}

	/* dodaje agenta S */
	private void AddAgentSButtonAction()
	{
		
	}

	/* usuwa agenta S */
	private void RemoveAgentSButtonAction()
	{
		
	}
    
    /* dodaje agenta do symulacji 
	 * home - miejsce zamieszkania
	 * work - miejsce, w ktorym agent pracuje */
	private void AddAgentD(Vector2 home, Vector2 work)
	{
		GameObject agentObject = (GameObject)Instantiate(agentDPrefab);
		AgentD agent = agentObject.GetComponent<AgentD>();

		agent.HomePlace = home;
		agent.WorkPlace = work;
		agent.WorkStart = UnityEngine.Random.Range(0, Clock.Day);
		agent.WorkEnd = (agent.WorkStart + Clock.Day / 3) % Clock.Day;
		agentObject.transform.position = new Vector3(home.x, agentObject.transform.position.y, home.y);
	}
	
}
