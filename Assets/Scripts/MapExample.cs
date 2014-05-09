using UnityEngine;
using System.Collections;


//tworzy przykladowa mape (siec drog)
public class MapExample : MonoBehaviour 
{
	Map map;

	// Use this for initialization
	void Start () 
	{
		map = (Map)GameObject.FindGameObjectWithTag("Map").GetComponent("Map");

		if(map != null)
		{
			map.AddCrossroads(new Vector2(-4, 8));
			map.AddCrossroads(new Vector2(-4, 4));
			map.AddCrossroads(new Vector2(-4, 0));
			map.AddCrossroads(new Vector2(-2, -2));
			map.AddCrossroads(new Vector2(0, 4));
			map.AddCrossroads(new Vector2(0, 0));
			map.AddCrossroads(new Vector2(4, 8));
			map.AddCrossroads(new Vector2(4, 0));
			map.AddCrossroads(new Vector2(4, -2));
			map.AddCrossroads(new Vector2(4, -8));
			map.AddCrossroads(new Vector2(8, 8));
			map.AddCrossroads(new Vector2(8, -8));

			map.AddRoad(new Vector2(-4, 8), new Vector2(-4, 4));
			map.AddRoad(new Vector2(-4, 4), new Vector2(-4, 0));
			map.AddRoad(new Vector2(-4, 0), new Vector2(-2, -2));
			map.AddRoad(new Vector2(-4, 4), new Vector2(0, 4));
			map.AddRoad(new Vector2(-4, 8), new Vector2(4, 8));
			map.AddRoad(new Vector2(-4, 8), new Vector2(0, 4));
			map.AddRoad(new Vector2(4, 8), new Vector2(0, 4));
			map.AddRoad(new Vector2(4, 8), new Vector2(8, 8));
			map.AddRoad(new Vector2(0, 4), new Vector2(4, 0));
			map.AddRoad(new Vector2(-2, -2), new Vector2(0, 0));
			map.AddRoad(new Vector2(0, 0), new Vector2(0, 4));
			map.AddRoad(new Vector2(-2, -2), new Vector2(4, -2));
			map.AddRoad(new Vector2(4, 0), new Vector2(4, -2));
			map.AddRoad(new Vector2(0, 0), new Vector2(0, 4));
			map.AddRoad(new Vector2(4, -2), new Vector2(4, -8));
			map.AddRoad(new Vector2(8, 8), new Vector2(8, -8));
			map.AddRoad(new Vector2(4, -8), new Vector2(8, -8));
		}
		else
			Debug.LogError("przykladowa mapa niezaladowana. nie odnaleziono Map.");
	}
}
