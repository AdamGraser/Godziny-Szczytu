using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

/* klasa wykorzystywana do zapisu i odczytu symulacji z pliku */
[Serializable()]
public class SimulationFileContent : ISerializable
{
    public MapFileContent TheMap {set;get;} //przygotowana do zapisu lub odczytu zawartosc mapy
    public List<AgentFileContent> AgentDList {set;get;} //przygotowanii do odczytu lub zapiosu agenci D
    public List<AgentFileContent> AgentSList { set; get; } // przygotowanie do odczytu lub zapisu agenci S

    /* konstruktor na potrzeby Serializable - stosowac podczas zapisu */
    public SimulationFileContent()
    {
        TheMap = new MapFileContent();
        AgentDList = new List<AgentFileContent>();
        AgentSList = new List<AgentFileContent>();
    }

    /* kontruktor wymagany dla interfejsu Serializable
     * info - plik, z ktorego czytamy (?) */
    public SimulationFileContent(SerializationInfo info, StreamingContext ctxt)
    {
        TheMap = new MapFileContent();
        AgentDList = new List<AgentFileContent>();
        AgentSList = new List<AgentFileContent>();

        TheMap = (MapFileContent)info.GetValue("Map", typeof(MapFileContent));
        AgentDList = (List<AgentFileContent>)info.GetValue("AgentDList", typeof(List<AgentFileContent>));
        AgentSList = (List<AgentFileContent>)info.GetValue("AgentSList", typeof(List<AgentFileContent>));
    }

    /* dodaje do pliku skladowe klasy 
     * info - plik, do ktorego zapisujemy (?) */
    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("Map", TheMap);
        info.AddValue("AgentDList", AgentDList);
        info.AddValue("AgentSList", AgentSList);
    }

}

