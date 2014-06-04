using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

/**<summary>Kontroler obslugujacy (nasluchujacy) wszystkie menu i wykonujacy zadane przez przyciski akcje</summary>*/
public class Controller : MonoBehaviour 
{
    /* zdarzenia */
    private delegate void ActionDelegate();
    /**<summary>Aktywna akcja, ktora ma byc wykonywana</summary>*/
    private ActionDelegate activeAction;
    /**<summary>Czy akcja ma byc wykonywana w ciagle, czy tylko raz</summary>*/
    private bool isActionContinous;

    /* obsluga GUI */
    /**<summary>Obiekt zarzadzajacy GUI</summary>*/
    public GUI gui;

    /* obsluga road menu */
    /**<summary>Prefab grida (siatki pokazujacej punkty, na ktorych mozna klasc skrzyzowania)</summary>*/
    public GameObject gridPrefab;
    /**<summary>Obiekt grida</summary>*/
    private GameObject grid;
    /**<summary>Prefab gwiazdy pokazujacej pozycje, na ktorych nowopolozone skrzyzowanie polaczy sie ze wczesniejszym</summary>*/
    public GameObject possibleRoadDirectionsPrefab;
    /**<summary>Obiekt gwiazdy</summary>*/
    private GameObject possibleRoadDirections;
    /**<summary>Czy aktualnie jest zaznaczone skrzyzowanie</summary>*/
    private bool isCrossroadsSelected;
    /**<summary>Aktualnie zaznaczone skrzyzowanie</summary>*/
    private Crossroads selectedCrossroads;

    /* obsluga region menu */
    /**<summary>Prefab znaku strefy neutralnej</summary>*/
    public GameObject neutralRegionPrefab;
    /**<summary>Prefab znaku strefy mieszkalnej</summary>*/
    public GameObject residentialRegionPrefab;
    /**<summary>Prefab znaku strefy przemyslowej</summary>*/
    public GameObject industrialRegionPrefab;
    /**<summary>Wygenerowane znaki stref</summary>*/
    private Dictionary<Crossroads, GameObject> regionSigns;

    /*obsluga agent menu */
    /**<summary>Prefab Agenta D</summary>*/
    public GameObject agentDPrefab;
    /**<summary>Prefab Agenta S</summary>*/
    public GameObject agentSPrefab;
    /**<summary>Obiekt Agenta S</summary>*/
    private AgentS agentS;
    /**<summary>Obiekt symbolu strefy mieszkalnej dla Agenta S</summary>*/
    private GameObject agentSHomeSign;
    /**<summary>Obiekt symbolu strefy przemyslowej dla Agenta S</summary>*/
    private GameObject agentSWorkSign;
    /**<summary>Czy miejsce zamieszkania dla Agenta S zostalo wybrane</summary>*/
    private bool isHomeChosen;

    /* obsluga timeMenu */
    /**<summary>Predkosc symulacji</summary>*/
    private float simulationSpeed;

    /* GUI - pobieranie danych */
    /**<summary>Czy formularz pobierania danych jest aktywny</summary>*/
    private bool activateInputForm;
    /**<summary>Wartosc pobrana od uzytkownika</summary>*/
    private string input;
    /**<summary>Tekst wyswietlany uzytkownikowi</summary>*/
    private string textBoxText;
    /**<summary>Przycisk zatwierdzajacy</summary>*/
    private string buttonText;
    /**<summary>Rozmiar i pozycja wiadomosci</summary>*/
    private Rect textBoxRect;
    /**<summary>Rozmiar i pozycja wiadomosci (tesktu wyswietlanego uzytkownikowi)</summary>*/
    private Rect inputBoxRect;
    /**<summary>Rozmiar i pozycja przycisku zatwierdzajacego</summary>*/
    private Rect buttonRect;
    /**<summary>Odstep miedzy kontrolkami</summary>*/
    public float widgetDistance;
    /**<summary>Metoda, ktora ma zostac wywolana po zatwierdzeniu formularza</summary>*/
    private ActionDelegate methodToInvoke;

    /* pozostale */
    /**<summary>Mapa</summary>*/
    public Map map;

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */

    /** <summary>Funkcja przygotowujaca kontroler do zabawy. Wywolywana na poczatku istnienia obiektu, przed funkcja Start.</summary> */
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

    /** <summary>Funkcja przygotowujaca Agenta D. Wywolywana na poczatku istnienia obiektu, po funkcji Awake.</summary> */
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

    /** <summary>Funkcja wywolywana podczas kazdej klatki.</summary> */
    void Update()
    {
        if(activeAction != null && isActionContinous)
            activeAction();
    }

    /** <summary>Funkcja rysujaca formularz pobierania danych od uzytkownika.</summary> */
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

    /** <summary>Zanyka aplikacje</summary> */
    public void Exit()
    {
        Application.Quit();
    }

    /**<summary>Tworzy formularz pozwalajacy na podanie nazwy pliku, do ktorego ma zostac zapisany aktualny stan symulacji</summary> */
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

    /**<summary>Tworzy formularz pozwalajacy na podanie nazwy pliku, z ktorego ma zostac zaladowana symulacja</summary> */
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

    /**<summary>Aktywuje podmenu do zarzadzania drogami</summary> */
    public void ActivateRoadSubmenu()
    {
        SetActiveAction(ActivateRoadSubmenu);

        DeactivateAllObjects();
        gui.ActivateRoadMenu();

        grid.SetActive(true); //pokaz grid
    }

    /**<summary>Aktywuje podmenu do zarzadzania strefami miejskimi</summary> */
    public void ActivateRegionSubmenu()
    {
        SetActiveAction(ActivateRegionSubmenu);

        DeactivateAllObjects();
        gui.ActivateRegionMenu();

        CreateRegionSigns(); //wygeneruj symbole stref
    }

    /**<summary>Aktywuje podmenu do zarzadzania agentami</summary> */
    public void ActivateAgentSubmenu()
    {
        SetActiveAction(ActivateAgentSubmenu);

        DeactivateAllObjects();
        gui.ActivateAgentMenu();
    }

    /* ****************************************
     *           OBSLUGA ROAD MENU
     * **************************************** */

    /**<summary>Dodaje we wskazane miejsce skrzyzowanie i - jesli wczesniej zaznaczono inne skrzyzowanie - laczy je z nim</summary> */
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

    /**<summary>Usuwa wskazane skrzyzowanie (wraz z polaczonymi z nim drogami) lub droge</summary> */
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

    /**<summary>ustawia region kliknietego skrzyzowania na neutralny </summary> */
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

    /**<summary>ustawia region kliknietego skrzyzowania na mieszkalny </summary> */
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

    /**<summary>ustawia region kliknietego skrzyzowania na przemyslowy </summary> */
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

    /**<summary>Tworzy formularz pozwalajacy na podanie liczby Agentow D do stworzenia</summary> */
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

    /**<summary>Tworzy formularz pozwalajacy na podanie liczby Agentow D do usuniecia</summary> */
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

    /**<summary>Kaze wybrac uzytkownikowi miejsce zamieszakania i pracy dla agenta S i nastepnie go tworzy</summary> */
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

    /**<summary>Usuwa Agenta S</summary>*/
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

    /**<summary>Spowalnia dwukrotnie symulacje</summary>*/
    public void MakeSimulationSlower()
    {
        SetActiveAction(MakeSimulationSlower);

        if(Time.timeScale > 0.125 || Time.timeScale == 0)
        {
            simulationSpeed /= 2;
            Time.timeScale = simulationSpeed;
        }
    }

    /**<summary>Przyspiesza dwukrotnie symulacje</summary>*/
    public void MakeSimulationFaster()
    {
        SetActiveAction(MakeSimulationFaster);

        if(Time.timeScale < 16)
        {
            simulationSpeed *= 2;
            Time.timeScale = simulationSpeed;
        }
    }

    /**<summary>Pauzuje i wznawia symulacje</summary>*/
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
    /**<summary>Zapisuje aktualny stan symulacji (siec drog, zycie codzienne agentow i smartGPS) od wskazanego pliku</summary> */
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

    /**<summary>Laduje zapisany stan symulacji z pliku podanego przez uzytkownika</summary>*/
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

    /**<summary>Generuje mape na podstawie danych z pliku</summary>
     * <param name="content">Odczytana zawartosc pliku zrodlowego</param>*/
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

    /**<summary>Generuje Agentow D na podstawie danych z pliku</summary>
     * <param name="content">Odczytana zawartosc pliku zrodlowego</param>*/
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

    /**<summary>Generuje Agenta S na podstawie danych z pliku</summary>
     * <param name="content">Odczytana zawartosc pliku zrodlowego</param>
     * <returns>Zwraca referencej do utworzonego Agenta S lub null, jesli agent nie zostal stworzony*/
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

    /**<summary>Tworzy skrzyzowanie na podanej pozycji (jezeli wczesniej nie istnialo) i zwraca referencje do niego.</summary>
     * <param name="pos">Pozycja docelowa skrzyzowania</param>
     * <returns>Jezeli skrzyzowanie juz wczesniej istnialo, zwraca referencje do juz istniejacego skrzyzowania o pozycji pos */
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

    /**<summary>sprawdza, czy mozna polaczyc ze soba podane skrzyzowania</summary>
     * <param name="c1">Testowane skrzyzowanie
     * <param name="c2">Testowane skrzyzowanie
     * <returns>true - jesli skrzyzowania moga byc ze soba polaczone. W przeciwnym wypadku zwraca false</returns>*/
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

    /**<summary>Generuje symbole stref bazujac na mapie map</summary>*/
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

    /**<summary>Usuwa symbole stref</summary>*/
    private void ClearRegionSigns()
    {
        foreach(var r in regionSigns)
            GameObject.Destroy(r.Value);
        regionSigns.Clear();
    }

    /* ****************************************
     *           OBSLUGA AGENT MENU
     * **************************************** */

    /**<summary>Dodaje podana przez uzytkownika lcizbe agentow D do symulacji.
     * Przydziela im losowo miejsca i czas pracy oraz zamieszkania.</summary> */
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

    /**<summary>Usuwa wskazana liczbe Agentow D z symulacji</summary>*/
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

    /**<summary>Dodaje Agent D do symulacji</summary>
     * <param name="home">Dom agenta</param>
     * <param name="work"Miejsce pracy agenta</param>*/
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
    /**<summary>Tworzy formularz pobierania danych od uzytkownika</summary>*/
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

    /**<summary>Oblicza pozycje poszczegolnych kontrolek formularza pobierania danych</summary>
     * <param name="textBoxSize">Rozmiar wiadmosci</param>
     * <param name="inputBoxSize">Rozmiar pola pobierania danych</param>
     * <param name="buttonSize">Rozmiar przycisku</param>*/
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

    /**<summary>Ustaiwa podana akcje jako aktywna</summary>
     * <param name="action">Akcja, ktora ma byc wykonywana</param>
     * <param name="isContinous">Czy akcja ma byc ciagla</param>*/
    private void SetActiveAction(ActionDelegate action, bool isContinous = false)
    {
        activeAction = action;
        isActionContinous = isContinous;
    }

    /**<summary>Deaktywuje wszystkie obiekty, ktore sa tworzone dla konkretnych dzialan*/
    private void DeactivateAllObjects()
    {
        DeactivateRoadObjects();
        DeactivateRegionObjects();
        DeactivateAgentObjects();
    }

    /**<summary>Deaktywuje wszystkie obiekty, ktore sa tworzone dla akcji zwiazanych z drogami*/
    private void DeactivateRoadObjects()
    {
        possibleRoadDirections.SetActive(false);
        grid.SetActive(false);
    }

    /**<summary>Deaktywuje wszystkie obiekty, ktore sa tworzone dla akcji zwiazanych ze strefami*/
    private void DeactivateRegionObjects()
    {
        ClearRegionSigns();
    }

    /**<summary>Deaktywuje wszystkie obiekty, ktore sa tworzone dla akcji zwiazanych z agentami*/
    private void DeactivateAgentObjects()
    {
        agentSHomeSign.SetActive(false);
        agentSWorkSign.SetActive(false);

        isHomeChosen = false;
    }
}
