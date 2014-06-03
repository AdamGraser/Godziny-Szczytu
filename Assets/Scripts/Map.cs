using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/* Reprezentuje mape w postaci grafu (polaczonych ze soba punktow z okresleniem dlugosci pomiedzy nimi. */
[Serializable()]
public class Map : MonoBehaviour, ISerializable
{
    public GameObject crossroadsPrefab;
    public GameObject roadPrefab;

    private Dictionary<Vector2, Crossroads> crossroads; //zbior skrzyzowan. kluczem jest ich logiczna pozycja
    public Dictionary<Vector2, Crossroads> AllCrossroads { get { return crossroads; } } 
    private Dictionary<Vector2, Road> roads; //zbior drog. kluczem jest ich punkt srodkowy
    public Dictionary<Vector2, Road> AllRoads { get { return roads; } }

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */
    void Awake()
    {
        crossroads = new Dictionary<Vector2, Crossroads>();
        roads = new Dictionary<Vector2, Road>();
    }

    /* ***********************************************************************************
     *              FUNKCJE PUBLICZNE DODAJACE / USUWAJACE DROGI I SKRZYZOWANIA
     * *********************************************************************************** */

    /* dodaje skrzyzowanie na podana pozycje logiczna */
    public void AddCrossroads(Vector2 pos)
    {
        Crossroads cross; //nowoutworzone skrzyzowanie

        if(!crossroads.ContainsKey(pos))
        {
            cross = ((GameObject)Instantiate(crossroadsPrefab)).GetComponent<Crossroads>();
            cross.LogicPosition = pos;
            crossroads.Add(pos, cross);
        }
    }

    /* usuwa skrzyzowanie znajdujace sie na podanej pozycji, wraz z polaczonymi drogami */
    public void RemoveCrossroads(Vector2 pos)
    {
        Crossroads cross; //skrzyzowanie do usuniecia
        List<Vector2> roadsToDelete = new List<Vector2>(); //jedne z dwoch koncow drog, ktore nalezu usunac

        if(crossroads.ContainsKey(pos))
        {
            cross = crossroads[pos];

            //usun drogi miedzy skrzyzowaniami
            foreach(var c in cross.ConnectedCrossroads)
                roadsToDelete.Add(c.Value.LogicPosition);
            foreach(var r in roadsToDelete)
                RemoveRoad((pos + r) / 2);

            //usun samo skrzyzowanie
            crossroads.Remove(pos);
            GameObject.Destroy(cross.gameObject);
        }
    }

    /* tworzy droga znajdujaca sie miedzy podanymi skrzyzowaniami. 
     * nastepnie tworzy polaczenia miedzy nimi (logiczne, nizszy poziom abstrakcji ;) ) 
     * c1Pos, c2Pos - pozycje skrzyzowan, pomiedzy ktorymi ma znajdowac sie droga */
    public void AddRoad(Vector2 c1Pos, Vector2 c2Pos)
    {
        Road road;
        float angle; //obrot drogi
        Crossroads c1;
        Crossroads c2;

        try
        {
            c1 = crossroads[c1Pos];
            c2 = crossroads[c2Pos];

            //skrzyzowania nie moge byc juz polaczone
            if(c1 != null && c2 != null && !c1.ConnectedCrossroads.ContainsKey(c2Pos))
            {
                road = ((GameObject)Instantiate(roadPrefab)).GetComponent<Road>();

                //nadaj punkty koncowe
                road.Start = c1;
                road.End = c2;

                //nadaj odpowiednie polozenie drogi
                road.LogicPosition = (c1Pos + c2Pos) / 2f;

                //nadaj odpowiednia dlugosc
                road.transform.localScale = new Vector3(1, 1, road.Length);

                //obroc pod odpowienim katem
                angle = Mathf.Atan((c1Pos.x - c2Pos.x) / (c1Pos.y - c2Pos.y));
                road.transform.Rotate(0, angle * Mathf.Rad2Deg, 0);

                //dodaj do kolekcji
                roads.Add(road.LogicPosition, road);

                //polacz ze soba logicznie skrzyzowania
                c1.Connect(c2);
                c2.Connect(c1);
            }
        }
        catch(KeyNotFoundException)
        {
            Debug.LogWarning("Nie znaleziono podanego klucza");
        }
    }

    /* usuwa droge i polaczenia miedzy skrzyzowaniani 
     * pos - punkt srodkowy drogi */
    public void RemoveRoad(Vector2 pos)
    {
        Road road;
        Crossroads c1;
        Crossroads c2;

        if(roads.ContainsKey(pos))
        {
            road = roads[pos];
            c1 = crossroads[road.Start.LogicPosition];
            c2 = crossroads[road.End.LogicPosition];

            //usun polaczenie logiczne
            c1.Disconnect(c2);
            c2.Disconnect(c1);

            //usun sama droge
            GameObject.Destroy(road.gameObject);
            roads.Remove(pos);
        }
    }

    /* ***********************************************************************************
     *                             POZOSTALE FUNKCJE PUBLICZNE
     * *********************************************************************************** */

    /* zwraca droge, ktora zawiera w sobie podany punkt.
     * jesli taka drogi nie istnieje, zwraca null */
    public Road FindRoadContainingPoint(Vector2 point)
    {
        foreach(var r in roads)
        {
            if(Math.IsPointBelongingToSegment(r.Value.Start.LogicPosition, r.Value.End.LogicPosition, point))
                return r.Value;
        }

        return null;
    }

    /* znajduje i zwraca droge, ktora przecina podana droge road.
     * zwraca null, jesli nie istnieje zadna taka droga 
     * c1, c2 - konce drogi */
    public Road FindRoadIntersectingRoad(Crossroads c1, Crossroads c2)
    {
        //sprawdzanie kazdej drogi
        foreach(var r in roads)
        {
            //sprawdz, czy odcinki sie przecinaja (zrodlo: http://www.algorytm.org/geometria-obliczeniowa/przecinanie-sie-odcinkow.html )
            if(Math.Det(c1.LogicPosition, c2.LogicPosition, r.Value.Start.LogicPosition) *
               Math.Det(c1.LogicPosition, c2.LogicPosition, r.Value.End.LogicPosition) < 0 &&
               Math.Det(r.Value.Start.LogicPosition, r.Value.End.LogicPosition, c1.LogicPosition) *
               Math.Det(r.Value.Start.LogicPosition, r.Value.End.LogicPosition, c2.LogicPosition) < 0)
            {
                return r.Value;
            }

        }

        return null;
    }

    /* zwraca liste skrzyzowan danego regionu */
    public List<Crossroads> GetCrossroadsListWithRegion(Region region)
    {
        List<Crossroads> list = new List<Crossroads>();

        foreach(var c in crossroads)
        {
            if(c.Value.CityRegion == region)
                list.Add(c.Value);
        }

        return list;
    }

    /* czysci cala mape */
    public void Clear()
    {
        List<Crossroads> crosses = new List<Crossroads>();

        foreach(var c in AllCrossroads)
            crosses.Add(c.Value);

        foreach(var c in crosses)
            RemoveCrossroads(c.LogicPosition);

        AllCrossroads.Clear();
    }


    /* ***********************************************************************************
     *                   FUNKCJE ZWIAZANE Z ZAPISEM I ODCZYTEM Z PLIKU
     * *********************************************************************************** */
    /* kontruktor wymagany dla interfejsu Serializable
     * info - plik, z ktorego czytamy (?) */
    public Map(SerializationInfo info, StreamingContext ctxt)
    {
        LoadFromFile(info, ctxt);
    }

    /* laduje dane z pliku 
     * info - plik, z ktorego pobierane sa dane (?) */
    public void LoadFromFile(SerializationInfo info, StreamingContext ctxt)
    {
        List<Vector2Serializable> crossList = new List<Vector2Serializable>();
        List<Region> crossRegionList = new List<Region>();
        List<Vector2Serializable> roadStartList = new List<Vector2Serializable>();
        List<Vector2Serializable> roadEndList = new List<Vector2Serializable>();
        
        //odzczyt z pliku
        crossList = (List<Vector2Serializable>)info.GetValue("Crossroads", typeof(List<Vector2Serializable>));
        crossRegionList = (List<Region>)info.GetValue("CrossroadsRegions", typeof(List<Region>));
        roadStartList = (List<Vector2Serializable>)info.GetValue("RoadStarts", typeof(List<Vector2Serializable>));
        roadEndList = (List<Vector2Serializable>)info.GetValue("RoadEnds", typeof(List<Vector2Serializable>));

        //czyszczenie mapy
        Clear();

        //tworzenie obiektow
        for(int i = 0; i < crossList.Count; ++i)
        {
            AddCrossroads(crossList[i].Vect);
            AllCrossroads[crossList[i].Vect].CityRegion = crossRegionList[i];
        }

        for(int i = 0; i < roadEndList.Count; ++i)
            AddRoad(roadStartList[i].Vect, roadEndList[i].Vect);
    }

    /* dodaje do pliku skladowe klasy 
     * info - plik, do ktorego zapisujemy (?) */
    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        List<Vector2Serializable> crossList = new List<Vector2Serializable>();
        List<Region> crossRegionList = new List<Region>();
        List<Vector2Serializable> roadStartList = new List<Vector2Serializable>();
        List<Vector2Serializable> roadEndList = new List<Vector2Serializable>();

        foreach(var c in AllCrossroads)
        {
            crossList.Add(new Vector2Serializable(c.Value.LogicPosition));
            crossRegionList.Add(c.Value.CityRegion);
        }
        foreach(var r in AllRoads)
        {
            roadStartList.Add(new Vector2Serializable(r.Value.Start.LogicPosition));
            roadEndList.Add(new Vector2Serializable(r.Value.End.LogicPosition));
        }

        //zapis do pliku
        info.AddValue("Crossroads", crossList);
        info.AddValue("CrossroadsRegions", crossRegionList);
        info.AddValue("RoadStarts", roadStartList);
        info.AddValue("RoadEnds", roadEndList);
    }

    /* ***********************************************************************************
     *                                     POZOSTALE
     * *********************************************************************************** */

    /* zwykly konstruktor */
    public Map()
    {
    }

    /* ***********************************************************************************
     *                                 PRYWATNA KLASA MATH
     *                       Rozszerza funkcjonalnosc klas Math i Mathf
     * *********************************************************************************** */
    private class Math
    {
        /* oblicza wspolczynnik macierzy 3x3, utworzonej z danych pochodzacych z 3 punktow */
        public static float Det(Vector2 a, Vector2 b, Vector2 c)
        {
            return a.x*b.y + b.x*c.y + c.x*a.y - c.x*b.y - a.x*c.y - b.x*a.y;
        }

        /* sprawdza, czy punkt C przynalezy do odcinka |AB| */
        public static bool IsPointBelongingToSegment(Vector2 a, Vector2 b, Vector2 c)
        {
            float det = Det(a, b, c);

            if(det != 0)
                return false;
            else if(Mathf.Min(a.x, b.x) <= c.x && Mathf.Max(a.x, b.x) >= c.x && 
                    Mathf.Min(a.y, b.y) <= c.y && Mathf.Max(a.y, b.y) >= c.y)
            {
                return true;
            }

            return false;
        }
    }    
}
