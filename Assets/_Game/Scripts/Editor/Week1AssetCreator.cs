#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class Week1AssetCreator
{
    [MenuItem("Profile7/Создать Неделю 1 — Холлоуэй")]
    public static void Create()
    {
        var s = ScriptableObject.CreateInstance<SuspectSO>();
        s.suspectId = "suspect_01";
        s.displayName = "Марк Холлоуэй";
        s.weekNumber = 1;
        s.isGuilty = false; // НЕ ВИНОВЕН — пожар организовал Салас
        s.pressureThreshold = 5;

        s.dossierText =
@"Марк Холлоуэй, 44 года. Поджог собственного склада с целью получения страховой выплаты. Один рабочий — Сантьяго Гомес, 31 год — погиб при пожаре.

Родился в Детройте, 1981. Первый бизнес — авторемонтная мастерская, закрылась в 2004 после пожара. Страховая выплата полностью покрыла долги. Следствие установило неисправность электропроводки — дело закрыто.

Второй бизнес — склад химических материалов, открыт в 2009 совместно с Виктором Саласом (доля 50/50). До 2022 убыточен. В 2023 — резкий рост выручки после муниципального контракта.

Склад застрахован на $2,4 млн — вдвое выше рыночной стоимости. Переоценка проведена в 2021 году. В документах указаны оба совладельца.

Финансовое положение: личный долг Холлоуэя — $180 000 по кредитной линии. Ежемесячный платёж — $4 200. Три платежа на ту же сумму компании «GreenTech Disposal Services» — назначение не установлено.

Партнёрство с Саласом в последний год характеризуется конфликтом — подробности неизвестны.

Жена — Линда, преподаватель. Двое детей. Церковный приход, волонтёрство.

САНТЬЯГО ГОМЕС. 31 год. Гондурас, 2019. Работал на складе с 2021. Единственный кормилец, трое детей, младшему — восемь месяцев. Выходил на неофициальные ночные смены — оплата наличными.";

        // ─── КОНТАКТЫ ───
        s.contacts = new[]
        {
            new ContactData
            {
                contactId = "contact_linda",
                displayName = "Линда Холлоуэй (жена)",
                response = "«Марк был дома после одиннадцати. Я слышала как он вошёл. Последние месяцы он был на нервах — партнёрство разваливалось. Виктор требовал выкупить его долю за бесценок. Марк отказывался. Он хотел уйти, а не воевать.»"
            },
            new ContactData
            {
                contactId = "contact_raul",
                displayName = "Рауль Эспиноза (бывший сотрудник)",
                response = "«Я уволился за месяц до пожара. Салас вёл себя странно — стал приезжать на склад по вечерам, хотя раньше не появлялся. За два дня до пожара я видел его машину на парковке склада в десять вечера. Холлоуэй об этом не знал — они к тому моменту почти не разговаривали.»"
            },
            new ContactData
            {
                contactId = "contact_holm",
                displayName = "Питер Холм (сосед)",
                response = "«Я видел машину Холлоуэя — она уехала около одиннадцати вечера. Больше не возвращалась. Но знаете что странно — примерно за полчаса до этого подъехала другая машина. Тёмная, я не разглядел марку. Постояла минут двадцать и уехала.»"
            },
            new ContactData
            {
                contactId = "contact_salas",
                displayName = "Виктор Салас (бизнес-партнёр)",
                response = "«Марк угрожал мне. Говорил что лучше сожжёт всё чем отдаст мне долю. Я думал это слова. И ещё — он платил какой-то компании, GreenTech. Я случайно увидел выписку. Спросил — он разозлился и сказал чтоб я не лез в его дела.»"
            }
        };

        // ─── УЛИКИ ───
        s.evidence = new[]
        {
            new EvidenceData
            {
                evidenceId = "evidence_accesslog",
                title = "Журнал пропусков склада",
                baseDescription =
@"Электронная система контроля доступа. Записи за ночь пожара:
- Виктор Салас: вход 22:45, выход 23:05
- Марк Холлоуэй: вход 23:14, выход — не зафиксирован
- Сантьяго Гомес: вход 23:47, выход — не зафиксирован",
                expertDescription =
@"Технический анализ: сервер системы контроля перезагружен в 23:10. Все записи после 22:50 могут быть неполными или повреждёнными. Выход Саласа в 23:05 зафиксирован до перезагрузки — единственная достоверная запись. Выходы Холлоуэя и Гомеса технически не могли сохраниться. Запись о входе Холлоуэя в 23:14 сделана после перезагрузки — точность времени не гарантирована.",
                zones = new EvidenceZoneData[]
                {
                    new EvidenceZoneData { label = "Строка 22:45", detail = "Вход: В. Салас — 22:45", isCritical = false },
                    new EvidenceZoneData { label = "Строка 23:05", detail = "Выход: В. Салас — 23:05 (единственная достоверная запись)", isCritical = true },
                    new EvidenceZoneData { label = "Строка 23:10", detail = "СЕРВЕР ПЕРЕЗАГРУЖЕН — все данные после этого момента ненадёжны", isCritical = true },
                    new EvidenceZoneData { label = "Строка 23:14", detail = "Вход: М. Холлоуэй — 23:14 (после перезагрузки, время может быть неверным)", isCritical = true },
                    new EvidenceZoneData { label = "Строка 23:47", detail = "Вход: С. Гомес — 23:47", isCritical = false },
                    new EvidenceZoneData { label = "Системный лог", detail = "Перезагрузка инициирована удалённо, не вручную", isCritical = true }
                },
                maxInspections = 4
            },
            new EvidenceData
            {
                evidenceId = "evidence_firereport",
                title = "Отчёт пожарной экспертизы",
                baseDescription = "Три точки возгорания одновременно. Использован ускоритель горения — промышленный растворитель. Поджигатель знал расположение вентиляционных шахт и систему хранения. Профессиональный поджог.",
                expertDescription =
@"Растворитель идентифицирован как TechSolv-7 — узкоспециализированный реагент, продаётся только по корпоративным контрактам. Проверка реестра поставщика: ООО «Салас Индастриал» — активный клиент с 2020 года. Холлоуэй в реестре не значится. Склад Холлоуэя не использовал TechSolv-7 в производственных процессах.",
                zones = new EvidenceZoneData[]
                {
                    new EvidenceZoneData { label = "Точка 1", detail = "Северный угол — следы ускорителя, источник возгорания", isCritical = false },
                    new EvidenceZoneData { label = "Точка 2", detail = "Центр склада — вторичный очаг, промышленный растворитель", isCritical = false },
                    new EvidenceZoneData { label = "Точка 3", detail = "Вентиляция — поджигатель знал расположение шахт", isCritical = true },
                    new EvidenceZoneData { label = "Хим. анализ", detail = "Растворитель: TechSolv-7, продаётся только по корпоративным контрактам", isCritical = true },
                    new EvidenceZoneData { label = "Реестр", detail = "Покупатель TechSolv-7: ООО Салас Индастриал (активный клиент с 2020)", isCritical = true },
                    new EvidenceZoneData { label = "Заключение", detail = "Профессиональный поджог — исполнитель знал здание изнутри", isCritical = false }
                },
                maxInspections = 4
            },
            new EvidenceData
            {
                evidenceId = "evidence_bankstatement",
                title = "Выписка со счёта Холлоуэя",
                baseDescription = "Три платежа по $4 200 компании «GreenTech Disposal Services» за два месяца. Компания зарегистрирована два месяца назад. Юридический адрес — жилой дом. Договоров на оказание услуг не обнаружено.",
                expertDescription =
@"«GreenTech» — номинальная компания без деятельности. Директор — Карл Рид, ранее работал юристом в компании Виктора Саласа. Рид уволен Саласом за полгода до регистрации GreenTech. Схема: Рид предположительно использует информацию о пожаре 2004 года для шантажа Холлоуэя. Связь Рида с Саласом может указывать на координированное давление.",
                zones = new EvidenceZoneData[]
                {
                    new EvidenceZoneData { label = "Платёж 1", detail = "$4,200 → GreenTech Disposal — 15 января", isCritical = false },
                    new EvidenceZoneData { label = "Платёж 2", detail = "$4,200 → GreenTech Disposal — 15 февраля", isCritical = false },
                    new EvidenceZoneData { label = "Платёж 3", detail = "$4,200 → GreenTech Disposal — 15 марта (за неделю до пожара)", isCritical = true },
                    new EvidenceZoneData { label = "GreenTech", detail = "Компания зарегистрирована 2 месяца назад, юр. адрес — жилой дом", isCritical = true },
                    new EvidenceZoneData { label = "Директор", detail = "Карл Рид — бывший юрист компании Саласа", isCritical = true },
                    new EvidenceZoneData { label = "Назначение", detail = "Договоров на оказание услуг не обнаружено — платежи без основания", isCritical = false }
                },
                maxInspections = 4
            }
        };

        // ─── ПОКАЗАНИЯ ───
        s.testimonies = new[]
        {
            new TestimonyData
            {
                witnessName = "Коул (старший следователь)",
                baseTestimony = "«Два пожара за двадцать лет — это паттерн. Страховка вдвое выше стоимости, долг $180 000, ежемесячный платёж совпадает с суммой GreenTech. Журнал подтверждает присутствие. Отсутствие записи о выходе говорит само за себя.»",
                clarification = "«Я копнул GreenTech. Платежи $4 200 — ровно сумма ежемесячного долга Холлоуэя. Это не юридические расходы — нет ни одного договора. Кто-то снимал с него деньги. Шантаж. Вопрос — за что и кто стоит за этой компанией.»",
                clarificationLines = new TestimonyLineData[]
                {
                    new TestimonyLineData { text = "Я копнул глубже в финансы.", isLie = false },
                    new TestimonyLineData { text = "GreenTech — чистая компания, оказывает реальные услуги.", isLie = true, lieReason = "GreenTech зарегистрирована 2 месяца назад, договоров нет — это подставная фирма" },
                    new TestimonyLineData { text = "Платежи по $4,200 совпадают с суммой ежемесячного долга.", isLie = false },
                    new TestimonyLineData { text = "Холлоуэй платил добровольно, никакого давления.", isLie = true, lieReason = "Сумма и регулярность указывают на шантаж, а не добровольные платежи" },
                    new TestimonyLineData { text = "Кто-то снимал с Холлоуэя деньги. Это похоже на шантаж.", isLie = false },
                    new TestimonyLineData { text = "Связь GreenTech с Саласом пока не установлена.", isLie = true, lieReason = "Директор GreenTech Карл Рид — бывший юрист Саласа, связь прямая" }
                },
                startingTrust = 3
            },
            new TestimonyData
            {
                witnessName = "Пэйдж (эксперт-аналитик)",
                baseTestimony = "«Три точки возгорания — поджог. Два человека без записи о выходе — оба физически могли быть на складе в момент пожара. Алиби Холлоуэя подтверждается только показаниями жены. Жена не является независимым свидетелем — её слова не верифицируемы.»",
                clarification = "«Перепроверила журнал. Сервер перезагружен в 23:10 — все данные после этого момента ненадёжны. Салас вышел до перезагрузки — это факт. Пожар начался в 01:30 — через два часа после последней записи. За это время на склад мог войти кто угодно без фиксации.»",
                clarificationLines = new TestimonyLineData[]
                {
                    new TestimonyLineData { text = "Перепроверила журнал пропусков.", isLie = false },
                    new TestimonyLineData { text = "Сервер работал стабильно всю ночь.", isLie = true, lieReason = "Сервер перезагружен в 23:10 — данные после ненадёжны" },
                    new TestimonyLineData { text = "Салас вышел до перезагрузки — его запись достоверна.", isLie = false },
                    new TestimonyLineData { text = "Пожар начался в 01:30 — через два часа после последней записи.", isLie = false },
                    new TestimonyLineData { text = "Показания жены Холлоуэя полностью подтверждены.", isLie = true, lieReason = "Жена — заинтересованный свидетель, её показания не верифицируемы" },
                    new TestimonyLineData { text = "За два часа на склад мог войти кто угодно без фиксации.", isLie = false }
                },
                startingTrust = 3
            },
            new TestimonyData
            {
                witnessName = "Нэш (полевой агент)",
                baseTestimony = "«Салас подал заявление о принудительном расторжении партнёрства за два дня до пожара. По условиям договора — при осуждении одного партнёра его доля автоматически переходит другому. Отдельно: телефонные записи Гомеса за последнюю неделю содержат необъяснимую активность — проверяю.»",
                clarification = "«Проверил. Семь звонков Саласу за последнюю неделю. Гомес не просто работал у Холлоуэя — он был информатором Саласа внутри склада. Знал расписание, знал кто приходит ночью. Салас знал что Гомес будет там.»",
                clarificationLines = new TestimonyLineData[]
                {
                    new TestimonyLineData { text = "Проверил телефонные записи Гомеса.", isLie = false },
                    new TestimonyLineData { text = "Гомес звонил Саласу один раз за месяц.", isLie = true, lieReason = "Семь звонков за последнюю неделю — это не один раз" },
                    new TestimonyLineData { text = "Гомес был информатором — знал расписание склада.", isLie = false },
                    new TestimonyLineData { text = "Салас не знал что Гомес будет на складе.", isLie = true, lieReason = "7 звонков за неделю — Салас точно знал о присутствии Гомеса" },
                    new TestimonyLineData { text = "Салас знал что Гомес будет на складе.", isLie = false },
                    new TestimonyLineData { text = "Это меняет характер дела — не просто поджог, а убийство.", isLie = false }
                },
                startingTrust = 3
            }
        };

        // ─── ДОПРОС ───
        s.tonedQuestions = new TonedQuestionData[]
        {
            new TonedQuestionData
            {
                topic = "Алиби в ночь пожара",
                questionPress = "Где вы были?! У нас есть записи что вы были на складе!",
                answerPress = "(Нервно.) Да, я был на складе до одиннадцати! Проверял документы. Потом домой. Жена подтвердит!",
                pressurePress = 2,
                questionNeutral = "Расскажите где вы были в ночь пожара.",
                answerNeutral = "На складе до одиннадцати — проверял документы для адвоката. Потом домой. Узнал о пожаре от пожарных около двух ночи.",
                pressureNeutral = 0,
                questionEmpathy = "Я понимаю, это тяжёлая ситуация. Просто расскажите как прошёл тот вечер.",
                answerEmpathy = "Я был на складе... готовил документы. Хотел уйти от Виктора — найти адвоката и разделить бизнес мирно. Поехал домой около одиннадцати. Жена не спала, мы поговорили.",
                pressureEmpathy = -1
            },
            new TonedQuestionData
            {
                topic = "Завышенная страховка",
                questionPress = "Склад застрахован на $2.4 миллиона — вдвое выше стоимости! Это вы организовали?!",
                answerPress = "(Сжимает кулаки.) Переоценку делали вместе с Виктором! Мы оба подписали! Хватит меня обвинять!",
                pressurePress = 2,
                questionNeutral = "Склад застрахован вдвое выше стоимости. Кто инициировал переоценку?",
                answerNeutral = "Переоценку делали в 2021-м. Мы с Виктором оба подписали. Я не помню кто предложил первым.",
                pressureNeutral = 0,
                questionEmpathy = "Страховка — это нормальная практика. Расскажите как она оформлялась.",
                answerEmpathy = "Виктор предложил переоценить. Сказал что склад стоит больше с учётом оборудования. Я не возражал — это казалось разумным. Сейчас понимаю как это выглядит.",
                pressureEmpathy = -1
            },
            new TonedQuestionData
            {
                topic = "Долг $180,000",
                questionPress = "У вас долг $180,000! Страховка покрыла бы его полностью! Признайте — вам нужны были деньги!",
                answerPress = "(Встаёт.) Да, у меня долг! И что?! У половины бизнесменов долги! Это не значит что я поджигатель!",
                pressurePress = 2,
                questionNeutral = "У вас личный долг $180,000. Страховая выплата покрыла бы его. Это совпадение?",
                answerNeutral = "Да, и что? У меня есть бизнес который приносит доход. Контракт с муниципалитетом. Я не стал бы сжигать работающий бизнес.",
                pressureNeutral = 1,
                questionEmpathy = "Финансовое давление может толкнуть людей на отчаянные шаги. Как вы справлялись с долгом?",
                answerEmpathy = "Было тяжело. Но бизнес наконец пошёл в гору после муниципального контракта. Я видел свет в конце тоннеля. Зачем мне сжигать то, что начало работать?",
                pressureEmpathy = -1
            },
            new TonedQuestionData
            {
                topic = "Гомес на складе",
                questionPress = "Вы знали что Гомес будет на складе! Вы отправили его на смерть!",
                answerPress = "(Бледнеет. Долгое молчание.) Нет... Его не было в расписании. У него трое детей... (Голос ломается.)",
                pressurePress = 3,
                questionNeutral = "Вы знали что Гомес будет на складе той ночью?",
                answerNeutral = "Нет. Его не было в расписании. У него трое детей. (Замолкает.)",
                pressureNeutral = 0,
                questionEmpathy = "Гомес... у него была семья. Расскажите что вы знали о его ночных сменах.",
                answerEmpathy = "Сантьяго подрабатывал ночами. Я знал, но не запрещал — ему нужны были деньги. Младшему восемь месяцев... Если бы я знал что кто-то... (Не может продолжать.)",
                pressureEmpathy = -1
            }
        };

        s.conditionalQuestions = new[]
        {
            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Contact, requiredChoiceId = "contact_linda",
                question = "Ваша жена говорит Виктор требовал вашу долю за бесценок. Почему не упомянули?",
                answer = "«Потому что это выглядит как мотив. Но мой мотив был уйти, а не сжигать. Я хотел продать свою долю — Виктор отказывался платить рыночную цену.»",
                pressureChange = 1 },

            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Contact, requiredChoiceId = "contact_raul",
                question = "Бывший сотрудник видел машину Саласа на парковке склада за два дня до пожара. Поздно вечером. Знали об этом?",
                answer = "(Пауза.) «Нет. Виктор имел право приезжать — это его склад тоже. Но... зачем ночью? Мы к тому моменту почти не разговаривали.» (Задумывается.)",
                pressureChange = 0 },

            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Contact, requiredChoiceId = "contact_holm",
                question = "Сосед видел вашу машину уезжающей в 23:00. И ещё одну тёмную машину за полчаса до вас. Знаете чья?",
                answer = "«Тёмная машина? Нет... Подождите. У Виктора тёмно-синий BMW. Но я его не видел той ночью — я приехал позже.»",
                pressureChange = 0 },

            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Contact, requiredChoiceId = "contact_salas",
                question = "Салас говорит вы угрожали сжечь склад. Это было?",
                answer = "(Долгая пауза. Смотрит прямо.) «Я сказал: 'Лучше сожгу всё чем отдам тебе за бесценок.' В запале. Люди говорят всякое когда злятся. Я не поджигатель.»",
                pressureChange = 2 },

            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_accesslog",
                question = "Система перезагрузилась в 23:10. Ваш выход не мог сохраниться. Знали об этом?",
                answer = "«Нет. Но это объясняет почему нет записи. Я ушёл до перезагрузки — или в это время.»",
                pressureChange = 1 },

            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_firereport",
                question = "TechSolv-7 продаётся только корпоративным клиентам. В реестре — только Салас. Откуда это вещество на вашем складе?",
                answer = "(Бледнеет.) «Я не покупал это вещество. Если оно там было — его принёс Виктор. У него был доступ. Он ушёл в 23:05.»",
                pressureChange = 1 },

            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_bankstatement",
                question = "GreenTech связана с бывшим юристом Саласа. Вы платили шантажисту?",
                answer = "(Молчание. Потом тихо.) «Карл Рид знал про 2004 год. Подробности которые могли меня уничтожить. Виктор видимо нашёл его и... да. Я платил. Но это не делает меня поджигателем.»",
                pressureChange = 2 },

            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Testimony, requiredChoiceId = "Коул (старший следователь)",
                question = "Коул считает GreenTech — шантаж. Кто вас шантажировал и за что?",
                answer = "(Долгая пауза.) «Человек который знал детали 2004 года. Я думал это закончилось. Не закончилось.»",
                pressureChange = 1 },

            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Testimony, requiredChoiceId = "Пэйдж (эксперт-аналитик)",
                question = "Пэйдж подтверждает: журнал ненадёжен после 22:50. Пожар в 01:30. Где вы были с одиннадцати до двух ночи?",
                answer = "«Дома. Жена слышала. Я лёг около полуночи. Полтора часа между моим уходом и пожаром. Кто-то поджёг после меня.»",
                pressureChange = 1 },

            new ConditionalInterrogationQA { requiredChoiceType = ChoiceType.Testimony, requiredChoiceId = "Нэш (полевой агент)",
                question = "Гомес звонил Саласу семь раз за неделю до пожара. Вы знали что ваш рабочий работал на партнёра?",
                answer = "(Встаёт. Садится обратно.) «Нет. Гомес был хорошим человеком. Если Виктор использовал его как информатора — он знал что Гомес будет там. Он знал.» (Голос ломается.)",
                pressureChange = 1 }
        };

        s.followUps = new[]
        {
            new FollowUpData { followUpId = "fu_benefit",
                question = "Кому выгоден ваш приговор?",
                answer = "«Виктору. При осуждении — моя доля автоматически переходит ему. Это условие в договоре. Я узнал об этом от адвоката уже после пожара.» (Горький смех.)" },
            new FollowUpData { followUpId = "fu_2004",
                question = "Что на самом деле произошло в 2004?",
                answer = "«В мастерской была старая проводка. Эксперт подтвердил — короткое замыкание. Дело закрыто. Но Рид нашёл черновик первичного осмотра — который не вошёл в итоговое заключение. Там были вопросы. Он этим и пользовался.»" },
            new FollowUpData { followUpId = "fu_gomez",
                question = "Что бы вы сказали семье Гомеса?",
                answer = "(Долгое молчание. Глаза наполняются слезами.) «Что мне жаль. Что его не должно было быть там. Что кто-то сознательно подверг его опасности. И это был не я.» (Отворачивается.)" }
        };

        s.consequenceGuilty = "Марк Холлоуэй приговорён к 12 годам. Виктор Салас вступил в права на бизнес через 48 часов после приговора. Линда Холлоуэй подала апелляцию. Расследование в отношении Саласа прекращено за недостаточностью доказательств.";
        s.consequenceNotGuilty = "Следствие переключилось на Виктора Саласа. Обнаружены закупки TechSolv-7 за неделю до пожара. Карл Рид пошёл на сотрудничество со следствием. Дело передано в прокуратуру.";

        // ─── БЛЕФ-ВОПРОСЫ ───
        s.bluffQuestions = new BluffQuestionData[]
        {
            new BluffQuestionData
            {
                question = "У нас есть свидетель, который видел вас на складе в час ночи. Что скажете?",
                answerSuccess = "(Бледнеет.) Я... да, я возвращался. Забыл документы. Но ушёл до пожара, клянусь!",
                answerFail = "(Спокойно.) Это невозможно. Я был дома. Ваш свидетель ошибается или лжёт.",
                requiredChoiceType = ChoiceType.Contact,
                requiredChoiceId = "contact_holm",
                pressureChange = 2
            },
            new BluffQuestionData
            {
                question = "Мы знаем про ваши платежи Саласу. Зачем вы ему платили?",
                answerSuccess = "(Долгая пауза.) Не Саласу. GreenTech... Рид шантажировал меня. Из-за 2004 года.",
                answerFail = "Я не знаю о каких платежах вы говорите. Покажите документы.",
                requiredChoiceType = ChoiceType.Evidence,
                requiredChoiceId = "evidence_bankstatement",
                pressureChange = 2
            }
        };

        // ─── ПРОТИВОРЕЧИЯ ───
        s.contradictions = new ContradictionData[]
        {
            new ContradictionData
            {
                witnessA = "Коул",
                witnessB = "Пэйдж",
                description = "Коул утверждает что два пожара за 20 лет — это паттерн преступника. Пэйдж отмечает что журнал ненадёжен после 22:50 и данные не доказывают присутствие Холлоуэя."
            },
            new ContradictionData
            {
                witnessA = "Коул",
                witnessB = "Нэш",
                description = "Коул видит мотив в страховке и долгах Холлоуэя. Нэш указывает что Салас имел мотив — при осуждении партнёра его доля переходит автоматически."
            }
        };

        // ─── АРГУМЕНТЫ ───
        s.arguments = new ArgumentData[]
        {
            new ArgumentData { argumentId = "arg_1", text = "Два пожара за 20 лет — подозрительный паттерн", supportsGuilty = true, weight = 1, alwaysVisible = true },
            new ArgumentData { argumentId = "arg_2", text = "Страховка вдвое выше стоимости склада", supportsGuilty = true, weight = 2, alwaysVisible = true },
            new ArgumentData { argumentId = "arg_3", text = "Личный долг $180,000 — мотив для поджога", supportsGuilty = true, weight = 1, alwaysVisible = true },
            new ArgumentData { argumentId = "arg_4", text = "Холлоуэй присутствовал на складе в ночь пожара", supportsGuilty = true, weight = 1, alwaysVisible = true },
            new ArgumentData { argumentId = "arg_5", text = "TechSolv-7 зарегистрирован только на компанию Саласа", supportsGuilty = false, weight = 3, requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_firereport" },
            new ArgumentData { argumentId = "arg_6", text = "Салас подал заявление о расторжении за 2 дня до пожара", supportsGuilty = false, weight = 2, requiredChoiceType = ChoiceType.Testimony, requiredChoiceId = "Нэш" },
            new ArgumentData { argumentId = "arg_7", text = "Гомес звонил Саласу 7 раз за неделю — был информатором", supportsGuilty = false, weight = 2, requiredChoiceType = ChoiceType.Testimony, requiredChoiceId = "Нэш" },
            new ArgumentData { argumentId = "arg_8", text = "Журнал пропусков ненадёжен после перезагрузки сервера", supportsGuilty = false, weight = 1, requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_accesslog" },
            new ArgumentData { argumentId = "arg_9", text = "Холлоуэй платил шантажисту — скрывал прошлое", supportsGuilty = true, weight = 1, requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_bankstatement" },
            new ArgumentData { argumentId = "arg_10", text = "Бизнес стал прибыльным — нет смысла сжигать", supportsGuilty = false, weight = 2, alwaysVisible = true }
        };

        // Timeline — chronology with contradictions to find
        s.timelineEntries = new TimelineEntryData[]
        {
            new TimelineEntryData { entryId = "tl_1", time = "22:45", description = "Салас вошёл на склад", source = "Журнал пропусков", alwaysVisible = true },
            new TimelineEntryData { entryId = "tl_2", time = "~23:00", description = "Холлоуэй уехал со склада", source = "Сосед Холм", requiredChoiceType = ChoiceType.Contact, requiredChoiceId = "contact_holm" },
            new TimelineEntryData { entryId = "tl_3", time = "23:05", description = "Салас вышел со склада", source = "Журнал пропусков", alwaysVisible = true },
            new TimelineEntryData { entryId = "tl_4", time = "23:10", description = "Сервер перезагружен — данные после ненадёжны", source = "Экспертиза журнала", requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_accesslog" },
            new TimelineEntryData { entryId = "tl_5", time = "23:14", description = "Холлоуэй вошёл на склад (запись после перезагрузки)", source = "Журнал пропусков", alwaysVisible = true },
            new TimelineEntryData { entryId = "tl_6", time = "~22:30", description = "Тёмная машина подъехала к складу", source = "Сосед Холм", requiredChoiceType = ChoiceType.Contact, requiredChoiceId = "contact_holm" },
            new TimelineEntryData { entryId = "tl_7", time = "23:47", description = "Гомес вошёл на склад", source = "Журнал пропусков", alwaysVisible = true },
            new TimelineEntryData { entryId = "tl_8", time = "01:30", description = "Начало пожара", source = "Пожарная служба", alwaysVisible = true },
            new TimelineEntryData { entryId = "tl_9", time = "~23:00", description = "Холлоуэй был дома, разговаривал с женой", source = "Линда Холлоуэй", requiredChoiceType = ChoiceType.Contact, requiredChoiceId = "contact_linda" }
        };

        s.timelineContradictions = new TimelineContradictionData[]
        {
            new TimelineContradictionData
            {
                entryA = "tl_2", entryB = "tl_5",
                explanation = "Сосед видел как Холлоуэй уехал около 23:00. Но журнал фиксирует вход Холлоуэя в 23:14. Либо он вернулся, либо запись после перезагрузки ненадёжна."
            },
            new TimelineContradictionData
            {
                entryA = "tl_4", entryB = "tl_5",
                explanation = "Сервер перезагружен в 23:10 — все записи после этого времени могут быть некорректны. Вход Холлоуэя в 23:14 технически недостоверен."
            },
            new TimelineContradictionData
            {
                entryA = "tl_9", entryB = "tl_5",
                explanation = "Линда утверждает что Холлоуэй был дома около 23:00. Журнал показывает его вход на склад в 23:14. Показания жены — заинтересованного свидетеля."
            }
        };
        s.maxContradictionAttempts = 5;

        // Connection board
        s.connectionCards = new ConnectionCardData[]
        {
            new ConnectionCardData { cardId = "card_holloway", label = "Холлоуэй", type = ConnectionCardData.CardType.Person, alwaysVisible = true },
            new ConnectionCardData { cardId = "card_salas", label = "Салас", type = ConnectionCardData.CardType.Person, alwaysVisible = true },
            new ConnectionCardData { cardId = "card_gomez", label = "Гомес", type = ConnectionCardData.CardType.Person, alwaysVisible = true },
            new ConnectionCardData { cardId = "card_reid", label = "Рид", type = ConnectionCardData.CardType.Person, requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_bankstatement" },
            new ConnectionCardData { cardId = "card_techsolv", label = "TechSolv-7", type = ConnectionCardData.CardType.Item, requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_firereport" },
            new ConnectionCardData { cardId = "card_greentech", label = "GreenTech", type = ConnectionCardData.CardType.Item, alwaysVisible = true },
            new ConnectionCardData { cardId = "card_insurance", label = "Страховка $2.4M", type = ConnectionCardData.CardType.Item, alwaysVisible = true },
            new ConnectionCardData { cardId = "card_fire2004", label = "Пожар 2004", type = ConnectionCardData.CardType.Event, alwaysVisible = true },
            new ConnectionCardData { cardId = "card_fire", label = "Пожар склада", type = ConnectionCardData.CardType.Event, alwaysVisible = true },
            new ConnectionCardData { cardId = "card_dissolution", label = "Расторжение", type = ConnectionCardData.CardType.Event, requiredChoiceType = ChoiceType.Testimony, requiredChoiceId = "Нэш" }
        };

        s.connections = new ConnectionData[]
        {
            new ConnectionData { cardA = "card_salas", cardB = "card_techsolv", revealText = "Салас — единственный зарегистрированный покупатель TechSolv-7. Холлоуэй в реестре не значится." },
            new ConnectionData { cardA = "card_reid", cardB = "card_greentech", revealText = "Карл Рид — директор номинальной компании GreenTech Disposal Services." },
            new ConnectionData { cardA = "card_reid", cardB = "card_salas", revealText = "Рид ранее работал юристом в компании Саласа. Связь может быть не случайной." },
            new ConnectionData { cardA = "card_gomez", cardB = "card_salas", revealText = "Гомес звонил Саласу 7 раз за неделю до пожара. Был информатором — знал расписание склада." },
            new ConnectionData { cardA = "card_salas", cardB = "card_insurance", revealText = "При осуждении партнёра его доля автоматически переходит другому. Салас получает всё." },
            new ConnectionData { cardA = "card_reid", cardB = "card_fire2004", revealText = "Рид нашёл черновик первичного осмотра пожара 2004 года и использовал для шантажа Холлоуэя." },
            new ConnectionData { cardA = "card_salas", cardB = "card_dissolution", revealText = "Салас подал заявление о принудительном расторжении за 2 дня до пожара." }
        };
        s.maxConnectionAttempts = 10;

        // ─── СРАВНЕНИЕ ДОКУМЕНТОВ ───
        s.documentCompare = new DocumentCompareData
        {
            leftTitle = "СТРАХОВОЙ ПОЛИС",
            rightTitle = "ОТЧЁТ НЕЗАВИСИМОЙ ОЦЕНКИ",
            lines = new DocumentLineData[]
            {
                new DocumentLineData { leftText = "Владелец: Марк Холлоуэй", rightText = "Объект: склад Холлоуэй-Салас", isDiscrepancy = false },
                new DocumentLineData { leftText = "Застрахованная сумма: $2,400,000", rightText = "Оценочная стоимость: $1,200,000", isDiscrepancy = true, revealFact = "Страховка вдвое выше реальной стоимости склада" },
                new DocumentLineData { leftText = "Дата оформления: март 2021", rightText = "Дата оценки: март 2021", isDiscrepancy = false },
                new DocumentLineData { leftText = "Инициатор: оба совладельца", rightText = "Заказчик оценки: В. Салас", isDiscrepancy = true, revealFact = "Переоценку заказал Салас, хотя в полисе указаны оба" },
                new DocumentLineData { leftText = "Тип покрытия: пожар, затопление", rightText = "Рекомендация: стандартное покрытие", isDiscrepancy = false },
                new DocumentLineData { leftText = "Бенефициар: Холлоуэй-Салас LLC", rightText = "Примечание: при осуждении совладельца — выплата второму", isDiscrepancy = true, revealFact = "При осуждении одного партнёра вся выплата идёт другому" }
            }
        };

        SaveAsset(s, "suspect_01_holloway");
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
