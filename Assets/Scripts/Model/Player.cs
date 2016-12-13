﻿// @Author: Enter Authors Here

using UnityEngine;
using System.Collections;

public class Player
{
    //Fields
    private Hand hand;
    private string userName;
    private bool hasPlayedCleanSlate;
    private bool isAI;
    private bool hasFinishedTurn;
    private GameAction _desiredGameAction;
    private Card selectedCard;
    private bool isMissingTurn;


    // Use this for initialization
    void Start()
    {

    }

    public void Initialise(string userName, bool isAI)
    {
        this.userName = userName;
        this.isAI = isAI;
        this.hasPlayedCleanSlate = false;
        this.hand = new Hand();
        this.hasFinishedTurn = false;
        _desiredGameAction = new GameAction();
        selectedCard = null;
        isMissingTurn = false;

    }

    //Gets the state of hasPlayedCleanSlate
    public bool getCleanSlate()
    {
        return hasPlayedCleanSlate;
    }

    //Sets the state of hasPlayedCleanSlate
    //should probably only be able to set it to true?
    //to stop players abusing this method?
    public void setCleanSlate()
    {

        hasPlayedCleanSlate = true;
    }

    // returns the card or action that the user has performed once their turn is complete
    public GameAction getDesiredAction()
    {
        // TODO: Implement method that can return a generic, so we can return an action rather than a card/int (we can't do both).
        return new GameAction();
    }

    public void setDesiredAction()
    {
        // TODO: Implement

    }


    // Update is called once per frame
    void Update()
    {

    }

    public Hand getHand()
    {
        return hand;
    }

    public bool isAi() {
        return isAI;
    }

    public bool getMissingTurn() {
        return isMissingTurn;
    }
    public void setMissingTurn() {
        isMissingTurn = !isMissingTurn;
    }
}