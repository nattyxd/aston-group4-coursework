﻿// @Author: Nathaniel Baulch-Jones
// @Author: David Powell
// @Author: Rahima Khanom

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameModel : GuildsElement
{

    //Fields
    private List<Player> _players; // TODO: Check there's no conflict with Unity "Player" Class
    private Deck _drawDeck;
    private Deck _discardDeck;
    private int _currentPlayer;
    private StaticAi _ai;
    private GameObject[] _faces;
    private const int GameMode = 1;
    private Card _currentCard;
    private int _difficulty;
    private int _noOfPlayers;

    private const int StartingHandSize = 7;
    private const int NumStandardCardsPerDeck = 10;

    private const int DefaultTurnLength = 10;
    private float _expiryCountDown;

    private bool reversedPlay = false; // (Crazy Professor) card in action
    private bool messenger = false; //MESSENGER

    public void Initialise()
    {
        //Script procedure to initialise a new game:
        _discardDeck = new Deck();
        _drawDeck = new Deck();
        _difficulty = 1; // TODO: this is hard coded for now
        _noOfPlayers = 4; // TODO: Different game modes may have <4 || >4 players?
        _expiryCountDown = DefaultTurnLength;

        // For guilds 1-4, create cards 1-20 and add to the draw deck
        // Value DB Google Drive: https://docs.google.com/spreadsheets/d/1aoWqoUjY1dmnW7_qooTxKf3aJFOO_YblTjn5QbRmTHU/
        for (int g = 1; g < 5; g++)
        {
            for (int v = 1; v < 21; v++)
            {
                Card c = new Card();
                //TODO: Pass a face
                c.Initialise(g, v, null);
                _drawDeck.push(c);
            }
        }

        // add unique cards (only Triumph card thus far)
        Card triumph = new Card();
        triumph.Initialise(0, 0, null);
        _drawDeck.push(triumph);

        // Shuffle is required for distributing cards to players randomly
        _drawDeck.shuffle();

        //SET UP PLAYERS
        // TODO: Only hardcoded for a single player right now
        _players = new List<Player>();

        _ai = new StaticAi();
        _ai.Initialise(_difficulty, _noOfPlayers);

        if (GameMode == 1) // GameMode 1 is a singleplayer game
        {
            Player newPlayer = new Player();
            String userName = "Player 1";
            newPlayer.Initialise(userName, false);
            _players.Add(newPlayer);
            for (int p = 1; p < _noOfPlayers; p++) // initalise AIs for each other player (4 in a standard single player)
            {
                Player ai = new Player();
                ai.Initialise("AI " + p, true);
                _players.Add(ai);
            }

            // Give players cards
            foreach (var curPlayer in _players)
            {
                var curHand = curPlayer.getHand();
                for (var i = 1; i <= StartingHandSize; i++)
                {
                    Card cardToAdd = _drawDeck.pop();
                    curHand.add(cardToAdd);
                }
            }

            // decide which player goes first, in single player the player is always at Index 0 in _players
            _currentPlayer = Random.Range(0, 4);
 

        }
        else
        {
            // TODO: Multiplayer setup
        }
    }

    // Implement basic anti-cheat functionality. Model should decline invalid plays regardless of action
    public bool IsCardPlayable(int guildValue, int cardValue)
    {
        if (_discardDeck.getAmountOfCards() > 0)
        {
            if (_discardDeck.peek().getGuild() == guildValue)
            {
                return true;
            }
            else if (_discardDeck.peek().getValue() == cardValue)
            {
                return true;
            }
            else
            {
                return guildValue == 0; // Triumph card is always playable
            }
        }
        // cards are always playable if there is nothing in the discard deck yet
  
        return true;
    }

    public void ResetCountdownTimer()
    {
        Debug.Log("Reset countdown");
        _expiryCountDown = DefaultTurnLength;
    }


    public void UpdateCountDown()
    {
        Debug.Log("Actually updating text + " + _expiryCountDown);
        _expiryCountDown -= Time.fixedDeltaTime;
        var curPlayer = app.model.GetCurrentPlayer();
        if (curPlayer == 0)
        {
            app.viewer.UpdateCountDown("     Player, it is your turn! (" + Mathf.Floor(_expiryCountDown + 1) + ")");
        }
        else
        {
            app.viewer.UpdateCountDown("CPU " + curPlayer + " is taking their turn! (" +
                                       Mathf.Floor(_expiryCountDown + 1) + ")");
        }

        if(_expiryCountDown <= 0)
        {
            Debug.Log("(GameModel.cs) No action was taken by the player.");
            app.Notify(GameNotification.TimeRanOut, this, false, null);
            ResetCountdownTimer();
        }
    }



    public int GetStartingHandSize()
    {
        return StartingHandSize;
    }

    public int GetCurrentPlayer()
    {
        return _currentPlayer;
    }

    public Card PeekInPlayCard()
    {
        if (!_discardDeck.isEmpty())
        {
            return _discardDeck.peek();
        }
        else
        {
            return null;
        }
    }

    public Hand GetPlayerHand(int playerId)
    {
        return _players[playerId].getHand();
    }


    public void StartTurn()
    {
        // TODO: Implement
    }

    public void TakeAction(GameAction a)
    {
        //TODO: Implement

    }

    public void DrawToPlayer(int playerID, int amount)
    {
        // TODO: Code to handle the draw deck being empty
        for (var i = 0; i < amount; i++)
        {
            if (_players[playerID].getHand().getHandSize() >= 10)
            {
                Debug.Log("Hand Full");
            }
            else
            {
                _players[playerID].getHand().add(_drawDeck.pop());
                Debug.Log("(GameModelcs) Actually added a card to their hand.");
            }
        }
        Debug.Log("(GameModel.cs) Added " + amount + " card(s) to player " + playerID + "'s hand.");
    }

    public void EndTurn()
    {
        //// TODO: Implement
        //If current player has just used shieldbearer, it will increment the number of rounds immune
        if (_players[_currentPlayer].isImmune() == true)
        {
            _players[_currentPlayer].incrementRoundsImmune();
        }
        //If any player in the array (that is not the current player)is immune and has been immune for 1 or more rounds,
        //it will set their immunity to false and reset their rounds
        foreach (Player p in _players)
        {
            if ((_players[_currentPlayer] != p) && (p.getRoundsImmune() >= 1))
            {
                p.setImmune();
            }
        }
        //If the game is in normal play, it will cycle through the array normally, else, it will cycle through it reversed
        if (reversedPlay == false)
        {

            if (messenger == true) {
                messenger = false;
            }
                else if (_currentPlayer == (_players.Count - 1))
            {
                _currentPlayer = 0;
            }
            else
            {
                _currentPlayer++;
            }
        }
        else
        {
            if (messenger == true)
            {
                messenger = false;
            }
            else if (_currentPlayer == 0)
            {
                _currentPlayer = (_players.Count - 1);
            }
            else
            {
                _currentPlayer--;
            }
        }
        if (_players[_currentPlayer].getMissingTurn() == true) {
            Debug.Log(_currentPlayer + " is MISSING THEIR TURN");
            _players[_currentPlayer].setMissingTurn();
            EndTurn();
        }
            // IF PLAYER IS MISSING TURN INCREMENT AND SET THEIR BOOLEAN TO FALSE
        ResetCountdownTimer();
        _ai.UpdateAiKnowledge(_currentPlayer, 1, PeekInPlayCard());
        CheckForWinner(); // check for winner before we increment anything
        StartTurn(); // move on to the next player
    }

    public void CheckForWinner()
    {
        for (int i = 0; i < _players.Count; i++) {
            if (_players[i].getHand().getHandSize() == 0) {
//                EndGame(); // TODO: DO this properly
                if (_players[i].isAi())
                {
                    SceneManager.LoadScene("Defeat");
                }
                else
                {
                    SceneManager.LoadScene("Victory_Scene");
                }
                //Needs to pass on who won
            }
        }
    }

    public void EndGame()
    {
        throw new NotImplementedException();
    }

    // deal with a card or special action being taken
    public void HandleAction(GameAction gameAction)
    {
        Debug.Log("(GameModel.cs) The gameaction choice was: " + gameAction.getChoice());
        switch (gameAction.getChoice())
        {
            case null:
                Debug.Log("(GameModel.cs) Warning: NULL ACTION");
                break;
            case GameNotification.TimeRanOut:
                DrawToPlayer(_currentPlayer, 1);
                Debug.Log("(GameModel.cs) Time ran out. Player drew a card as punishment.");
                break;

            case GameNotification.CardPickedUp:
                Debug.Log("(GameModel.cs) (Predraw) The player currently has " + _players[_currentPlayer].getHand().getHandSize());
                DrawToPlayer(_currentPlayer, 1);
                Debug.Log("(GameModel.cs) I am drawing to Player " + _currentPlayer + ". They now have " + _players[_currentPlayer].getHand().getHandSize() + " cards.");
                // deal with card picked up
                break;

            case GameNotification.CleanSlate:
                //discard hand and get new cards
                _players[_currentPlayer].setCleanSlate();
                for (int i = 0; i < _players[_currentPlayer].getHand().getHandSize(); i++)
                {
                    _discardDeck.push(_players[_currentPlayer].getHand().getCardAtIndex(i));
                    _players[_currentPlayer].getHand().addAtIndex(i, _drawDeck.pop());
                }
                break;

            case GameNotification.CardPlayed:
//                Debug.Log("(GameModel.cs) A card was played");
                _discardDeck.push(gameAction.getSelectedCard());

                //WARNING: Check for Triumph card first because if it's played it breaks everything else
                //This is a temporary workaround for the demo ONLY
                //TODO: Fix spaghetti

                if (gameAction.WasTriumphCard())
                {
                    // TODO: make triumph card more exciting
                    Debug.Log("OOOOOOOOOOOOOOOOOO TRIUMPH CARD");
                    _discardDeck.push(_players[0].getHand().getCardAtIndex(0));
                    _players[0].getHand().removeAtIndex(0);
                    _discardDeck.push(_players[0].getHand().getCardAtIndex(0));
                    _players[0].getHand().removeAtIndex(0);
                    for (int i = 1; i < _players.Count; i++)
                    {
                        DrawToPlayer(i, 1);
                    }

                    //TODO: IMPLEMENT SPY
                    //TODO: FIX THUG
                    //TODO: TEST SHIELDBEARER
                }
                else if (gameAction.getSelectedCard().getValue() == 11)
                {
                    Debug.Log("PROFESSOR- SWAP 2CARDS");
                    //Professor
                    //picks random target
                    int target = Random.Range(0, 4);
                    while (target == _currentPlayer)
                    {
                        target = Random.Range(0, 4);
                    }
                    Debug.Log("1: " + _players[_currentPlayer].getHand().toString());
                    Debug.Log("2: " + _players[target].getHand().toString());

                    //swap two cards
                    Hand targetH = _players[target].getHand();
                    Hand playersH = _players[_currentPlayer].getHand();
                    //if greater than 2 swap
                    if ((targetH.getHandSize() >= 2) && (playersH.getHandSize() >= 2))
                    {
                        Card targetH1 = targetH.getCardAtIndex(0);
                        Card targetH2 = targetH.getCardAtIndex(1);
                        Card playerH1 = playersH.getCardAtIndex(0);
                        Card playerH2 = playersH.getCardAtIndex(1);
                        targetH.addAtIndex(0, playerH1);
                        targetH.addAtIndex(1, playerH2);
                        playersH.addAtIndex(0, targetH1);
                        playersH.addAtIndex(1, targetH2);

                        _players[target].setHand(targetH);
                        _players[_currentPlayer].setHand(playersH);
                    }
                    else {
                       //resolve this by giving the player that only got 1 card, a card from the draw deck
                    }
        
                    Debug.Log("1: " + _players[_currentPlayer].getHand().toString());
                    Debug.Log("2: " + _players[target].getHand().toString());

                    //update view
                    GameAction choice = new GameAction();
                    choice.Initialise("special.cardupdate");
                    app.Notify(GameNotification.SpecialCardUpdate, this, choice);
                    //remove from target instead of current player if that card was swapped
                    if (playersH.hasCard(gameAction.getSelectedCard()) == true){
                        removeCardPlayed(gameAction, _currentPlayer);
                    }
                    else
                    {
                        removeCardPlayed(gameAction, target);
                    }

                }
                else if (gameAction.getSelectedCard().getValue() == 12)
                {
                    Debug.Log("CRAZY PROFESSOR- REVERSED PLAY");
                    reversedPlay = !reversedPlay;
                    
                }
                else if (gameAction.getSelectedCard().getValue() == 13)
                {
                    Debug.Log("SHIELDBEARER - immune for one round");
                    //targetedPlayer in GameAction cannot equal currentPlayer
                    //One turn only
                    _players[_currentPlayer].setImmune();

                }
                else if (gameAction.getSelectedCard().getValue() == 14)
                {
                    Debug.Log("APPRENTICE - EVERYONE PICK UP");
                    for (var i = 0; i < _players.Count; i++)
                    {
                        if (i != _currentPlayer)
                        {
                            DrawToPlayer(i, 1);
                        }
                    }
                }
                else if (gameAction.getSelectedCard().getValue() == 15)
                {
                    Debug.Log("MESSENGER - EXTRA TURN");
                    messenger = true;
                }
                else if (gameAction.getSelectedCard().getValue() == 16)
                {
                    Debug.Log("SPY - LOOK AT ANOTHER PLAYERS CARDS");
                }
                else if(gameAction.getSelectedCard().getValue() == 17)
                {
                    Debug.Log("THUG - change card in middle");
                //    if (_discardDeck.second() != null)
                //    {
                //        int current = _discardDeck.second().getGuild();
                //        int selected = Random.Range(1, 4);
                //        while (selected == current)
                //        {
                //            selected = Random.Range(1, 4);
                //        }
                //        Card temp = _discardDeck.second();
                //        temp.setGuild(selected);
                //        _discardDeck.push(temp);
                //        GameAction choice = new GameAction();
                //        choice.Initialise("special.cardupdate");
                //        app.Notify(GameNotification.SpecialCardUpdate, this, choice);
                //        Debug.Log("The top card should now be" + _discardDeck.peek().getGuild() + _discardDeck.peek().getValue());
                //    }
                //if(_discardDeck.second() != null)
                //    {
                //        //get second card
                //        Card thug = _discardDeck.pop();
                //        Card wanted = _discardDeck.pop();
                //        _discardDeck.push(thug);
                //        //set the guild to something it wasn't before
                //        int _current = wanted.getGuild();
                //        Debug.Log("!!!!The card was:" + wanted.ToString());
                //        int _new = Random.Range(1, 4);
                //        while (_new == _current)
                //        {
                //            _new = Random.Range(1, 4);
                //        }
                //        wanted.setGuild(_new);
                //        Debug.Log("!!!The card is now:" + wanted.);
                //        //add it to top of deck
                //        _discardDeck.push(thug);
                //        Debug.Log("The top card should now be" + wanted.ToString());
                //        Debug.Log("The top card is actually" + _discardDeck.peek().getGuild() + _discardDeck.peek().getValue());

                //    }

                }
                else if (gameAction.getSelectedCard().getValue() == 18)
                {
                    Debug.Log("JESTER - MISSES TURN");
                    int target = Random.Range(0, 4);
                    while ((target == _currentPlayer) && (_players[target].isImmune() == true))
                    {
                        target = Random.Range(0, 4);
                    }

                    if (_players[target].getMissingTurn() == false)
                    {
                        _players[target].setMissingTurn();
                    }
                    else { }

                }
                else if (gameAction.getSelectedCard().getValue() == 19)
                {
                    Debug.Log("SMITH- PICKED UP FROM MIDDLE");
                    if (_discardDeck.getAmountOfCards() > 1)
                    {
                        _players[_currentPlayer].getHand().add(_discardDeck.second());
                    }

                }
                else if (gameAction.getSelectedCard().getValue() == 20)
                {
                    //Not working right now 
                    //TODO: fix
                    //TODO: to remove card from player, we now need to remove it from the player we swapped with 
                    Debug.Log("WIZARD- SWAP HANDS");
                    //Randomly select target
                    int target = Random.Range(0, 4);
                    while (target == _currentPlayer)
                    {
                        target = Random.Range(0, 4);
                    }

                    //swap hands
                    Hand temp1 = _players[_currentPlayer].getHand();
                    Hand temp2 = _players[target].getHand();
                    Debug.Log("1: " + temp1.toString());
                    Debug.Log("2: " + temp2.toString());
                    _players[_currentPlayer].setHand(temp2);
                    _players[target].setHand(temp1);
                    Debug.Log("1: " + _players[_currentPlayer].getHand().toString());
                    Debug.Log("2: " + _players[target].getHand().toString());

                    //update view
                    GameAction choice = new GameAction();
                    choice.Initialise("special.cardupdate");
                    app.Notify(GameNotification.SpecialCardUpdate, this, choice);
                    //remove from target instead of current player
                    removeCardPlayed(gameAction, target);


                }
                removeCardPlayed(gameAction, _currentPlayer);

                // Remove card by iterating through player's hand
                //TODO: Find solution using references instead of loops - this is fine because it's not too inefficient - but it's quite messy.
                // Update the model depending on special actions of the card
               
                break;
            default:
                Debug.Log("(GameModel.cs) Unknown Command");
                break;
        }

        EndTurn(); // implement the player's turn
    }

    public bool ShouldAiTakeTurn()
    {
        var sensitivity = 50; // the higher the sensitivity, the less likely the AI is to trigger
        return ((Random.Range(0, sensitivity) == 1) && (_expiryCountDown < (DefaultTurnLength / 1.5f)) );
    }

    public GameAction GenerateAiAction() {
       return _ai.AiTurn(_players[_currentPlayer]);
    }

    public void removeCardPlayed(GameAction gameAction, int PlayerID)
    {
        Debug.Log("(GameModel.cs) Attempting to remove the player's played card. Hand size is: " + _players[_currentPlayer].getHand().getHandSize());
        for (var i = 0; i <= _players[PlayerID].getHand().getHandSize() - 1; i++)
        {
            if ((_players[PlayerID].getHand().getCardAtIndex(i).getGuild() ==
                 gameAction.getSelectedCard().getGuild()) &&
                ((_players[PlayerID].getHand().getCardAtIndex(i).getValue() ==
                  gameAction.getSelectedCard().getValue())))
            {
                Debug.Log("(GameModel.cs) About to remove card! Was looking for '" + gameAction.getSelectedCard().getGuild() + ", " + gameAction.getSelectedCard().getValue() + "'. Found '" + _players[PlayerID].getHand().getCardAtIndex(i).getGuild() + ", " + (_players[PlayerID].getHand().getCardAtIndex(i).getValue()) + "' :).");
                _players[PlayerID].getHand().removeAtIndex(i);
                break;
            }
        }
    }
    // Update is called once per frame
    public void Update()
    {
        UpdateCountDown();
        if (_players[GetCurrentPlayer()].isAi() && ShouldAiTakeTurn())
        {
            // game logic for triggering the AI's turn
            GameAction choice = GenerateAiAction();
            app.Notify(GameNotification.AiTookTurn, this, choice);
        }
    }
}