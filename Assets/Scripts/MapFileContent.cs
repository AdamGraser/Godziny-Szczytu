using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

/* klasa wykorzystywana do zapisu i odczytu z pliku danych nt. mapy */
[Serializable()]
public class MapFileContent : ISerializable
{
    public List<Vector2Serializable> CrossroadsPositionList { get; set; } //pozycje skrzyzowan
    public List<Region> CrossroadRegionList { get; set; } //regiony skrzyzowan
    public List<Vector2Serializable> RoadStartList { get; set; } //poczatki drog
    public List<Vector2Serializable> RoadEndList { get; set; } //konce drog

    /* konstruktor na potrzeby Serializable - stosowac podczas zapisu */
    public MapFileContent()
    {
        CrossroadsPositionList = new List<Vector2Serializable>();
        CrossroadRegionList = new List<Region>();
        RoadStartList = new List<Vector2Serializable>();
        RoadEndList = new List<Vector2Serializable>();
    }

    /* kontruktor pobierajacy agenta
     * map - mapa, z ktorej pobrac informacje */
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

    /* kontruktor wymagany dla interfejsu Serializable
     * info - plik, z ktorego czytamy (?) */
    public MapFileContent(SerializationInfo info, StreamingContext ctxt)
    {
        CrossroadsPositionList = (List<Vector2Serializable>)info.GetValue("CrossroadsPositionList", typeof(List<Vector2Serializable>));
        CrossroadRegionList = (List<Region>)info.GetValue("CrossroadRegionList", typeof(List<Region>));
        RoadStartList = (List<Vector2Serializable>)info.GetValue("RoadStartList", typeof(List<Vector2Serializable>));
        RoadEndList = (List<Vector2Serializable>)info.GetValue("RoadEndList", typeof(List<Vector2Serializable>));
    }

    /* dodaje do pliku skladowe klasy 
     * info - plik, do ktorego zapisujemy (?) */
    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("CrossroadsPositionList", CrossroadsPositionList);
        info.AddValue("CrossroadRegionList", CrossroadRegionList);
        info.AddValue("RoadStartList", RoadStartList);
        info.AddValue("RoadEndList", RoadEndList);
    }

}

