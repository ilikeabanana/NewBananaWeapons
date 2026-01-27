using NewBananaWeapons;
using System.Collections;
using UnityEngine;


public class Bomb : MonoBehaviour
{
    float timeBeforeExplosion = 3f;
    float timeBeforeNextColorSwitch;
    AudioSource source;

    Material bombMat;

    void Start()
    {
        bombMat = GetComponent<MeshRenderer>().material;
        timeBeforeNextColorSwitch = timeBeforeExplosion / 4;
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        timeBeforeExplosion -= Time.deltaTime;

        timeBeforeNextColorSwitch -= Time.deltaTime;
        
        if(timeBeforeNextColorSwitch <= 0f)
        {
            source.PlayOneShot(AddressableManager.negativeNotifi);
            timeBeforeNextColorSwitch = timeBeforeExplosion / 4;
            bombMat.color = bombMat.color == Color.black ? Color.red : Color.black;
        }

        if(timeBeforeExplosion <= 0f)
        {
            Instantiate(AddressableManager.explosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
