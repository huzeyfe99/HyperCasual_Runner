using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RidingCylinder : MonoBehaviour
{
    private bool _filled; //Silindirin tam olarak dolup dolmad���n� tutacak
    private float _value; //Silindirin say�sal olarak ne kadar doldu�unu tutacak

    public void IncrementCylinderVolume(float value) //Silindirin boyutunu artt�racak yada k���ltecek
    {
        _value += value; //Ald���m�z boyutu silindirin boyutuna ekliyor

        if (_value > 1) //Silindiirin boyutunu tam olarak 1 yap ve 1'den ne karad b�y�kse o b�y�kl�kte yeni bir silindir olu�tur
        {
            float leftValue = _value - 1; //1'den kalan de�er
            int cylinderCount = PlayerController.Current.cylinders.Count;
            transform.localPosition = new Vector3(transform.localPosition.x, -0.5f  * (cylinderCount - 1) - 0.25f , transform.localPosition.z); //(silindirSay�s� - 1) * -0.5 + b�y�kl�kDe�eri * -0.25
            transform.localScale = new Vector3(0.5f, transform.localScale.y, 0.5f);
            PlayerController.Current.CreateCylinder(leftValue);
        }
        else if (_value < 0) //Karkterimize bu silindiri yok etmesini s�yleyece�iz
        {
            PlayerController.Current.DestroyCylinder(this);
        }
        else //Silindirin boyutunu g�ncelle
        {
            int cylinderCount = PlayerController.Current.cylinders.Count;
            transform.localPosition = new Vector3(transform.localPosition.x, -0.5f * (cylinderCount - 1) - 0.25f * _value, transform.localPosition.z);
            transform.localScale = new Vector3(0.5f * _value, transform.localScale.y, 0.5f * _value);
        }
    }
}
