using System;
using System.Runtime.Serialization;
using UnityEngine;

/* rozszerza funkcjonalnosc klasy Vector2 */
[Serializable()]
public class Vector2Serializable : ISerializable
{
    //vector 2
    public Vector2 Vect { set; get; }

    /* kontruktor */
    public Vector2Serializable()
    {
    }

    /* kontruktor przyjmujacy Vector2 */
    public Vector2Serializable(Vector2 v)
    {
        Vect = v;
    }

    /* kontruktor wymagany dla interfejsu Serializable
     * info - plik, z ktorego czytamy (?) */
    public Vector2Serializable(SerializationInfo info, StreamingContext ctxt)
    {
        float x, y;
        x = (float)info.GetValue("X", typeof(float));
        y = (float)info.GetValue("Y", typeof(float));
        Vect = new Vector2(x, y);
    }

    /* dodaje do pliku skladowe klasy 
     * info - plik, do ktorego zapisujemy (?) */
    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("X", Vect.x);
        info.AddValue("Y", Vect.y);
    }

}