using UnityEngine;


//tworzy przykladowa mape (siec drog)
public class MapExample : MonoBehaviour 
{
    Map map;

    // Use this for initialization
    void Start() 
    {
        map = (Map)GameObject.FindGameObjectWithTag("Map").GetComponent("Map");

        if(map != null)
        {
            /*map.AddCrossroads(new Vector2(-4, 8));
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

            map.AllCrossroads[new Vector2(-4, 8)].CityRegion = Region.Residential;
            map.AllCrossroads[new Vector2(-4, 4)].CityRegion = Region.Residential;
            map.AllCrossroads[new Vector2(4, 8)].CityRegion = Region.Residential;
            map.AllCrossroads[new Vector2(-4, 0)].CityRegion = Region.Residential;
            map.AllCrossroads[new Vector2(4, -8)].CityRegion = Region.Industrial;
            map.AllCrossroads[new Vector2(8, -8)].CityRegion = Region.Industrial;*/

            map.AddCrossroads(new Vector2(-4, 8));
            map.AddCrossroads(new Vector2(6, 8));
            map.AddCrossroads(new Vector2(-6, 6));
            map.AddCrossroads(new Vector2(6, 6));
            map.AddCrossroads(new Vector2(8, 6));
            map.AddCrossroads(new Vector2(-4, 4));
            map.AddCrossroads(new Vector2(4, 4));
            map.AddCrossroads(new Vector2(-6, 2));
            map.AddCrossroads(new Vector2(6, 2));
            map.AddCrossroads(new Vector2(-4, 0));
            map.AddCrossroads(new Vector2(0, 0));
            map.AddCrossroads(new Vector2(4, 0));
            map.AddCrossroads(new Vector2(-6, -2));
            map.AddCrossroads(new Vector2(6, -2));
            map.AddCrossroads(new Vector2(-4, -4));
            map.AddCrossroads(new Vector2(4, -4));
            map.AddCrossroads(new Vector2(8, -4));
            map.AddCrossroads(new Vector2(-6, -6));
            map.AddCrossroads(new Vector2(6, -6));

            map.AddRoad(new Vector2(-4, 8), new Vector2(6, 8));
            map.AddRoad(new Vector2(-6, 6), new Vector2(-6, 2));
            map.AddRoad(new Vector2(-6, -2), new Vector2(-6, -6));
            map.AddRoad(new Vector2(-4, 4), new Vector2(-4, 0));
            map.AddRoad(new Vector2(-4, 0), new Vector2(-4, -4));
            map.AddRoad(new Vector2(4, 4), new Vector2(4, 0));
            map.AddRoad(new Vector2(4, 0), new Vector2(4, -4));
            map.AddRoad(new Vector2(6, 6), new Vector2(6, 2));
            map.AddRoad(new Vector2(6, -2), new Vector2(6, -6));
            map.AddRoad(new Vector2(8, 6), new Vector2(8, -4));
            map.AddRoad(new Vector2(-6, 6), new Vector2(-4, 4));
            map.AddRoad(new Vector2(-4, 4), new Vector2(0, 0));
            map.AddRoad(new Vector2(0, 0), new Vector2(4, -4));
            map.AddRoad(new Vector2(4, -4), new Vector2(6, -6));
            map.AddRoad(new Vector2(-6, 2), new Vector2(-4, 0));
            map.AddRoad(new Vector2(4, 0), new Vector2(6, -2));
            map.AddRoad(new Vector2(6, 6), new Vector2(4, 4));
            map.AddRoad(new Vector2(4, 4), new Vector2(0, 0));
            map.AddRoad(new Vector2(0, 0), new Vector2(-4, -4));
            map.AddRoad(new Vector2(-4, -4), new Vector2(-6, -6));
            map.AddRoad(new Vector2(6, 2), new Vector2(4, 0));
            map.AddRoad(new Vector2(-4, 0), new Vector2(-6, -2));
            map.AddRoad(new Vector2(-6, 6), new Vector2(-4, 8));
            map.AddRoad(new Vector2(6, 8), new Vector2(8, 6));
            map.AddRoad(new Vector2(8, -4), new Vector2(6, -6));

            map.AllCrossroads[new Vector2(6, 6)].CityRegion = Region.Residential;
            map.AllCrossroads[new Vector2(4, 4)].CityRegion = Region.Residential;
            map.AllCrossroads[new Vector2(6, 2)].CityRegion = Region.Residential;
            map.AllCrossroads[new Vector2(6, -2)].CityRegion = Region.Residential;
            map.AllCrossroads[new Vector2(4, -4)].CityRegion = Region.Residential;
            map.AllCrossroads[new Vector2(6, -6)].CityRegion = Region.Residential;
            map.AllCrossroads[new Vector2(-6, 6)].CityRegion = Region.Industrial;
            map.AllCrossroads[new Vector2(-4, 4)].CityRegion = Region.Industrial;
            map.AllCrossroads[new Vector2(-6, 2)].CityRegion = Region.Industrial;
            map.AllCrossroads[new Vector2(-6, -2)].CityRegion = Region.Industrial;
            map.AllCrossroads[new Vector2(-4, -4)].CityRegion = Region.Industrial;
            map.AllCrossroads[new Vector2(-6, -6)].CityRegion = Region.Industrial;
        }
        else
            Debug.LogError("przykladowa mapa niezaladowana. nie odnaleziono obiektu Map.");
    }
}
