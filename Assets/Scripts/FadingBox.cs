using System.Collections;
using UnityEngine;

/**<summary>Klasa wyswietlajaca podany komunikat przez okreslony czas</summary>*/
public class FadingBox : Box
{
    private float selfDestructTime;
    /**<summaryCzas, przez ktory kotrolka bedzie istniec</summary>*/
    public float SelfDestructTime
    {
        set
        {
            selfDestructTime = value;
            StartCoroutine("DestructInTime");
        }
    }

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */

    /**<summary>Coroutine. Po wskazanym czasie niszczy kontrolke</summary>*/
    protected IEnumerator DestructInTime()
    {
        for(; ; )
        {
            //przerwa wymagana, by agent w palni sie zaladowal
            yield return new WaitForSeconds(selfDestructTime);

            GameObject.Destroy(gameObject);

            yield break;
        }
    }
}