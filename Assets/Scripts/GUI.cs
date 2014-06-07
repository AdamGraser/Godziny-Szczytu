using UnityEngine;

/**<summary>Klasa odpowiadajaca za wwyswietlanie menu, komunikatow itd</summary>*/
public class GUI : MonoBehaviour
{
    /* menu */
    /**<summary>Menu aplikacji</summary>*/
    public GameObject applicationMenu;
    /**<summary>Menu prowadzace do roznych podmenu</summary>*/
    public GameObject generalMenu;
    /**<summary>Menu zarzadzania predkoscia symulacji</summary>*/
    public GameObject timeMenu;

    /* podmenu */
    /**<summary>Podenu zarzadzania drogami</summary>*/
    public GameObject roadMenu;
    /**<summary>Podenu zarzadzania strefami</summary>*/
    public GameObject regionMenu;
    /**<summary>Podenu zarzadzania agentami</summary>*/
    public GameObject agentMenu;

    /* inne */
    /**<summary>Prefab wiadomosci</summary>*/
    public GameObject boxPrefab;
    /**<summary>Prefab zanikajacej wiadomosci</summary>*/
    public GameObject fadingBoxPrefab;
    /**<summary>Kontrolka informujaca o predkosci symulacji</summary>*/
    private Box simulationSpeedBox;

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */
    /** <summary>Funkcja przygotowujaca DUI do dzialania. Wywolywana na poczatku istnienia obiektu</summary> */
    void Start()
    {
        DeactivateAllSubmenus();

        simulationSpeedBox = ((GameObject)Instantiate(boxPrefab)).GetComponent<Box>();
        simulationSpeedBox.Width = 125;
        simulationSpeedBox.Height = 20;
        simulationSpeedBox.X = (Screen.width - simulationSpeedBox.Width) / 2;
        simulationSpeedBox.Y = Screen.height - simulationSpeedBox.Height - 40;
    }

    /** <summary>Funkcja wywolywana podczas kazdej klatki.</summary> */
    void Update()
    {
        simulationSpeedBox.Text = "Predkosc: " + Time.timeScale;
    }

    /* ***********************************************************************************
     *                                   PUBLICZNE AKCJE
     * *********************************************************************************** */

    /** <summary>Aktywuje podmenu do zarzadzania drogami</summary> */
    public void ActivateRoadMenu()
    {
        DeactivateAllSubmenus();

        roadMenu.SetActive(true);
    }

    /** <summary>Aktywuje podmenu do zarzadzania strefami</summary> */
    public void ActivateRegionMenu()
    {
        DeactivateAllSubmenus();

        regionMenu.SetActive(true);
    }

    /** <summary>Aktywuje podmenu do zarzadzania agentami</summary> */
    public void ActivateAgentMenu()
    {
        DeactivateAllSubmenus();

        agentMenu.SetActive(true);
    }

    /* ***********************************************************************************
     *                           FUNKCJE WSPOMAGAJACE INNE METODY
     * *********************************************************************************** */

    /** <summary>Deaktywuje wszystkei podmenu</summary> */
    private void DeactivateAllSubmenus()
    {
        roadMenu.SetActive(false);
        regionMenu.SetActive(false);
        agentMenu.SetActive(false);
    }

    /* ***********************************************************************************
     *                                      POZOSTALE
     * *********************************************************************************** */
    
    /** <summary>Tworzy zanikajaca wiadomosc</summary> 
     * <param name="text">Tresc wiadomosci</param>
     * <param name="width">Szerokosc kontrolki z wiadomoscia</param>
     * <param name="height">Wysokosc kontrolki z wiadomoscia</param>
     * <param name="duration">Czas trwania wiadomosci</param>*/
    public void ShowFadingBox(string text, float width, float height, float duration)
    {
        FadingBox box;

        box = ((GameObject)Instantiate(fadingBoxPrefab)).GetComponent<FadingBox>();
        box.Width = width;
        box.Height = height;
        box.X = (Screen.width - width) / 2;
        box.Y = (Screen.height - height) / 2;
        box.Text = text;
        box.SelfDestructTime = duration;
    }

}
