#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class Week1AssetCreator
{
    [MenuItem("Profile7/Create Week 1 - Holloway")]
    public static void CreateWeek1()
    {
        var s = ScriptableObject.CreateInstance<SuspectSO>();
        s.suspectId = "suspect_01";
        s.displayName = "Mark Holloway";
        s.weekNumber = 1;
        s.isGuilty = false;

        // ─── MONDAY: DOSSIER ───
        s.dossierText =
@"MARK HOLLOWAY, 44 years old. Accused of arson of his own warehouse to collect insurance. One worker — Santiago Gomez, 31 — died in the fire.

Holloway was born in Detroit, 1981. First business — an auto repair shop, closed in 2004 after a fire. Insurance payout covered debts. Second business — chemical materials warehouse, opened in 2009 jointly with Victor Salas (50/50 share). Unprofitable until 2022. Sharp revenue growth in 2023 after a municipal contract. Warehouse insured for $2.4 million — twice the appraised value. Reappraisal done in 2021.

Wife — Linda, teacher. Two children. Church parish, volunteering.

SANTIAGO GOMEZ. 31 years old. Honduras, 2019. Worked at Holloway's warehouse since 2021. Sole breadwinner. Three children, youngest — eight months old. Unofficial night shifts — paid in cash.";

        // ─── CONTACTS ───
        s.contacts = new[]
        {
            new ContactData
            {
                contactId = "contact_linda",
                displayName = "Linda Holloway (wife)",
                response = "\"Mark was home after eleven. I heard him come in. These last months he was very tense — the partnership was falling apart. Victor was demanding to buy out his share.\""
            },
            new ContactData
            {
                contactId = "contact_raul",
                displayName = "Raul Espinoza (former employee)",
                response = "\"Holloway was trying to dissolve the partnership. Hired a lawyer back in February. Salas was furious. Three days before the fire they were screaming at each other in the office.\""
            },
            new ContactData
            {
                contactId = "contact_holm",
                displayName = "Peter Holm (neighbor)",
                response = "\"I saw Holloway's car — it left around eleven in the evening. It didn't come back until morning, when the fire department arrived.\""
            },
            new ContactData
            {
                contactId = "contact_salas",
                displayName = "Victor Salas (business partner)",
                response = "\"Mark threatened me. Said he'd rather burn everything down than give me his share. I thought it was just words. Now I don't know.\""
            }
        };

        // ─── TUESDAY: EVIDENCE ───
        s.evidence = new[]
        {
            new EvidenceData
            {
                evidenceId = "evidence_accesslog",
                title = "Warehouse Access Log",
                baseDescription =
@"Electronic access control system. Records for the night of the fire:
- Victor Salas: entry 22:45, exit 23:05
- Mark Holloway: entry 23:14, exit — NOT RECORDED
- Santiago Gomez: entry 23:47, exit — NOT RECORDED",
                expertDescription =
@"Technical analysis of the system revealed a server reboot at 23:10. All events after 22:50 may be incomplete or corrupted. Salas's exit at 23:05 was recorded before the reboot — the only reliable entry. Holloway's and Gomez's exits technically could not have been saved."
            },
            new EvidenceData
            {
                evidenceId = "evidence_firereport",
                title = "Fire Investigation Report",
                baseDescription = "Three simultaneous ignition points. Accelerant used. Industrial solvent. Someone knew the layout of the ventilation shafts.",
                expertDescription =
@"The substance has been identified as TechSolv-7 — a highly specialized industrial solvent sold only to corporate clients under contract. Registry check: 'Salas Industrial LLC' has been an active client of the TechSolv-7 supplier since 2020. Holloway is NOT listed in the registry."
            },
            new EvidenceData
            {
                evidenceId = "evidence_bankstatement",
                title = "Holloway's Bank Statement",
                baseDescription = "Three payments of $4,200 to 'GreenTech Disposal Services' over two months. Company registered two months ago. Legal address — a residential building.",
                expertDescription =
@"'GreenTech' is a shell company with no real activity. Director — Karl Reed, who previously worked as a lawyer at Victor Salas's company. Payments were disguised as legal services — but not a single contract exists. Possible blackmail: Reed may have had compromising information on Holloway (the 2004 incident)."
            }
        };

        // ─── WEDNESDAY: TESTIMONIES ───
        s.testimonies = new[]
        {
            new TestimonyData
            {
                witnessName = "Cole (Senior Investigator)",
                baseTestimony = "\"Motive is obvious: double insurance, collapsing partnership, precedent in 2004. Access log confirms Holloway's presence. Absence of an exit record speaks for itself.\"",
                clarification = "\"I checked GreenTech. Shell company with no contracts. If these are legal expenses — where's the contract? This looks like blackmail. Someone knew about 2004 and had Holloway by the throat.\""
            },
            new TestimonyData
            {
                witnessName = "Paige (Expert Analyst)",
                baseTestimony = "\"Three ignition points — arson. Someone knew the ventilation shaft layout. The log shows two people without exit records. Physically, both could have been in the warehouse at the time of the fire.\"",
                clarification = "\"I rechecked the log. The server was rebooted at 23:10. All exits after 22:50 are technically unreliable. The only confirmed fact — Salas exited before the reboot. Everything else is a data gap, not proof of presence.\""
            },
            new TestimonyData
            {
                witnessName = "Nash (Field Agent)",
                baseTestimony = "\"Salas filed for forced dissolution of the partnership two days before the fire. Under the terms of the agreement — if one partner is convicted of a criminal offense, his share automatically transfers to the other. Holloway apparently didn't know about this.\"",
                clarification = "\"I pulled Gomez's phone records. A week before the fire — seven calls to Salas. Gomez wasn't just working for Holloway. He was Salas's informant inside the warehouse. He knew the schedule, knew who comes at night.\""
            }
        };

        // ─── THURSDAY: INTERROGATION ───
        s.standardQuestions = new[]
        {
            new InterrogationQA
            {
                question = "Where were you on the night of the fire?",
                answer = "\"At the warehouse until about eleven — checking documents for the lawyer. Then home. Wife heard me come in. Found out about the fire from the firefighters around 2 AM.\""
            },
            new InterrogationQA
            {
                question = "You knew the insurance was twice the appraised value?",
                answer = "\"The reappraisal was in 2021. Victor himself suggested it — said it's standard practice during expansion.\""
            },
            new InterrogationQA
            {
                question = "GreenTech Disposal payments — what are those?",
                answer = "\"Legal expenses. I'm dissolving the partnership with Salas. The lawyer works through that company.\""
            },
            new InterrogationQA
            {
                question = "Did you know Gomez would be at the warehouse that night?",
                answer = "\"No. He wasn't on the schedule. This haunts me.\" (Pause. Stares at the table.)"
            }
        };

        s.conditionalQuestions = new[]
        {
            new ConditionalInterrogationQA
            {
                requiredChoiceType = ChoiceType.Contact, requiredChoiceId = "contact_linda",
                question = "Your wife says Victor was demanding your share. Why didn't you say this earlier?",
                answer = "\"Because it looks like a motive. But my motive was to leave, not to burn. I wanted to sell my share — Victor refused to pay market price.\""
            },
            new ConditionalInterrogationQA
            {
                requiredChoiceType = ChoiceType.Contact, requiredChoiceId = "contact_raul",
                question = "You hired a lawyer in February to dissolve the partnership. Why hide it?",
                answer = "\"I wasn't hiding it. Just didn't say right away. It's a civil matter, not criminal. I wanted to leave legally.\""
            },
            new ConditionalInterrogationQA
            {
                requiredChoiceType = ChoiceType.Contact, requiredChoiceId = "contact_holm",
                question = "Neighbor saw your car leaving around 23:00. Access log shows your entry at 23:14. How did you leave before you entered?",
                answer = "(Pause. Looks away.) \"I... don't know how their system works. Maybe I entered again — forgot something in the office, it happens. Or the neighbor got the time wrong. Was he looking at a clock?\""
            },
            new ConditionalInterrogationQA
            {
                requiredChoiceType = ChoiceType.Contact, requiredChoiceId = "contact_salas",
                question = "Salas says you threatened to burn the warehouse. Is that true?",
                answer = "(Long pause. Looks straight ahead.) \"I said: 'I'd rather burn everything than give it to you for nothing.' It was said in anger. I'm not an arsonist.\""
            },
            new ConditionalInterrogationQA
            {
                requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_accesslog",
                question = "The access system was rebooted at 23:10. Your exit technically couldn't have been saved. Did you know this?",
                answer = "\"No. But that explains why there's no record. I left before the reboot — or around that time. Exactly then.\""
            },
            new ConditionalInterrogationQA
            {
                requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_firereport",
                question = "TechSolv-7 is only sold to corporate clients. Only Salas is in the registry. How did this substance end up at your warehouse?",
                answer = "(Pause. Holloway goes pale.) \"I... didn't buy that substance. If it was there — Victor brought it. He had access. He left at 23:05.\""
            },
            new ConditionalInterrogationQA
            {
                requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_bankstatement",
                question = "GreenTech is linked to Salas's lawyer. Were you paying a blackmailer or hiring him against Salas?",
                answer = "(Silence. Then quietly.) \"Karl Reed knew about 2004. Knew details that could destroy me. Victor apparently found him and... yes. I was paying. But that doesn't make me an arsonist.\""
            },
            new ConditionalInterrogationQA
            {
                requiredChoiceType = ChoiceType.Testimony, requiredChoiceId = "Cole (Senior Investigator)",
                question = "Cole thinks GreenTech is blackmail. Who was blackmailing you and why?",
                answer = "(Long pause.) \"Someone who knew what happened in 2004. The details. I thought it was over. It wasn't.\""
            },
            new ConditionalInterrogationQA
            {
                requiredChoiceType = ChoiceType.Testimony, requiredChoiceId = "Paige (Expert Analyst)",
                question = "Paige confirms: the log is unreliable after 22:50. No proof you were there when the fire started at 01:30. Where were you between 23:14 and 2 AM?",
                answer = "\"Home. Wife heard me. I went to bed around midnight. That's an hour and a half between me leaving and the fire. Someone set it after me.\""
            },
            new ConditionalInterrogationQA
            {
                requiredChoiceType = ChoiceType.Testimony, requiredChoiceId = "Nash (Field Agent)",
                question = "Gomez called Salas seven times the week before the fire. Did you know your worker was working for your partner?",
                answer = "(Stands up. Sits back down.) \"What? No. Gomez... Gomez was a good man. If Victor used him as an informant — he knew Gomez would be there that night. He knew.\" (Voice breaks.)"
            }
        };

        // Follow-up questions
        s.followUps = new[]
        {
            new FollowUpData
            {
                followUpId = "followup_benefit",
                question = "Who really benefits from your conviction?",
                answer = "\"Victor. If I'm convicted — my share goes to him automatically. That clause was in the partnership agreement. I only found out about it from my lawyer... after the fire.\" (Quiet, bitter laugh.)"
            },
            new FollowUpData
            {
                followUpId = "followup_2004",
                question = "What really happened in 2004?",
                answer = "\"The auto shop... the wiring was old. Insurance ruled it accidental. But someone always thought otherwise. Karl Reed found the original inspection report — the one that was never filed. He's been using it ever since.\""
            },
            new FollowUpData
            {
                followUpId = "followup_gomez",
                question = "If you could say one thing to Gomez's family, what would it be?",
                answer = "(Long silence. Eyes fill with tears.) \"That I'm sorry. That he shouldn't have been there. That someone put him in danger knowingly. And it wasn't me.\" (Looks away. Interview over.)"
            }
        };

        // ─── CONSEQUENCES ───
        s.consequenceGuilty = "Mark Holloway was sentenced to 12 years. Victor Salas assumed full ownership of the business within 48 hours of the verdict. Linda Holloway filed an appeal. The investigation into Salas for arson was closed due to insufficient evidence.";
        s.consequenceNotGuilty = "The investigation shifted to Victor Salas. TechSolv-7 purchases made a week before the fire were discovered. Karl Reed cooperated with the investigation. The case was referred to the prosecutor's office.";

        // Save asset
        const string folder = "Assets/Resources/Suspects";
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/Resources", "Suspects");

        AssetDatabase.CreateAsset(s, $"{folder}/suspect_01_holloway.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = s;
        Debug.Log("Week 1 (Holloway) asset created at " + folder);
    }
}
#endif
