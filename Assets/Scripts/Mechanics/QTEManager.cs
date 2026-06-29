using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class QTEManager : MonoBehaviour
{
    [Header("UI QTE Combo")]
    public GameObject qteComboUI;
    public RectTransform qteShrinkRing;
    public TextMeshProUGUI qteKeyText;

    [Header("UI QTE Parry")]
    public GameObject parryPromptUI;
    public RectTransform parryShrinkRing;

    private readonly KeyCode[] availableKeys = { KeyCode.F, KeyCode.G, KeyCode.V, KeyCode.T, KeyCode.R, KeyCode.E };

    public void Initialize()
    {
        if (qteComboUI != null) qteComboUI.SetActive(false);
        if (parryPromptUI != null) parryPromptUI.SetActive(false);
    }

    // Coroutine ini mengembalikan nilai boolean (sukses/gagal) menggunakan Action callback
    public IEnumerator StartComboQTE(Action<bool> onResult)
    {
        bool isSuccess = false;
        bool inputReceived = false;

        KeyCode targetKey = availableKeys[UnityEngine.Random.Range(0, availableKeys.Length)];
        if (qteKeyText != null) qteKeyText.text = targetKey.ToString();
        if (qteComboUI != null) qteComboUI.SetActive(true);

        float duration = 0.8f;
        float elapsedTime = 0f;
        Vector3 startScale = new Vector3(2.5f, 2.5f, 1f);
        Vector3 endScale = new Vector3(0.3f, 0.3f, 1f);

        if (qteShrinkRing != null) qteShrinkRing.localScale = startScale;

        while (elapsedTime < duration && !inputReceived)
        {
            if (qteShrinkRing != null)
                qteShrinkRing.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / duration);

            foreach (KeyCode key in availableKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    inputReceived = true;
                    if (key == targetKey)
                    {
                        float currentScale = qteShrinkRing.localScale.x;
                        isSuccess = (currentScale <= 1.4f && currentScale >= 0.6f);
                    }
                    break;
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (qteComboUI != null) qteComboUI.SetActive(false);
        onResult?.Invoke(isSuccess);
    }

    public IEnumerator StartParryQTE(Action<bool> onResult)
    {
        bool isSuccess = false;
        bool inputReceived = false;

        if (parryPromptUI != null) parryPromptUI.SetActive(true);

        float duration = 0.8f;
        float elapsedTime = 0f;
        Vector3 startScale = new Vector3(2.5f, 2.5f, 1f);
        Vector3 endScale = new Vector3(0.3f, 0.3f, 1f);

        if (parryShrinkRing != null) parryShrinkRing.localScale = startScale;

        while (elapsedTime < duration && !inputReceived)
        {
            if (parryShrinkRing != null)
                parryShrinkRing.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / duration);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                inputReceived = true;
                float currentScale = parryShrinkRing.localScale.x;
                isSuccess = (currentScale <= 1.4f && currentScale >= 0.6f);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (parryPromptUI != null) parryPromptUI.SetActive(false);
        onResult?.Invoke(isSuccess);
    }
}