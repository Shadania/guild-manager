using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// The script that oversees your progress in the DemoScene
// Handles tutorial and will tell you when you've finished the game
public class DemoSceneManager : MonoBehaviour
{
    #region Declarations
    public Text TutorialTextBox;
    public GameObject TutorialBox;
    public Button NextButton;

    private int _objectiveCounter = -1;
    private bool _currObjectiveComplete = true;
    private int _currStringIdx = -1;
    private int _kingCounter = 0;

    private List<string> _allText = new List<string>();
    #endregion Declarations

    private void Start()
    {
        #region AddAllText
        _allText.Add("Hello and welcome to the game!");
        _allText.Add("You are an ambitious individual who would like to one day rule this land.");
        _allText.Add("To become the king of all the towns in the area, you need to first start your guild.");
        _allText.Add("A guild needs a lot of managing! You first need to gather some wood for a Guild Management Desk.");
        _allText.Add("Go find a tree, walk up to it, hover your mouse over it and press F repeatedly to harvest the wood. Do this until the tree vanishes!");
        _allText.Add("NEXTOBJECTIVE"); // 0
        // PLAYER GATHERS 10 WOOD

        _allText.Add("Good job! You can see the contents of your inventory by clicking on the \"Inventory\" button in the top right of your screen.");
        _allText.Add("Now, go find a nice spot for your desk. When you've found one, press F and the Desk will appear in front of you.");
        _allText.Add("NEXTOBJECTIVE"); // 1
        // PLAYER PRESSES F TO MAKE DESK APPEAR

        _allText.Add("Nice spot! Your guild needs a name. Interact with the Desk (Press F on it when you're behind the Desk) and change the name of your guild. When you're finished, hit ENTER to confirm. ESCAPE works to get out of any window.");
        _allText.Add("NEXTOBJECTIVE"); // 2
        // PLAYER CHANGES GUILD NAME

        _allText.Add("I sure wouldn't mind being lorded over for the rest of my life by a guild with a name like that!");
        _allText.Add("To make that happen however, you need to win favor with the locals. Doing tasks for them will surely improve relations and get your name around.");
        _allText.Add("Go find the closest village and look around for their Quest Board. There will be Parchments stuck to the board that you can interact with to get a Quest.");
        _allText.Add("NEXTOBJECTIVE"); // 3
        // PLAYER TAKES A QUEST

        _allText.Add("I see why they'd ask someone else to do that... If you forget what the quest was about you can always click on the Quests button in the top right to look at which quests you have.");
        _allText.Add("Go complete the quest!");
        _allText.Add("NEXTOBJECTIVE"); // 4
        // PLAYER FULLFILLS THE QUEST

        _allText.Add("You're done with the quest! Complete it through the Quest menu.");
        _allText.Add("NEXTOBJECTIVE"); // 5
        // PLAYER PRESSES COMPLETE BUTTON

        _allText.Add("Good job, now you know how the Quest system works.");
        _allText.Add("Oh no, an enemy! Press Left-Click when he's in range, but don't let him get too close! Close-quarters combat is not your strength!");
        _allText.Add("NEXTOBJECTIVE"); // 6
        // SPAWN ENEMY NEAR PLAYER
        // ENEMY DIES

        _allText.Add("Good job! Now, you need a recruit! Recruits are your loyal followers who will do everything you tell them to. Go into the Desk's Recruitment Queue and recruit someone.");
        _allText.Add("NEXTOBJECTIVE"); // 7
        // SPAWN RECRUIT CARD
        // PLAYER RECRUITS PERSON

        _allText.Add("That person does look like they could lift some heavy weights. Like I said, your Guild Members can do a whole lot of things, like attack Enemies, gather resources, guard villages...");
        _allText.Add("You can also change their names, check out their inventories, give them gear, give them orders regarding how to treat enemies in sight...");
        _allText.Add("You can play around with this for a bit. When you're ready to learn how exactly to become king of a village, click the Next button.");

        _allText.Add("Alright! Did you notice how the Quest said it had an Influence reward? Villages really like it when you do tasks for them!");
        _allText.Add("Other ways you can earn influence over a village is killing an enemy near it, having your Guildies patrol around it, or even investing some bare gold into the Village at the townhall.");
        _allText.Add("When you accumulate enough Influence, you can buy Council positions in the village at the Townhall. Place your Guildies in those positions.");
        _allText.Add("When the full council is part of your guild, you can spend even more Influence to become King of the village, which allows you to begin collecting taxes!");
        _allText.Add("Other buildings in a Village are the marketplace, where you can buy and sell resources, and the Smithy, where you can buy, sell and upgrade your weapons and armor.");
        _allText.Add("You can find the vital information back in the Help menu! And now, go forth and conquer!");
        _allText.Add("END"); // 8
        // CLOSE TUTORIAL BOX


        #endregion AddAllText

        // Start Tutorial!
        TutorialBox.SetActive(true);
        NextButton.gameObject.SetActive(true);
        NextButton.onClick.AddListener(NextClicked);
        NextClicked();
    }
    private void Update()
    {
        if (!_currObjectiveComplete)
        {
            switch (_objectiveCounter)
            {
                case 0:
                    {
                        Inventory pInv = GameManager.Instance.PlayerAvatar.GetComponent<Inventory>();
                        if (pInv.GetAmtResource(Inventory.ResourceType.Wood) >= 10)
                        {
                            _currObjectiveComplete = true;
                            NextButton.gameObject.SetActive(true);
                            NextClicked();
                        }
                    }
                    
                    break;

                case 1:
                    if (Input.GetButtonDown("Interact"))
                    {
                        // Place desk
                        PlayerController player = GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>();
                        GuildManagementDesk desk = GameManager.Instance.PlayerGuildDesk.GetComponent<GuildManagementDesk>();
                        Vector3 deskPos = player.transform.position;
                        deskPos += player.transform.forward * 5;
                        deskPos.y -= 1.7f;
                        desk.transform.position = deskPos;
                        
                        Vector3 deskRotEuler = player.transform.rotation.eulerAngles;
                        deskRotEuler.x -= 90.0f;
                        deskRotEuler.y += 180.0f;
                        Quaternion deskRot = new Quaternion();
                        deskRot.eulerAngles = deskRotEuler;
                        desk.transform.rotation = deskRot;
                        
                        desk.gameObject.SetActive(true);
                        desk.CanSpawnRecruitCards = false;

                        // Consume wood
                        Inventory pInv = player.GetComponent<Inventory>();
                        pInv.RemoveResources(Inventory.ResourceType.Wood, 10);

                        // Go to next objective
                        _currObjectiveComplete = true;
                        NextButton.gameObject.SetActive(true);
                        NextClicked();

                        desk.GuildNameChangedEvent.AddListener(DeskGuildNameChanged);
                    }

                    break;
            }
        }
    }
    

    // Next button & general progress through the text
    private void NextClicked()
    {
        _currStringIdx++;

        if (_allText.Count == (_currStringIdx+2)) // the second to last next
        {
            NextButton.transform.GetChild(0).GetComponent<Text>().text = "Close";
        }
        // if the string right now is END, the tutorial has ended.
        if (_allText[_currStringIdx] == "END")
        {
            TutorialBox.SetActive(false);
            
            // set up things for the endgame
            _currObjectiveComplete = false;

            List<Village> vills = Village.GetVillages();

            for (int i = 0; i < vills.Count; ++i)
            {
                vills[i].PlayerBecameKing.AddListener(IncreaseKingCounter);
            }

            EnemyHideout.StartSpawning();
            Village.StartSpawningVillagers();
        }
        // if the string coming after this one is a NEXTOBJECTIVE,
        else if (_allText[_currStringIdx+1] == "NEXTOBJECTIVE")
        {
            // disable the next button
            NextButton.gameObject.SetActive(false);
            // start the objective
            _currObjectiveComplete = false;
            // increment the objective counter
            _objectiveCounter++;
            // set text
            TutorialTextBox.text = _allText[_currStringIdx];
            // increment current string to skip over this one
            _currStringIdx++;

            Debug.Log("Next string is a nextobjective");
        }
        else
        {
            TutorialTextBox.text = _allText[_currStringIdx];
        }
    }



    #region Objectives
    // OBJECTIVE 2
    private void DeskGuildNameChanged()
    {
        // Objective 2 completed
        if (!_currObjectiveComplete && (_objectiveCounter == 2))
        {
            _currObjectiveComplete = true;
            NextButton.gameObject.SetActive(true);
            NextClicked();

            // remove listener
            PlayerController player = GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>();
            GuildManagementDesk desk = player.MyGuild.Desk;

            desk.GuildNameChangedEvent.RemoveListener(DeskGuildNameChanged);

            // add listener for objective 3
            player.GetComponent<QuestHolder>().QuestAcquiredEvent.AddListener(PickedUpAQuest);
        }
    }

    // OBJECTIVE 3
    private void PickedUpAQuest()
    {
        if (!_currObjectiveComplete && (_objectiveCounter == 3))
        {
            _currObjectiveComplete = true;
            NextButton.gameObject.SetActive(true);
            NextClicked();

            // remove listener
            QuestHolder qHolder = GameManager.Instance.PlayerAvatar.GetComponent<QuestHolder>();
            qHolder.QuestAcquiredEvent.RemoveListener(PickedUpAQuest);

            // add listener for objective 4
            qHolder.QuestDoneEvent.AddListener(DoneFirstQuest);
        }
    }

    // OBJECTIVE 4
    private void DoneFirstQuest()
    {
        if (!_currObjectiveComplete && (_objectiveCounter == 4))
        {
            _currObjectiveComplete = true;
            NextButton.gameObject.SetActive(true);
            NextClicked();

            // remove listener
            QuestHolder qHolder = GameManager.Instance.PlayerAvatar.GetComponent<QuestHolder>();
            qHolder.QuestDoneEvent.RemoveListener(DoneFirstQuest);

            // add listener for next quest
            qHolder.QuestCompletedEvent.AddListener(CompletedFirstQuest);
        }
    }

    // OBJECTIVE 5
    private void CompletedFirstQuest()
    {
        if (!_currObjectiveComplete && (_objectiveCounter == 5))
        {
            _currObjectiveComplete = true;
            NextButton.gameObject.SetActive(true);
            NextClicked();

            // remove listener
            QuestHolder qHolder = GameManager.Instance.PlayerAvatar.GetComponent<QuestHolder>();
            qHolder.QuestCompletedEvent.RemoveListener(CompletedFirstQuest);

            // prepare for objective 6: kill an enemy
            GameObject enemy = Instantiate(GameManager.Instance.EnemyPrefab);

            Vector3 pos = qHolder.transform.position;

            // enemy must be slightly near the player
            float range = 30.0f;
            float angle = Random.Range(0, Mathf.PI * 2);
            pos.x += Mathf.Cos(angle) * range;
            pos.z += Mathf.Sin(angle) * range;
            pos.y += 30.0f;

            enemy.transform.position = pos;
            enemy.GetComponent<Health>().onDeath.AddListener(KilledFirstEnemy);
        }
    }

    // OBJECTIVE 6
    private void KilledFirstEnemy(GameObject source)
    {
        if (!_currObjectiveComplete && (_objectiveCounter == 6))
        {
            _currObjectiveComplete = true;
            NextButton.gameObject.SetActive(true);
            NextClicked();

            // no need to remove listener, enemy's dead anyway

            // prepare for next objective
            GuildManagementDesk desk = GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().MyGuild.Desk.GetComponent<GuildManagementDesk>();
            desk.CanSpawnRecruitCards = true;
            desk.RecruitedSomeone.AddListener(RecruitedFirstGuildie);
        }
    }

    // OBJECTIVE 7
    private void RecruitedFirstGuildie()
    {
        if (!_currObjectiveComplete && (_objectiveCounter == 7))
        {
            _currObjectiveComplete = true;
            NextButton.gameObject.SetActive(true);
            NextClicked();

            // remove listener
            GuildManagementDesk desk = GameManager.Instance.PlayerAvatar.GetComponent<PlayerController>().MyGuild.Desk.GetComponent<GuildManagementDesk>();
            desk.RecruitedSomeone.RemoveAllListeners();
        }
    }
    
    // FINAL OBJECTIVE
    private void IncreaseKingCounter()
    {
        _kingCounter++;

        if (_kingCounter == Village.GetVillages().Count)
        {
            // Player has completed the game

            TutorialBox.SetActive(true);
            NextButton.gameObject.SetActive(true); // text should still be "Close"
            TutorialTextBox.text = "Congratulations! You have become King of West Flanders and finished this demo. Feel free to continue roaming the lands that you now all own, or quit the game.";
            NextButton.onClick.RemoveAllListeners();
            NextButton.onClick.AddListener(DefinitivelyCloseTutorialBox);
        }
    }

    // Closing the "You won the game!" box
    private void DefinitivelyCloseTutorialBox()
    {
        TutorialBox.SetActive(false);
    }
    #endregion Objectives
}
