using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private CanvasGroup currentScreen;
    [SerializeField] private List<CanvasGroup> screens;
    [SerializeField] private List<Transform> buttons;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform loadingDotsHolder;
    [SerializeField] private CanvasGroup sceneChangeScreen;

    private bool changingScreen = false;

    private void Awake()
    {
        StartCoroutine(AnimateLoadingDots());
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            QuitGame();
        }
    }

    public void QuitGame() {
        Debug.Log("Quit game.");
    }

    public void ButtonEnter(int index) {
        buttons[index].DOScale(1.1f, 0.2f);
        buttons[index].Find("main").DOScale(1.1f, 0.2f);
    }

    public void ButtonExit(int index) {
        buttons[index].DOScale(1f, 0.2f);
        buttons[index].Find("main").DOScale(1f, 0.2f);
    }

    public void ChangeScreen(int index) {
        if(changingScreen) {
            return;
        }
        changingScreen = true;

        CanvasGroup previousScreen = currentScreen;
        previousScreen.DOFade(0f, 0.2f).OnComplete(() => {
            previousScreen.gameObject.SetActive(false);
            changingScreen = false;
        });
        currentScreen = screens[index];
        currentScreen.gameObject.SetActive(true);
        currentScreen.DOFade(1f, 0.2f);    }

    public void StartGame() {
        StartCoroutine(StartGameProcess());
    }
    private IEnumerator StartGameProcess() {
        ChangeScreen(0);

        sceneChangeScreen.gameObject.SetActive(true);
        sceneChangeScreen.DOFade(1f, 3f);

        mainCamera.DOMove(new Vector3(-15, 15, -30), 5f).SetEase(Ease.InExpo).SetId(mainCamera.gameObject);

        yield return new WaitForSeconds(3.5f);

        DOTween.Kill(mainCamera.gameObject);
        SceneManager.LoadScene(1);
    }

    private IEnumerator AnimateLoadingDots() {
        for(int i = 0; i < loadingDotsHolder.childCount; i++) {
            Transform child = loadingDotsHolder.GetChild(i);

            child.DOMoveY(child.transform.position.y + 10, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutBack);

            yield return new WaitForSeconds(0.1f);
        }
    }
}
