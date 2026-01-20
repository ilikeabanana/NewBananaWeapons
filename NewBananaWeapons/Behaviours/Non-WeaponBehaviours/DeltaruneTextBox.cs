using System.Collections;
using TMPro;
using UnityEngine;

public class DeltaruneTextBox : MonoBehaviour
{
    public static DeltaruneTextBox Instance { get; private set; }

    [SerializeField] TMP_Text tmpText;
    [SerializeField] AudioClip clipToUseOnCharacterType;
    CanvasGroup group;
    AudioSource source;
    GameObject textBox;

    public Transform target;

    // Use this for initialization
    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        textBox = transform.GetChild(0).gameObject;
        source = GetComponent<AudioSource>();
        Instance = this;

        textBox.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null) return;
        float dist = Vector3.Distance(NewMovement.Instance.transform.position, target.transform.position);
        if (dist <= 10)
        {
            group.alpha = dist / 10f;
        }
        else
        {
            group.alpha = 0;
        }
    }

    public void TextboxText(string text, Transform textboxTarget)
    {
        textBox.SetActive(true);
        target = textboxTarget;
        tmpText.text = "";
        StopAllCoroutines();
        StartCoroutine(applyText(text));
    }

    IEnumerator applyText(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            yield return new WaitForSeconds(0.05f);
            tmpText.text += text[i];
            source.PlayOneShot(clipToUseOnCharacterType);
        }
        yield return new WaitForSeconds(1f);
        textBox.SetActive(false);
    }
}
