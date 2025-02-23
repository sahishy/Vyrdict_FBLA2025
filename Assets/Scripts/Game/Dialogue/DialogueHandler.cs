using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DialogueHandler : MonoBehaviour
{
    public static DialogueHandler instance;

    private List<string> dialogues = new List<string>();
    public bool activeDialogue = false;
    private bool activeDialogueAnimation = false;

    [Header("References")]
    [SerializeField] private RectTransform dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;

    private Coroutine dialogueTextAnimationCoroutine;

    private void Awake() {
        instance = this;
    }

    public void AddDialogue(string text) {

        activeDialogue = true;
        GameHandler.instance.ToggleTimeFreeze(true);

        dialogues.Add(text);

        if(dialogues.Count == 1) {
            dialogueText.text = "";
            dialogueTextAnimationCoroutine = StartCoroutine(DialogueTextAnimation(dialogues[0]));

            dialoguePanel.parent.gameObject.SetActive(true);
            dialoguePanel.DOAnchorPos(new Vector2(0, 40), 0.5f).SetEase(Ease.InBack);

            CardsHandler.instance.ToggleDeckVisibility(false);

        }
        
    }
    
    public void ContinueDialogue() {

        if(dialogues.Count == 0) {
            return;
        }

        if(activeDialogueAnimation) {
            StopCoroutine(dialogueTextAnimationCoroutine);
            activeDialogueAnimation = false;
            dialogueText.text = dialogues[0];
            return;
        }

        dialoguePanel.transform.localScale = Vector3.one;
        DOTween.Kill(dialoguePanel.gameObject);
        dialoguePanel.transform.DOPunchScale(Vector3.one * 0.05f, 0.2f).SetId(dialoguePanel.gameObject);

        dialogues.RemoveAt(0);

        if(dialogues.Count == 0) {
            CloseDialogue();
        } else {
            dialogueText.text = "";
            dialogueTextAnimationCoroutine = StartCoroutine(DialogueTextAnimation(dialogues[0]));
        }
    }

    public void CloseDialogue() {
        activeDialogue = false;
        GameHandler.instance.ToggleTimeFreeze(false);

        dialoguePanel.DOAnchorPos(new Vector2(0, -60), 0.5f).SetEase(Ease.InBack).OnComplete(() => {
            dialoguePanel.parent.gameObject.SetActive(false);
        });

        CardsHandler.instance.ToggleDeckVisibility(true);
    }

    private IEnumerator DialogueTextAnimation(string dialogue) {
        activeDialogueAnimation = true;

        float normalDelay = 0.04f;
        float periodDelay = 0.5f;
        float commaDelay = 0.2f;

        for (int i = 0; i < dialogue.Length; i++) {
            dialogueText.text = dialogue.Substring(0, i + 1);

            if(dialogue[i] == '.') {
                yield return new WaitForSeconds(periodDelay);
            } else if(dialogue[i] == ',') {
                yield return new WaitForSeconds(commaDelay);
            } else {
                yield return new WaitForSeconds(normalDelay);
            }
        }

        activeDialogueAnimation = false;
    }
}
