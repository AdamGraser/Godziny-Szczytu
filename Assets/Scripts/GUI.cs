using UnityEngine;

/* klasa odpowiadajaca za wyswietlanie menu, komunikator itp. */
public class GUI : MonoBehaviour
{
    /* menu */
    public GameObject applicationMenu;
    public GameObject generalMenu;
    public GameObject timeMenu;

    /* podmenu */
    public GameObject roadMenu;
    public GameObject regionMenu;
    public GameObject agentMenu;

    /* inne */
    public GameObject boxPrefab; //prefab boxa 
    public GameObject fadingBoxPrefab; //prefab znikajacego boxa
    private Box simulationSpeedBox; //box wyswietlajacy predkosc symulacji

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */
    void Start()
    {
        DeactivateAllSubmenus();

        simulationSpeedBox = ((GameObject)Instantiate(boxPrefab)).GetComponent<Box>();
        simulationSpeedBox.Width = 125;
        simulationSpeedBox.Height = 20;
        simulationSpeedBox.X = (Screen.width - simulationSpeedBox.Width) / 2;
        simulationSpeedBox.Y = Screen.height - simulationSpeedBox.Height - 40;
    }

    void Update()
    {
        simulationSpeedBox.Text = "Predkosc: " + Time.timeScale;
    }

    /* ***********************************************************************************
     *                                   PUBLICZNE AKCJE
     * *********************************************************************************** */

    /* wlacza menu zarzadzania drogami */
    public void ActivateRoadMenu()
    {
        DeactivateAllSubmenus();

        roadMenu.SetActive(true);
    }

    /* wlacza menu zarzadzania regionami */
    public void ActivateRegionMenu()
    {
        DeactivateAllSubmenus();

        regionMenu.SetActive(true);
    }

    /* wlacza menu zarzadzania agentami */
    public void ActivateAgentMenu()
    {
        DeactivateAllSubmenus();

        agentMenu.SetActive(true);
    }

    /* ***********************************************************************************
     *                           FUNKCJE WSPOMAGAJACE INNE METODY
     * *********************************************************************************** */

    /* wylacza wszystkie podmenu */
    private void DeactivateAllSubmenus()
    {
        roadMenu.SetActive(false);
        regionMenu.SetActive(false);
        agentMenu.SetActive(false);
    }

    /* ***********************************************************************************
     *                                      POZOSTALE
     * *********************************************************************************** */
    
    /* wyswietla fading box
     * text - tekst, ktory ma zostac wyswietlony
     * width, height - wysokosc i szerokosc boxa */
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
