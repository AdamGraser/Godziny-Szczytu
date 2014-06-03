using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentD : MonoBehaviour
{
    /* sposob jazdy */
    public float velocity; //predkosc agenta
    public float sightAngle; //kat widzenia z przodu (odchylenie od kata 0)
    protected bool brakeOn; //czy hamulec jest aktualni uzywany
    protected bool isDriving; //czy aktualnei agent jedzie
    protected HashSet<int> collidingObjects; //ID obiektow uniemozliwiajacyh dalsza jazde
    public float resetTime; //po ilu sekundach stania w miejscu agent ma sie zresetowac
    public float resetVelocity; //maksymalna predkosc, przy ktorej agent moze sie zresetowac
    protected float startResetTime; //czas, w ktorym agent sie zatrzymal
    protected bool isResetTime; //czy pora sie wylaczyc

    /* potrzebne przy skrecie */
    public GameObject frontLeftWheel; //przednie lewe kolo
    public GameObject frontRightWheel; //przednie prawe kolo
    public float crossRay; //promien skrzyzowania
    protected bool isTurning; //czy agent skreca
    protected bool firstFrameTurning; //czy to klatka, w ktorej zaczal skrecac
    protected float destAngle; //kat, pod jakim ma byc obrocony agent po zakonczeniu skrecania
    protected float lastAngle; //kat uzyskany przez agenta w poprzedniej klatce
    protected int actualCrossID; //ID skrzyzowania, na ktorym aktualnie sie znajdujemy
    protected Vector3 arcCenterPos; //srodek luku, po ktorym sie przemieszczamy

    /* skladowe dotyczace sciezki, po ktorej jezdzi agent */
    protected Vector2 destination; //lokalny punkt docelowy
    protected Gps gps; //gieps
    protected List<Vector2> path; //sciezka wyznaczona przez gps

    /* 'zycie agenta' */
    public Vector2 HomePlace { get; set; } //miejsce zamieszkania
    public Vector2 WorkPlace { get; set; } //miejsce pracy
    public float WorkMoveOut { get; set; } //czas wyjazdu z pracy
    public float HomeMoveOut { get; set; } //czas wujazdu z domu
    protected Vector3 startPosition; //miejsce, w ktorym sie pojawi agent, gdy bedzie wyruszal w swiat
    protected float startAngle; //kat, pod ktorym zostanie ustawiony agent na poczatku podrozy
    protected Clock clock; //zegar, wedlug ktorego agent bedzie wyjezdzal z domu lub pracy

    /* inne */
    public float length; //dlugosc samochodu
    public GameObject model; //fizyczny model agenta (wylaczany, gdy agent jest nieaktywny)

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */

    protected virtual void Awake()
    {
        brakeOn = false;
        isDriving = false;
        collidingObjects = new HashSet<int>();
        isTurning = false;
        firstFrameTurning = false;
        isTurning = false;
        destAngle = 0;
        lastAngle = 0;
        actualCrossID = 0;
        arcCenterPos = Vector3.zero;
        destination = Vector2.zero;
        gps = new Gps();
        path = new List<Vector2>();
        HomePlace = Vector2.zero;
        WorkPlace = Vector2.zero;
        WorkMoveOut = 0;
        HomeMoveOut = 0;
        startPosition = Vector3.zero;
        startAngle = 0;
    }

    protected virtual void Start()
    {
        gps.LoadMap((Map)GameObject.FindWithTag("Map").GetComponent("Map"));
        clock = (Clock)GameObject.FindWithTag("Clock").GetComponent("Clock");

        isDriving = true; //na czas odpalana coroutines
        StartCoroutine("GoHome");
        StartCoroutine("GoWork");
        isDriving = false;

        //schowaj sie pod mape
        Finish();
    }

    protected void Update()
    {
        if(isDriving)
        {
            if(isTurning)
                Turn();
            
            if(!brakeOn)
                rigidbody.AddRelativeForce(0, 0, velocity * Time.timeScale);

            ResetIfNeeded();
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        // jesli wjechales na skrzyzowanie, to oblicz, jak nalezy skrecic
        if(other.CompareTag("Crossroads") && other.gameObject.GetInstanceID() != actualCrossID)
        {
            //czy agent skonczyl swoja podroz
            if(!NextDestination())
            {
                brakeOn = true;
                Finish();
            }
            //jesli nie, to skrec
            else
            {
                actualCrossID = other.gameObject.GetInstanceID();
                CalculateTurningMembers(other.transform.position);
            }
        }
        //jesli obiekt, na ktory natrafilismy jest przed nami
        else if(IsObjectInFrontOf(other.transform))
        {
            //jesli to jest agent lub sciana, to sie zatrzymaj
            if(other.CompareTag("Agent") || other.CompareTag("Wall"))
                AddToCollidingObjects(other.GetInstanceID());
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        RemoveFromCollidingObjects(other.GetInstanceID());
    }

    /* ***********************************************************************************
     *                        FUNKCJE POTRZEBNE PRZY SKRECANIU
     * *********************************************************************************** */

    /* wylicza wszystkie potrzebne skladowe, wymagane do wykonania prawidlowego skretu  na skrzyzowaniu
     * crossPos - pozycja skrzyzowania, na ktorym wykonywany bedzie skret */
    protected void CalculateTurningMembers(Vector3 crossPos)
    {
        Vector3 startPos; //pozycja startowa
        Vector3 endPos; //pozycja koncowa (po ukonczeniu wykonywania luku
        Vector3 destPos; //pozycja skrzyzowania docelowego
        float startAngle; //kat (orientacja), pod ktorym jest na poczatku obrocony agent
        float endAngle; //kat (orientacja), pod ktorym bedzie na koncu obrocony agent
        float crossDistance; //odleglsoc miedzy srodkowym punktem skrzyzowania i samochodem

        try
        {
            //Debug.Log("AgentPos: " + transform.position);
            destPos = new Vector3(destination.x, crossPos.y, destination.y);
            startPos = new Vector3(transform.position.x, crossPos.y, transform.position.z);
            startAngle = transform.eulerAngles.y;
            endAngle = GetRotationLookingAt(crossPos, destPos);

            //Debug.Log("crossPos: " + crossPos + " destCrossPos: " + destCrossPos);

            //oblicz pozycje koncowa
            crossDistance = (startPos - crossPos).magnitude; //odleglosc miedzy pozycja skrzyzowania a pozycja agenta
            endPos = new Vector3(crossPos.x + crossRay / 2f * Mathf.Cos(endAngle * Mathf.Deg2Rad) + crossDistance * Mathf.Sin(endAngle * Mathf.Deg2Rad),
                                 0,
                                 crossPos.z + crossRay / 2f * -Mathf.Sin(endAngle * Mathf.Deg2Rad) + crossDistance * Mathf.Cos(endAngle * Mathf.Deg2Rad));

            //Debug.Log("StartPos: " + startPos + " @ " + startAngle + " EndPos: " + endPos + " @ " + endAngle);

            arcCenterPos = GetRotationPoint(startPos, Mathf.Round(startAngle), endPos, Mathf.Round(endAngle));

            //obroc przednie kola do skretu
            frontLeftWheel.transform.LookAt(arcCenterPos);
            frontLeftWheel.transform.Rotate(0, 90f, 0);
            frontRightWheel.transform.LookAt(arcCenterPos);
            frontRightWheel.transform.Rotate(0, 90f, 0);

            isTurning = true;
            destAngle = endAngle;
            lastAngle = endAngle;
            firstFrameTurning = true;

            //Debug.Log("Agent: Jestem na: " + actualCrossPos + ". Zmierzam do: " + destination);
        }
        catch(ArgumentException)
        {
            isTurning = false;
            destAngle = 0;
            lastAngle = 0;
            frontLeftWheel.transform.rotation = new Quaternion();
            frontRightWheel.transform.rotation = new Quaternion();
            firstFrameTurning = false;
        }
    }


    /* zwraca kat (wzgledem osi y), pod jakim musialby byc ustawiony obiekt, 
     * by patrzec w punktu from na punkt at */
    protected float GetRotationLookingAt(Vector3 from, Vector3 at)
    {
        Vector3 pos = transform.position; //aktualna pozycja
        Quaternion angle = transform.rotation; //aktualny kat 
        float angleToReturn = 0; //kat, ktory zostanie zwrocony
        
        transform.position = from;
        transform.LookAt(at);
        angleToReturn = transform.eulerAngles.y;
        
        transform.position = pos;
        transform.rotation = angle;
        
        return angleToReturn;
    }
    
    /* zwraca punkt, wokol ktorego musi sie obracac agent, by skrecic po wyznaczonym luku.
     * startPos - punkt startowy luku
     * startAngle - kat (orientacja) startowy (w stopniach)
     * endPos - punkt koncowy luku
     * endAngle - kat koncowy (w stopniach) 
     * jesli z podanych danych bedzie wynikach, ze agent nie bedzie skrecal, zostanie rzucony wyjatek ArgumentException*/
    protected Vector3 GetRotationPoint(Vector3 startPos, float startAngle, Vector3 endPos, float endAngle)
    {
        //wspolczynniki prostych przechodzacych przez punkt A i B
        float startA;
        float startB;
        float endA;
        float endB;
        //srodek luku (punkt wynikowy)
        Vector3 arcCenter = Vector3.zero;
        //modulo katow start i end
        float startAngleMod;
        float endAngleMod;
        
        //szukamy prostopadlych, nie stycznych
        startAngle = 90 - startAngle + 90;
        endAngle = 90 - endAngle + 90;
        
        startA = (float)Math.Tan(startAngle * Mathf.Deg2Rad);
        startB = startPos.z - startA * startPos.x; 
        endA = (float)Math.Tan(endAngle * Mathf.Deg2Rad);
        endB = endPos.z - endA * endPos.x;
        
        //Debug.Log("sA: " + startA + " sB: " + startB + " eA: " + endA + " eB: " + endB);
        
        if(startA == endA || (float.IsInfinity(startA) && float.IsInfinity(endA))) //jesli proste sa rownolegle
            throw new ArgumentException("Otrzymane proste sa rownolegle");
        
        //oblicz punkt przeciecia prostych
        startAngleMod = startAngle % 180f;
        endAngleMod = endAngle % 180f;
        //Debug.Log("startAngle: " + startAngle + " endAngle: " + endAngleMod);
        if(startAngleMod == 90.0 || startAngleMod == -90.0)
        {
            //Debug.Log("Chosen option 1: startAngle % 180f == 90");
            arcCenter.x = startPos.x;
            arcCenter.z = arcCenter.x * endA + endB;
        } 
        else if(endAngleMod == 90 || endAngleMod == -90.0)
        {
            //Debug.Log("Chosen option 2: endAngle % 180f == 90");
            arcCenter.x = endPos.x;
            arcCenter.z = arcCenter.x * startA + startB;
        }
        else
        {
            //Debug.Log("Chosen option 3");
            arcCenter.x = (endB - startB) / (startA - endA);
            arcCenter.z = arcCenter.x * endA + endB;
        }
        
        arcCenter.y = startPos.y;
        
        //Debug.Log("Caluclated arcCenter: " + arcCenter);
        
        return arcCenter;
    }
    
    /* skrec. jesli wykonano zadany kat, przestan skrecac */
    protected void Turn()
    {
        float actualAngle = transform.eulerAngles.y;
        float absAngleDifference = Mathf.Abs(actualAngle - lastAngle);
        
        //obroc przednie kola do skretu
        frontLeftWheel.transform.LookAt(arcCenterPos);
        frontLeftWheel.transform.Rotate(0, 90f, 0);
        frontRightWheel.transform.LookAt(arcCenterPos);
        frontRightWheel.transform.Rotate(0, 90f, 0);

        //Debug.Log("ID: " + gameObject.GetInstanceID() + " Pos: " + transform.position + " last: " + lastAngle + 
        //    " act: " + actualAngle + " dest: " + destAngle + " diff: " + absAngleDifference);
        //jesli przekroczono kat docelowy, zakoncz skrecanie
        /*if(!firstFrameTurning && //klatka, w ktorej NIE zaczal skrecac (w pierwszej klatce odbija w druga strone, WTF)
           ((!isTurningLeft && destAngle < actualAngle && lastAngle < actualAngle && destAngle > lastAngle) || //skreca w prawo i nie przechodzi przez 0
           (isTurningLeft && destAngle > actualAngle && lastAngle > actualAngle && destAngle < lastAngle) ||  //skreca w lewo i nie przechodzi przez 0
           (!isTurningLeft && destAngle < actualAngle && lastAngle > actualAngle && destAngle < lastAngle) || //skreca w prawo i przechodzi przez 0
           (isTurningLeft && destAngle < actualAngle && lastAngle < actualAngle && destAngle < lastAngle)))   //skreca w lewo i przechodzi przez 0*/
        if(!firstFrameTurning && //w pierwszej ramce jest obliczany lastAngle, niezbedny do dalszych porownan
           (absAngleDifference <= 180 && ((actualAngle < destAngle && destAngle < lastAngle) || (actualAngle > destAngle && destAngle > lastAngle))) ||
           (absAngleDifference > 180 && ((destAngle < actualAngle && destAngle < lastAngle) || (destAngle > actualAngle && destAngle > lastAngle))))
        {
            //przestan skrecac
            isTurning = false;
            frontLeftWheel.transform.rotation = new Quaternion();
            frontRightWheel.transform.rotation = new Quaternion(); 
            firstFrameTurning = false;

            //zastosuj poprawke
            transform.LookAt(new Vector3(destination.x + crossRay / 2f * Mathf.Cos(destAngle * Mathf.Deg2Rad),
                                         transform.position.y,
                                         destination.y + crossRay / 2f * -Mathf.Sin(destAngle * Mathf.Deg2Rad)));

            destAngle = 0;
        }
        
        lastAngle = transform.eulerAngles.y;
        firstFrameTurning = false;
    }

    /* ***********************************************************************************
     *                 FUNKCJE ZWIAZANE Z 'ZYCIEM CODZIENNYM' (COROUTINES)
     * *********************************************************************************** */
    
    /* coroutine. agent rozpoczyna podroz z domu do pracy */
    protected virtual IEnumerator GoWork()
    {
        for(; ; )
        {
            //nie przeszkadzaj agentowi kiedy jezdzi (bo spowoduje wypadek ;) )
            if(!isDriving)
            {
                //gdzie agent ma wyladowac
                CalculateStartMembers(HomePlace, WorkPlace);

                // sprawdz, czy pole, w ktorym nalezy polozyc agenta, jest puste
                //jesli nie, to sprawdz za chwile
                if(!IsPlaceEmpty(startPosition))
                    yield return new WaitForSeconds(0.5f);
                else
                {
                    //Debug.Log("Agent: Jest: " + clock.DayTime + ". Mialem wyjechac o: " + HomeMoveOut + "\n" +
                    //          "Agent: Jade z domu: " + HomePlace.LogicPosition + " do roboty: " + WorkPlace.LogicPosition);

                    transform.position = startPosition;
                    transform.rotation = Quaternion.Euler(0, startAngle, 0);
                    model.SetActive(true);
                }                
            }

            if(clock.DayTime > HomeMoveOut)
                yield return new WaitForSeconds(clock.DayTime - HomeMoveOut);
            else
                yield return new WaitForSeconds(Clock.Day + clock.DayTime - HomeMoveOut);
        }
    }

    /* coroutine. rozpoczyna podroz z pracy do domu */
    protected virtual IEnumerator GoHome()
    {
        for(; ; )
        {
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
                }
            }

            if(clock.DayTime > WorkMoveOut)
                yield return new WaitForSeconds(clock.DayTime - WorkMoveOut);
            else
                yield return new WaitForSeconds(Clock.Day + clock.DayTime - WorkMoveOut);
        }
    }

    /* ***********************************************************************************
     *          FUNKCJE ZWIAZANE Z ROZPOCZECIEM, KONTYNUACJA I ZAKONCZENIEM JAZDY
     * *********************************************************************************** */

    /* oblicza poczatkowa pozycje i kat. wyznacza trase do celu 
     * start - startowe skrzyzowanie 
     * end - skrzyzowanie docelowe */
    protected virtual void CalculateStartMembers(Vector2 start, Vector2 end)
    {
        rigidbody.isKinematic = false;
        path.AddRange(gps.FindPath(start, end));
        isDriving = true;
        brakeOn = false;
        
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
        
        //wymagane. inaczej actualCrossPos == destination
        NextDestination();
    }

    /* ustawia skaldowa destination na kolejny punkt docelowy.
     * zwraca true, jesli taki punkt [docelowy] istnieje; w przeciwnym przepadku zwraca false */
    protected virtual bool NextDestination()
    {
        bool returnVal = path.Count == 0 ? false : true;

        if(returnVal)
        {
            destination = path[0];
            path.Remove(destination);
        }

        return returnVal;
    }
    
    /* konczy podroz. znika pod ziemie i tam czeka na godzine, w ktorej wyjedzie */
    protected virtual void Finish()
    {
        Debug.Log("BASE Finish!");
        //Debug.Log("Agent: Dojechalem do: " + destination + ". Bez odbioru.");
        actualCrossID = 0;
        isDriving = false;
        transform.Translate(0, -3, 0);
        rigidbody.isKinematic = true;
        model.SetActive(false);
        frontLeftWheel.transform.rotation = new Quaternion();
        frontRightWheel.transform.rotation = new Quaternion();
        path.Clear();
    }

    /* sprawdza, czy dane miejsce jest puste (jesli chodzi o agentow) */
    protected bool IsPlaceEmpty(Vector3 pos)
    {
        Collider[] colliders = Physics.OverlapSphere(pos, length / 2f);

        foreach(Collider c in colliders)
        {
            if(c.CompareTag("Agent"))
            {
                return false;
            }
        }

        return true;
    }

    /* Resetuje agenta, gdy nadejdzie taka potrzeba
     * Resetuje (ustawia na pozycje startowa), gdy agent stal bezczynnie przez resetTime sekund */
    protected void ResetIfNeeded()
    {
        if(rigidbody.velocity.magnitude <= resetVelocity)
        {
            if(startResetTime == 0)
                startResetTime = Time.time;
            else if(Time.time - startResetTime >= resetTime)
            {
                startResetTime = 0;
                isResetTime = false;
                rigidbody.MovePosition(new Vector3(transform.position.x, -3, transform.position.z)); //poczekaj chwile pod ziemia
                StartCoroutine("FinishInTime");
            }
        }
        else
            startResetTime = 0;
    }

    /* traktuje jakby agent ukonczyl jazde. wylacza agenta pewien czas po wywolaniu funkcji */
    protected IEnumerator FinishInTime()
    {
        for(; ; )
        {
            // sprawdz, czy pole, w ktorym nalezy polozyc agenta, jest puste
            //jesli nie, to sprawdz za chwile
            if(!isResetTime)
            {
                isResetTime = true;
                Debug.Log("zaraz sie wylacze");
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                Debug.Log("wylaczam sie");
                startResetTime = 0;
                Finish();
                yield break;
            }
        }
    }

    /* ***********************************************************************************
     *                             FUNKCJE ZWIAZANE Z KOLIZJA
     * *********************************************************************************** */

    /* sprawdza, czy podany obiekt znajduje sie przed naszym agentem */
    protected bool IsObjectInFrontOf(Transform obj)
    {
        bool returnVal = false;
        float angle = transform.eulerAngles.y;

        transform.LookAt(obj.transform.position);

        if(Mathf.Abs(angle - transform.eulerAngles.y) <= sightAngle || //normalny przypadek
            Mathf.Abs(angle - transform.eulerAngles.y) >= 360 - sightAngle) //kiedy kat przechodzi przez kat 0
            returnVal = true;

        transform.rotation = Quaternion.Euler(0, angle, 0);

        return returnVal;
    }

    /* dodaje ID obiektu do listy obiektow kolidujacych z agentem. Zatrzymuje agenta */
    protected void AddToCollidingObjects(int id)
    {
        collidingObjects.Add(id);
        brakeOn = true;
    }

    /* usuwa z listy obiektow kolidujacych obiekt o podanym ID. jesli list jest pusta, wprawia agenta w ruch */
    protected void RemoveFromCollidingObjects(int id)
    {
        collidingObjects.Remove(id);

        if(collidingObjects.Count == 0)
            brakeOn = false;
    }
}

