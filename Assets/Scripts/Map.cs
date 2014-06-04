using System.Collections.Generic;
using UnityEngine;

/**<summary>Klasa przechowujaca informacje nt. skrzyzowan i drog</summary> */
public class Map : MonoBehaviour
{
    /**<summary>Prefab skrzyzowania</summary>*/
    public GameObject crossroadsPrefab;
    /**<summary>Prefab drogi</summary>*/
    public GameObject roadPrefab;

    private Dictionary<Vector2, Crossroads> crossroads;
    /**<summary>Zbior skrzyzowan. Kluczem jest ich logiczna pozycja</summary>*/
    public Dictionary<Vector2, Crossroads> AllCrossroads { get { return crossroads; } } 

    private Dictionary<Vector2, Road> roads;
    /**<summary>Zbior drog. Kluczem jest isc srodkowy punkt</summary>*/
    public Dictionary<Vector2, Road> AllRoads { get { return roads; } }

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */
    /** <summary>Funkcja przygotowujaca mape do zabawy. Wywolywana na poczatku istnienia obiektu.</summary> */
    void Awake()
    {
        crossroads = new Dictionary<Vector2, Crossroads>();
        roads = new Dictionary<Vector2, Road>();
    }

    /* ***********************************************************************************
     *              FUNKCJE PUBLICZNE DODAJACE / USUWAJACE DROGI I SKRZYZOWANIA
     * *********************************************************************************** */

    /**<summary>Dodaje skrzyzowanie na podana pozycje</summary> 
     * <param name="pos">Pozycja logiczna, na ktorej zostanie umieszczone skrzyzowanie</param>*/
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

    /**<summary>Usuwa skrzyzowanie znajdujace sie na podanej pozycji, wraz z polaczonymi z nim drogami</summary> 
     * <param name="pos">Pozycja skrzyzowania, ktore ma zostac usuniete</param>*/
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

    /**<summary>Tworzy droge znajdujaca sie miedzy podanymi pozycjami skrzyzowan. 
     * Nastepnie tworzy polaczenia miedzy nimi (logiczne, nizszy poziom abstrakcji)</summary>
     * <param name="c1Pos">Pozycja skrzyzowania</param>
     * <param name="c2Pos">Pozycja skrzyzowania</param> */
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

    /**<summary>Usuwa droga i polaczenia miedzy skrzyzowaniami, ktore ta droga laczyla</summary> 
     * <param name="pos">Punkt srodkowy drogi, ktora ma zostac usunieta</param>*/
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

    /**<summary>Szuka drogi zawierajacaej podany punkt.</summary>
     <param name="point">Testowany punkt</param>
     <returns>Zwraca referencje do drogi, jesli taka zostala znaleziona. W przeciwnym wypadku zwraca null</returns>*/
    public Road FindRoadContainingPoint(Vector2 point)
    {
        foreach(var r in roads)
        {
            if(Math.IsPointBelongingToSegment(r.Value.Start.LogicPosition, r.Value.End.LogicPosition, point))
                return r.Value;
        }

        return null;
    }

    /**<summary>Szuka drogi, ktora przecina podana droga.</summary>
     * <param name="c1">Jeden z koncow testowanej drogi</param>
     * <param name="c2">Drugi z koncow testowanej drogi</param>
     * <returns>Zwraca referencje do drogi, jesli taka zostala znaleziona. W przeciwnym wypadku zwraca null</returns>*/
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

    /**<summary>Szuka skrzyzowan o danej strefie</summary>
     * <param name="region">Strefa, do ktorej maja byc przypisane skrzyzowania</param>
     * <returns>Zwraca liste znalezionych skrzyzowan</returns>*/
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

    /**<summary>Czysci cala mape</summary>*/
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
     *                                 PRYWATNA KLASA MATH
     *                       Rozszerza funkcjonalnosc klas Math i Mathf
     * *********************************************************************************** */
    /**<summary>Rozszerza funkcjonalnosc klas Math i Mathf</summary>*/
    private class Math
    {
        /**<summary>Oblicza wspolczynnik macierzy 3x3, utworzonej z danych pochodzacych z 3 punktow</summary>
         * <param name="a">Punkt</param>
         * <param name="b">Punkt</param>
         * <param name="c">Punkr</param>
         * <returns>Zwraca obliczony wspolczynnik</returns>*/
        public static float Det(Vector2 a, Vector2 b, Vector2 c)
        {
            return a.x*b.y + b.x*c.y + c.x*a.y - c.x*b.y - a.x*c.y - b.x*a.y;
        }

        /**<summary>Sprawdza, czy punkt C przynalezy do odcinak |AB|</summary>
         * <param name="a">Punkt odcinka</param>
         * <param name="b">Punkt odcinka</param>
         * <param name="c">Testowany punkr</param>
         * <returns>Zwracatrue, jesli punkt C nalezy do |AB|. W przeciwnym wypadku zwraca false</returns>*/
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
