using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TutorialPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;

    private TutorialDataSO currentData;
    private int currentPage;
    private Action onComplete;

    void Start()
    {
        panel.SetActive(false);
        nextButton.onClick.AddListener(NextPage);
        skipButton.onClick.AddListener(Skip);
    }

    public void Show(TutorialDataSO data, Action onCompleteCallback)
    {
        if (data == null || data.pages.Count == 0)
        {
            onCompleteCallback?.Invoke();
            return;
        }

        currentData = data;
        currentPage = 0;
        onComplete = onCompleteCallback;

        panel.SetActive(true);
        DisplayCurrentPage();
    }

    void DisplayCurrentPage()
    {
        contentText.text = currentData.pages[currentPage];
    }

    void NextPage()
    {
        currentPage++;

        if (currentPage >= currentData.pages.Count)
        {
            Close();
            return;
        }

        DisplayCurrentPage();
    }

    void Skip()
    {
        Close();
    }

    void Close()
    {
        panel.SetActive(false);
        onComplete?.Invoke();
        onComplete = null;
    }
}