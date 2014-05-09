using UnityEngine;
using System.Collections;

/* obsluguje akcje zwiazane z modyfikacja sieci drog przez uzytkownika */
public class RoadController : Controller 
{
	/* prefaby obiektow oraz pola przechowujace referencje ich obiektow */
	/* podmenu do zarzadzania drogami */
	public GameObject roadMenuPrefab;
	private GameObject roadMenu = null;
	/* grid wyznaczajacy miejsca, na ktorych mozna umieszczac drogi */
	public GameObject gridPrefab;
	private GameObject grid = null;
	/* sietka pokazujaca mozliwe punkty, w ktorych mozna postawic droge */
	public GameObject possibleRoadDirectionsPrefab;
	private GameObject possibleRoadDirections = null;

	/* pozycja skrzyzowania zaznaczonego/utworzonego przez uzytkownika. 
	 * Potrzebne do tworzenia drog */
	private Vector2 crossPos;
	private bool crossSelected;
	/* mapa */
	private Map map;

	/* w zaleznosci od eventu przydziela odpowiednia akcje wykonywana cyklicznie */
	public override Event LastEvent
	{
		set
		{
			switch(value)
			{
			case Event.AddRoadButtonClicked:
				base.LastEvent = value;
				activeAction = AddRoadButtonAction;
				break;
			case Event.RemoveRoadButtonClicked:
				base.LastEvent = value;
				activeAction = RemoveRoadButtonAction;
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
		roadMenu = (GameObject)Instantiate(roadMenuPrefab);
		Menu menu = roadMenu.GetComponent<Menu>();
		menu.listener = this;

		crossSelected = false;

		grid = (GameObject)Instantiate(gridPrefab);

		map = (Map)GameObject.FindGameObjectWithTag("Map").GetComponent("Map");

		isActionContinous = true;
	}

	/* niszczy grid i menu */
	public void OnDestroy()
	{
		map.RemoveLonelyCrossroads();
		Destroy(grid);
		Destroy(roadMenu);
		Destroy(possibleRoadDirections);
	}
		
	/* w miejcu klikniecia tworzy skrzyzowanie i/lub droge */
	private void AddRoadButtonAction()
	{
		RaycastHit hit = new RaycastHit();
		Ray ray = new Ray();
		Vector2 cross2Pos;

		ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if(Input.GetMouseButtonDown(0))
		{
			Physics.Raycast(ray, out hit);

			cross2Pos = new Vector2(Mathf.Round(hit.point.x), Mathf.Round(hit.point.z));
			//jezeli wczesniej zadne skrzyzowanie nie bylo utworzone/zaznaczone, to aktualne nim bedzie
			if(!crossSelected)
			{
				crossPos = cross2Pos;
				map.AddCrossroads(crossPos);

				//utworz siatke wskazujaca kierunki drog
				possibleRoadDirections = (GameObject)Instantiate(possibleRoadDirectionsPrefab);
				possibleRoadDirections.transform.Translate(new Vector3(Mathf.Round(hit.point.x), 0, Mathf.Round(hit.point.z)));

				crossSelected = true;
			}
			//w przeciwynm wypadku utworz droge miedzy skrzyzowaniami
			else
			{
				//pozycja skrzyzowania musi pasowac do tej wskazywanej przez possibleRoadDirections
				if(map.IsCrossroadPlacedOnPossiblePlace(crossPos, cross2Pos))
				{
					map.AddCrossroads(cross2Pos);
					map.AddRoad(crossPos, cross2Pos);
				}

				GameObject.Destroy(possibleRoadDirections);

				crossSelected = false;
			}
		}
	}

	/* usuwa droge z miejsca, w ktorym kliknieto */
	private void RemoveRoadButtonAction()
	{
		RaycastHit hit = new RaycastHit();
		Ray ray = new Ray();
		Vector2 pointClicked; //miejsce w ktorym kliknieto

		//usun ewentualne zanzaczenie
		crossSelected = false;
		GameObject.Destroy(possibleRoadDirections);
		
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		if(Input.GetMouseButtonDown(0))
		{
			Physics.Raycast(ray, out hit);
			pointClicked = new Vector2(Mathf.Round(hit.point.x), Mathf.Round(hit.point.z));

			//jezeli nie usunieto skrzyzowania, moze uzytkownik chcial usunac droge
			if(!map.RemoveCrossroads(pointClicked))
			{
				Vector2[] crossArray = map.FindRoadContainingPoint(pointClicked);
				if(crossArray != null)
					map.RemoveRoad(crossArray[0], crossArray[1]);
			}
		}
	}

}
