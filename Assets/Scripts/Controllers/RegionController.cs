using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* obsluguje akcje zwiazane z dodawaniem i usuwaniem regionow (stref) */
public class RegionController : Controller 
{
	/* prefaby obiektow oraz pola przechowujace referencje ich obiektow */
	/* podmenu do zarzadzania regionami */
	public GameObject regionMenuPrefab;
	private GameObject regionMenu = null;
	/* znaczniki regionow */
	public GameObject neutralRegionPrefab;
	public GameObject urbanRegionPrefab;
	public GameObject industrialRegionPrefab;

	/* kontener przechowujacy utworzone znaczniki regionow. kluczem sa ich pozycje 2D */
	private Dictionary<Vector2, GameObject> regionSigns;

	/* mapa */
	private Map map;
	
	/* w zaleznosci od eventu przydziela odpowiednia akcje wykonywana cyklicznie */
	public override Event LastEvent
	{
		set
		{
			switch(value)
			{
			case Event.NeutralRegionButtonClicked:
				base.LastEvent = value;
				activeAction = NeutralRegionButtonAction;
				break;
			case Event.UrbanRegionButtonClicked:
				base.LastEvent = value;
				activeAction = UrbanRegionButtonAction;
				break;
			case Event.IndustrialRegionButtonClicked:
				base.LastEvent = value;
				activeAction = IndustrialRegionButtonAction;
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
		regionMenu = (GameObject)Instantiate(regionMenuPrefab);
		Menu menu = regionMenu.GetComponent<Menu>();
		menu.listener = this;

		map = (Map)GameObject.FindGameObjectWithTag("Map").GetComponent("Map");

		isActionContinous = true;

		regionSigns = new Dictionary<Vector2, GameObject>();
		CreateAllRegionSigns();
	}
	
	/* niszczy grid i menu */
	public void OnDestroy()
	{
		GameObject.Destroy(regionMenu);

		foreach(var s in regionSigns)
			GameObject.Destroy(s.Value);
		regionSigns.Clear();
	}
	
	/* resetuje region skrzyzowania i sasiadujacych drog */
	private void NeutralRegionButtonAction()
	{
		RaycastHit hit = new RaycastHit();
		Ray ray = new Ray();
		Vector2 crossPos;
		
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		if(Input.GetMouseButtonDown(0))
		{
			Physics.Raycast(ray, out hit);
			
			crossPos = new Vector2(Mathf.Round(hit.point.x), Mathf.Round(hit.point.z));

			//jezeli trafiono w skrzyzowanie
			if(map.DoCrossroadsExist(crossPos))
			{
				map.ChangeCrossroadsRegion(crossPos, Region.Neutral);
				GameObject.Destroy(regionSigns[crossPos]);
				regionSigns.Remove(crossPos);
				regionSigns.Add(crossPos, (GameObject)Instantiate(neutralRegionPrefab, new Vector3(crossPos.x, 0.01f, crossPos.y), new Quaternion()));
			}
		}
	}
	
	/* ustawia region skrzyzowania na miejski. update'uje regiony sasiadujacych drog */
	private void UrbanRegionButtonAction()
	{
		RaycastHit hit = new RaycastHit();
		Ray ray = new Ray();
		Vector2 crossPos;
		
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		if(Input.GetMouseButtonDown(0))
		{
			Physics.Raycast(ray, out hit);
			
			crossPos = new Vector2(Mathf.Round(hit.point.x), Mathf.Round(hit.point.z));
			
			//jezeli trafiono w skrzyzowanie
			if(map.DoCrossroadsExist(crossPos))
			{
				map.ChangeCrossroadsRegion(crossPos, Region.Urban);
				GameObject.Destroy(regionSigns[crossPos]);
				regionSigns.Remove(crossPos);
				regionSigns.Add(crossPos, (GameObject)Instantiate(urbanRegionPrefab, new Vector3(crossPos.x, 0.01f, crossPos.y), new Quaternion()));
			}
		}
	}
	
	/* ustawia region skrzyzowania na miejski. update'uje regiony sasiadujacych drog */
	private void IndustrialRegionButtonAction()
	{
		RaycastHit hit = new RaycastHit();
		Ray ray = new Ray();
		Vector2 crossPos;
		
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		if(Input.GetMouseButtonDown(0))
		{
			Physics.Raycast(ray, out hit);
			
			crossPos = new Vector2(Mathf.Round(hit.point.x), Mathf.Round(hit.point.z));
			
			//jezeli trafiono w skrzyzowanie
			if(map.DoCrossroadsExist(crossPos))
			{
				map.ChangeCrossroadsRegion(crossPos, Region.Industrial);
				GameObject.Destroy(regionSigns[crossPos]);
				regionSigns.Remove(crossPos);
				regionSigns.Add(crossPos, (GameObject)Instantiate(industrialRegionPrefab, new Vector3(crossPos.x, 0.01f, crossPos.y), new Quaternion()));
			}
		}
	}

	/* tworzy znaczniki dla istniejacych juz regionow i zapelnia nimi slownik */
	private void CreateAllRegionSigns()
	{
		List<Vector2> crossList = map.GetAllCrossroads();
		GameObject sign = null; //oznaczenie, ktore stworzyc

		foreach(var c in crossList)
		{
			switch(map.GetCrossroadsRegion(c))
			{
			case Region.Neutral:
				sign = neutralRegionPrefab;
				break;
			case Region.Urban:
				sign = urbanRegionPrefab;
				break;
			case Region.Industrial:
				sign = industrialRegionPrefab;
				break;
			}

			regionSigns.Add(c, (GameObject)Instantiate(sign, new Vector3(c.x, 0.01f, c.y), new Quaternion()));
		}
	}


}
