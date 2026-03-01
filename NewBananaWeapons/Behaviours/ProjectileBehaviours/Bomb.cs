using NewBananaWeapons;
using System.Collections;
using ULTRAKILL.Portal;
using UnityEngine;


public class Bomb : MonoBehaviour
{
    public AudioClip tickSound;
    public GameObject explosion;
    float timeBeforeExplosion = 3f;
    float timeBeforeNextColorSwitch;
    AudioSource source;

    Material bombMat;

    void Start()
    {

        gameObject.AddComponent<SimplePortalTraveler>();

        GetComponentInChildren<MeshRenderer>().material = new Material(GetComponentInChildren<MeshRenderer>().material);

        bombMat = GetComponentInChildren<MeshRenderer>().material;
        timeBeforeNextColorSwitch = timeBeforeExplosion / 4;
        source = GetComponent<AudioSource>();

        StartCoroutine(ShaderManager.ApplyShaderToGameObject(explosion));

        foreach (var exp in explosion.GetComponentsInChildren<Explosion>())
        {
            StartCoroutine(ShaderManager.ApplyShaderToGameObject(exp.explosionChunk));
        }
    }

    // Update is called once per frame
    void Update()
    {
        timeBeforeExplosion -= Time.deltaTime;

        timeBeforeNextColorSwitch -= Time.deltaTime;
        
        if(timeBeforeNextColorSwitch <= 0f)
        {
            source.PlayOneShot(tickSound);
            timeBeforeNextColorSwitch = timeBeforeExplosion / 4;
            bombMat.color = bombMat.color == Color.black ? Color.red : Color.black;
        }

        if(timeBeforeExplosion <= 0f)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
