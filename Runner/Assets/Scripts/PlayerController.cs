using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Current;

    public float limitX; //Sa�a sola hareket

    public float runningSpeed; //Karakterin maks h�z�n� tutacak,
    public float xSpeed; //Public yapt�k ��nk� edit�rden h�zl�ca ayarlayabilmek i�in
    private float _currentRunningSpeed; //Karakterimizin edit�rden gelen mevcut ko�ma h�z�n� tutacak

    public GameObject ridingCylinderPrefab; //Silindir prefab�n� tutacak
    public List<RidingCylinder> cylinders; //Karakterin aya��ndaki silindirleri tutacak

    private bool _spawningBridge; //True ise k�pr� olu�turuyor olacak false ise olu�turmuyor
    public GameObject bridgePiecePrefab; //K�pr� par�alar�n� tutacak
    private BridgeSpawner _bridgeSpawner; //Kopru olustur s�n�f�na eri�iyoruz
    private float _creatingBridgeTimer; //Nesneleri olusturmak i�in beklenilmesi gereken zaman

    private bool _finished; //Karakterin biti� �izgisine gelip gelmedi�ini tutacal

    private float _scoreTimer = 0; //Biti� �izgisinden sonra skor kazanaca�� s�reyi tutar

    public Animator animator;

    private float _lastTouchedX;
    private float _dropSoundTimer;

    public AudioSource cylinderAudioSource, triggerAudioSource, itemAudioSource; //Silindir sesi, bir nesneye temas etti�inde ��kacak ses, item seslerini tutacak kaynaklar
    public AudioClip gatherAudioClip, dropAudioClip, coinAudioClip, buyAudioClip, equipItemAudioClip, unequipItemAudioClip; //Silindir toplama sesi, silindirin hacmi k���l�rken ��kacak  ses, alt�nlar� toplad���nda ��kacak ses, item sat�n ald���nda ��kacak ses, e�ya giyme ve ��karma sesleri

    public List<GameObject> wearSpots;

    void Update()
    {
        if (LevelController.Current == null || !LevelController.Current.gameActive)
        {
            return;
        }

        float newX = 0; //Karakterin x eksenindeki yeni konumunu tutacak
        float touchXDelta = 0; //Kulan�c�n�n parma��n� ya da fareyi ne kadar sa�a sola kayd�rd���n� tutacak

        if (Input.touchCount > 0) //Kullan�c�n�n telefon ekran�na dokundu�unun kontrolunu yap�yor
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                _lastTouchedX = Input.GetTouch(0).position.x;
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                touchXDelta = (Input.GetTouch(0).position.x - _lastTouchedX) / Screen.width;
                _lastTouchedX = Input.GetTouch(0).position.x;
            }
            touchXDelta = Input.GetTouch(0).deltaPosition.x / Screen.width;
        } else if (Input.GetMouseButton(0)) //Farede tu�a bas�l�yor mu diye kontrol ediyor
        {
            touchXDelta = Input.GetAxis("Mouse X"); //Farenin x d�zleminde ne kadar hareket etti�ini dokunXDelta ya at�yoruz
        }

        newX = transform.position.x + xSpeed * touchXDelta * Time.deltaTime; //Karakterin x d�zleminde hareketini tutacak
        newX = Mathf.Clamp(newX, -limitX, limitX); //Yeni pozisyonu sa� ve sol limitlerde s�n�rland�rmak i�in Mathf.Clamp 

        Vector3 newPosition = new Vector3(newX, transform.position.y, transform.position.z + _currentRunningSpeed * Time.deltaTime); //Karakterin bir sonraki pozisyonunu tutacak 
        transform.position = newPosition; //Karakterin ilerlemesini sa�lar

        if (_spawningBridge) //True ise k�pr�y� olu�turmaya ba�lar
        {
            PlayDropSound();
            _creatingBridgeTimer -= Time.deltaTime;
            if (_creatingBridgeTimer < 0)
            {
                _creatingBridgeTimer = 0.01f;
                IncrementCylinderVolume(-0.01f); //Silindirlerin hacmini k���lt�yoruz

                GameObject createdBridgePiece = Instantiate(bridgePiecePrefab, this.transform); //Yeni k�pr� par�as� olu�turur
                createdBridgePiece.transform.SetParent(null);
                Vector3 direction = _bridgeSpawner.endReference.transform.position - _bridgeSpawner.startReference.transform.position; //2 nokta aras�ndaki y�n vekt�r�n� elde etmi� oluyoruz
                float distance = direction.magnitude; //2 nokta aras�ndaki mesafe (magnitude = y�n vekt�r�n�n a��rl��� oluyor, y�n vekt�r�n�n a��rl���da 2 nokta aras�ndaki mesafeyi veriyor)

                direction = direction.normalized; //��lemlerde kullanabilmek i�in birim vekt�re d�n��t�ruyoruz
                createdBridgePiece.transform.forward = direction;


                float characterDistance = transform.position.z - _bridgeSpawner.startReference.transform.position.z; //Karakterimiz ba�lang��dan ne kadar uzakda
                characterDistance = Mathf.Clamp(characterDistance, 0, distance); //0 ve maksimum uzakl�k aras�nda s�n�rland�r�yoruz

                Vector3 newPiecePosition = _bridgeSpawner.startReference.transform.position + direction * characterDistance; //Olu�turulan objenin konumunu tutar ve karakterimizle ayn� y�nde ilerler
                newPiecePosition.x = transform.position.x; //Karakterimiz sa�a sola ne kadar gittiyse olu�turulan par�a da o kadar sa�a sola gitsin
                createdBridgePiece.transform.position = newPiecePosition; //Olulturulan par�an�n pozisyonunu yeni vekt�re e�itliyoruz

                if (_finished)
                {
                    _scoreTimer -= Time.deltaTime;
                    if (_scoreTimer < 0)
                    {
                        _scoreTimer = 0.3f;
                        LevelController.Current.ChangeScore(1);
                    }
                }
            }
        }
    }

    public void ChangeSpeed(float value) //levelcontrollerin playercontrollerin h�z�n� de�i�tirir
    {
        _currentRunningSpeed = value;
    }
    private void OnTriggerEnter(Collider other) //Karakterimiz IsTrigger se�ene�i i�aretli olan bir collider ile �arp��t��� zaman bu fonksiyonumuz �al��acak
    {
        if (other.tag == "AddCylinder") //E�er �ar���t���m�z objenin etiketi AddSilinidr ile belli miktar silindirleri b�y�t ve �arp��t���m�z objeyi yok et
        {
            cylinderAudioSource.PlayOneShot(gatherAudioClip, 0.1f);
            IncrementCylinderVolume(0.1f);
            Destroy(other.gameObject);
        } 
        else if (other.tag == "SpawnBridge") //Karakterin �arpt��� collider KopruOlusturucu ise BaslaKopru fonksiyonunu  �al��t�r�r
        {
            StartSpawningBridge(other.transform.parent.GetComponent<BridgeSpawner>());
        }
        else if (other.tag == "StopSpawnBridge")
        {
            StopSpawningBridge();
            if (_finished)
            {
                LevelController.Current.FinishGame();
            }
        }
        else if (other.tag == "Finish")
        {
            _finished = true;
            StartSpawningBridge(other.transform.parent.GetComponent<BridgeSpawner>());
        }
        else if (other.tag == "Coin")
        {
            triggerAudioSource.PlayOneShot(coinAudioClip, 0.1f);
            other.tag = "Untagged";
            LevelController.Current.ChangeScore(10);
            Destroy(other.gameObject);
        }

    }

    private void OnTriggerStay(Collider other) //Karakteimiz IsTrigger se�ene�i a��k olan bi collider'�n �st�nde gitti�i s�re boyunca bu fonksiyon �al��acak
    {
        if (LevelController.Current.gameActive)
        {
            if (other.tag == "Trap")
            {
                PlayDropSound();
                IncrementCylinderVolume(-Time.fixedDeltaTime);
            }
        }
    }

    public void IncrementCylinderVolume(float value) //Karakterimizin alt�nda silindir yoksa silindir olu�turacak, silindiri b�y�tecek, silindir yeteri kadar b�y�d�yse yeni silinidr olu�turacak
    {
        if (cylinders.Count == 0) //Karakterin aya��n�n alt�nda silindir yoksa
        {
            if (value > 0)
            {
                CreateCylinder(value); //Karakterin alt�nda silindir olu�turacak
            }
            else
            {
                if (_finished) //E�er karakter biti� �izgisine ula�t�ysa di�er levele ge�
                {
                    LevelController.Current.FinishGame();
                }
                else
                {
                    Die();
                }
            }
        }
        else //En alttaki silindirin boyutunu g�nceller
        {
            cylinders[cylinders.Count - 1].IncrementCylinderVolume(value);
        }

    }

    public void Die()
    {
        animator.SetBool("dead", true); //Karakter �ld���nde �lme animasyonu �al���r
        gameObject.layer = 6; //Karakter �ld���nde layer'� 6.layer'a e�itlenir(6.layer CharacterDead)
        Camera.main.transform.SetParent(null);
        LevelController.Current.GameOver();
    }

    public void CreateCylinder(float value)  //Silindir Olu�turur
    {
        RidingCylinder createdCylinder = Instantiate(ridingCylinderPrefab, transform).GetComponent<RidingCylinder>();
        cylinders.Add(createdCylinder);
        createdCylinder.IncrementCylinderVolume(value);
    }

    public void DestroyCylinder(RidingCylinder cylinder) //Olu�turulan silindiri yok eder
    {
        cylinders.Remove(cylinder);
        Destroy(cylinder.gameObject);
    }

    public void StartSpawningBridge(BridgeSpawner spawner) //K�pr� olu�turmaya ba�lar
    {
        _bridgeSpawner = spawner;
        _spawningBridge = true;
    }

    public void StopSpawningBridge() //K�pr� olu�turmay� bitirir
    {
        _spawningBridge = false;
    }

    public void PlayDropSound()
    {
        _dropSoundTimer -= Time.deltaTime;
        if (_dropSoundTimer < 0)
        {
            _dropSoundTimer = 0.15f;
            cylinderAudioSource.PlayOneShot(dropAudioClip, 0.1f);
        }
    }
}
