#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CaseAssetCreator
{
    [MenuItem("Assets/Create/Profile7/Case 1 — Test Case")]
    static void CreateTestCase()
    {
        var c = ScriptableObject.CreateInstance<CaseSO>();
        c.caseId = "case_test";
        c.displayName = "Убийство в офисе";
        c.caseNumber = 1;
        c.totalMoves = 8;
        c.trueCulpritId = "zotov";

        c.briefingText = "В офисе компании «Астра» найден мёртвым главный бухгалтер Семёнов. " +
            "Охранник обнаружил тело утром. Дверь была заперта изнутри. " +
            "На столе — разбросанные документы финансовой проверки.";

        // Persons
        c.persons = new CasePersonData[]
        {
            new() { personId = "zotov", displayName = "Зотов А.В.", role = PersonRole.Suspect,
                hiddenAgenda = HiddenAgenda.Deflecting,
                description = "Заместитель директора, 45 лет. Нервничает. Первым сообщил о пропаже ключей." },
            new() { personId = "marina", displayName = "Марина Семёнова", role = PersonRole.Witness,
                hiddenAgenda = HiddenAgenda.Covering,
                description = "Жена погибшего. Даёт алиби мужу на вечер — он якобы был дома." },
            new() { personId = "petrov", displayName = "Петров И.К.", role = PersonRole.Suspect,
                hiddenAgenda = HiddenAgenda.SelfPreserving,
                description = "Охранник ночной смены. Говорит что ничего не слышал." },
            new() { personId = "lisa", displayName = "Лиза Волкова", role = PersonRole.Witness,
                hiddenAgenda = HiddenAgenda.Honest,
                description = "Секретарь. Видела Зотова в офисе поздно вечером." }
        };

        // Interrogations
        c.interrogations = new CaseInterrogationData[]
        {
            new() { targetPersonId = "zotov", questions = new InterrogationQuestionData[]
            {
                new() { questionText = "Где вы были вечером?",
                    answerText = "Уехал домой в 18:00. Больше в офис не возвращался.",
                    isLie = true, truthText = "Был в офисе до 22:00.",
                    revealedFragmentId = "frag_zotov_alibi" },
                new() { questionText = "Зачем вам были нужны финансовые документы?",
                    answerText = "Рутинная проверка квартальных отчётов. Ничего необычного.",
                    isLie = true, truthText = "Документы вскрывали схему хищений.",
                    revealedFragmentId = "frag_finance_motive" },
                new() { questionText = "Кто имел доступ к офису Семёнова?",
                    answerText = "Только я, охранник и сам Семёнов. Уборщица — утром.",
                    isLie = false, truthText = "",
                    revealedFragmentId = "frag_access" }
            }},
            new() { targetPersonId = "petrov", questions = new InterrogationQuestionData[]
            {
                new() { questionText = "Что происходило ночью?",
                    answerText = "Тишина. Никто не входил после 20:00.",
                    isLie = true, truthText = "Видел как кто-то выходил около 23:00.",
                    revealedFragmentId = "frag_petrov_night" },
                new() { questionText = "Вы проверяли камеры?",
                    answerText = "Камеры не работали. Электрик обещал починить.",
                    isLie = false, truthText = "",
                    revealedFragmentId = "frag_cameras" }
            }},
            new() { targetPersonId = "marina", questions = new InterrogationQuestionData[]
            {
                new() { questionText = "Муж был дома вечером?",
                    answerText = "Да, пришёл в 19:00. Мы ужинали вместе.",
                    isLie = false, truthText = "",
                    revealedFragmentId = "frag_marina_alibi" },
                new() { questionText = "Были ли у мужа враги на работе?",
                    answerText = "Он говорил что Зотов давит... Что-то про документы...",
                    isLie = false, truthText = "",
                    revealedFragmentId = "frag_conflict" }
            }}
        };

        // Locations
        c.locations = new LocationData[]
        {
            new() { locationId = "office", displayName = "Кабинет Семёнова",
                description = "Кабинет на 3 этаже. Дверь была заперта изнутри.",
                zones = new LocationZoneData[]
                {
                    new() { zoneName = "Стол", description = "Разбросаны документы финансовой проверки. Под одним листом — записка: «Зотов — 12 млн».",
                        revealedFragmentId = "frag_note_12m" },
                    new() { zoneName = "Окно", description = "Окно приоткрыто. На подоконнике — свежие царапины. Можно вылезти на пожарную лестницу.",
                        revealedFragmentId = "frag_window" },
                    new() { zoneName = "Замок", description = "Дверной замок исправен. Но запасной ключ — у заместителя.",
                        revealedFragmentId = "frag_spare_key" }
                }},
            new() { locationId = "parking", displayName = "Парковка",
                description = "Подземная парковка. Камеры не работают.",
                zones = new LocationZoneData[]
                {
                    new() { zoneName = "Место Зотова", description = "Машина Зотова стояла на месте до 23:15 по журналу выезда.",
                        revealedFragmentId = "frag_car_log" },
                    new() { zoneName = "Журнал въезда", description = "Зотов: выезд 23:17. Семёнов: въезд 17:30, выезда нет.",
                        revealedFragmentId = "frag_parking_log" }
                }}
        };

        // Database queries
        c.databaseQueries = new DatabaseQueryData[]
        {
            new() { queryId = "db_zotov_finance", displayName = "Финансы Зотова",
                resultText = "На счету Зотова — переводы на 12 млн от подставных фирм за последний год. Источник — бюджет «Астры».",
                revealedFragmentId = "frag_zotov_money" },
            new() { queryId = "db_petrov_record", displayName = "Личное дело Петрова",
                resultText = "Петров — бывший полицейский. Уволен за превышение полномочий. Судимостей нет.",
                revealedFragmentId = "frag_petrov_record" },
            new() { queryId = "db_semenov_threats", displayName = "Угрозы Семёнову",
                resultText = "Семёнов подавал заявление об угрозах 2 месяца назад. Заявление отозвано. Угрожавший не установлен.",
                revealedFragmentId = "frag_threats" }
        };

        // Confrontations
        c.confrontations = new ConfrontationData[]
        {
            new() { personA = "zotov", personB = "lisa",
                resultText = "Лиза: «Я видела вас в офисе в 21:30.» Зотов бледнеет: «Да... я заходил... забрал документы... но Семёнов был жив!»",
                whoBreaks = "zotov",
                revealedFragmentId = "frag_zotov_lied" },
            new() { personA = "petrov", personB = "marina",
                resultText = "Марина: «Муж звонил в 22:00 с работы.» Петров: «Значит он был ещё жив в 22:00...» Молчание.",
                whoBreaks = "petrov",
                revealedFragmentId = "frag_timeline_22" }
        };

        // Deduction fragments
        c.fragments = new DeductionFragmentData[]
        {
            // Motives
            new() { fragmentId = "frag_finance_motive", displayText = "Зотов похищал деньги, Семёнов узнал",
                fragmentType = FragmentType.Motive, isTrue = true, relatedPersonId = "zotov" },
            new() { fragmentId = "frag_conflict", displayText = "Зотов давил на Семёнова из-за документов",
                fragmentType = FragmentType.Motive, isTrue = false, relatedPersonId = "zotov" },
            new() { fragmentId = "frag_threats", displayText = "Семёнову угрожали неизвестные",
                fragmentType = FragmentType.Motive, isTrue = false, relatedPersonId = "" },

            // Opportunities
            new() { fragmentId = "frag_access", displayText = "Доступ имели Зотов, охранник и сам Семёнов",
                fragmentType = FragmentType.Opportunity, isTrue = false, relatedPersonId = "" },
            new() { fragmentId = "frag_spare_key", displayText = "Запасной ключ у Зотова",
                fragmentType = FragmentType.Opportunity, isTrue = true, relatedPersonId = "zotov" },
            new() { fragmentId = "frag_window", displayText = "Окно приоткрыто, есть пожарная лестница",
                fragmentType = FragmentType.Opportunity, isTrue = false, relatedPersonId = "" },
            new() { fragmentId = "frag_cameras", displayText = "Камеры не работали",
                fragmentType = FragmentType.Opportunity, isTrue = false, relatedPersonId = "petrov" },

            // Evidence
            new() { fragmentId = "frag_note_12m", displayText = "Записка «Зотов — 12 млн»",
                fragmentType = FragmentType.Evidence, isTrue = true, relatedPersonId = "zotov" },
            new() { fragmentId = "frag_zotov_money", displayText = "12 млн на счетах Зотова от подставных фирм",
                fragmentType = FragmentType.Evidence, isTrue = true, relatedPersonId = "zotov" },
            new() { fragmentId = "frag_car_log", displayText = "Машина Зотова стояла до 23:15",
                fragmentType = FragmentType.Evidence, isTrue = true, relatedPersonId = "zotov" },
            new() { fragmentId = "frag_parking_log", displayText = "Зотов выехал в 23:17, Семёнов не выезжал",
                fragmentType = FragmentType.Evidence, isTrue = true, relatedPersonId = "zotov" },
            new() { fragmentId = "frag_zotov_lied", displayText = "Зотов солгал — был в офисе до 21:30+",
                fragmentType = FragmentType.Evidence, isTrue = true, relatedPersonId = "zotov" },

            // Suspects
            new() { fragmentId = "frag_suspect_zotov", displayText = "Зотов А.В. — заместитель, имел мотив и доступ",
                fragmentType = FragmentType.Suspect, isTrue = true, relatedPersonId = "zotov" },
            new() { fragmentId = "frag_suspect_petrov", displayText = "Петров — охранник, был на месте всю ночь",
                fragmentType = FragmentType.Suspect, isTrue = false, relatedPersonId = "petrov" },

            // Extra fragments from other investigations
            new() { fragmentId = "frag_zotov_alibi", displayText = "Зотов утверждает что уехал в 18:00",
                fragmentType = FragmentType.Evidence, isTrue = false, relatedPersonId = "zotov" },
            new() { fragmentId = "frag_petrov_night", displayText = "Петров: «Никто не входил после 20:00»",
                fragmentType = FragmentType.Evidence, isTrue = false, relatedPersonId = "petrov" },
            new() { fragmentId = "frag_marina_alibi", displayText = "Жена: Семёнов пришёл домой в 19:00",
                fragmentType = FragmentType.Evidence, isTrue = false, relatedPersonId = "marina" },
            new() { fragmentId = "frag_petrov_record", displayText = "Петров — бывший полицейский, уволен",
                fragmentType = FragmentType.Evidence, isTrue = false, relatedPersonId = "petrov" },
            new() { fragmentId = "frag_timeline_22", displayText = "Семёнов был жив в 22:00 (звонил жене)",
                fragmentType = FragmentType.Evidence, isTrue = true, relatedPersonId = "" }
        };

        // Correct chain
        c.correctMotiveId = "frag_finance_motive";
        c.correctOpportunityId = "frag_spare_key";
        c.correctEvidenceId = "frag_note_12m"; // any true evidence works
        c.correctSuspectId = "frag_suspect_zotov";

        // Consequences
        c.consequenceCorrectArrest = new CaseConsequenceData
        {
            headlineText = "АРЕСТ ЗОТОВА: Заместитель директора «Астры» арестован за убийство и хищение 12 млн",
            detailText = "Следствие подтвердило версию. Зотов дал признательные показания."
        };
        c.consequenceWrongArrest = new CaseConsequenceData
        {
            headlineText = "СКАНДАЛ: Арестован невиновный. Настоящий убийца остаётся на свободе.",
            detailText = "Зотов продолжает работать. Хищения возобновились."
        };
        c.consequenceUnsolved = new CaseConsequenceData
        {
            headlineText = "ПОЗОР ПОЛИЦИИ: Дело об убийстве бухгалтера «Астры» остаётся нераскрытым.",
            detailText = "Пресса требует отставки. Общественность в ярости."
        };
        c.consequenceWeakCase = new CaseConsequenceData
        {
            headlineText = "ПРОВАЛ В СУДЕ: Адвокат Зотова разрушил обвинение. Подозреваемый освобождён.",
            detailText = "Недостаточно улик. Зотов вернулся в офис и уничтожил оставшиеся документы."
        };

        string path = "Assets/Resources/Cases/Case1_Test.asset";
        AssetDatabase.CreateAsset(c, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = c;
        Debug.Log($"Created test case at {path}");
    }

    // ════════════════════════════════════════════════════
    // ДЕЛО 2 — Отравление в ресторане «Каштан»
    // ════════════════════════════════════════════════════
    [MenuItem("Assets/Create/Profile7/Case 2 — Отравление")]
    static void CreateCase2()
    {
        var c = ScriptableObject.CreateInstance<CaseSO>();
        c.caseId = "case_poison";
        c.displayName = "Отравление в ресторане";
        c.caseNumber = 2;
        c.totalMoves = 8;
        c.trueCulpritId = "kravtsova";

        c.briefingText = "Шеф-повар ресторана «Каштан» Григорий Лунёв скончался во время дегустации нового меню. " +
            "Предварительная причина — отравление. На кухне присутствовали четверо. " +
            "Ресторан закрыт, все участники дегустации задержаны для опроса.";

        c.persons = new CasePersonData[]
        {
            new() { personId = "kravtsova", displayName = "Кравцова Н.Д.", role = PersonRole.Suspect,
                hiddenAgenda = HiddenAgenda.Deflecting,
                description = "Совладелица ресторана, 38 лет. Спокойна. Говорит что Лунёв был её другом." },
            new() { personId = "taras", displayName = "Тарас Шульга", role = PersonRole.Suspect,
                hiddenAgenda = HiddenAgenda.SelfPreserving,
                description = "Су-шеф, 29 лет. Амбициозный. Хотел занять место Лунёва." },
            new() { personId = "diana", displayName = "Диана Рокотова", role = PersonRole.Witness,
                hiddenAgenda = HiddenAgenda.Honest,
                description = "Сомелье, работает 3 года. Подавала вино к дегустации." },
            new() { personId = "efimov", displayName = "Ефимов Р.С.", role = PersonRole.Witness,
                hiddenAgenda = HiddenAgenda.Covering,
                description = "Ресторанный критик, был приглашён на дегустацию. Знакомый Кравцовой." }
        };

        c.interrogations = new CaseInterrogationData[]
        {
            new() { targetPersonId = "kravtsova", questions = new InterrogationQuestionData[]
            {
                new() { questionText = "Каковы ваши отношения с Лунёвым?",
                    answerText = "Деловые партнёры и друзья. Я вложила в ресторан всё что имела.",
                    isLie = true, truthText = "Ресторан убыточен. Лунёв отказывался менять концепцию.",
                    revealedFragmentId = "c2_frag_partnership" },
                new() { questionText = "Кто готовил блюда для дегустации?",
                    answerText = "Лунёв лично. Тарас помогал с соусами. Я не заходила на кухню.",
                    isLie = true, truthText = "Кравцова была на кухне и добавляла специи в одно из блюд.",
                    revealedFragmentId = "c2_frag_kitchen_lie" },
                new() { questionText = "Кому выгодна смерть Лунёва?",
                    answerText = "Никому. Лунёв — лицо ресторана. Без него мы закроемся.",
                    isLie = true, truthText = "Страховка на Лунёва — 8 млн. Получатель — Кравцова.",
                    revealedFragmentId = "c2_frag_insurance_motive" }
            }},
            new() { targetPersonId = "taras", questions = new InterrogationQuestionData[]
            {
                new() { questionText = "Вы хотели стать главным шефом?",
                    answerText = "Да, не скрываю. Но не ценой убийства. Лунёв обещал уйти через год.",
                    isLie = false, truthText = "",
                    revealedFragmentId = "c2_frag_taras_ambition" },
                new() { questionText = "Кто имел доступ к ингредиентам?",
                    answerText = "Я, Лунёв и Кравцова. Она заходила утром, принесла специи от поставщика.",
                    isLie = false, truthText = "",
                    revealedFragmentId = "c2_frag_spice_delivery" }
            }},
            new() { targetPersonId = "diana", questions = new InterrogationQuestionData[]
            {
                new() { questionText = "Вы видели что-то необычное?",
                    answerText = "Кравцова принесла специи в отдельном пакете. Обычно поставщик привозит сам.",
                    isLie = false, truthText = "",
                    revealedFragmentId = "c2_frag_diana_saw" },
                new() { questionText = "Как себя чувствовал Лунёв до дегустации?",
                    answerText = "Нормально. Но после третьего блюда побледнел. Я вызвала скорую.",
                    isLie = false, truthText = "",
                    revealedFragmentId = "c2_frag_third_dish" }
            }}
        };

        c.locations = new LocationData[]
        {
            new() { locationId = "kitchen", displayName = "Кухня ресторана",
                description = "Профессиональная кухня. Всё стерильно, но один рабочий стол не убран.",
                zones = new LocationZoneData[]
                {
                    new() { zoneName = "Рабочий стол", description = "Следы порошка рядом с третьим блюдом. Не похоже на обычные специи — слишком мелкий помол.",
                        revealedFragmentId = "c2_frag_powder" },
                    new() { zoneName = "Мусорное ведро", description = "Пустой пакетик без маркировки. Внутри — остатки белого порошка.",
                        revealedFragmentId = "c2_frag_packet" },
                    new() { zoneName = "Холодильник", description = "Все ингредиенты промаркированы. Ничего подозрительного.",
                        revealedFragmentId = "c2_frag_fridge_clean" }
                }},
            new() { locationId = "office_r", displayName = "Кабинет управляющего",
                description = "Маленький кабинет за кухней. Бумаги, компьютер, сейф.",
                zones = new LocationZoneData[]
                {
                    new() { zoneName = "Бумаги на столе", description = "Финансовые отчёты. Ресторан в минусе 3 месяца подряд. Банк требует погашения кредита.",
                        revealedFragmentId = "c2_frag_debt" },
                    new() { zoneName = "Компьютер", description = "Открыта почта Кравцовой. Письмо страховой: «Полис на жизнь Лунёва Г.А., сумма 8 000 000 руб.»",
                        revealedFragmentId = "c2_frag_insurance_doc" }
                }}
        };

        c.databaseQueries = new DatabaseQueryData[]
        {
            new() { queryId = "c2_db_poison", displayName = "Токсикология — результаты",
                resultText = "В крови Лунёва обнаружен таллий — тяжёлый металл. Смертельная доза. Подмешан в пищу.",
                revealedFragmentId = "c2_frag_thallium" },
            new() { queryId = "c2_db_kravtsova", displayName = "Финансы Кравцовой",
                resultText = "Кравцова — единственный бенефициар страхового полиса Лунёва. Полис оформлен 4 месяца назад.",
                revealedFragmentId = "c2_frag_beneficiary" },
            new() { queryId = "c2_db_taras_record", displayName = "Личное дело Шульги",
                resultText = "Шульга чист. Ранее работал в двух ресторанах. Конфликтов не зафиксировано.",
                revealedFragmentId = "c2_frag_taras_clean" }
        };

        c.confrontations = new ConfrontationData[]
        {
            new() { personA = "kravtsova", personB = "diana",
                resultText = "Диана: «Вы сами принесли специи утром, я видела.» Кравцова: «Это были обычные... от нового поставщика...» Диана: «Какого поставщика? Покажите накладную.» Кравцова молчит.",
                whoBreaks = "kravtsova",
                revealedFragmentId = "c2_frag_no_invoice" },
            new() { personA = "taras", personB = "efimov",
                resultText = "Ефимов: «Я видел как Кравцова что-то сыпала в соус.» Тарас: «Я тоже видел! Думал это приправа.» Ефимов побледнел — он знал и молчал.",
                whoBreaks = "efimov",
                revealedFragmentId = "c2_frag_efimov_knew" }
        };

        c.fragments = new DeductionFragmentData[]
        {
            // Motives
            new() { fragmentId = "c2_frag_insurance_motive", displayText = "Страховка 8 млн на Лунёва — получатель Кравцова",
                fragmentType = FragmentType.Motive, isTrue = true, relatedPersonId = "kravtsova" },
            new() { fragmentId = "c2_frag_debt", displayText = "Ресторан в долгах, банк требует погашения",
                fragmentType = FragmentType.Motive, isTrue = true, relatedPersonId = "kravtsova" },
            new() { fragmentId = "c2_frag_taras_ambition", displayText = "Тарас хотел занять место шефа",
                fragmentType = FragmentType.Motive, isTrue = false, relatedPersonId = "taras" },
            new() { fragmentId = "c2_frag_partnership", displayText = "Кравцова называет Лунёва другом и партнёром",
                fragmentType = FragmentType.Motive, isTrue = false, relatedPersonId = "kravtsova" },

            // Opportunities
            new() { fragmentId = "c2_frag_spice_delivery", displayText = "Кравцова принесла специи от «поставщика» утром",
                fragmentType = FragmentType.Opportunity, isTrue = true, relatedPersonId = "kravtsova" },
            new() { fragmentId = "c2_frag_kitchen_lie", displayText = "Кравцова была на кухне — хотя утверждает обратное",
                fragmentType = FragmentType.Opportunity, isTrue = true, relatedPersonId = "kravtsova" },
            new() { fragmentId = "c2_frag_third_dish", displayText = "Лунёву стало плохо после третьего блюда",
                fragmentType = FragmentType.Opportunity, isTrue = false, relatedPersonId = "" },
            new() { fragmentId = "c2_frag_fridge_clean", displayText = "Холодильник чист — яд не в основных ингредиентах",
                fragmentType = FragmentType.Opportunity, isTrue = false, relatedPersonId = "" },

            // Evidence
            new() { fragmentId = "c2_frag_thallium", displayText = "Причина смерти — таллий в пище",
                fragmentType = FragmentType.Evidence, isTrue = true, relatedPersonId = "" },
            new() { fragmentId = "c2_frag_powder", displayText = "Следы неизвестного порошка у третьего блюда",
                fragmentType = FragmentType.Evidence, isTrue = true, relatedPersonId = "kravtsova" },
            new() { fragmentId = "c2_frag_packet", displayText = "Пустой пакетик без маркировки в мусоре",
                fragmentType = FragmentType.Evidence, isTrue = true, relatedPersonId = "kravtsova" },
            new() { fragmentId = "c2_frag_insurance_doc", displayText = "Страховой полис на компьютере Кравцовой",
                fragmentType = FragmentType.Evidence, isTrue = true, relatedPersonId = "kravtsova" },
            new() { fragmentId = "c2_frag_beneficiary", displayText = "Кравцова — единственный бенефициар полиса",
                fragmentType = FragmentType.Evidence, isTrue = true, relatedPersonId = "kravtsova" },
            new() { fragmentId = "c2_frag_no_invoice", displayText = "Нет накладной на «специи» — Кравцова не может объяснить",
                fragmentType = FragmentType.Evidence, isTrue = true, relatedPersonId = "kravtsova" },
            new() { fragmentId = "c2_frag_diana_saw", displayText = "Диана видела необычный пакет со специями",
                fragmentType = FragmentType.Evidence, isTrue = false, relatedPersonId = "kravtsova" },
            new() { fragmentId = "c2_frag_efimov_knew", displayText = "Ефимов видел как Кравцова сыпала порошок — и молчал",
                fragmentType = FragmentType.Evidence, isTrue = false, relatedPersonId = "efimov" },
            new() { fragmentId = "c2_frag_taras_clean", displayText = "Шульга чист — нет мотива и судимостей",
                fragmentType = FragmentType.Evidence, isTrue = false, relatedPersonId = "taras" },

            // Suspects
            new() { fragmentId = "c2_frag_suspect_kravtsova", displayText = "Кравцова Н.Д. — совладелица, страховка, доступ к кухне",
                fragmentType = FragmentType.Suspect, isTrue = true, relatedPersonId = "kravtsova" },
            new() { fragmentId = "c2_frag_suspect_taras", displayText = "Тарас Шульга — су-шеф, амбиции, доступ к еде",
                fragmentType = FragmentType.Suspect, isTrue = false, relatedPersonId = "taras" }
        };

        c.correctMotiveId = "c2_frag_insurance_motive";
        c.correctOpportunityId = "c2_frag_spice_delivery";
        c.correctEvidenceId = "c2_frag_powder";
        c.correctSuspectId = "c2_frag_suspect_kravtsova";

        c.consequenceCorrectArrest = new CaseConsequenceData
        {
            headlineText = "АРЕСТ КРАВЦОВОЙ: Совладелица ресторана «Каштан» отравила шеф-повара ради страховки",
            detailText = "Экспертиза подтвердила таллий в специях. Кравцова задержана при попытке обналичить полис."
        };
        c.consequenceWrongArrest = new CaseConsequenceData
        {
            headlineText = "ОШИБКА СЛЕДСТВИЯ: Арестован невиновный. Кравцова покинула страну.",
            detailText = "Настоящая отравительница скрылась. Страховка обналичена через подставное лицо."
        };
        c.consequenceUnsolved = new CaseConsequenceData
        {
            headlineText = "ДЕЛО ЗАКРЫТО: Смерть шеф-повара списана на «несчастный случай».",
            detailText = "Общественность возмущена. Ресторан закрыт, но виновных нет."
        };
        c.consequenceWeakCase = new CaseConsequenceData
        {
            headlineText = "ОПРАВДАНИЕ: Адвокат Кравцовой добился освобождения — улик недостаточно.",
            detailText = "Кравцова на свободе. Полис заморожен, но она планирует повторную попытку взыскания."
        };

        string path = "Assets/Resources/Cases/Case2_Poison.asset";
        AssetDatabase.CreateAsset(c, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = c;
        Debug.Log($"Created case 2 at {path}");
    }

    // ════════════════════════════════════════════════════
    // ДЕЛО 3 — Поджог городского архива
    // ════════════════════════════════════════════════════
    [MenuItem("Assets/Create/Profile7/Case 3 — Поджог")]
    static void CreateCase3()
    {
        var c = ScriptableObject.CreateInstance<CaseSO>();
        c.caseId = "case_arson";
        c.displayName = "Пожар в архиве";
        c.caseNumber = 3;
        c.totalMoves = 9;
        c.trueCulpritId = "glumov";

        c.briefingText = "Городской архив сгорел ночью. Пожарные нашли следы поджога — ускоритель горения. " +
            "Архивариус Надежда Фомина госпитализирована с ожогами — она была внутри. " +
            "В архиве хранились документы земельного кадастра за последние 20 лет.";

        c.persons = new CasePersonData[]
        {
            new() { personId = "glumov", displayName = "Глумов В.П.", role = PersonRole.Suspect,
                hiddenAgenda = HiddenAgenda.Deflecting,
                description = "Чиновник земельного комитета, 52 года. Уверенно держится. Говорит что узнал о пожаре из новостей." },
            new() { personId = "fomina", displayName = "Надежда Фомина", role = PersonRole.Witness,
                hiddenAgenda = HiddenAgenda.Honest,
                description = "Архивариус, 61 год. Госпитализирована. Вернулась за забытыми ключами и застала пожар." },
            new() { personId = "belov", displayName = "Белов К.А.", role = PersonRole.Suspect,
                hiddenAgenda = HiddenAgenda.SelfPreserving,
                description = "Застройщик, 40 лет. Его компания ведёт строительство на спорных участках." },
            new() { personId = "rita", displayName = "Рита Чернова", role = PersonRole.Witness,
                hiddenAgenda = HiddenAgenda.Covering,
                description = "Секретарь земельного комитета. Работает на Глумова 7 лет." }
        };

        c.interrogations = new CaseInterrogationData[]
        {
            new() { targetPersonId = "glumov", questions = new InterrogationQuestionData[]
            {
                new() { questionText = "Зачем кому-то жечь архив?",
                    answerText = "Понятия не имею. Может бездомные грелись. Здание старое, электропроводка гнилая.",
                    isLie = true, truthText = "Глумов знает что в архиве — доказательства его афер с землёй.",
                    revealedFragmentId = "c3_frag_glumov_dismisses" },
                new() { questionText = "Вы бывали в архиве?",
                    answerText = "Пару раз, по работе. Ничего особенного.",
                    isLie = true, truthText = "Глумов был там за неделю до пожара, забирал документы.",
                    revealedFragmentId = "c3_frag_glumov_visited" },
                new() { questionText = "Какие документы хранились в сгоревшем крыле?",
                    answerText = "Откуда мне знать? Я не архивариус.",
                    isLie = true, truthText = "Именно в этом крыле — кадастровые дела, которые Глумов подделал.",
                    revealedFragmentId = "c3_frag_cadastre_wing" }
            }},
            new() { targetPersonId = "fomina", questions = new InterrogationQuestionData[]
            {
                new() { questionText = "Что вы видели когда вернулись?",
                    answerText = "Огонь шёл из кадастрового крыла. Пахло бензином. Я пыталась спасти документы... обожглась.",
                    isLie = false, truthText = "",
                    revealedFragmentId = "c3_frag_gasoline_smell" },
                new() { questionText = "Кто в последнее время интересовался кадастровыми документами?",
                    answerText = "Глумов приходил неделю назад. Просил дело по участкам на Озёрной. Я отказала — нужен запрос.",
                    isLie = false, truthText = "",
                    revealedFragmentId = "c3_frag_glumov_request" }
            }},
            new() { targetPersonId = "belov", questions = new InterrogationQuestionData[]
            {
                new() { questionText = "Ваша стройка на Озёрной — законна?",
                    answerText = "Все разрешения получены через земельный комитет. Глумов лично подписал.",
                    isLie = false, truthText = "",
                    revealedFragmentId = "c3_frag_permits" },
                new() { questionText = "Вы знали что в архиве есть документы по вашим участкам?",
                    answerText = "Нет. Меня интересует стройка, а не бумажки.",
                    isLie = true, truthText = "Белов знал — Глумов предупредил его.",
                    revealedFragmentId = "c3_frag_belov_knew" }
            }},
            new() { targetPersonId = "rita", questions = new InterrogationQuestionData[]
            {
                new() { questionText = "Вы замечали странное поведение Глумова?",
                    answerText = "Нет, всё как обычно.",
                    isLie = true, truthText = "Глумов был крайне нервным последний месяц. Звонил кому-то «насчёт архива».",
                    revealedFragmentId = "c3_frag_rita_covers" },
                new() { questionText = "Глумов давал вам какие-нибудь поручения, связанные с архивом?",
                    answerText = "Нет... Ну, один раз просил узнать график работы архива. Когда он закрывается.",
                    isLie = false, truthText = "",
                    revealedFragmentId = "c3_frag_schedule" }
            }}
        };

        c.locations = new LocationData[]
        {
            new() { locationId = "archive", displayName = "Руины архива",
                description = "Сгоревшее крыло. Стеллажи обрушились. Запах гари.",
                zones = new LocationZoneData[]
                {
                    new() { zoneName = "Очаг возгорания", description = "Три точки поджога. Канистра из-под бензина в углу — частично оплавлена, но серийный номер читается.",
                        revealedFragmentId = "c3_frag_canister" },
                    new() { zoneName = "Стеллажи", description = "Кадастровые дела за 2018-2023 полностью уничтожены. Именно участки Озёрного района.",
                        revealedFragmentId = "c3_frag_targeted_docs" },
                    new() { zoneName = "Запасной выход", description = "Замок сломан снаружи. Свежие следы инструмента.",
                        revealedFragmentId = "c3_frag_broken_lock" }
                }},
            new() { locationId = "parking_a", displayName = "Парковка у архива",
                description = "Камера на соседнем здании засняла парковку ночью.",
                zones = new LocationZoneData[]
                {
                    new() { zoneName = "Запись камеры", description = "В 02:40 на парковку заехал тёмный седан. Номер нечитаем. Водитель — мужчина в куртке, понёс что-то к запасному входу.",
                        revealedFragmentId = "c3_frag_camera" },
                    new() { zoneName = "Следы шин", description = "Рисунок протектора соответствует премиальным шинам. Не массовая модель.",
                        revealedFragmentId = "c3_frag_tires" }
                }}
        };

        c.databaseQueries = new DatabaseQueryData[]
        {
            new() { queryId = "c3_db_canister", displayName = "Канистра — серийный номер",
                resultText = "Канистра куплена на АЗС «ЛукОйл» на Проспекте Мира. Оплата картой. Владелец карты — Глумов В.П.",
                revealedFragmentId = "c3_frag_canister_owner" },
            new() { queryId = "c3_db_land", displayName = "Земельные сделки — Озёрная",
                resultText = "Участки на Озёрной переведены из муниципальной в частную собственность в 2020. Подпись — Глумов. Кадастровая стоимость занижена в 4 раза.",
                revealedFragmentId = "c3_frag_land_fraud" },
            new() { queryId = "c3_db_belov_finance", displayName = "Финансы Белова",
                resultText = "Компания Белова перевела 2 млн на счёт фирмы-однодневки. Конечный бенефициар — жена Глумова.",
                revealedFragmentId = "c3_frag_bribe" },
            new() { queryId = "c3_db_glumov_car", displayName = "Автомобиль Глумова",
                resultText = "На Глумова зарегистрирован тёмно-серый BMW 5 серии. Шины — Michelin Pilot Sport, премиальный сегмент.",
                revealedFragmentId = "c3_frag_glumov_car" }
        };

        c.confrontations = new ConfrontationData[]
        {
            new() { personA = "glumov", personB = "fomina",
                resultText = "Фомина: «Вы приходили за делами по Озёрной. Я отказала. А через неделю архив горит?» Глумов: «Совпадение. Я не обязан это слушать.» Но руки трясутся.",
                whoBreaks = "glumov",
                revealedFragmentId = "c3_frag_glumov_nervous" },
            new() { personA = "belov", personB = "rita",
                resultText = "Рита: «Глумов просил узнать когда архив закрывается...» Белов: «Он и мне звонил — сказал что «проблема с бумагами решится сама».» Оба замолкают.",
                whoBreaks = "belov",
                revealedFragmentId = "c3_frag_belov_call" }
        };

        c.fragments = new DeductionFragmentData[]
        {
            // Motives
            new() { fragmentId = "c3_frag_land_fraud", displayText = "Глумов подделал кадастровые документы — занизил стоимость в 4 раза",
                fragmentType = FragmentType.Motive, isTrue = true, relatedPersonId = "glumov" },
            new() { fragmentId = "c3_frag_bribe", displayText = "Белов заплатил 2 млн жене Глумова через подставную фирму",
                fragmentType = FragmentType.Motive, isTrue = true, relatedPersonId = "glumov" },
            new() { fragmentId = "c3_frag_permits", displayText = "Белов получил разрешения через Глумова",
                fragmentType = FragmentType.Motive, isTrue = false, relatedPersonId = "belov" },
            new() { fragmentId = "c3_frag_glumov_dismisses", displayText = "Глумов списывает пожар на случайность",
                fragmentType = FragmentType.Motive, isTrue = false, relatedPersonId = "glumov" },

            // Opportunities
            new() { fragmentId = "c3_frag_schedule", displayText = "Глумов выяснял график работы архива",
                fragmentType = FragmentType.Opportunity, isTrue = true, relatedPersonId = "glumov" },
            new() { fragmentId = "c3_frag_broken_lock", displayText = "Замок запасного входа сломан снаружи",
                fragmentType = FragmentType.Opportunity, isTrue = true, relatedPersonId = "" },
            new() { fragmentId = "c3_frag_glumov_visited", displayText = "Глумов был в архиве за неделю до пожара",
                fragmentType = FragmentType.Opportunity, isTrue = false, relatedPersonId = "glumov" },
            new() { fragmentId = "c3_frag_belov_knew", displayText = "Белов знал про документы в архиве",
                fragmentType = FragmentType.Opportunity, isTrue = false, relatedPersonId = "belov" },

            // Evidence
            new() { fragmentId = "c3_frag_canister_owner", displayText = "Канистра с бензином куплена картой Глумова",
                fragmentType = FragmentType.Evidence, isTrue = true, relatedPersonId = "glumov" },
            new() { fragmentId = "c3_frag_glumov_car", displayText = "BMW Глумова совпадает с машиной на камере",
                fragmentType = FragmentType.Evidence, isTrue = true, relatedPersonId = "glumov" },
            new() { fragmentId = "c3_frag_tires", displayText = "Следы премиальных шин на парковке",
                fragmentType = FragmentType.Evidence, isTrue = true, relatedPersonId = "glumov" },
            new() { fragmentId = "c3_frag_targeted_docs", displayText = "Уничтожены именно кадастровые дела Озёрного района",
                fragmentType = FragmentType.Evidence, isTrue = true, relatedPersonId = "glumov" },
            new() { fragmentId = "c3_frag_canister", displayText = "Канистра найдена на месте — серийный номер цел",
                fragmentType = FragmentType.Evidence, isTrue = false, relatedPersonId = "" },
            new() { fragmentId = "c3_frag_gasoline_smell", displayText = "Фомина: пахло бензином при пожаре",
                fragmentType = FragmentType.Evidence, isTrue = false, relatedPersonId = "" },
            new() { fragmentId = "c3_frag_camera", displayText = "На камере — тёмный седан и мужчина в куртке",
                fragmentType = FragmentType.Evidence, isTrue = false, relatedPersonId = "" },
            new() { fragmentId = "c3_frag_glumov_nervous", displayText = "Глумов нервничает при упоминании Озёрной",
                fragmentType = FragmentType.Evidence, isTrue = false, relatedPersonId = "glumov" },
            new() { fragmentId = "c3_frag_belov_call", displayText = "Глумов звонил Белову: «проблема решится сама»",
                fragmentType = FragmentType.Evidence, isTrue = false, relatedPersonId = "glumov" },
            new() { fragmentId = "c3_frag_rita_covers", displayText = "Рита прикрывала нервозность Глумова",
                fragmentType = FragmentType.Evidence, isTrue = false, relatedPersonId = "rita" },
            new() { fragmentId = "c3_frag_glumov_request", displayText = "Глумов просил дела по Озёрной — ему отказали",
                fragmentType = FragmentType.Evidence, isTrue = true, relatedPersonId = "glumov" },
            new() { fragmentId = "c3_frag_cadastre_wing", displayText = "Сгорело именно кадастровое крыло — не случайность",
                fragmentType = FragmentType.Evidence, isTrue = true, relatedPersonId = "" },

            // Suspects
            new() { fragmentId = "c3_frag_suspect_glumov", displayText = "Глумов В.П. — чиновник, мотив уничтожения улик, купил бензин",
                fragmentType = FragmentType.Suspect, isTrue = true, relatedPersonId = "glumov" },
            new() { fragmentId = "c3_frag_suspect_belov", displayText = "Белов К.А. — застройщик, выгодоприобретатель, знал про документы",
                fragmentType = FragmentType.Suspect, isTrue = false, relatedPersonId = "belov" }
        };

        c.correctMotiveId = "c3_frag_land_fraud";
        c.correctOpportunityId = "c3_frag_schedule";
        c.correctEvidenceId = "c3_frag_canister_owner";
        c.correctSuspectId = "c3_frag_suspect_glumov";

        c.consequenceCorrectArrest = new CaseConsequenceData
        {
            headlineText = "АРЕСТ ГЛУМОВА: Чиновник земельного комитета сжёг архив чтобы скрыть многомиллионные аферы",
            detailText = "Резервные копии кадастровых документов восстановлены. Глумов и Белов арестованы. Фомина награждена за мужество."
        };
        c.consequenceWrongArrest = new CaseConsequenceData
        {
            headlineText = "СУДЕБНАЯ ОШИБКА: Арестован непричастный. Глумов уничтожил оставшиеся улики.",
            detailText = "Земельные аферы продолжаются. Глумов получил повышение."
        };
        c.consequenceUnsolved = new CaseConsequenceData
        {
            headlineText = "БЕССИЛИЕ: Поджог архива так и не раскрыт. Документы утрачены навсегда.",
            detailText = "Без кадастровых дел невозможно оспорить сделки. Жители Озёрного района лишились земли."
        };
        c.consequenceWeakCase = new CaseConsequenceData
        {
            headlineText = "ПРОВАЛ ОБВИНЕНИЯ: Адвокаты Глумова развалили дело — «косвенные улики».",
            detailText = "Глумов на свободе. Подал встречный иск за «преследование». Архив не восстановлен."
        };

        string path = "Assets/Resources/Cases/Case3_Arson.asset";
        AssetDatabase.CreateAsset(c, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = c;
        Debug.Log($"Created case 3 at {path}");
    }
}
#endif
