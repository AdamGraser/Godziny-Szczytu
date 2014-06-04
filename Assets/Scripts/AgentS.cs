using System.Collections;
using System.IO;
using UnityEngine;

/**<summary>Klasa reprezentujaca Agenta S, przemiejszczajacego sie po miejsce w sposob inteligentny</summary>*/
public class AgentS : AgentD
{
    /* wymagane do obslugi inteligentnego GPS */
    /**<summary>Inteligentny GPS</summary>*/
    private SmartGps smartGps;
    /**<summary>Czas, w ktorym zaczeto pokonywac konrketny odcinek na trasie</summary>*/
    private float startTime;
    /**<summary>Czas, w ktorym skonczono pokonywac konkretny odcinek na trasie</summary>*/
    private float endTime;
    /**<summary>Skrzyzowanie, ktore jest poczatkiem aktualnego odcinka na trasie</summary>*/
    private Vector2 beginningCross;

    /* zycie agenta */
    /**<summary>Czy agent ma teraz jechac do domu, czy do pracy</summary>*/
    private bool goHome;

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */

    /** <summary>Funkcja przygotowujaca Agenta D do zabawy. Wywolywana na poczatku istnienia obiektu, przed funkcja Start.</summary> */
    protected override void Awake()
    {
        base.Awake();

        smartGps = new SmartGps();
        startTime = 0;
        endTime = 0;
        beginningCross = Vector2.zero;
        goHome = true;
    }

    /** <summary>Funkcja przygotowujaca Agenta D. Wywolywana na poczatku istnienia obiektu, po funkcji Awake.</summary> */
    protected override void Start()
    {
        //schowaj sie pod mape
        isDriving = false;
        transform.Translate(0, -3, 0);
        rigidbody.isKinematic = true;
        model.SetActive(false);
    }

    /** <summary>Funkcja wywolywana przy odpaleniu triggera (tego obiektu lub obiektu, z ktorym Agent sie spotkal)</summary>
     *  <param name="other">Obiekt, z ktorym spotkal sie nasz Agent.</param> */
    protected override void OnTriggerEnter(Collider other)
    {
        //zmierz czas i zapisz go do GPS
        if(other.CompareTag("Crossroads") && other.gameObject.GetInstanceID() != actualCrossID)
        {
            Vector2 crossPos = new Vector2(other.transform.position.x, other.transform.position.z); //skrzyzowanie, na ktorym stoimy

            endTime = Time.time;
            smartGps.RememberTime(beginningCross, crossPos, endTime - startTime, clock.Hour, path.Count == 0 ? true : false);

            startTime = endTime;
            beginningCross = crossPos;
        }

        base.OnTriggerEnter(other);
    }

    /* ***********************************************************************************
     *                 FUNKCJE ZWIAZANE Z 'ZYCIEM CODZIENNYM' (COROUTINES)
     * *********************************************************************************** */

    /**<summary>Coroutine. Sprawdza, czy mozna wyslac agenta do pracy. Jesli tak, to to robi. W przeciwnym wypadku czeka pol sekundy.</summary>*/
    protected override IEnumerator GoWork()
    {
        for(; ;)
        {
            //nie przeszkadzaj agentowi kiedy jezdzi (bo spowoduje wypadek ;) )
            if(!isDriving)
            {
                //przerwa wymagana, by agent w palni sie zaladowal
                yield return new WaitForSeconds(0.5f);

                //gdzie agent ma wyladowac
                CalculateStartMembers(HomePlace, WorkPlace);

                // sprawdz, czy pole, w ktorym nalezy polozyc agenta, jest puste
                //jesli nie, to sprawdz za chwile
                if(!IsPlaceEmpty(startPosition))
                {
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    //Debug.Log("Agent: Jest: " + clock.DayTime + ". Mialem wyjechac o: " + HomeMoveOut + "\n" +
                    //          "Agent: Jade z domu: " + HomePlace.LogicPosition + " do roboty: " + WorkPlace.LogicPosition);
                    transform.position = startPosition;
                    transform.rotation = Quaternion.Euler(0, startAngle, 0);
                    model.SetActive(true);
                    Explode();
                }
            }

            yield break;
        }
    }

    /**<summary>Coroutine. Sprawdza, czy mozna wyslac agenta do domu. Jesli tak, to to robi. W przeciwnym wypadku czeka pol sekundy.</summary>*/
    protected override IEnumerator GoHome()
    {
        for(; ; )
        {
            //przerwa wymagana, by agent w palni sie zaladowal
            yield return new WaitForSeconds(0.5f);

            //nie przeszkadzaj agentowi kiedy jezdzi (bo spowoduje wypadek ;) )
            if(!isDriving)
            {
                //gdzie agent ma wyladowac
                CalculateStartMembers(WorkPlace, HomePlace);

                // sprawdz, czy pole, w ktorym nalezy polozyc agenta, jest puste
                //jesli nie, to sprawdz za chwile
                if(!IsPlaceEmpty(startPosition))
                    yield return new WaitForSeconds(0.5f);
                else
                {
                    //Debug.Log("Agent: Jest: " + clock.DayTime + ". Mialem wyjechac o: " + WorkMoveOut + "\n" +
                    //          "Agent: Jade z pracy: " + WorkPlace.LogicPosition + " do domu: " + HomePlace.LogicPosition);
                    transform.position = startPosition;
                    transform.rotation = Quaternion.Euler(0, startAngle, 0);
                    model.SetActive(true);
                    Explode();
                }
            }

            yield break;
        }
    }

    /* ***********************************************************************************
     *          FUNKCJE ZWIAZANE Z ROZPOCZECIEM, KONTYNUACJA I ZAKONCZENIEM JAZDY
     * *********************************************************************************** */

    /**<summary>Oblicza poczatkowa pozycje i kat dla agenta. Wyznacza trase do celu.</summary>
     * <param name="start">startowe skrzyzowanie</param>
     * <param name="end">skrzyzowanie docelowe</param> */
    protected override void CalculateStartMembers(Vector2 start, Vector2 end)
    {
        rigidbody.isKinematic = false;

        path.AddRange(smartGps.FindPath(start, end, clock.Hour));
        isDriving = true;
        brakeOn = false;
        transform.Translate(0, 3, 0);

        //pierwszym punktem jest skrzyzowanie poczatkowe. nie potrzebujemy go
        NextDestination();

        //obroc sie w kierunku lokalnego skrzyzowania docelowego
        startAngle = GetRotationLookingAt(new Vector3(start.x, 0, start.y), new Vector3(path[0].x, 0, path[0].y));
        //ustaw sie za skrzyzowaniem, na drodze, ktora prowadzi do lokalnego skrzyzowania docelowego
        startPosition = new Vector3(start.x + crossRay / 2f * Mathf.Cos(startAngle * Mathf.Deg2Rad) +
                                    Mathf.Sin(startAngle * Mathf.Deg2Rad) * (crossRay + length),
                                    0,
                                    start.y + crossRay / 2f * -Mathf.Sin(startAngle * Mathf.Deg2Rad) +
                                    Mathf.Cos(startAngle * Mathf.Deg2Rad) * (crossRay + length));

        //dla smart gps
        beginningCross = start;
        startTime = Time.time;

        //wymagane. inaczej actualCrossPos == destination
        NextDestination();
    }

    /**<summary>Konczy podrozagenta. Zakpuje go pod ziemie, gdzie czeka na godzine, by znow sie pokazac na drogach</summary>*/
    protected override void Finish()
    {
        Debug.Log("Finish!");
        base.Finish();

        //od razu wyjedz na miasto
        if(goHome)
            StartCoroutine("GoHome");
        else
            StartCoroutine("GoWork");

        goHome = !goHome;
    }

    /**<summary>Wprawia agenta w zycie (w rutyne jezdzenia tam i z powrotem)</summary>*/
    public void Live()
    {
        StartCoroutine("GoWork");
    }

    /* ***********************************************************************************
     *                   FUNKCJE ZWIAZANE Z ZAPISEM I ODCZYTEM Z PLIKU
     * *********************************************************************************** */

    /**<summary>Zapisuje inteligentny GPS do pliku</summary>
     * <param name="stream">Plik, w ktorym ma byc zapisany GPS</param>*/
    public void SaveGPS(FileStream stream)
    {
        smartGps.SaveToFile(stream);
    }

    /**<summary>Laduje inteligentny GPS z pliku</summary>
     * <param name="stream">Plik, z ktorego ma zostac zaladowany GPS</param>
     * <param name="map">Mapa, potrzebna do poprawnego skonfigurowania GPS po zaladowaniu</param>*/
    public void LoadGPS(FileStream stream, Map map)
    {
        smartGps.LoadFromFile(stream, map);
    }

    /* ***********************************************************************************
     *                                     POZOSTALE
     * *********************************************************************************** */

    /**<summary>Laduje zegar</summary>
     *<param name="clock">Zegar do zaladowania</param>*/
    public void LoadClock(Clock clock)
    {
        this.clock = clock;
    }

    /**<summary>Laduje mapy do GPS, gdy ten nie byl ladowany z pliku</summary> 
     * <param name="map">Mapa do zaladowania</param>*/
    public void LoadGPSMap(Map map)
    {
        smartGps.LoadMap(map);
    }
}