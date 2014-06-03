using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

/* Glowny kontroler. Zawiera rozne tryby dzialania. Od nich zalezy, co bedzie robic klikniecie myszy i inne... */
public class Controller : MonoBehaviour 
{
    /* zdarzenia */
    private delegate void ActionDelegate();
    private ActionDelegate activeAction; //delegat przechowujacy ostatnia akcje, ktora zostala zgloszona
    private bool isActionContinous; //czy ostatnia akcja ma sie wykonywac caly czas (akcja ciagla) czy tylko raz

    /* obsluga GUI */
    public GUI gui; //klasa zarzadzajaca GUI

    /* obsluga road menu */
    public GameObject gridPrefab; //grid wyznaczajacy miejsca, na ktorych mozna umieszczac drogi
    private GameObject grid;
    public GameObject possibleRoadDirectionsPrefab; //siatka pokazujaca mozliwe punkty, w ktorych mozna postawic droge
    private GameObject possibleRoadDirections;
    private bool isCrossroadsSelected; //czy zostalo juz zaznaczone ktores ze skrzyzowan
    private Crossroads selectedCrossroads; //zaznaczone skrzyzowanie

    /* obsluga region menu */
    public GameObject neutralRegionPrefab; //prefab symbolu neutralnej strefy
    public GameObject residentialRegionPrefab; //prefab symbolu strefy mieszkalnej
    public GameObject industrialRegionPrefab; //prefab symbolu strefy przemyslowej
    private Dictionary<Crossroads, GameObject> regionSigns; //lista przechowujaca wygenerowane symbole stref

    /*obsluga agent menu */
    public GameObject agentDPrefab; //prefab agenta D
    public GameObject agentSPrefab; //prefab agenta S
    private AgentS agentS; //utworzony agent S
    private GameObject agentSHomeSign; //znak pokazujacy, gdzie mieszka agent S
    private GameObject agentSWorkSign; //znak pokazujacy, gdzie pracuje agnet S
    private bool isHomeChosen; //czy miejsce zamieszkanai dla agenta S juz zostalo wybrane

    /* obsluga timeMenu */
    private float simulationSpeed;

    /* GUI - pobieranie danych */
    private bool activateInputForm; //czy aktywowac forumlarz pobierania danych
    private string input; //wartosc, ktora wpisal uzytkownik
    private string textBoxText; //tekst wyswietlany uzytkownikowi
    private string buttonText; //tekst wyswietlany na przycisku
    private Rect textBoxRect; //rozmiar i pozycja textboxa
    private Rect inputBoxRect; //rozmiar i pozycja inputboxa
    private Rect buttonRect; //rozmiar i pozycja buttona
    public float widgetDistance; //rozmiar miedzy kontrolkami
    private ActionDelegate methodToInvoke; //metoda, ktora ma zostac wywolana po wypelnieniu formularza

    /* pozostale */
    public Map map; //mapa

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */

    void Awake()
    {
        activeAction = null;
        isActionContinous = false;

        /* dla opcji z road menu */
        grid = (GameObject)Instantiate(gridPrefab);
        grid.SetActive(false); //bedzie wlaczany tylko podczas operacji na drogach
        possibleRoadDirections = (GameObject)Instantiate(possibleRoadDirectionsPrefab);
        possibleRoadDirections.SetActive(false); //bedzie wlaczany tylko podczas operacji an drogach

        /* dla opcji z region menu */
        regionSigns = new Dictionary<Crossroads,GameObject>();

        /* dla opcji z agent menu */
        agentS = null;
        isHomeChosen = false;
        agentSHomeSign = (GameObject)Instantiate(residentialRegionPrefab);
        agentSWorkSign = (GameObject)Instantiate(industrialRegionPrefab);

        /* GUI */
        activateInputForm = false;
        input = "";
        textBoxText = "";
        buttonText = "";
    }

    void Start()
    {
        /* agent menu */
        agentSHomeSign.transform.position = new Vector3(0, -3, 0); //schowaj pod ziemie
        agentSHomeSign.SetActive(false);
        agentSWorkSign.transform.position = new Vector3(0, -3, 0); //schowaj pod ziemie
        agentSWorkSign.SetActive(false);

        /* time menu */
        simulationSpeed = Time.timeScale;
    }

    void Update()
    {
        if(activeAction != null && isActionContinous)
            activeAction();
    }

    void OnGUI()
    {
        if(activateInputForm)
            CreateInputForm();
    }

    /* ***********************************************************************************
     *                                    PUBLICZNE AKCJE
     * *********************************************************************************** */

    /* ****************************************
     *        OBSLUGA APPLICATION MENU
     * **************************************** */

    /* zamyka aplikacje */
    public void Exit()
    {
        Application.Quit();
    }

    /* towrzy formularz pozwalajacy na podanie nazwy pliku, do ktorego ma zostac zapisany aktualny stan symulacji */
    public void ActivateSaveForm()
    {
        SetActiveAction(ActivateSaveForm);

        CalculateInputFormMembers(new Vector2(400, 30), new Vector2(400, 30), new Vector2(50, 30));
        methodToInvoke = SaveToFile;
        textBoxText = "Type name of destination file:";
        buttonText = "OK";
        input = "";
        activateInputForm = true;
    }

    /* towrzy formularz pozwalajacy na podanie nazwy pliku, z ktorego ma zostac zaladowany stan symulacji */
    public void ActivateLoadForm()
    {
        SetActiveAction(ActivateLoadForm);

        CalculateInputFormMembers(new Vector2(400, 30), new Vector2(400, 30), new Vector2(50, 30));
        methodToInvoke = LoadFromFile;
        textBoxText = "Type name of source file:";
        buttonText = "OK";
        input = "";
        activateInputForm = true;
    }

    /* ****************************************
     *          OBSLUGA GENERAL MENU
     * **************************************** */

    /* wylacza wszystkie podmenu i aktywuje podmenu do zarzadzania drogami */
    public void ActivateRoadSubmenu()
    {
        SetActiveAction(ActivateRoadSubmenu);

        DeactivateAllObjects();
        gui.ActivateRoadMenu();

        grid.SetActive(true); //pokaz grid
    }

    /* wylacza wszystkie podmenu i aktywuje podmenu do zarzadzania regionami */
    public void ActivateRegionSubmenu()
    {
        SetActiveAction(ActivateRegionSubmenu);

        DeactivateAllObjects();
        gui.ActivateRegionMenu();

        CreateRegionSigns(); //wygeneruj symbole stref
    }

    /* wylacza wszystkie podmenu i aktywuje podmenu do zarzadzania agentami */
    public void ActivateAgentSubmenu()
    {
        SetActiveAction(ActivateAgentSubmenu);

        DeactivateAllObjects();
        gui.ActivateAgentMenu();
    }

    /* ****************************************
     *           OBSLUGA ROAD MENU
     * **************************************** */

    /* dodaje we wskazane miejsce skrzyzowanie i - jesli wczesniej zaznaczono inne skrzyzowanie - laczy je z nim */
    public void CreateRoad()
    {
        RaycastHit hit; // miejsce, w ktorym kliknal uzytkownik;
        Ray ray;
        Crossroads cross; //nowoutworzone skrzyzowanie

        SetActiveAction(CreateRoad, true);

        /* aktywuj wymagane obiekty */
        if(isCrossroadsSelected)
            possibleRoadDirections.SetActive(true);

        //gdzie kliknal uzytkownik
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit)) //jezeli uzytkownik kliknal na obiekt (prawdopodobnie: plansze)
        {
            cross = CreateCrossoadOnPosition(new Vector2(Mathf.Round(hit.point.x), Mathf.Round(hit.point.z)));

            //jezeli wczesniej nie bylo zaznaczone inne skrzyzowanie, to zaznacz to nowoutworzone.
            //ponadto pokaz gwiazde, ktora bedzie podpowiadal pozycje dla nastepnego skrzyzowania
            if(!isCrossroadsSelected)
            {
                isCrossroadsSelected = true;
                selectedCrossroads = cross;
                possibleRoadDirections.SetActive(true);
                possibleRoadDirections.transform.position = cross.RealPosition;
            }
            //w przeciwnym wypadku polacz dorga nowoutworzone skrzyzowanie z tym zaznaczonym.
            //by skrzyzowania mogly sie polaczyc, musza znajdowac sie w konkretnych pozycjach wzgledem siebie.
            //potencjalna droga nie moze tez przecinac innej
            else if(CanBeConnected(selectedCrossroads, cross) && map.FindRoadIntersectingRoad(selectedCrossroads, cross) == null)
            {
                map.AddRoad(selectedCrossroads.LogicPosition, cross.LogicPosition);
                
                //odznacz zaznaczone skrzyzowanie
                possibleRoadDirections.SetActive(false);
                isCrossroadsSelected = false;
            }
        }
    }

    /* usuwa wskazane skrzyzowanie (wraz z polaczonymi z nim drogami) lub droge */
    public void RemoveRoad()
    {
        RaycastHit hit;
        Ray ray;
        Vector2 point; //punkt, w ktory kliknal uzytkownik
        Road road;

        SetActiveAction(RemoveRoad, true);

        //usun ewentualne zaznaczenie
        isCrossroadsSelected = false;
        possibleRoadDirections.SetActive(false);

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit)) //jezeli uzytkownik kliknal na (dowolny) obiekt
        {
            point = new Vector2(Mathf.Round(hit.point.x), Mathf.Round(hit.point.z));

            //jesli uzytkownik kliknal na skrzyzowanie
            if(map.AllCrossroads.ContainsKey(point))
            {
                List<Crossroads> connectedCrosses = new List<Crossroads>();

                //usun drogi polaczone z tym skrzyzowaniem
                foreach(var c in map.AllCrossroads[point].ConnectedCrossroads) //ze wzgledu na ew. bledy synchronizacji
                    connectedCrosses.Add(c.Value);
                foreach(var r in connectedCrosses)
                    map.RemoveRoad((point + r.LogicPosition) / 2f);

                map.RemoveCrossroads(point);
            }
            //w przeciwnym wypadku prawdopodobnie kliknal w droge
            else 
            {
                road = map.FindRoadContainingPoint(point);

                if(road != null)
                    map.RemoveRoad(road.LogicPosition);
            }
        }
    
    }

    /* ****************************************
     *           OBSLUGA REGION MENU
     * **************************************** */

    /* ustawia region kliknietego skrzyzowania na neutralny */
    public void SetNeutralRegion()
    {
        RaycastHit hit;
        Ray ray;
        Vector2 point; //punkt, w ktory kliknal uzytkownik
        Crossroads cross;

        SetActiveAction(SetNeutralRegion, true);

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit)) //jezeli uzytkownik kliknal na (dowolny) obiekt
        {
            point = new Vector2(Mathf.Round(hit.point.x), Mathf.Round(hit.point.z));

            //jesli uzytkownik kliknal na skrzyzowanie
            if(map.AllCrossroads.ContainsKey(point))
            {
                cross = map.AllCrossroads[point];
                cross.CityRegion = Region.Neutral;

                //zmien wyswietlany symbol
                GameObject.Destroy(regionSigns[cross]);
                regionSigns.Remove(cross);
                regionSigns.Add(cross, (GameObject)Instantiate(neutralRegionPrefab, cross.RealPosition, new Quaternion()));
            }
        }
    }

    /* ustawia region kliknietego skrzyzowania na mieszkalny */
    public void SetResidentialRegion()
    {
        RaycastHit hit;
        Ray ray;
        Vector2 point; //punkt, w ktory kliknal uzytkownik
        Crossroads cross;

        SetActiveAction(SetResidentialRegion, true);

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit)) //jezeli uzytkownik kliknal na (dowolny) obiekt
        {
            point = new Vector2(Mathf.Round(hit.point.x), Mathf.Round(hit.point.z));

            //jesli uzytkownik kliknal na skrzyzowanie
            if(map.AllCrossroads.ContainsKey(point))
            {
                cross = map.AllCrossroads[point];
                cross.CityRegion = Region.Residential;

                //zmien wyswietlany symbol
                GameObject.Destroy(regionSigns[cross]);
                regionSigns.Remove(cross);
                regionSigns.Add(cross, (GameObject)Instantiate(residentialRegionPrefab, cross.RealPosition, new Quaternion()));
            }
        }
    }

    /* ustawia region kliknietego skrzyzowania na przemyslowy */
    public void SetIndustrialRegion()
    {
        RaycastHit hit;
        Ray ray;
        Vector2 point; //punkt, w ktory kliknal uzytkownik
        Crossroads cross;

        SetActiveAction(SetIndustrialRegion, true);

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit)) //jezeli uzytkownik kliknal na (dowolny) obiekt
        {
            point = new Vector2(Mathf.Round(hit.point.x), Mathf.Round(hit.point.z));

            //jesli uzytkownik kliknal na skrzyzowanie
            if(map.AllCrossroads.ContainsKey(point))
            {
                cross = map.AllCrossroads[point];
                cross.CityRegion = Region.Industrial;

                //zmien wyswietlany symbol
                GameObject.Destroy(regionSigns[cross]);
                regionSigns.Remove(cross);
                regionSigns.Add(cross, (GameObject)Instantiate(industrialRegionPrefab, cross.RealPosition, new Quaternion()));
            }
        }
    }

    /* ****************************************
     *           OBSLUGA AGENT MENU
     * **************************************** */
    
    /* towrzy formularz umozliwiajace generowanie agentow D */
    public void ActivateAgentDCreateForm()
    {
        SetActiveAction(ActivateAgentDCreateForm);

        CalculateInputFormMembers(new Vector2(400, 30), new Vector2(50, 30), new Vector2(50, 30));
        methodToInvoke = CreateAgentD;
        textBoxText = "How many Agets D add to the simulation?";
        buttonText = "OK";
        input = "";
        activateInputForm = true;
    }

    /* towrzy formularz umozliwiajace usuwanie agentow D */
    public void ActivateAgentDRemoveForm()
    {
        SetActiveAction(ActivateAgentDRemoveForm);

        CalculateInputFormMembers(new Vector2(400, 30), new Vector2(50, 30), new Vector2(50, 30));
        methodToInvoke = RemoveAgentsD;
        textBoxText = "How many Agets D remove from the simulation?";
        buttonText = "OK";
        input = "";
        activateInputForm = true;
    }

    /* kaze wybrac uzytkownikowi miejsce zamieszakania i pracy dla agenta S.
     * nastepnie go tworzy */
    public void CreateAgentS()
    {
        RaycastHit hit;
        Ray ray;
        Vector2 point; //punkt, w ktory kliknal uzytkownik

        SetActiveAction(CreateAgentS, true);

        agentSHomeSign.SetActive(true);
        agentSWorkSign.SetActive(true);

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit)) //jezeli uzytkownik kliknal na (dowolny) obiekt
        {
            point = new Vector2(Mathf.Round(hit.point.x), Mathf.Round(hit.point.z));

            //jesli uzytkownik kliknal na skrzyzowanie
            if(map.AllCrossroads.ContainsKey(point))
            {
                //jezeli agent nie istnial, stworz go ale jeszcze nie aktywuj
                if(agentS == null)
                    agentS = ((GameObject)Instantiate(agentSPrefab)).GetComponent<AgentS>();

                //jezeli uzytkownik wybral teraz miejsce zamieszkania dla agenta
                if(!isHomeChosen)
                {
                    agentSHomeSign.transform.position = new Vector3(point.x, 0, point.y);
                    agentS.HomePlace = map.AllCrossroads[point].LogicPosition;

                    isHomeChosen = true;
                }
                //jezeli uzytkownik wybral teraz miejsce pracy
                else if(point != agentS.HomePlace) //nie moze mieszkac i pracowac w tym samym miejscu
                {
                    agentSWorkSign.transform.position = new Vector3(point.x, 0, point.y);
                    agentS.WorkPlace = map.AllCrossroads[point].LogicPosition;
                    agentS.LoadClock(GameObject.FindGameObjectWithTag("Clock").GetComponent<Clock>());
                    agentS.LoadGPSMap(map);
                    agentS.Live();

                    isHomeChosen = false;
                }
            }
        }
    }

    /* usuwa agenta S */
    public void RemoveAgentS()
    {
        SetActiveAction(RemoveAgentS);

        if(agentS != null)
            GameObject.Destroy(agentS.gameObject);

        agentSHomeSign.transform.position = new Vector3(0, -3, 0); //schowaj pod ziemie
        agentSHomeSign.SetActive(false);
        agentSWorkSign.transform.position = new Vector3(0, -3, 0); //schowaj pod ziemie
        agentSWorkSign.SetActive(false);
    }

    /* ****************************************
     *           OBSLUGA TIME MENU
     * **************************************** */

    /* spowalnia dwukrotnie symulacje */
    public void MakeSimulationSlower()
    {
        SetActiveAction(MakeSimulationSlower);

        if(Time.timeScale > 0.125 || Time.timeScale == 0)
        {
            simulationSpeed /= 2;
            Time.timeScale = simulationSpeed;
        }
    }

    /* przyspiesza dwukrotnie symulacje */
    public void MakeSimulationFaster()
    {
        SetActiveAction(MakeSimulationFaster);

        if(Time.timeScale < 16)
        {
            simulationSpeed *= 2;
            Time.timeScale = simulationSpeed;
        }
    }

    /* pauzuje lub wznawia symulacje */
    public void PauseResumeSimulation()
    {
        SetActiveAction(PauseResumeSimulation);

        if(Time.timeScale != 0)
            Time.timeScale = 0;
        else
            Time.timeScale = simulationSpeed;
    }

    /* ***********************************************************************************
     *                         FUNKCJE WSPOMAGAJACE PUBLICZNE AKCJE 
     * *********************************************************************************** */

    /* ****************************************
     *        OBSLUGA APPLICATION MENU
     * **************************************** */
    /* zapisuje aktualny stan symulacji (siec drog, zycie codzienne agentow i smartGPS) od wskazanego pliku */
    private void SaveToFile()
    {
        string fileName = input;
        SimulationFileContent content = new SimulationFileContent();

        SetActiveAction(SaveToFile);

        try
        {
            /* zapisz sieci drog i agentow */
            Stream mainFileStream = File.Open(fileName + ".sim", FileMode.Create);
            BinaryFormatter mainFileFormatter = new BinaryFormatter();

            //wgraj dane agentow do contentu agentow
            var dTab = GameObject.FindGameObjectsWithTag("AgentD");
            foreach(var a in dTab)
                content.AgentDList.Add(new AgentFileContent(a.GetComponent<AgentD>()));

            var sTab = GameObject.FindGameObjectsWithTag("AgentS");
            foreach(var a in sTab)
                content.AgentSList.Add(new AgentFileContent(a.GetComponent<AgentS>()));

            //wgraj mape do contentu
            content.TheMap = new MapFileContent(map);

            //zapisz i zamknij
            mainFileFormatter.Serialize(mainFileStream, content);
            mainFileStream.Close();

            /* zapisz smartGPS agenta S */
            if(agentS != null)
            {
                FileStream gpsFileStream = File.Open(fileName + ".gps", FileMode.Create);
                agentS.SaveGPS(gpsFileStream);
                gpsFileStream.Close();
            }
            gui.ShowFadingBox("Simulation saved successfully!", 200, 30, 2);
        }
        catch(ArgumentException)
        {
            gui.ShowFadingBox("Incorrect filename typed!", 200, 30, 4);
        }
        catch(IOException)
        {
            gui.ShowFadingBox("I/O error!", 100, 30, 4);
        }
        
    }

    /* laduje zapisany stan symulacji z pliku podanego przez uzytkownika */
    private void LoadFromFile()
    {
        string fileName = input;
        SimulationFileContent content = new SimulationFileContent();

        SetActiveAction(LoadFromFile);

        try
        {
            /* zaladuj sieci drog i agentow */
            Stream mainFileStream = File.Open(fileName + ".sim", FileMode.Open);
            mainFileStream.Seek(0, SeekOrigin.Begin);
            BinaryFormatter mainFileFormatter = new BinaryFormatter();

            //zaladuj dane
            content = (SimulationFileContent)mainFileFormatter.Deserialize(mainFileStream);
            mainFileStream.Close();

            /* wygeneruj obiekty na podstawie uzyskanych danych */
            //generowanie mapy
            GenerateMap(content.TheMap);
            //generowanie Agentow D
            GenerateAgentsD(content);
            //generowanie Agentow S (tylko jednego)
            agentS = GenerateAgentS(content);

            /* zaladuj smartGPS agenta S i wpraw go w zycie*/
            if(agentS != null)
            {
                FileStream gpsFileStream = File.Open(fileName + ".gps", FileMode.Open);
                agentS.LoadClock(GameObject.FindGameObjectWithTag("Clock").GetComponent<Clock>());
                agentS.LoadGPS(gpsFileStream, map);
                agentS.Live();
                gpsFileStream.Close();
            }

            gui.ShowFadingBox("Simulation loaded successfully!", 200, 30, 2);
        }
        catch(ArgumentException)
        {
            gui.ShowFadingBox("Incorrect filename typed!", 200, 30, 4);
        }
        catch(FileNotFoundException)
        {
            gui.ShowFadingBox("File not found!", 100, 30, 4);
        }
        catch(IOException)
        {
            gui.ShowFadingBox("I/O error!", 100, 30, 4);
        }
    }

    /* generuje mape na podstawie danych z pliku */
    private void GenerateMap(MapFileContent content)
    {
        map.Clear();

        for(int i = 0; i < content.CrossroadsPositionList.Count; ++i)
        {
            map.AddCrossroads(content.CrossroadsPositionList[i].Vect);
            map.AllCrossroads[content.CrossroadsPositionList[i].Vect].CityRegion = content.CrossroadRegionList[i];
        }
        for(int i = 0; i < content.RoadStartList.Count; ++i)
            map.AddRoad(content.RoadStartList[i].Vect, content.RoadEndList[i].Vect);
    }

    /* generuje agentow D na podstawie danych z pliku */
    private void GenerateAgentsD(SimulationFileContent content)
    {
        //usun wszystkich dotychczasowych agentow D
        var tab = GameObject.FindGameObjectsWithTag("AgentD");
        foreach(var a in tab)
            GameObject.Destroy(a);

        //wygeneruj nowych agentow
        foreach(var c in content.AgentDList)
        {
            AgentD agent = ((GameObject)Instantiate(agentDPrefab)).GetComponent<AgentD>();
            agent.HomePlace = c.HomePlace.Vect;
            agent.HomeMoveOut = c.HomeMoveOut;
            agent.WorkPlace = c.WorkPlace.Vect;
            agent.WorkMoveOut = c.WorkMoveOut;
        }
    }

    /* generuje agenta S na podstawie danych z pliku 
     * zwraca referencje do nowoutworzonego agenta */
    private AgentS GenerateAgentS(SimulationFileContent content)
    {
        AgentS agent = null;

        //usun dotychczasowego agenta S
        if(agentS != null)
            GameObject.Destroy(agentS.gameObject);

        //wygeneruj nowych agentow
        if(content.AgentSList.Count != 0)
        {
            agent = ((GameObject)Instantiate(agentSPrefab)).GetComponent<AgentS>();
            agent.HomePlace = content.AgentSList[0].HomePlace.Vect;
            agent.HomeMoveOut = content.AgentSList[0].HomeMoveOut;
            agent.WorkPlace = content.AgentSList[0].WorkPlace.Vect;
            agent.WorkMoveOut = content.AgentSList[0].WorkMoveOut;
        }

        return agent;
    }

    /* ****************************************
     *            OBSLUGA ROAD MENU
     * **************************************** */

    /* tworzy skrzyzowanie na podanej pozycji (jezeli wczesniej nie istnialo) i zwraca referencje do niego.
     * jezeli skrzyzowanie juz wczesniej istnialo, zwraca referencje do juz istniejacego skrzyzowania o pozycji pos */
    private Crossroads CreateCrossoadOnPosition(Vector2 pos)
    {
        Crossroads cross;
        Road road;

        if(!map.AllCrossroads.ContainsKey(pos)) //jezeli skrzyzowanie na tej pozycji nie istnieje
        {
            map.AddCrossroads(pos);
            cross = map.AllCrossroads[pos];

            /* jezeli skrzyzowanie zostalo postawione na drodze, to ja podziel na dwa mniejsze odcinki */
            road = map.FindRoadContainingPoint(pos);
            if(road != null)
            {
                map.AddRoad(pos, road.Start.LogicPosition);
                map.AddRoad(pos, road.End.LogicPosition);
                map.RemoveRoad(road.LogicPosition);
            }
        }
        else
        {
            cross = map.AllCrossroads[pos];
        }

        return cross;
    }

    /* sprawdza, czy mozna polaczyc ze soba podane skrzyzowania */
    private bool CanBeConnected(Crossroads c1, Crossroads c2)
    {
        float a = (c1.LogicPosition.y - c2.LogicPosition.y) / (c1.LogicPosition.x - c2.LogicPosition.x); //wspolczynnik a porstej miedzy skrzyzowaniami

        if(float.IsInfinity(a) || a == 0 || a == -1 || a == 1)
            return true;

        return false;
    }

    /* ****************************************
     *           OBSLUGA REGION MENU
     * **************************************** */

    /* generuje symbole stref bazujac na mapie map */
    private void CreateRegionSigns()
    {
        GameObject prefab; //prefab, ktory ma zostac utworzony

        foreach(var c in map.AllCrossroads)
        {
            switch(c.Value.CityRegion)
            {
                case Region.Residential: prefab = residentialRegionPrefab; break;
                case Region.Industrial: prefab = industrialRegionPrefab; break;
                default: prefab = neutralRegionPrefab; break;
            }

            regionSigns.Add(c.Value, (GameObject)Instantiate(prefab, c.Value.RealPosition, new Quaternion()));
        }
    }

    /* usuwa symbole stref i czysci liste */
    private void ClearRegionSigns()
    {
        foreach(var r in regionSigns)
            GameObject.Destroy(r.Value);
        regionSigns.Clear();
    }

    /* ****************************************
     *           OBSLUGA AGENT MENU
     * **************************************** */

    /* dodaje podana przez uzytkownika lcizbe agentow D do symulacji.
     * przydziela im losowo miejsca i czas pracy oraz zamieszkania */
    private void CreateAgentD()
    {
        int count = 0; //liczba agentow do dodania

        SetActiveAction(CreateAgentD);

        try
        {
            count = int.Parse(input);

            List<Crossroads> urbanCrosses = map.GetCrossroadsListWithRegion(Region.Residential);
            List<Crossroads> industrialCrosses = map.GetCrossroadsListWithRegion(Region.Industrial);

            //Debug.Log("uCrosses: " + urbanCrosses.Count + " iCrosses: " + industrialCrosses.Count);

            if(industrialCrosses.Count > 0 && urbanCrosses.Count > 0)
            {
                for(int i = 0; i < count; ++i)
                {
                    SpawnAgentD(urbanCrosses[UnityEngine.Random.Range(0, urbanCrosses.Count)],
                               industrialCrosses[UnityEngine.Random.Range(0, industrialCrosses.Count)]);
                }
            }
        }
        catch(ArgumentNullException)
        {
            Debug.LogWarning("Nie udalo sie sparsowac int: input == null");
        }
        catch(FormatException)
        {
            Debug.LogWarning("Nie udalo sie sparsowac int: zly format");
        }
        catch(OverflowException)
        {
            Debug.LogWarning("Nie udalo sie sparsowac int: przepelnienie");
        }
    }

    /* usuwa wskazana liczbe agentow D z symulacji */
    private void RemoveAgentsD()
    {
        int count = 0; //liczba agentow do dodania

        SetActiveAction(RemoveAgentsD);

        try
        {
            count = int.Parse(input);

            var tab = GameObject.FindGameObjectsWithTag("AgentD");
            for(int i = 0; i < tab.Length && i < count; ++i)
                GameObject.Destroy(tab[i]);
        }
        catch(ArgumentNullException)
        {
            Debug.LogWarning("Nie udalo sie sparsowac int: input == null");
        }
        catch(FormatException)
        {
            Debug.LogWarning("Nie udalo sie sparsowac int: zly format");
        }
        catch(OverflowException)
        {
            Debug.LogWarning("Nie udalo sie sparsowac int: przepelnienie");
        }
    }

    /* dodaje agenta do symulacji 
     * home - miejsce zamieszkania
     * work - miejsce, w ktorym agent pracuje */
    private void SpawnAgentD(Crossroads home, Crossroads work)
    {
        GameObject agentObject = (GameObject)Instantiate(agentDPrefab);
        AgentD agent = agentObject.GetComponent<AgentD>();

        agent.HomePlace = home.LogicPosition;
        agent.WorkPlace = work.LogicPosition;
        agent.HomeMoveOut = UnityEngine.Random.Range(0, Clock.Day);
        agent.WorkMoveOut = (agent.HomeMoveOut + Clock.Day / 3) % Clock.Day;

        //Debug.Log("Controller: Powstan! Home: " + agent.HomePlace + " Out: " + agent.HomeMoveOut +
        //          " Work: " + agent.WorkPlace + " Out: " + agent.WorkMoveOut);
    }

    /* ****************************************
     *                   GUI
     * **************************************** */
    /* tworzy formularz pobieranai danych od uzytkownika 
     * method - metoda, ktora ma byc odpalona po wypelnieniu formularza */
    private void CreateInputForm()
    {
        GUIStyle style = new GUIStyle();

        style.alignment = TextAnchor.MiddleCenter;

        UnityEngine.GUI.Box(textBoxRect, textBoxText);
        input = UnityEngine.GUI.TextField(inputBoxRect, input);

        //jezeli zatwierdzono
        if(UnityEngine.GUI.Button(buttonRect, buttonText))
        {
            activateInputForm = false;
            methodToInvoke();
        }
    }

    /* oblicza pozycje poszczegolnych kontrolek inputForma na bazie ich rozmiarow
     * textBoxSize - rozmiar textBoxa
     * inputBoxSize - rozmiar inputBoxa 
     * buttonSize - rozmiar buttona
     * method - metoda, ktora ma zostac wykonana po wypelnieniu formularza */
    private void CalculateInputFormMembers(Vector2 textBoxSize, Vector2 inputBoxSize, Vector2 buttonSize)
    {
        textBoxRect = new Rect();
        inputBoxRect = new Rect();
        buttonRect = new Rect();

        textBoxRect.width = textBoxSize.x;
        textBoxRect.height = textBoxSize.y;
        textBoxRect.x = (Screen.width - textBoxRect.width) / 2f;
        textBoxRect.y = (Screen.height - textBoxSize.y - inputBoxSize.y - buttonSize.y - 2 * widgetDistance) / 2f;

        inputBoxRect.width = inputBoxSize.x;
        inputBoxRect.height = inputBoxSize.y;
        inputBoxRect.x = (Screen.width - inputBoxRect.width) / 2f;
        inputBoxRect.y = textBoxRect.y + textBoxRect.height + widgetDistance;

        buttonRect.width = buttonSize.x;
        buttonRect.height = buttonSize.y;
        buttonRect.x = (Screen.width - buttonRect.width) / 2f;
        buttonRect.y = inputBoxRect.y + inputBoxRect.height + widgetDistance;
    }

    /* ****************************************
     *                POZOSTALE
     * **************************************** */

    /* ustawia akcje action jako akcje aktywna. 
     * isContinous - czy akcja ma byc ciagla */
    private void SetActiveAction(ActionDelegate action, bool isContinous = false)
    {
        activeAction = action;
        isActionContinous = isContinous;
    }

    /* deaktywuje wszystkie obiekty, ktore sa stworzone dla konkretnych dzialan
     * (np. grid) */
    private void DeactivateAllObjects()
    {
        DeactivateRoadObjects();
        DeactivateRegionObjects();
        DeactivateAgentObjects();
    }

    /* deaktywuje wszystkie obiekty, ktore sa stworzone dla poprawnego zarzadzania drogami
     * (np. grid) */
    private void DeactivateRoadObjects()
    {
        possibleRoadDirections.SetActive(false);
        grid.SetActive(false);
    }

    /* deaktywuje wszystkie obiekty, ktore sa stworzone dla poprawnego zarzadzania strefami (regionami)
     * (np. krzyzyki) */
    private void DeactivateRegionObjects()
    {
        ClearRegionSigns();
    }

    /* deaktywuje wszystkie obiekty, ktore sa stworzone dla poprawnego zarzadzania agentami */
    private void DeactivateAgentObjects()
    {
        agentSHomeSign.SetActive(false);
        agentSWorkSign.SetActive(false);

        isHomeChosen = false;
    }
}
