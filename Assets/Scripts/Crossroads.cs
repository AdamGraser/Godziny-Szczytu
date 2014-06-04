using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary>Klasa reprezentujaca skrzyzowanie</summary>*/
public class Crossroads : MonoBehaviour 
{
    /* ogolne */
    private Dictionary<Vector2, Crossroads> connectedCrossroads;
    /**<summary>Skrzyzowania polaczone z tym skrzyzowaniem</summary>*/
    public Dictionary<Vector2, Crossroads> ConnectedCrossroads { get { return connectedCrossroads; } }
    /**<summary>Strefa, do ktorej nalezy to skrzyzowanie</summary>*/
    public Region CityRegion { set; get; } //region (strefa), do ktorej nalezy dane skrzyzowanie
    /**<summary>Pozycja logiczna. odzworowuje wspolrzedne: X i Z. Bezposrednio powiazana z rzeczywista pozycja.
     * Zmiana pozycji logicznej skutkuje zmiana pozycji rzeczywistej</summary> */
    public Vector2 LogicPosition
    {
        get
        {
            return new Vector2(transform.position.x, transform.position.z);
        }

        set
        {
            transform.position = new Vector3(value.x, transform.position.y, value.y);
        }
    }
    /**<summary>Rzeczywista pozycja na swiecie</summary>*/
    public Vector3 RealPosition 
    { 
        set 
        {
            transform.position = value;
        } 
        
        get 
        {
            return transform.position;
        } 
    } 

    /* zwiazane z dopuszczaniem ruchu (otwieraniem drog) */
    /**<summary>Prefab sygnalizacji swietlnej</summary>*/
    public GameObject trafficLightPrefab;
    /**<summary>Czas (w sekundach), przez ktory ma sie palic zielone swiatlo</summary>*/
    public float greenLightTime;
    /**<summary>Czas (w sekundach) miedzy zaapaleniem sie zielonego swiatla i zgasnieciem zielonego dla innej drogi</summary>*/
    public float intertime;
    /**<summary>Czy nalezy chwile poczekac, zanim zostanie zapalone zielone swiatlo</summary>*/
    private bool isWaitingTime;
    /**<summary>Sygnalizacja swietlna</summary>*/
    private Dictionary<Crossroads, TrafficLight> lights;
    /**<summary>Kolejnosc zapalania sie zielonych swiatel dla drog</summary>*/
    private List<TrafficLight> greenLightOrder;
    /**<summary>Czy aktualnie pali sie zielone swiatlo</summary>*/
    private int greenLight;

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */

    /** <summary>Funkcja przygotowujaca skrzyzowanie do dzialania</summary> */
    void Awake()
    {
        CityRegion = Region.Neutral;
        connectedCrossroads = new Dictionary<Vector2, Crossroads>();
        lights = new Dictionary<Crossroads, TrafficLight>();
        greenLightOrder = new List<TrafficLight>();
        greenLight = 0;
        isWaitingTime = false;
    }

    /* ***********************************************************************************
     *                                     COROUTINES
     * *********************************************************************************** */

    /** <summary>Coroutine. Zamyka aktualnie otwarta droge i otwiera nastepna</summary> */
    private IEnumerator OpenNextRoad()
    {
        while(lights.Count > 2) //jezeli skrzyzowanie laczy dwa inne, to zachowuje sie jak zakret
        {
            if(lights.Count > greenLight)
                greenLightOrder[greenLight].ActivateRedLight();

            if(isWaitingTime)
            {
                isWaitingTime = false;

                yield return new WaitForSeconds(intertime);
            }
            else
            {
                greenLight = (++greenLight) % lights.Count;
                greenLightOrder[greenLight].ActivateGreenLight();

                isWaitingTime = true; //poczekaj chwile przed otwarciem nowej drogi

                yield return new WaitForSeconds(greenLightTime);
            }
        }
    }

    /* ***********************************************************************************
     *                                 FUNKCJE PUBLICZNE
     * *********************************************************************************** */

    /** <summary>Laczy nasze skrzyzowanie z podanym (ednak nie laczy podanego z naszym!</summary>
     <param name="cross">Skrzyzownie, ktora ma zostac polaczone z naszym.</param>*/
    public void Connect(Crossroads cross)
    {
        TrafficLight light = ((GameObject)Instantiate(trafficLightPrefab)).GetComponent<TrafficLight>();

        connectedCrossroads.Add(cross.LogicPosition, cross);

        light.Owner = this;
        light.SourceCrossroads = cross;
        lights.Add(cross, light);
        greenLightOrder.Add(light);

        if(lights.Count == 3) //skrzyzowanie nie pelni juz rozli zwyklego 'zakretu'
        {
            //aktywuj wszystkie sciany
            foreach(var w in lights)
            {
                w.Value.gameObject.SetActive(true);
                w.Value.ActivateRedLight();
            }

            StartCoroutine("OpenNextRoad");
        }
        else if(lights.Count < 3) //jesli skrzyzowanie jest zakretem
            light.gameObject.SetActive(false);
            
    }

    /**<summary>Usuwa polaczenie miedzy naszym skrzyzowaniem a podanym (jednak podane skrzyzowanie wciaz jest polaczone z naszym!)</summary>
     * <param name="cross">Skrzyzowanie, ktora od ktorego ma zostac rozlaczone nasze skrzyzowanie</param>*/
    public void Disconnect(Crossroads cross)
    {
        try
        {
            TrafficLight light = lights[cross];

            greenLightOrder.Remove(light);
            lights.Remove(cross);
            connectedCrossroads.Remove(cross.LogicPosition);

            //jesli skrzyzowanie sluzy jako zakret, deaktywuj sciany
            if(lights.Count == 2)
            {
                foreach(var w in lights)
                {
                    w.Value.gameObject.SetActive(false);
                    w.Value.ActivateRedLight();
                }
                    
            }

            GameObject.Destroy(light.gameObject);

        }
        catch(KeyNotFoundException)
        {
            Debug.LogWarning("Nie znaleziono podanego skrzyzowania " + cross.LogicPosition + ", ktore rzekomo mialo byc polaczone z" + LogicPosition);
        }
    }
}
