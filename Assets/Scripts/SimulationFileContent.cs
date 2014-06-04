using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

/**<summary>Klasa wykorzystywana do zapisu i odczytu z pliku danych nt. calej symulacji</summary>*/
[Serializable()]
public class SimulationFileContent : ISerializable
{
    /**<summary>Przygotowana do zapisu lub odczytu zawartosc mapy</summary>*/
    public MapFileContent TheMap {set;get;}
    /**<summary>Przygotowana do zapisu lub odczytu zawartosc lista Agentow D</summary>*/
    public List<AgentFileContent> AgentDList {set;get;}
    /**<summary>Przygotowana do zapisu lub odczytu zawartosc lista Agentow S</summary>*/
    public List<AgentFileContent> AgentSList { set; get; }

    /**<summary>Konstruktor na potrzeby Serializable - stosowac podczas zapisu</summary>*/
    public SimulationFileContent()
    {
        TheMap = new MapFileContent();
        AgentDList = new List<AgentFileContent>();
        AgentSList = new List<AgentFileContent>();
    }

    /**<summary>Kontruktor</summary>
     * <param name="info">Plik, z ktorego czytamy dane (?)</param>
     * <param name="ctxt">Kontekst (?)</param>*/
    public SimulationFileContent(SerializationInfo info, StreamingContext ctxt)
    {
        TheMap = new MapFileContent();
        AgentDList = new List<AgentFileContent>();
        AgentSList = new List<AgentFileContent>();

        TheMap = (MapFileContent)info.GetValue("Map", typeof(MapFileContent));
        AgentDList = (List<AgentFileContent>)info.GetValue("AgentDList", typeof(List<AgentFileContent>));
        AgentSList = (List<AgentFileContent>)info.GetValue("AgentSList", typeof(List<AgentFileContent>));
    }

    /**<summary>Zapisuje dane do pliku wyznaczonego przez info</summary>
     * <param name="info">Plik, do ktorego zapisujemy dane (?)</param>
     * <param name="ctxt">Kontekst (?)</param>*/
    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("Map", TheMap);
        info.AddValue("AgentDList", AgentDList);
        info.AddValue("AgentSList", AgentSList);
    }

}

