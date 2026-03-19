#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class Week3AssetCreator
{
    [MenuItem("Profile7/Создать Неделю 3 — Белов")]
    public static void Create()
    {
        var s = ScriptableObject.CreateInstance<SuspectSO>();
        s.suspectId = "suspect_03";
        s.displayName = "Артём Белов";
        s.weekNumber = 3;
        s.isGuilty = false; // НЕ ВИНОВЕН — подставлен начальником IT

        s.dossierText =
@"Артём Белов, 29 лет. Обвиняется в краже конфиденциальных данных 12 000 пациентов из клиники «МедЛайн» и попытке продажи страховой компании «Гарант-Плюс».

Белов — системный администратор «МедЛайн», стаж 3 года. Характеристика от HR — «компетентный, замкнутый, конфликтов не зафиксировано». Зарплата — ниже рыночной. Дважды просил повышение — оба раза отказ.

Начальник IT-отдела — Игорь Зотов (41 год). Обнаружил утечку и сообщил руководству. Именно по его заявлению возбуждено дело.

Серверные логи фиксируют вход в базу данных с учётной записи Белова в 23:47. Скачано 12 000 записей за 15 минут. В ящике стола Белова обнаружен USB-накопитель с копией базы.

С анонимного email-аккаунта велась переписка с «Гарант-Плюс» о продаже данных. Компания отказалась и сообщила в полицию.

Белов отрицает причастность. Утверждает что кто-то использовал его учётную запись.

Жена — Ольга, бухгалтер. Ребёнок — 4 года. Ипотека.";

        s.contacts = new[]
        {
            new ContactData { contactId = "c3_zotov", displayName = "Игорь Зотов (начальник IT)",
                response = "«Белов всегда был недоволен зарплатой. Говорил что его не ценят. Я сам обнаружил утечку — лог-мониторинг показал аномальный объём данных. Сразу доложил директору. Мне неприятно — Белов хороший специалист. Но факты есть факты.»" },
            new ContactData { contactId = "c3_nina", displayName = "Нина Павлова (коллега)",
                response = "«Артём — честный человек. Но последний месяц он задерживался допоздна. Зотов давал ему какие-то задания после рабочего дня — миграция серверов или что-то такое. Артём говорил что устал и что Зотов ведёт себя странно. Подробнее не рассказывал.»" },
            new ContactData { contactId = "c3_garant", displayName = "Максим Черных (директор «Гарант-Плюс»)",
                response = "«Нам предложили данные анонимно. Зашифрованная почта, оплата в криптовалюте. Мы отказались и сообщили в полицию. Продавец был осторожен — использовал VPN, анонимный ящик. Не похоже на обычного сотрудника который решил подзаработать.»" },
            new ContactData { contactId = "c3_olga", displayName = "Ольга Белова (жена)",
                response = "«Артём последний месяц не спал. Говорил что на работе что-то не так — что Зотов ведёт себя странно, даёт задания которые не связаны с его работой. Хотел написать жалобу директору. Не успел.»" }
        };

        s.evidence = new[]
        {
            new EvidenceData { evidenceId = "e3_serverlogs", title = "Серверные логи",
                baseDescription = "Вход в базу данных пациентов с учётной записи user_belov в 23:47. Скачано 12 000 записей за 15 минут. IP-адрес — внутренний VPN клиники. Сессия завершена в 00:02.",
                expertDescription = "MAC-адрес устройства в логах не совпадает ни с одним зарегистрированным устройством Белова (ноутбук, рабочая станция). MAC совпадает с рабочей станцией Зотова — серийный номер подтверждён. Кто-то вошёл под учётной записью Белова с компьютера Зотова." },
            new EvidenceData { evidenceId = "e3_usb", title = "USB-накопитель",
                baseDescription = "Чёрный USB-накопитель без маркировки, обнаружен в верхнем ящике стола Белова. Содержит полную копию базы данных пациентов. Дата записи файлов — 02:15 ночи.",
                expertDescription = "Файлы записаны в 02:15. Камера парковки клиники показывает: машина Белова покинула парковку в 22:30. Зотов вышел из здания в 03:00. Белов физически не мог записать файлы в 02:15 — его не было в здании." },
            new EvidenceData { evidenceId = "e3_emails", title = "Переписка с «Гарант-Плюс»",
                baseDescription = "Анонимная переписка о продаже медицинских данных. Стиль — технический, профессиональный. Автор разбирается в структуре базы данных и ценности медицинских записей для страховых компаний.",
                expertDescription = "Лингвистический анализ: автор использует термины из внутренних отчётов IT-отдела — «миграция кластеров», «индексация по ICD-10». Белов в своей рабочей переписке этих терминов не использует — это лексикон Зотова. Метаданные писем указывают на браузер Firefox 118 под Linux — Белов работает на Windows, Зотов — единственный пользователь Linux в отделе." }
        };

        s.testimonies = new[]
        {
            new TestimonyData { witnessName = "Коул (старший следователь)",
                baseTestimony = "«Логин Белова, данные на его флешке, мотив — недоволен зарплатой. Классический инсайдер. Плюс ипотека. Деньги от продажи данных решили бы его финансовые проблемы.»",
                clarification = "«Но я поднял финансы Белова. Никаких подозрительных поступлений — ни на его счета, ни на счета жены. За последний год — только зарплата и детские. Если он продал данные — где деньги? Криптовалютных кошельков не обнаружено.»" },
            new TestimonyData { witnessName = "Пэйдж (эксперт-аналитик)",
                baseTestimony = "«Технически — всё указывает на Белова. Его логин, время совпадает с его поздними задержками, USB в его столе. Цепочка улик последовательна. Но MAC-адрес в логах не проверялся повторно — стоит перепроверить.»",
                clarification = "«Перепроверила. MAC-адрес устройства не принадлежит ни одному компьютеру Белова. Совпадает с рабочей станцией Зотова. Как администратор, Зотов может сбросить любой пароль и войти под любой учётной записью. Это его штатная функция.»" },
            new TestimonyData { witnessName = "Нэш (полевой агент)",
                baseTestimony = "«Зотов сам обнаружил утечку и доложил. Обычно тот кто раскрывает — не виноват. Но Зотов имел полный административный доступ ко всем системам. Мог подменить логи, создать учётные записи, стереть следы. Финансовая ситуация Зотова — проверяю.»",
                clarification = "«Зотов две недели назад оформил кредит на $50 000 — 'ремонт дома'. Ни одного чека за стройматериалы. Деньги обналичены в течение трёх дней. Дом Зотова — съёмная квартира. Ремонтировать нечего.»" }
        };

        s.standardQuestions = new[]
        {
            new InterrogationQA { question = "Вы скачивали данные пациентов?",
                answer = "«Нет. Я никогда не обращался к базе пациентов — это не моя зона ответственности. Я занимаюсь инфраструктурой: серверы, сети, бэкапы. Клиническими данными занимается другой отдел.»" },
            new InterrogationQA { question = "Флешка с данными найдена в вашем столе. Как она туда попала?",
                answer = "(Растерянно.) «Я не знаю. Это не моя флешка. У меня есть своя — синяя, с логотипом. Эта чёрная, без маркировки. Я её никогда не видел. Любой мог положить её в мой стол — ящики не запираются.»" },
            new InterrogationQA { question = "Вы были недовольны зарплатой. Просили повышение дважды.",
                answer = "«Да, просил. И мне отказали. Но это нормальная рабочая ситуация — не повод совершать преступление. Я искал другую работу, а не продавал данные.»" },
            new InterrogationQA { question = "Зачем вы задерживались допоздна в последний месяц?",
                answer = "«Зотов попросил помочь с миграцией серверов. Давал задания после рабочего дня. Теперь я понимаю зачем — ему нужно было моё присутствие в логах в нерабочее время. Чтобы это выглядело естественно.»" }
        };

        s.conditionalQuestions = new[]
        {
            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Contact, requiredChoiceId = "c3_zotov",
                question = "Зотов говорит вы были недовольны и он вас ценил. Но он же вас и сдал. Как так?",
                answer = "«Именно так. Он создал ситуацию и тут же 'раскрыл' её. Идеальный свидетель — тот кто написал заявление. Никто не подозревает доносчика.»" },
            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Contact, requiredChoiceId = "c3_nina",
                question = "Коллега говорит Зотов давал вам задания после работы. Какие именно?",
                answer = "«Настройка VPN-каналов, перенос бэкапов, обновление прав доступа. Технические вещи. Но теперь я понимаю — каждое задание оставляло мой логин в логах. Он строил картину — 'Белов работал поздно, имел доступ'.»" },
            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Contact, requiredChoiceId = "c3_garant",
                question = "«Гарант-Плюс» говорит продавец использовал VPN и шифрование. Вы умеете настраивать VPN?",
                answer = "«Да, это моя работа. Но Зотов тоже умеет — он мой начальник, он знает всё что знаю я. Плюс у него административные права которых у меня нет.»" },
            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Contact, requiredChoiceId = "c3_olga",
                question = "Жена говорит вы хотели пожаловаться на Зотова директору. О чём?",
                answer = "«Зотов последний месяц вёл себя странно. Просил делать вещи которые не входили в план работ. Один раз я увидел у него на экране базу пациентов — спросил зачем ему это, он сказал 'не твоё дело'. Я хотел написать жалобу. А через неделю — обвинили меня.»" },
            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "e3_serverlogs",
                question = "MAC-адрес в логах не совпадает с вашими устройствами. Совпадает с компьютером Зотова. Ваш комментарий?",
                answer = "(Облегчение на лице.) «Вот. Вот доказательство. Это не мой компьютер. Зотов вошёл под моим логином со своей машины. Он администратор — он может сбросить мой пароль за секунду.»" },
            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "e3_usb",
                question = "Файлы на флешке записаны в 02:15. Камера парковки показывает — вас не было в здании. Но флешка в вашем столе.",
                answer = "«Потому что кто-то положил её туда. Зотов был в здании до трёх ночи — камера подтверждает. У него было время записать файлы и подбросить флешку. Ящики не запираются.»" },
            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "e3_emails",
                question = "Лингвистический анализ переписки указывает на терминологию Зотова, не вашу. Вы когда-нибудь использовали термин «миграция кластеров»?",
                answer = "«Нет. Это не мой термин. Зотов так говорит в каждом отчёте. Я использую 'перенос данных'. Спросите любого в отделе — это его словечко.»" },
            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Testimony, requiredChoiceId = "Коул (старший следователь)",
                question = "Коул не нашёл у вас никаких подозрительных поступлений. Если вы продавали данные — где деньги?",
                answer = "«Нет денег потому что я ничего не продавал. Проверьте Зотова — его кредит на $50 000 'на ремонт' в съёмной квартире. Вот где деньги.»" },
            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Testimony, requiredChoiceId = "Пэйдж (эксперт-аналитик)",
                question = "Пэйдж подтверждает — MAC-адрес принадлежит Зотову. Он мог войти под вашим логином?",
                answer = "«Конечно мог. Он — администратор. Сброс пароля, вход под любой учёткой — это его рабочие инструменты. Для этого даже не нужно взламывать — просто нажать кнопку в панели управления.»" },
            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Testimony, requiredChoiceId = "Нэш (полевой агент)",
                question = "Зотов взял кредит $50 000 на ремонт — но живёт в съёмной квартире. Деньги обналичены. Вы знали об этом?",
                answer = "«Нет. Но это объясняет всё. Зотов продал данные, получил аванс, обналичил через кредит чтобы не светить источник. А меня подставил чтобы отвести подозрения.»" }
        };

        s.followUps = new[]
        {
            new FollowUpData { followUpId = "fu3_why",
                question = "Зачем Зотову вас подставлять? Что он выигрывает?",
                answer = "«Козла отпущения. Если утечку обнаружат — а её обнаружат — кто-то должен быть виноват. Зотов написал заявление первым, стал героем. Меня уволят или посадят. Он продолжит.»" },
            new FollowUpData { followUpId = "fu3_proof",
                question = "У вас есть что-нибудь что доказывает вашу невиновность?",
                answer = "(Достаёт телефон.) «Скриншоты сообщений от Зотова. Каждый вечер — 'задержись, нужно доделать миграцию'. Каждый раз — задание которое оставляет мой логин в системе. Он планировал это неделями.»" },
            new FollowUpData { followUpId = "fu3_family",
                question = "Если вас осудят — что будет с семьёй?",
                answer = "(Долгое молчание.) «Ольга одна с ребёнком. Ипотека. Четыре года сыну.» (Голос ломается.) «Я не воровал эти данные. Пожалуйста. Проверьте Зотова.»" }
        };

        s.consequenceGuilty = "Артём Белов приговорён к 4 годам. Через полгода — новая утечка данных из «МедЛайн»: 30 000 записей. Белов в это время в заключении. Дело Зотова открыто. Ольга Белова подала на развод — не выдержала.";
        s.consequenceNotGuilty = "Следствие переключилось на Игоря Зотова. Обнаружен анонимный криптовалютный кошелёк с поступлениями от двух страховых компаний. Зотов задержан при попытке продать следующую партию данных. Белов восстановлен на работе.";

        s.pressureThreshold = 4;

        s.bluffQuestions = new BluffQuestionData[]
        {
            new BluffQuestionData
            {
                question = "Мы отследили IP-адрес продавца данных. Он ведёт прямо к вашему ноутбуку.",
                answerSuccess = "(Растерянно.) Это... это невозможно. Мой ноутбук был дома в ту ночь. Проверьте MAC-адрес!",
                answerFail = "(Уверенно.) Проверяйте. Мой ноутбук чист. Я даже могу дать его на экспертизу прямо сейчас.",
                requiredChoiceType = ChoiceType.Evidence,
                requiredChoiceId = "evidence_serverlogs",
                pressureChange = 2
            },
            new BluffQuestionData
            {
                question = "Ваш начальник Зотов передал нам вашу переписку с покупателем данных.",
                answerSuccess = "(Вспыхивает.) Зотов?! Это он! Он подставил меня! Спросите его откуда у него $50,000!",
                answerFail = "(Тихо.) Такой переписки не существует. Потому что я не продавал данные.",
                requiredChoiceType = ChoiceType.Testimony,
                requiredChoiceId = "Нэш",
                pressureChange = 2
            }
        };

        s.contradictions = new ContradictionData[]
        {
            new ContradictionData
            {
                witnessA = "Коул",
                witnessB = "Пэйдж",
                description = "Коул считает улики против Белова железными — логин, флешка, задержки допоздна. Пэйдж обнаружила что MAC-адрес в логах не принадлежит ноутбуку Белова."
            },
            new ContradictionData
            {
                witnessA = "Коул",
                witnessB = "Нэш",
                description = "Коул уверен в виновности Белова — классический инсайдер. Нэш указывает что у Белова нет подозрительных поступлений, а Зотов обналичил $50,000."
            }
        };

        s.arguments = new ArgumentData[]
        {
            new ArgumentData { argumentId = "arg_1", text = "Доступ к базе осуществлён с учётной записи Белова", supportsGuilty = true, weight = 1 },
            new ArgumentData { argumentId = "arg_2", text = "USB-накопитель с данными найден в столе Белова", supportsGuilty = true, weight = 1 },
            new ArgumentData { argumentId = "arg_3", text = "Белов задерживался допоздна — имел возможность", supportsGuilty = true, weight = 1 },
            new ArgumentData { argumentId = "arg_4", text = "MAC-адрес в логах принадлежит рабочей станции Зотова", supportsGuilty = false, weight = 3 },
            new ArgumentData { argumentId = "arg_5", text = "Файлы записаны в 02:15 — Белов уехал в 22:30", supportsGuilty = false, weight = 2 },
            new ArgumentData { argumentId = "arg_6", text = "У Белова нет подозрительных денежных поступлений", supportsGuilty = false, weight = 2 },
            new ArgumentData { argumentId = "arg_7", text = "Зотов обналичил $50,000 без чеков за стройматериалы", supportsGuilty = false, weight = 2 },
            new ArgumentData { argumentId = "arg_8", text = "Лингвистический анализ указывает на терминологию Зотова", supportsGuilty = false, weight = 2 },
            new ArgumentData { argumentId = "arg_9", text = "Стиль переписки похож на Белова (технический жаргон)", supportsGuilty = true, weight = 1 },
            new ArgumentData { argumentId = "arg_10", text = "Зотов имел полный доступ как администратор системы", supportsGuilty = false, weight = 2 }
        };

        // Timeline events
        s.timelineStartHour = 21f;
        s.timelineEndHour = 3f;
        s.timelineEvents = new TimelineEventData[]
        {
            new TimelineEventData { eventId = "tl_belov_stay", description = "Белов задержался на работе", correctHour = 21f, alwaysVisible = true },
            new TimelineEventData { eventId = "tl_belov_leave", description = "Белов уехал домой", correctHour = 22.5f, requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_usb" },
            new TimelineEventData { eventId = "tl_access_db", description = "Доступ к базе данных", correctHour = 23.783f, alwaysVisible = true },
            new TimelineEventData { eventId = "tl_usb_write", description = "Запись на USB-накопитель", correctHour = 2.25f, requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_usb" },
            new TimelineEventData { eventId = "tl_zotov_leave", description = "Зотов покинул здание", correctHour = 3f, requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_usb" },
            new TimelineEventData { eventId = "tl_email_sent", description = "Отправлено письмо покупателю", correctHour = 2.5f, requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_email" }
        };

        // Connection board
        s.connectionCards = new ConnectionCardData[]
        {
            new ConnectionCardData { cardId = "card_belov", label = "Белов", type = ConnectionCardData.CardType.Person, alwaysVisible = true },
            new ConnectionCardData { cardId = "card_zotov", label = "Зотов", type = ConnectionCardData.CardType.Person, alwaysVisible = true },
            new ConnectionCardData { cardId = "card_garant", label = "Гарант-Плюс", type = ConnectionCardData.CardType.Person, alwaysVisible = true },
            new ConnectionCardData { cardId = "card_mac", label = "MAC-адрес", type = ConnectionCardData.CardType.Item, requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_serverlogs" },
            new ConnectionCardData { cardId = "card_usb", label = "USB-накопитель", type = ConnectionCardData.CardType.Item, alwaysVisible = true },
            new ConnectionCardData { cardId = "card_50k", label = "Кредит $50,000", type = ConnectionCardData.CardType.Item, requiredChoiceType = ChoiceType.Testimony, requiredChoiceId = "Нэш" },
            new ConnectionCardData { cardId = "card_account", label = "Учётная запись", type = ConnectionCardData.CardType.Item, alwaysVisible = true },
            new ConnectionCardData { cardId = "card_leak", label = "Утечка данных", type = ConnectionCardData.CardType.Event, alwaysVisible = true },
            new ConnectionCardData { cardId = "card_linguistics", label = "Лингвистика", type = ConnectionCardData.CardType.Event, requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_email" }
        };

        s.connections = new ConnectionData[]
        {
            new ConnectionData { cardA = "card_mac", cardB = "card_zotov", revealText = "MAC-адрес в серверных логах принадлежит рабочей станции Зотова, а не ноутбуку Белова." },
            new ConnectionData { cardA = "card_zotov", cardB = "card_account", revealText = "Зотов как администратор мог сбросить пароль любого аккаунта и войти под чужим именем." },
            new ConnectionData { cardA = "card_zotov", cardB = "card_50k", revealText = "Зотов оформил кредит на $50,000 'на ремонт дома'. Ни одного чека. Деньги обналичены." },
            new ConnectionData { cardA = "card_linguistics", cardB = "card_zotov", revealText = "Лингвистический анализ переписки показывает терминологию из отчётов Зотова." },
            new ConnectionData { cardA = "card_usb", cardB = "card_zotov", revealText = "USB записан в 02:15. Камера показывает: Белов уехал в 22:30, Зотов — в 03:00." },
            new ConnectionData { cardA = "card_zotov", cardB = "card_leak", revealText = "Зотов сам обнаружил и сообщил об утечке. Обычно тот кто находит — не виноват. Но у него был полный доступ." }
        };
        s.maxConnectionAttempts = 9;

        SaveAsset(s, "suspect_03_belov");
    }

    static void SaveAsset(SuspectSO s, string filename)
    {
        const string folder = "Assets/Resources/Suspects";
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/Resources", "Suspects");
        AssetDatabase.CreateAsset(s, folder + "/" + filename + ".asset");
        AssetDatabase.SaveAssets();
        Selection.activeObject = s;
        Debug.Log("Создан: " + folder + "/" + filename + ".asset");
    }
}
#endif
