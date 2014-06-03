using System;
using System.Runtime.Serialization;

/* klasa wykorzystywana do zapisu i odczytu z pliku danych nt. agenta */
[Serializable()]
public class AgentFileContent : ISerializable
{
    public Vector2Serializable HomePlace { get; set; } //miejsce, w ktorym mieszka agent
    public float HomeMoveOut { get; set; } //czas, o ktorym agent wyjezdza z domu
    public Vector2Serializable WorkPlace { get; set; } //miejsce, w ktorym pracuje agent
    public float WorkMoveOut { get; set; } //czas, o ktorym agent wyjazdza z pracy

    /* konstruktor na potrzeby Serializable - stosowac podczas zapisu */
    public AgentFileContent()
    {
    }

    /* kontruktor pobierajacy agenta
     * agent - agent D, z ktorego pobrac informacje */
    public AgentFileContent(AgentD agent)
    {
        HomePlace = new Vector2Serializable(agent.HomePlace);
        HomeMoveOut = agent.HomeMoveOut;
        WorkPlace = new Vector2Serializable(agent.WorkPlace);
        WorkMoveOut = agent.WorkMoveOut;
    }

    /* kontruktor pobierajacy agenta
     * agent - agent S, z ktorego pobrac informacje */
    public AgentFileContent(AgentS agent)
    {
        HomePlace = new Vector2Serializable(agent.HomePlace);
        HomeMoveOut = 0;
        WorkPlace = new Vector2Serializable(agent.WorkPlace);
        WorkMoveOut = 0;
    }

    /* kontruktor wymagany dla interfejsu Serializable
     * info - plik, z ktorego czytamy (?) */
    public AgentFileContent(SerializationInfo info, StreamingContext ctxt)
    {
        HomePlace = (Vector2Serializable)info.GetValue("HomePlace", typeof(Vector2Serializable));
        HomeMoveOut = (float)info.GetValue("HomeMoveOut", typeof(float));
        WorkPlace = (Vector2Serializable)info.GetValue("WorkPlace", typeof(Vector2Serializable));
        WorkMoveOut = (float)info.GetValue("WorkMoveOut", typeof(float));
    }

    /* dodaje do pliku skladowe klasy 
     * info - plik, do ktorego zapisujemy (?) */
    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("HomePlace", HomePlace);
        info.AddValue("HomeMoveOut", HomeMoveOut);
        info.AddValue("WorkPlace", WorkPlace);
        info.AddValue("WorkMoveOut", WorkMoveOut);
    }

}

