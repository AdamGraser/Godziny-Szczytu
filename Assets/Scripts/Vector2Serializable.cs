using System;
using System.Runtime.Serialization;
using UnityEngine;

/**<summary>Klasa rozszerzajaca funkcjonalnosc klasy Vector2 o serializacje</summary>*/
[Serializable()]
public class Vector2Serializable : ISerializable
{
    /**<summary>Vector2, ktory ma zostac zapisany lub odczytany z pliku</summary>*/
    public Vector2 Vect { set; get; }

    /**<summary>Konstruktor na potrzeby Serializable - stosowac podczas zapisu</summary>*/
    public Vector2Serializable()
    {
    }

    /**<summary>Kontruktor</summary>
     * <param name="v">Vector2, z ktorego wyciaga informacje</param> */
    public Vector2Serializable(Vector2 v)
    {
        Vect = v;
    }

    /**<summary>Kontruktor</summary>
     * <param name="info">Plik, z ktorego czytamy dane (?)</param>
     * <param name="ctxt">Kontekst (?)</param>*/
    public Vector2Serializable(SerializationInfo info, StreamingContext ctxt)
    {
        float x, y;
        x = (float)info.GetValue("X", typeof(float));
        y = (float)info.GetValue("Y", typeof(float));
        Vect = new Vector2(x, y);
    }

    /**<summary>Zapisuje dane do pliku wyznaczonego przez info</summary>
     * <param name="info">Plik, do ktorego zapisujemy dane (?)</param>
     * <param name="ctxt">Kontekst (?)</param>*/
    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("X", Vect.x);
        info.AddValue("Y", Vect.y);
    }

}