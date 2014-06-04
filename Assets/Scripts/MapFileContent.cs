using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

/**<summary>Klasa wykorzystywana do zapisu i odczytu z pliku danych nt. mapy</summary>*/
[Serializable()]
public class MapFileContent : ISerializable
{
    /**<summary>Pozycje kolejny skrzyzowan</summary>*/
    public List<Vector2Serializable> CrossroadsPositionList { get; set; }
    /**<summary>Regiony (strefy miejskie), pod ktore podlegaja kolejne skrzyzowania</summary>*/
    public List<Region> CrossroadRegionList { get; set; }
    /**<summary>Pozycje skrzyzowan, ktore sa poczatkami kolejnych drog</summary>*/
    public List<Vector2Serializable> RoadStartList { get; set; }
    /**<summary>Pozycje skrzyzowan, ktore sa koncami kolejnych drog</summary>*/
    public List<Vector2Serializable> RoadEndList { get; set; }

    /**<summary>Konstruktor na potrzeby Serializable - stosowac podczas zapisu</summary>*/
    public MapFileContent()
    {
        CrossroadsPositionList = new List<Vector2Serializable>();
        CrossroadRegionList = new List<Region>();
        RoadStartList = new List<Vector2Serializable>();
        RoadEndList = new List<Vector2Serializable>();
    }

    /**<summary>Kontruktor</summary>
     * <param name="map">Mapa, z ktorej wyciaga informacje</param> */
    public MapFileContent(Map map)
    {
        CrossroadsPositionList = new List<Vector2Serializable>();
        CrossroadRegionList = new List<Region>();
        RoadStartList = new List<Vector2Serializable>();
        RoadEndList = new List<Vector2Serializable>();

        foreach(var c in map.AllCrossroads)
        {
            CrossroadsPositionList.Add(new Vector2Serializable(c.Value.LogicPosition));
            CrossroadRegionList.Add(c.Value.CityRegion);
        }

        foreach(var r in map.AllRoads)
        {
            RoadStartList.Add(new Vector2Serializable(r.Value.Start.LogicPosition));
            RoadEndList.Add(new Vector2Serializable(r.Value.End.LogicPosition));
        }
    }

    /**<summary>Kontruktor</summary>
     * <param name="info">Plik, z ktorego czytamy dane (?)</param>
     * <param name="ctxt">Kontekst (?)</param>*/
    public MapFileContent(SerializationInfo info, StreamingContext ctxt)
    {
        CrossroadsPositionList = (List<Vector2Serializable>)info.GetValue("CrossroadsPositionList", typeof(List<Vector2Serializable>));
        CrossroadRegionList = (List<Region>)info.GetValue("CrossroadRegionList", typeof(List<Region>));
        RoadStartList = (List<Vector2Serializable>)info.GetValue("RoadStartList", typeof(List<Vector2Serializable>));
        RoadEndList = (List<Vector2Serializable>)info.GetValue("RoadEndList", typeof(List<Vector2Serializable>));
    }

    /**<summary>Zapisuje dane do pliku wyznaczonego przez info</summary>
     * <param name="info">Plik, do ktorego zapisujemy dane (?)</param>
     * <param name="ctxt">Kontekst (?)</param>*/
    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("CrossroadsPositionList", CrossroadsPositionList);
        info.AddValue("CrossroadRegionList", CrossroadRegionList);
        info.AddValue("RoadStartList", RoadStartList);
        info.AddValue("RoadEndList", RoadEndList);
    }

}

