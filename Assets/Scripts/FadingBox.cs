using System.Collections;
using UnityEngine;

/* klasa wyswietlajaca podany komunikat i znikajaca po danym czasie*/
public class FadingBox : Box
{
    //czas, po ktorym kontrolka ma przestac istniec
    private float selfDestructTime;
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

    /* coroutine. po wskazanym czasie dookonuje samosdestrukcji */
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