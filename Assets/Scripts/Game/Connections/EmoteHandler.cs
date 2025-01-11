using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EmoteHandler : MonoBehaviour
{
    public static EmoteHandler instance;

    //key: emote - value: sprite of emote
    private Dictionary<Emote, Sprite> allEmotes = new Dictionary<Emote, Sprite>();

    [Header("References")]
    [SerializeField] private GameObject emotePrefab;
    [SerializeField] private List<Sprite> emoteSprites = new List<Sprite>();

    private void Awake() {
        instance = this;

        //INITIALIZE EMOTES DICTIONARY
        List<Emote> emotes = new List<Emote>((Emote[])System.Enum.GetValues(typeof(Emote)));
        for(int i = 0; i < emotes.Count; i++) {
            allEmotes.Add(emotes[i], emoteSprites[i]);
        }
    }

    //creates a temporary emote at a group - used to communicate how a group is feeling
    public void CreateEmote(Emote emote, ConnectionGroup group, float delay = 0f) {
        //CREATE THE EMOTE WITH THE DELAY
        StartCoroutine(CreateEmoteHelper(emote, group, delay));
    }
    private IEnumerator CreateEmoteHelper(Emote emote, ConnectionGroup group, float delay) {
        yield return new WaitForSeconds(delay);

        //GET AVERAGE POSITION OF ALL OF THE TILES IN THE GROUP, USED FOR THE EMOTE POSITION
        Vector3 emotePosition = ConnectionsHandler.instance.GetAverageConnectionGroupTilePosition(group) + new Vector3(0, 2, 0);

        //CREATE THE EMOTE
        GameObject newEmote = Instantiate(emotePrefab, emotePosition, Quaternion.identity);
        Transform icon = newEmote.transform.Find("Display").Find("Icon");
        icon.localScale = Vector3.zero;
        icon.GetComponent<Image>().sprite = allEmotes[emote];

        //EMOTE ANIMATION
        icon.DOScale(1f, 0.5f).SetEase(Ease.OutBack).OnComplete(() => {
            icon.DORotate(new Vector3(60, 0, 10), 0.2f).OnComplete(() => {
                icon.DORotate(new Vector3(60, 0, -10), 0.2f).OnComplete(() => {
                    icon.DORotate(new Vector3(60, 0, 10), 0.2f).OnComplete(() => {
                        icon.DORotate(new Vector3(60, 0, 0), 0.2f);
                    });
                });
            });
        });

        icon.DOScale(0f, 0.5f).SetEase(Ease.InBack).SetDelay(5f).OnComplete(() => {
            Destroy(newEmote);
        });
    }
}

public enum Emote {
    None,
    Happy,
    Sad,
    Angry
}