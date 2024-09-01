using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;

public class SpeechManager : MonoBehaviour
{
    public TMP_Text title, play;
    public SpriteRenderer curtain;
    public SpriteRenderer ketchup;
    public Animator ketchupAnimator;
    public TMP_Text speech;
    public SpeechElement[] conversation;
    [Serializable]
    public class SpeechElement
    {
        public Animator target;
        public string text;
    }
    bool keyPressed = false;
    private void Update()
    {
        if(Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            keyPressed = true;
        }
    }
    private void Start()
    {
        StartCoroutine(WriteText());
    }

    private IEnumerator WriteText()
    {
        var delay = new WaitForSeconds(0.05f);

        while(curtain.color.a > 0f)
        {
            var color = curtain.color;
            color.a -= Time.deltaTime;
            curtain.color = color;
            yield return null;
        }
        speech.transform.parent.gameObject.SetActive(true);
        for (int line = 0; line < conversation.Length; line++)
        {
            keyPressed = false;
            var target = conversation[line].target;
            var text = conversation[line].text;

            target.SetTrigger("Talk");
            for (int i = 0; i < text.Length; i++)
            {
                speech.text = text.Substring(0, i + 1);
                if (keyPressed)
                {
                    speech.text = text;
                    i = text.Length;
                    keyPressed = false;
                }
                yield return delay;
            }

            target.SetTrigger("Idle");

            while (!keyPressed)
            {
                yield return null;
            }

            keyPressed = false;
        }

        speech.transform.parent.gameObject.SetActive(false);

        while (curtain.color.a < 1f)
        {
            var color = curtain.color;
            color.a += Time.deltaTime;
            curtain.color = color;
            yield return null;
        }
        ketchup.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        while (curtain.color.a > 0f)
        {
            var color = curtain.color;
            color.a -= Time.deltaTime;
            curtain.color = color;
            yield return null;
        }

        curtain.color = new Color(0, 0, 0, 1);
        curtain.sortingOrder -= 3;

        speech.transform.parent.gameObject.SetActive(true);
        ketchupAnimator.SetTrigger("Talk");
        var ketchupText = "Where am I?!";
        for (int i = 0; i < ketchupText.Length; i++)
        {
            speech.text = ketchupText.Substring(0, i + 1);
            if (keyPressed)
            {
                speech.text = ketchupText;
                i = ketchupText.Length;
                keyPressed = false;
            }
            yield return delay;
        }

        ketchupAnimator.SetTrigger("Idle");

        while (!keyPressed)
        {
            yield return null;
        }

        keyPressed = false;

        speech.transform.parent.gameObject.SetActive(false);
        var time = 0f;
        var startScale = ketchup.transform.localScale;
        var startPosition = ketchup.transform.position;
        while(time < 1f)
        {
            ketchup.transform.localScale = new Vector3(Mathf.Lerp(startScale.x, 5f, time), Mathf.Lerp(startScale.y, 5f, time), Mathf.Lerp(startScale.z, 5f, time));
            ketchup.transform.position = new Vector3(Mathf.Lerp(startPosition.x, -0.3f, time), Mathf.Lerp(startPosition.y, 0.04f, time), 0);
            time += Time.deltaTime;
            yield return null;
        }
        // TODO: Title Screen
        title.enabled = true;
        yield return new WaitForSeconds(1f);
        play.enabled = true;

        while(!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        SceneManager.LoadScene(1);
    }
}
