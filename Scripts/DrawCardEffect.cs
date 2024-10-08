using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCardEffect : MonoBehaviour, ICardEffect
{
    public GameObject card;
    public BoardManager board;
    public PlayerDeck decks;
    private CardEffects cardEffect;

    void Start()
    {
        cardEffect = FindObjectOfType<CardEffects>();
        board = FindObjectOfType<BoardManager>();
        decks = FindObjectOfType<PlayerDeck>();
    }
    public void Execute(GameObject activingCard)
    {
        DisplayCard activingC = activingCard.GetComponent<DisplayCard>();

        switch(activingC.card.effect)
        {
            case "DrawAsManyAsI" :
                Debug.Log("Activando el efecto de la carta " + activingC.card.name);
                DrawAsManyAsI(activingCard);
                break;
            case "Divine General Mahoraga" :
                Mahoraga(activingCard);
                break;    
            default :
                break;
        }
    }

    public void Initialize()
    {
        
    }

    public void ShowMessagePanel(string sms)
    {
        Debug.Log(sms);
    }

    public bool CanActive()
    {
        //Revisar la Implementaciob de este metodo
        return true;
    }

    public void EndEffect(GameObject activingCard)
    {
        DisplayCard activingC = activingCard.GetComponent<DisplayCard>();

        if(!activingC.card.isUnit && (activingC.card.GetKind()[0] == "Increase"))
        {
            Transform grav = activingC.card.owner == Card.Owner.Player ? board.transformGraveyard : board.opponentTransformGraveyard;
            board.CleanRow(activingCard.transform.parent, grav);
        }
    }

    //Portador de Dedos's effect
    public void DrawAsManyAsI(GameObject activingCard)
    {
        DisplayCard activingC = activingCard.GetComponent<DisplayCard>();

        if(activingC.card.owner == Card.Owner.Player)
        {
            int howCard = HowManyThereAre(board.transformGraveyard, activingCard);
            if(howCard != 0)
            {
                Draw(board.transformDeck, board.transformPlayerHand, howCard);
            }
            else
            {
                Draw(board.transformDeck, board.transformPlayerHand, 1);
            }
        }
        else 
        {
            int howCard = HowManyThereAre(board.opponentTransformGraveyard, activingCard);
            if(howCard != 0)
            {
                Draw(board.opponentTransformDeck, board.opponentTransformPlayerHand, howCard);
            }
            else 
            {
                Draw(board.opponentTransformDeck, board.opponentTransformPlayerHand, 1);
            }
        }
    }

    //Mahoraga's effect
    public void Mahoraga(GameObject activingCard)
    {
        DisplayCard activatingCardLogic = activingCard.GetComponent<DisplayCard>();
        Transform playerMeleeRow = board.transformMeleeRow;
        Transform opponentMeleeRow = board.opponentTransformMeleeRow;

        // Verificar si el activador es el jugador o el oponente
        Transform ownerDeck, ownerHand;
        if (activatingCardLogic.card.owner == Card.Owner.Player)
        {
            ownerDeck = board.transformDeck;
            ownerHand = board.transformPlayerHand;
        }
        else
        {
            ownerDeck = board.opponentTransformDeck;
            ownerHand = board.opponentTransformPlayerHand;
        }

        // Calcular cuántas cartas hay en las MeleeRows
        int playerMeleeRowCount = CountCardsInRow(playerMeleeRow);
        int opponentMeleeRowCount = CountCardsInRow(opponentMeleeRow);

        // Calcular la diferencia entre las filas Melee
        int diference = Mathf.Abs(playerMeleeRowCount - opponentMeleeRowCount);
        Debug.Log(diference);
        // Robar la cantidad correspondiente a la diferencia
        if (diference > 0 && ThereIsCardInDeck(ownerDeck))
        {
            Draw(ownerDeck, ownerHand, diference);
            Debug.Log("Hemos robado " + diference);
        }
    }

//------Metodos basicos para utilizar en los efectos -------------------------------------------------------------------------
    private void Draw(Transform deck, Transform hand, int cards)
    {
        DisplayCard[] cardsInDeck = deck.GetComponentsInChildren<DisplayCard>();
        DisplayCard cardToDraw;

        if(cardsInDeck.Length != 0)
        {
            int counter = 0;
            while (cardsInDeck.Length != 0 && counter < cards)
            {
                int random = Random.Range(0, cardsInDeck.Length);
                cardToDraw = cardsInDeck[random];
                cardsInDeck = decks.RemovedCardFromArray(cardsInDeck, random);

                if(cardToDraw.gameObject.GetComponent<CardLogic>() == null)
                {
                    cardToDraw.gameObject.AddComponent<CardLogic>();
                }
                if(hand.childCount >= 10)
                {
                    if(hand == board.transformPlayerHand)
                    {
                        cardToDraw.card.isActivated = true;
                        cardToDraw.SetUp(cardToDraw.card);

                        cardToDraw.transform.SetParent(board.transformGraveyard);
                        cardToDraw.transform.localPosition = Vector3.zero;
                        cardToDraw.transform.localScale = Vector3.one;
                        cardToDraw.transform.localRotation = Quaternion.identity;

                        cardToDraw.card.owner = Card.Owner.Player;  
                    }
                    else
                    {
                        cardToDraw.card.isActivated = true;
                        cardToDraw.SetUp(cardToDraw.card);

                        cardToDraw.transform.SetParent(board.opponentTransformGraveyard);
                        cardToDraw.transform.localPosition = Vector3.zero;
                        cardToDraw.transform.localScale = Vector3.one;
                        cardToDraw.transform.localRotation = Quaternion.identity;

                        cardToDraw.card.owner = Card.Owner.Opponent;
                    }
                }
                else 
                {
                    cardToDraw.card.isActivated = true;
                    cardToDraw.SetUp(cardToDraw.card);

                    cardToDraw.transform.SetParent(hand);
                    cardToDraw.transform.localPosition = Vector3.zero;
                    cardToDraw.transform.localScale = Vector3.one;
                    cardToDraw.transform.localRotation = Quaternion.identity;

                    if(hand == board.transformPlayerHand) cardToDraw.card.owner = Card.Owner.Player;
                    else cardToDraw.card.owner = Card.Owner.Opponent;
                }
                counter ++;
            }
        }
    }

    private bool ThereIsCardInDeck(Transform deck)
    {
        DisplayCard[] cardsInDeck = deck.GetComponentsInChildren<DisplayCard>();
        if(cardsInDeck.Length != 0) return true;
        else return false;
    }

    private int HowManyThereAre(Transform Row, GameObject Card)
    {
        DisplayCard[] cards = Row.GetComponentsInChildren<DisplayCard>();
        DisplayCard cardToCount = Card.GetComponent<DisplayCard>();
        int howManyCard = 0;
        if(cards.Length != 0)
        {
            foreach(DisplayCard c in cards)
            {
                if(c.card.name.Equals(cardToCount.card.name))
                {
                    howManyCard +=1;
                }
            }
            return howManyCard;
        }
        return howManyCard;
    }

    private int CountCardsInRow(Transform row)
    {
        DisplayCard[] cardsInRow = row.GetComponentsInChildren<DisplayCard>();
        return cardsInRow.Length;
    }
}
