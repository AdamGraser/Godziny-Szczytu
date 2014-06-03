using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* klasa reprezentujaca skrzyzowanie */
public class Crossroads : MonoBehaviour 
{
    /* ogolne */
    private Dictionary<Vector2, Crossroads> connectedCrossroads; //skrzyzowania, ktore sasiaduja z tym
    public Dictionary<Vector2, Crossroads> ConnectedCrossroads { get { return connectedCrossroads; } }
    public Region CityRegion { set; get; } //region (strefa), do ktorej nalezy dane skrzyzowanie
    /* pozycja logiczna. odzworowuje wspolrzedne: X i Z. bezposrednio powiazana z rzeczywista pozycja.
     * zmiana pozycji logicznej skutkuje zmiana pozycji rzeczywistej */
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
    /* prawdziwa pozycja na swiecie */
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
    public GameObject trafficLightPrefab; //prefab swiatel
    public float greenLightTime; //czas w sekunbdach, przez ktory pali sie zielone swiatlo
    public float intertime; //czas w sekundach miedzy zgasnieciem zielonego i zapaleniem sie innego zielonego
    private bool isWaitingTime; //czy nalezy chwile poczekac, zanim zapalimy zielone swiatlo
    private Dictionary<Crossroads, TrafficLight> lights; //swiatla prowadzace do danych skrzyzowan
    private List<TrafficLight> greenLightOrder; //kolejnosc zapalania sie zielonych swiatel
    private int greenLight; //aktualnie palace sie zielone swiatlo

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */

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

    /* zamyka aktualnie otwarta droge i otwiera nastepna. */
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

    /* laczy nasze skrzyzowanie z podanym. (jednak nie laczy podanego z naszym!) */
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

    /* usuwa polaczenie miedzy naszym skrzyzowaniem a podanym. (jednak podane skrzyzowanie wciaz jest polaczone z naszym!) */
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
