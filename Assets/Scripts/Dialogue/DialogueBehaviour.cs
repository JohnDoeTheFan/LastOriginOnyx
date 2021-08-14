using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueBehaviour : MonoBehaviour
{
    [SerializeField]
    private DialogueGui dialogueGui;

    private DialogueSet dialogueSet;
    private DialogueDeck dialogueDeck;
    private DialogueSet.Dialogue dialogue;
    private int dialogueIndex;
    private int setIndex;

    private bool isPreserved = false;
    public bool IsAvailable => !isPreserved;

    private bool HasNextDialogue => dialogue != null;

    private void Update()
    {
        if(Input.GetKeyDown("z"))
        {
            dialogueGui.SkipLine();
        }
    }

    public bool StartDialogue(DialogueSet.Dialogue dialogue, Action onEnd)
    {
        void dialogueSetting()
        {
            dialogueDeck = null;
            dialogueSet = null;
            this.dialogue = dialogue;
        };

        return StartDialogueRoutine(dialogueSetting, onEnd);
    }

    public bool StartDialogueSet(DialogueSet set, Action onEnd)
    {
        void dialogueSetting()
        {
            dialogueDeck = null;
            dialogueSet = set;
            dialogue = dialogueSet.dialogues[0];
        }

        return StartDialogueRoutine(dialogueSetting, onEnd);
    }

    public bool StartDialogueDeck(DialogueDeck deck, Action onEnd)
    {
        void dialogueSetting()
        {
            dialogueDeck = deck;
            dialogueSet = dialogueDeck.dialogSets[0];
            dialogue = dialogueSet.dialogues[0];
        };

        return StartDialogueRoutine(dialogueSetting, onEnd);
    }

    private bool StartDialogueRoutine(Action dialogueSetting, Action onEnd)
    {
        if (IsAvailable)
        {
            dialogueSetting();

            dialogueIndex = 1;
            setIndex = 1;
            StartCoroutine(DialogueRoutine(onEnd));

            return true;
        }
        else
            return false;
    }

    private class Line : DialogueGui.ILine
    {
        readonly DialogueSet.Dialogue dialogue;
        public Line(DialogueSet.Dialogue dialogue)
        {
            this.dialogue = dialogue;
        }

        string DialogueGui.ILine.SpeakerName => dialogue.name;

        string DialogueGui.ILine.Content => dialogue.text;

        float DialogueGui.ILine.UpdateDuration => dialogue.updateDuration;
    }

    private IEnumerator DialogueRoutine(Action onEnd)
    {
        isPreserved = true;
        while(HasNextDialogue)
        {
            bool isLineFinished = false;
            dialogueGui.StartLine(new Line(GetDialogue()), () => isLineFinished = true);
            yield return new WaitUntil(() => isLineFinished);
            yield return null;
            yield return new WaitUntil(() => Input.GetKeyDown("z"));
        }

        isPreserved = false;
        dialogueGui.HideGui();
        onEnd();
    }

    private DialogueSet.Dialogue GetDialogue()
    {
        DialogueSet.Dialogue retVal = dialogue;

        NextDialogue();

        return retVal;
    }

    private void NextDialogue()
    {
        if (dialogueSet != null)
        {
            if (dialogueSet.dialogues.Length > dialogueIndex)
            {
                dialogue = dialogueSet.dialogues[dialogueIndex];
                dialogueIndex++;
            }
            else
            {
                NextSet();
            }
        }
    }

    private void NextSet()
    {
        if(dialogueDeck != null)
        {
            if(dialogueDeck.dialogSets.Length > setIndex)
            {
                dialogueSet = dialogueDeck.dialogSets[setIndex];
                setIndex++;

                dialogue = dialogueSet.dialogues[0];
                dialogueIndex = 1;
            }
            else
            {
                dialogueSet = null;
                dialogue = null;
            }
        }
    }


}