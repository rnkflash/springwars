using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Main : MonoBehaviour
{
    public CardZone handZone;

    public List<GameObject> partsOfHudToDisable;

    public Interactor interactor;

    public List<CardId> deck = new List<CardId>();
    public List<CardId> hand = new List<CardId>();
    public List<CardId> discard = new List<CardId>();
    
    public UnityAction<InteractiveObject> OnReleaseDrag;

    public CMSEntity levelEntity;
    List<string> levelSeq = new List<string>()
    {
        E.Id<Level0>(),
    };

    void Awake()
    {
        interactor = new Interactor();
        interactor.Init();

        if (G.run == null)
        {
            G.run = new RunState();

            G.run.maxHealth = 15;
            G.run.health = G.run.maxHealth;

            G.run.deck.Add(new CardId(E.Id<BarrackCard>()));
            G.run.deck.Add(new CardId(E.Id<ArcheryRangeCard>()));
            G.run.deck.Add(new CardId(E.Id<MageGuildCard>()));
        }

        G.main = this;
    }

    IEnumerator Start()
    {
        CMS.Init();

        G.hud.DisableHud();
        G.ui.DisableInput();

        G.fader.FadeOut();

        if (G.run.level < levelSeq.Count)
            yield return LoadLevel(CMS.Get<CMSEntity>(levelSeq[G.run.level]));
        else
            SceneManager.LoadScene("ldgame/end_screen");

        deck = new List<CardId>();
        hand = new List<CardId>();
        discard = new List<CardId>();

        foreach(var d in G.run.deck)
        {
            deck.Add(d);
        }
        
        deck.Shuffle();
        
        yield return DrawCard();

        G.ui.EnableInput();
        G.hud.EnableHud();

    }
    public void EndTurn()
    {
        StartCoroutine(EndTurnCoroutine());
    }

    IEnumerator EndTurnCoroutine()
    {
        G.hud.DisableHud();

        /*var obstacles = interactor.FindAll<IOnEndTurnObstacle>();

        foreach (var ob in obstacles)
        {
            foreach (var challengeActive in challengesActive)
            {
                if (!challengeActive.IsComplete())
                {
                    yield return ob.OnEndTurn(challengeActive);
                }
            }
        }*/

        /*foreach (var f in field.objects)
        {
            var endTurn = G.main.interactor.FindAll<IOnEndTurnFieldDice>();
            foreach (var et in endTurn)
                yield return et.OnEndTurnInField(f.state);
        }*/

        //yield return hand.Clear();

        yield return DrawCard();

        G.hud.EnableHud();
    }


    public void StartDrag(DraggableSmoothDamp draggableSmoothDamp)
    {
        G.drag_card = draggableSmoothDamp.GetComponent<InteractiveObject>();
        G.audio.Play<SFX_Animal>();
    }

    public void StopDrag()
    {
        OnReleaseDrag?.Invoke(G.drag_card);
        G.drag_card = null;
    }

    IEnumerator DrawCard()
    {
        G.audio.Play<SFX_DiceDraw>();
        
        for (var i = 0; i < G.run.drawSize; i++)
        {
            if (deck.Count == 0)
            {
                AddCard<BarrackCard>();
            }
            else
            {
                var cardId = deck.Pop();
                var cardState = AddCard(cardId);
                cardState.cardId = cardId;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    /*IEnumerator DiscardHand()
    {
        G.audio.Play<SFX_Kill>();
        foreach (var cardId in hand)
        {
            
        }
    }*/

    public IEnumerator LoadLevel(CMSEntity entity)
    {
        levelEntity = entity;

        if (levelEntity.Is<TagExecuteScript>(out var exs))
        {
            yield return exs.toExecute();
        }
    }

    public void TryPlayDice(InteractiveObject dice)
    {
        StartCoroutine(PlayDice(dice));
    }

    public IEnumerator PlayDice(InteractiveObject dice)
    {
        G.audio.Play<SFX_Animal>();
        
        dice.state.isPlayed = true;

        //field.Claim(dice);

        yield return new WaitForSeconds(0.25f);

        /*if (hand.objects.Count == 0)
        {
            yield return new WaitForSeconds(0.25f);
            G.hud.PunchEndTurn();
        }*/
    }

    public void AddCard<T>() where T : CMSEntity
    {
        AddCard(new CardId(E.Id<T>()));
    }

    public CardState AddCard(CardId cardId)
    {
        hand.Add(cardId);
        var instance = CreateCard(cardId.id);
        instance.moveable.targetPosition = instance.transform.position = Vector3.left * 7f;
        handZone.Claim(instance);
        return instance.state;
    }

    public InteractiveObject CreateCard(string t)
    {
        var card = CMS.Get<CMSEntity>(t);
        var state = new CardState();
        state.model = card;

        var instance = Instantiate(card.Get<TagPrefab>().prefab, handZone.transform);
        instance.SetState(state);
        return instance;
    }

    bool isWin;
    bool skip;
    public bool showEnergyValue;

    void Update()
    {
        foreach (var poh in partsOfHudToDisable)
            poh.SetActive(G.hud.gameObject.activeSelf);

        if (Input.GetMouseButtonDown(0))
        {
            skip = true;
        }

        G.ui.debug_text.text = "";

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.R))
        {
            G.run.level = 0;
            SceneManager.LoadScene(GameSettings.MAIN_SCENE);
        }
        
        if (Input.GetKeyDown(KeyCode.W))
        {
            StartCoroutine(WinSequence());
        }
        
        if (Input.GetKeyDown(KeyCode.N))
        {
            G.run.level++;
            SceneManager.LoadScene(GameSettings.MAIN_SCENE);
        }
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            G.run = null;
            SceneManager.LoadScene(0);
        }
        
        if (Input.GetKeyDown(KeyCode.D))
        {
            StartCoroutine(DrawCard());
            G.feel.UIPunchSoft();
        }
#endif
    }

    public IEnumerator KillDice(CardState card)
    {
        if (card.isDead)
            yield break;

        card.isDead = true;
        
        if (card.cardId != null)
            discard.Add(card.cardId);

        card.view.transform.DOScale(0f, 0.25f);
        yield return new WaitForSeconds(0.25f);

        card.view.Leave();
        Destroy(card.view.gameObject);
    }

    public IEnumerator CheckForWin()
    {
        yield break;
        
        //yield return WinSequence();
    }

    IEnumerator WinSequence()
    {
        if (isWin)
        {
            Debug.Log("<color=red>double trigger win sequence lol</color>");
            yield break;
        }

        G.hud.DisableHud();

        isWin = true;

        G.ui.win.SetActive(true);
        G.audio.Play<SFX_Win>();

        yield return new WaitForSeconds(1.22f);

        G.ui.win.SetActive(false);

        // yield return storage.Clear();
        /*yield return field.Clear();
        yield return hand.Clear();*/

        G.run.level++;

        //if (!IsFinal()) yield return ShowPicker();

        G.fader.FadeIn();

        yield return new WaitForSeconds(1f);

        if (!IsFinal())
            SceneManager.LoadScene(GameSettings.MAIN_SCENE);
        else
        {
            G.audio.Play<SFX_Magic>();
            SceneManager.LoadScene("ldgame/end_screen");
        }
    }

    bool IsFinal()
    {
        return G.run.level >= levelSeq.Count || levelEntity.Is<TagIsFinal>();
    }

    public class IntOutput
    {
        public int dmg;
    }

    IEnumerator Loss()
    {
        G.ui.defeat.SetActive(true);
        yield return new WaitForSeconds(1f);
        G.run = null;
        SceneManager.LoadScene(GameSettings.MAIN_SCENE);
    }

    public void ShowHud()
    {
        G.hud.gameObject.SetActive(true);
    }
    
    public void HideHud()
    {
        G.hud.gameObject.SetActive(false);
    }

    public IEnumerator SmartWait(float f)
    {
        skip = false;
        while (f > 0 && !skip)
        {
            f -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

}

public class Lifetime : MonoBehaviour
{
    public float ttl = 5f;

    void Update()
    {
        ttl -= Time.deltaTime;

        if (ttl < 0)
            Destroy(gameObject);
    }
}

interface IOnEndTurnFieldDice
{
    public IEnumerator OnEndTurnInField(CardState state);
}
