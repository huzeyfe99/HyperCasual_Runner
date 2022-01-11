using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeSpawner : MonoBehaviour
{
    public GameObject startReference, endReference; //Platformlar�n ba�lang�� ve biti� noktalar�n� tutacak
    public BoxCollider hiddenPlatform; //2 platform aras�ndaki g�r�nmeyen collider componentinin boyutunu tutacak

    void Start()
    {
        Vector3 direction = endReference.transform.position - startReference.transform.position; //2 nokta aras�ndaki y�n vekt�r�n� elde etmi� oluyoruz
        float distance = direction.magnitude; //2 nokta aras�ndaki mesafe (magnitude = y�n vekt�r�n�n a��rl��� oluyor, y�n vekt�r�n�n a��rl���da 2 nokta aras�ndaki mesafeyi veriyor)
        direction = direction.normalized; //��lemlerde kullanabilmek i�in birim vekt�re d�n��t�ruyoruz
        hiddenPlatform.transform.forward = direction; //2 referans noktas�n�n y�n�n�n de�i�ti�i zaman g�r�nmez collider'�n da y�n�n�n de�i�mesi gerekiyor
        hiddenPlatform.size = new Vector3(hiddenPlatform.size.x, hiddenPlatform.size.y, distance); //G�r�nmez colliderr'�n boyutland�rmas�

        hiddenPlatform.transform.position = startReference.transform.position + (direction * distance / 2) + (new Vector3(0, -direction.z, direction.y) * hiddenPlatform.size.y / 2); //G�r�nmez collider'�n konumland�r�lmas�
    }

    //Update fonksiyonunu silmemizin sebebi; Bu s�n�f�n i�lemleri sadece oyun ba�lay�nca 1 kere �al��acak ve bitecek
}
