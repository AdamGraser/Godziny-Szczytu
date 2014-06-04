using System;
using System.Runtime.Serialization;

/**<summary>Klasa wykorzystywana do zapisu i odczytu z pliku danych nt. agenta</summary>*/
[Serializable()]
public class AgentFileContent : ISerializable
{
    /**<summary>Miejsce. w ktorym mieszka agent</summary>*/
    public Vector2Serializable HomePlace { get; set; }
    /**<summary>Czas, w ktorym agent opuszcza dom</summary>*/
    public float HomeMoveOut { get; set; }
    /**<summary>Miejsce. w ktorym pracuje agent</summary>*/
    public Vector2Serializable WorkPlace { get; set; }
    /**<summary>Czas, w ktorym agent opuszcza prace</summary>*/
    public float WorkMoveOut { get; set; } 

    /**<summary>Konstruktor na potrzeby Serializable - stosowac podczas zapisu</summary>*/
    public AgentFileContent()
    {
    }

    /**<summary>Kontruktor</summary>
     * <param name="agent">Agent, z ktorego wyciaga informacje</param> */
    public AgentFileContent(AgentD agent)
    {
        HomePlace = new Vector2Serializable(agent.HomePlace);
        HomeMoveOut = agent.HomeMoveOut;
        WorkPlace = new Vector2Serializable(agent.WorkPlace);
        WorkMoveOut = agent.WorkMoveOut;
    }

    /**<summary>Kontruktor</summary>
     * <param name="agent">Agent, z ktorego wyciaga informacje</param> */
    public AgentFileContent(AgentS agent)
    {
        HomePlace = new Vector2Serializable(agent.HomePlace);
        HomeMoveOut = 0;
        WorkPlace = new Vector2Serializable(agent.WorkPlace);
        WorkMoveOut = 0;
    }

    /**<summary>Kontruktor</summary>
     * <param name="info">Plik, z ktorego czytamy dane (?)</param>
     * <param name="ctxt">Kontekst (?)</param>*/
    public AgentFileContent(SerializationInfo info, StreamingContext ctxt)
    {
        HomePlace = (Vector2Serializable)info.GetValue("HomePlace", typeof(Vector2Serializable));
        HomeMoveOut = (float)info.GetValue("HomeMoveOut", typeof(float));
        WorkPlace = (Vector2Serializable)info.GetValue("WorkPlace", typeof(Vector2Serializable));
        WorkMoveOut = (float)info.GetValue("WorkMoveOut", typeof(float));
    }

    /**<summary>Zapisuje dane do pliku wyznaczonego przez info</summary>
     * <param name="info">Plik, do ktorego zapisujemy dane (?)</param>
     * <param name="ctxt">Kontekst (?)</param>*/
    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("HomePlace", HomePlace);
        info.AddValue("HomeMoveOut", HomeMoveOut);
        info.AddValue("WorkPlace", WorkPlace);
        info.AddValue("WorkMoveOut", WorkMoveOut);
    }

}

