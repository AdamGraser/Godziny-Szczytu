using System;
using System.Collections;
using System.IO;
using UnityEngine;

/* agent, ktory przemieszcza sie po miescie w sposob inteligentny */
[Serializable()]
public class AgentS : AgentD
{
    /* wymagane do obslugi inteligentnego GPS */
    private SmartGps smartGps;
    private float startTime; //czas, w ktorym rozpoczeto przemierzac nowy odcinek drogi
    private float endTime; //czas w ktorym skonczono przemierzac aktualny odcinek drogi
    private Vector2 beginningCross; //skrzyzowanie, rozpoczynajace konretny odcinek trasy

    /* zycie agenta */
    private bool goHome; //czy ma teraz jechac do domu

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */

    protected override void Awake()
    {
        base.Awake();

        smartGps = new SmartGps();
        startTime = 0;
        endTime = 0;
        beginningCross = Vector2.zero;
        goHome = true;
    }

    protected override void Start()
    {
        //schowaj sie pod mape
        base.Finish();
    }

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

    /* coroutine. agent rozpoczyna podroz z domu do pracy */
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
                }
            }

            yield break;
        }
    }

    /* coroutine. rozpoczyna podroz z pracy do domu */
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

                    Debug.Log("start:" + startPosition + " real: " + transform.position);
                }
            }

            yield break;
        }
    }

    /* ***********************************************************************************
     *          FUNKCJE ZWIAZANE Z ROZPOCZECIEM, KONTYNUACJA I ZAKONCZENIEM JAZDY
     * *********************************************************************************** */

    /* oblicza poczatkowa pozycje i kat. wyznacza trase do celu 
     * start - startowe skrzyzowanie 
     * end - skrzyzowanie docelowe */
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

    /* konczy podroz. znika pod ziemie i tam czeka az bedzie znow mogl wyjechac */
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

    /* wprawia agenta S w zycie */
    public void Live()
    {
        StartCoroutine("GoWork");
    }

    /* ***********************************************************************************
     *                   FUNKCJE ZWIAZANE Z ZAPISEM I ODCZYTEM Z PLIKU
     * *********************************************************************************** */

    /* zapisuje gps do wskazanego pliku 
     * stream - strumien, w ktorym ma zosstac zapisany GPS*/
    public void SaveGPS(FileStream stream)
    {
        smartGps.SaveToFile(stream);
    }

    /* laduje gps ze wskazanego pliku
     * stream - strumien, z ktorego ma zostac zaladowany GPS
     * map - mapa */
    public void LoadGPS(FileStream stream, Map map)
    {
        smartGps.LoadFromFile(stream, map);
    }

    /* ***********************************************************************************
     *                                     POZOSTALE
     * *********************************************************************************** */

    /* laduje zegar */
    public void LoadClock(Clock clock)
    {
        this.clock = clock;
    }

    /* laduje mape do GSP */
    public void LoadGPSMap(Map map)
    {
        smartGps.LoadMap(map);
    }
}