﻿// @Author: Nathaniel Baulch-Jones

using System;
using System.Collections.Generic;
using UnityEngine;
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
    private Card currentCard;


    private const int StartingHandSize = 7;

    public void Initialise()
    {
        // Use this for initialization
        // setup game objects we need
        _discardDeck = new Deck();
        _drawDeck = new Deck();

        // For guilds 1-4, create cards 1-20 and add to the draw deck
        for (int g = 1; g < 5; g++)
        {
            for (int v = 1; v < 20; v++)
            {
                Card c = new Card();
                //TODO: Pass a face
                c.Initialise(g, v, null);
                _drawDeck.push(c);
            }
        }
        // add special cards
        Card triumph = new Card();
        triumph.Initialise(0, 0, null);
        _drawDeck.push(triumph);

        _drawDeck.shuffle();

        //SET UP PLAYERS
        // TODO: Only hardcoded for a single player right now
        _players = new List<Player>();

        //For singleplayer:
        int difficulty = 1; // TODO: All this is hard coded
        int noOfPlayers = 4;
        String userName = "Player 1";
        _ai = new StaticAi();
        _ai.Initialise(difficulty, noOfPlayers);

        if (GameMode == 1) // TODO: Implement multiplayer mode
        {
            Player newPlayer = new Player();
            newPlayer.Initialise(userName, false);
            _players.Add(newPlayer);
            for (int p = 1; p < noOfPlayers; p++)
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
                    Debug.Log("(Adding to starting hand): Guild " + cardToAdd.getGuild() + ", Value " + cardToAdd.getValue());
                    curHand.add(cardToAdd);
                }
            }

            // decide which player goes first, in single player the player is always at Index 0 in _players
            _currentPlayer = Random.Range(0, 4);

        }
    }

    public Hand GetPlayerHand(int playerId)
    {
        return _players[playerId].getHand();
    }

    public void StartTurn()
    {
        // TODO: Implement
        _players[_currentPlayer].setDesiredAction();
        Action choice = _players[_currentPlayer].getDesiredAction();
        TakeAction(choice);

    }

    public void TakeAction(Action a)
    {

        if (a.getChoice().Equals("cleanSlate"))
        {
            //discard hand and get new cards
            _players[_currentPlayer].setCleanSlate();
            for (int i = 0; i < _players[_currentPlayer].getHand().getHandSize(); i++)
            {
                _discardDeck.push(_players[_currentPlayer].getHand().getCardAtIndex(i));
                _players[_currentPlayer].getHand().addAtIndex(i, _drawDeck.pop());
            }

        }
        else if (a.getChoice().Equals("pickUp"))
        {
            _players[_currentPlayer].getHand().add(_drawDeck.pop());
        }
        else if (a.getChoice().Equals("playCard"))
        {
            //check if card equals value or guild
            Card c = a.getSelectedCard();
            if ((c.getGuild() == _discardDeck.peek().getGuild())
                || (c.getValue() == _discardDeck.peek().getGuild())
                || c.getGuild() == 0)
            {
                //TRIUMPH CARD
                if (c.getValue() == 0)
                {
                    //add 5 cards to each player excl. current
                    for (int i = 0; i < _players.Count; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            if (i != _currentPlayer)
                            {
                                _players[i].getHand().add(_drawDeck.pop());
                            }
                        }
                    }
                    //then remove card from player             
                }
                //WEAPON CARD
                else if (c.getValue() < 11)
                {
                    _discardDeck.push(c);
                    // _players[_currentPlayer].getHand().hasCard(c); remove card
                }
                //SPECIAL CARD
                else
                {
                    _discardDeck.push(c);
                    //remove card from player
                    switch (c.getValue())
                    {
                        case 11:
                            //Professor
                                //Takes selectedCard and secondCard in Action
                                //Swaps the cards with two random from targetedPlayer in Action
                            break;
                        case 12:
                            //Crazy Prof
                                //Field in gameController isReversed
                                //When true game loops backwards through Player List
                                //When false uses forward loop
                            break;
                        case 13:
                            //ShieldBearer
                                //targetedPlayer in Action cannot equal currentPlayer
                                //One turn only 
                            break;
                        case 14:
                            //Apprentice
                            for (int i = 0; i < _players.Count; i++)
                            {
                                if (i != _currentPlayer)
                                {
                                    _players[i].getHand().add(_drawDeck.pop());
                                }
                            }
                            break;
                        case 15:
                            //Messenger
                                //Rolls back current player before the end increment?
                            break;
                        case 16:
                            //Spy
                                //Returns hand of another player
                                //?: Where to put this view method so players can't access it normally
                            break;
                        case 17:
                            //Thug
                                //Change suit of card in middle
                                //?: Or add a new card to middle?
                            break;
                        case 18:
                            //Jester
                                //Player has boolean missingTurn
                                //If true, skips player and sets missingTurn to false
                            break;
                        case 19:
                            //Smith (only for non triumph cards)
                            if (_discardDeck.peek().getGuild() != 0)
                            {
                                _players[_currentPlayer].getHand().add(_discardDeck.pop());
                            }
                            break;
                        case 20:
                            //Wizard
                                //Swap hand objects
                                //Create a temp hand to store while swapping
                            break;
                    }
                }

            }
        }

        //throw new NotImplementedException();

    }

    public void EndTurn()
    {
        // TODO: Implement
        _currentPlayer++;
        if (_currentPlayer == _players.Count)
        {
            _currentPlayer = 0;
        }
    }

    public void CheckForWinner()
    {
        for (int i = 0; i < _players.Count; i++) {
            if (_players[i].getHand().getHandSize() == 0) {
                EndGame();
                //Needs to pass on who won
            }
        }
    }

    public void EndGame()
    {
        throw new NotImplementedException();
    }

    public void PlayCard()
    {
        // TODO: Implement
    }

    // Update is called once per frame
    public void Update()
    {
        // GAME LOGIC WILL BE RAN HERE!
    }
    public void pickCard()
    {
        //Do this plz
    }
}