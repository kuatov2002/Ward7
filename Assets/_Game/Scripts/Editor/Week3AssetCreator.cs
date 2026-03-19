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
                expertDescription = "MAC-адрес устройства в логах не совпадает ни с одним зарегистрированным устройством Белова (ноутбук, рабочая станция). MAC совпадает с рабочей станцией Зотова — серийный номер подтверждён. Кто-то вошёл под учётной записью Белова с компьютера Зотова.",
                zones = new EvidenceZoneData[]
                {
                    new EvidenceZoneData { label = "Логин", detail = "Учётная запись belov_a использована в 23:47", isCritical = false },
                    new EvidenceZoneData { label = "IP адрес", detail = "Внутренний VPN: 192.168.1.47", isCritical = false },
                    new EvidenceZoneData { label = "MAC адрес", detail = "MAC не совпадает с ноутбуком Белова — совпадает с рабочей станцией Зотова", isCritical = true },
                    new EvidenceZoneData { label = "Объём", detail = "12,247 записей скачаны за 15 минут — требуется прямой доступ к серверу", isCritical = false },
                    new EvidenceZoneData { label = "Время", detail = "Доступ в 23:47 — камера показывает что Белов уехал в 22:30", isCritical = true },
                    new EvidenceZoneData { label = "Админ лог", detail = "Пароль belov_a сброшен администратором в 23:40 — за 7 минут до доступа", isCritical = true }
                },
                maxInspections = 4 },
            new EvidenceData { evidenceId = "e3_usb", title = "USB-накопитель",
                baseDescription = "Чёрный USB-накопитель без маркировки, обнаружен в верхнем ящике стола Белова. Содержит полную копию базы данных пациентов. Дата записи файлов — 02:15 ночи.",
                expertDescription = "Файлы записаны в 02:15. Камера парковки клиники показывает: машина Белова покинула парковку в 22:30. Зотов вышел из здания в 03:00. Белов физически не мог записать файлы в 02:15 — его не было в здании.",
                zones = new EvidenceZoneData[]
                {
                    new EvidenceZoneData { label = "Устройство", detail = "USB 32GB, чёрный, без маркировки — не совпадает с личной флешкой Белова (синяя)", isCritical = true },
                    new EvidenceZoneData { label = "Содержимое", detail = "Копия базы данных пациентов — 12,247 записей", isCritical = false },
                    new EvidenceZoneData { label = "Время записи", detail = "Файлы записаны в 02:15 — Белов уехал в 22:30 по камерам", isCritical = true },
                    new EvidenceZoneData { label = "Камера", detail = "Камера парковки: машина Белова уехала 22:30, Зотов — 03:00", isCritical = true },
                    new EvidenceZoneData { label = "Место", detail = "Найден в незапертом ящике стола Белова", isCritical = false },
                    new EvidenceZoneData { label = "Отпечатки", detail = "Отпечатки Белова НЕ найдены на USB — протёрт", isCritical = true }
                },
                maxInspections = 4 },
            new EvidenceData { evidenceId = "e3_emails", title = "Переписка с «Гарант-Плюс»",
                baseDescription = "Анонимная переписка о продаже медицинских данных. Стиль — технический, профессиональный. Автор разбирается в структуре базы данных и ценности медицинских записей для страховых компаний.",
                expertDescription = "Лингвистический анализ: автор использует термины из внутренних отчётов IT-отдела — «миграция кластеров», «индексация по ICD-10». Белов в своей рабочей переписке этих терминов не использует — это лексикон Зотова. Метаданные писем указывают на браузер Firefox 118 под Linux — Белов работает на Windows, Зотов — единственный пользователь Linux в отделе.",
                zones = new EvidenceZoneData[]
                {
                    new EvidenceZoneData { label = "Отправитель", detail = "Анонимный аккаунт через зашифрованную почту", isCritical = false },
                    new EvidenceZoneData { label = "Получатель", detail = "Страховая компания Гарант-Плюс", isCritical = false },
                    new EvidenceZoneData { label = "Стиль", detail = "Технический жаргон — похож на Белова, но термины из отчётов Зотова", isCritical = true },
                    new EvidenceZoneData { label = "Метаданные", detail = "Браузер: Firefox 98.0 — версия установлена только на компьютере Зотова", isCritical = true },
                    new EvidenceZoneData { label = "Время", detail = "Отправлено в 02:30 — Белов уехал в 22:30", isCritical = true },
                    new EvidenceZoneData { label = "Цена", detail = "Запрошено $75,000 за базу данных", isCritical = false }
                },
                maxInspections = 4 }
        };

        s.testimonies = new[]
        {
            new TestimonyData { witnessName = "Коул (старший следователь)",
                baseTestimony = "«Логин Белова, данные на его флешке, мотив — недоволен зарплатой. Классический инсайдер. Плюс ипотека. Деньги от продажи данных решили бы его финансовые проблемы.»",
                clarification = "«Но я поднял финансы Белова. Никаких подозрительных поступлений — ни на его счета, ни на счета жены. За последний год — только зарплата и детские. Если он продал данные — где деньги? Криптовалютных кошельков не обнаружено.»",
                clarificationLines = new TestimonyLineData[]
                {
                    new TestimonyLineData { text = "Поднял финансы Белова.", isLie = false },
                    new TestimonyLineData { text = "Нашёл подозрительный перевод на $10,000.", isLie = true, lieReason = "Никаких подозрительных поступлений на счетах Белова и жены не обнаружено" },
                    new TestimonyLineData { text = "Если он продал данные — где деньги?", isLie = false },
                    new TestimonyLineData { text = "Белов живёт скромно, без роскоши.", isLie = false },
                    new TestimonyLineData { text = "Финансовый мотив не подтверждается.", isLie = false }
                },
                startingTrust = 3 },
            new TestimonyData { witnessName = "Пэйдж (эксперт-аналитик)",
                baseTestimony = "«Технически — всё указывает на Белова. Его логин, время совпадает с его поздними задержками, USB в его столе. Цепочка улик последовательна. Но MAC-адрес в логах не проверялся повторно — стоит перепроверить.»",
                clarification = "«Перепроверила. MAC-адрес устройства не принадлежит ни одному компьютеру Белова. Совпадает с рабочей станцией Зотова. Как администратор, Зотов может сбросить любой пароль и войти под любой учётной записью. Это его штатная функция.»",
                clarificationLines = new TestimonyLineData[]
                {
                    new TestimonyLineData { text = "Обнаружила аномалию в логах.", isLie = false },
                    new TestimonyLineData { text = "MAC-адрес в логах принадлежит ноутбуку Белова.", isLie = true, lieReason = "MAC-адрес принадлежит рабочей станции Зотова, не Белова" },
                    new TestimonyLineData { text = "Кто-то использовал учётные данные Белова с другого устройства.", isLie = false },
                    new TestimonyLineData { text = "Администратор системы мог сбросить любой пароль.", isLie = false },
                    new TestimonyLineData { text = "Технически — вход мог быть совершён кем угодно с root-доступом.", isLie = false }
                },
                startingTrust = 3 },
            new TestimonyData { witnessName = "Нэш (полевой агент)",
                baseTestimony = "«Зотов сам обнаружил утечку и доложил. Обычно тот кто раскрывает — не виноват. Но Зотов имел полный административный доступ ко всем системам. Мог подменить логи, создать учётные записи, стереть следы. Финансовая ситуация Зотова — проверяю.»",
                clarification = "«Зотов две недели назад оформил кредит на $50 000 — 'ремонт дома'. Ни одного чека за стройматериалы. Деньги обналичены в течение трёх дней. Дом Зотова — съёмная квартира. Ремонтировать нечего.»",
                clarificationLines = new TestimonyLineData[]
                {
                    new TestimonyLineData { text = "Проверил финансы Зотова.", isLie = false },
                    new TestimonyLineData { text = "Зотов оформил кредит на $50,000 на ремонт дома.", isLie = false },
                    new TestimonyLineData { text = "Нашёл чеки за стройматериалы.", isLie = true, lieReason = "Ни одного чека за стройматериалы — деньги обналичены" },
                    new TestimonyLineData { text = "Зотов подал заявление первым — обычно невиновные так делают.", isLie = false },
                    new TestimonyLineData { text = "Но Зотов имел полный доступ и мог подменить логи.", isLie = false },
                    new TestimonyLineData { text = "Зотов отказался от полиграфа — это его право.", isLie = true, lieReason = "Отказ от полиграфа в совокупности с $50,000 кредитом — подозрительно" }
                },
                startingTrust = 3 }
        };

        s.tonedQuestions = new TonedQuestionData[]
        {
            new TonedQuestionData
            {
                topic = "Задержки на работе",
                questionPress = "Вы задерживались допоздна каждый день! Именно тогда данные были украдены! Объясните!",
                answerPress = "(Растерянно.) Зотов просил! Миграция серверов! Он давал мне задания после шести! Я не знал зачем!",
                pressurePress = 2,
                questionNeutral = "Почему вы задерживались допоздна в последний месяц?",
                answerNeutral = "Зотов попросил помочь с миграцией серверов. Давал мне задания после рабочего дня. Теперь я понимаю зачем — ему нужно было моё присутствие в логах.",
                pressureNeutral = 0,
                questionEmpathy = "Коллеги говорят вы работали сверхурочно. Это было добровольно?",
                answerEmpathy = "Нет. Зотов настаивал. Говорил что миграция серверов срочная, что только я могу помочь. Я не хотел конфликтовать с начальником. Теперь понимаю — ему нужно было чтобы я был там допоздна. Для алиби.",
                pressureEmpathy = -1
            },
            new TonedQuestionData
            {
                topic = "Пароль от базы данных",
                questionPress = "Только вы знали свой пароль! Никто другой не мог войти под вашим аккаунтом!",
                answerPress = "Зотов — администратор! Он может сбросить любой пароль! Это его работа! Проверьте логи сброса паролей!",
                pressurePress = 2,
                questionNeutral = "Кто ещё мог иметь доступ к вашей учётной записи?",
                answerNeutral = "Зотов — администратор. Он может сбросить любой пароль и войти под любым аккаунтом. Это его работа.",
                pressureNeutral = 0,
                questionEmpathy = "Я понимаю, что это выглядит плохо — ваш аккаунт. Но есть ли объяснение?",
                answerEmpathy = "Зотов имеет root-доступ ко всей системе. Он может войти под кем угодно. Я даже не менял пароль месяцами — зачем? Он всё равно мог его сбросить.",
                pressureEmpathy = -1
            },
            new TonedQuestionData
            {
                topic = "Флешка в столе",
                questionPress = "В вашем столе нашли USB с украденными данными! Как это объясните?!",
                answerPress = "(Растерянно.) Я не клал её туда! У меня есть своя флешка — синяя, с логотипом! Эта — чёрная, безымянная! Я её никогда не видел!",
                pressurePress = 2,
                questionNeutral = "Как USB-накопитель с данными оказался в вашем столе?",
                answerNeutral = "Я не знаю. Я не клал её туда. У меня есть своя флешка — синяя, с логотипом. Эта — чёрная, безымянная. Я её никогда не видел.",
                pressureNeutral = 0,
                questionEmpathy = "Флешка в столе — это серьёзная улика. Но вы выглядите удивлённым.",
                answerEmpathy = "Потому что я действительно удивлён. Кто-то подложил. У Зотова есть ключ от всех кабинетов — он начальник IT. Он мог зайти в любой момент когда меня не было.",
                pressureEmpathy = -1
            },
            new TonedQuestionData
            {
                topic = "Мотив",
                questionPress = "Вы были недовольны зарплатой! Зотов сам об этом сказал! Деньги — ваш мотив!",
                answerPress = "(Тихо, но твёрдо.) Я никогда не жаловался на зарплату Зотову. Он лжёт. Спросите коллег. Спросите жену. Я был доволен работой.",
                pressurePress = 2,
                questionNeutral = "Был ли у вас финансовый мотив для продажи данных?",
                answerNeutral = "Нет. Проверьте мои счета — никаких подозрительных поступлений. Ни на мой счёт, ни на счёт жены. Если я продал данные — где деньги?",
                pressureNeutral = 0,
                questionEmpathy = "Иногда люди попадают в сложные ситуации. Были ли у вас финансовые трудности?",
                answerEmpathy = "Мы с женой нормально живём. Не роскошно, но нормально. Я не продавал данные. А вот Зотов две недели назад оформил кредит на $50,000 — 'на ремонт дома'. Спросите его где деньги.",
                pressureEmpathy = -1
            }
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
            new ArgumentData { argumentId = "arg_1", text = "Доступ к базе осуществлён с учётной записи Белова", supportsGuilty = true, weight = 1, alwaysVisible = true },
            new ArgumentData { argumentId = "arg_2", text = "USB-накопитель с данными найден в столе Белова", supportsGuilty = true, weight = 1, alwaysVisible = true },
            new ArgumentData { argumentId = "arg_3", text = "Белов задерживался допоздна — имел возможность", supportsGuilty = true, weight = 1, requiredChoiceType = ChoiceType.Contact, requiredChoiceId = "contact_nina" },
            new ArgumentData { argumentId = "arg_4", text = "MAC-адрес в логах принадлежит рабочей станции Зотова", supportsGuilty = false, weight = 3, requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_serverlogs" },
            new ArgumentData { argumentId = "arg_5", text = "Файлы записаны в 02:15 — Белов уехал в 22:30", supportsGuilty = false, weight = 2, requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_usb" },
            new ArgumentData { argumentId = "arg_6", text = "У Белова нет подозрительных денежных поступлений", supportsGuilty = false, weight = 2, requiredChoiceType = ChoiceType.Testimony, requiredChoiceId = "Коул" },
            new ArgumentData { argumentId = "arg_7", text = "Зотов обналичил $50,000 без чеков за стройматериалы", supportsGuilty = false, weight = 2, requiredChoiceType = ChoiceType.Testimony, requiredChoiceId = "Нэш" },
            new ArgumentData { argumentId = "arg_8", text = "Лингвистический анализ указывает на терминологию Зотова", supportsGuilty = false, weight = 2, requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_email" },
            new ArgumentData { argumentId = "arg_9", text = "Стиль переписки похож на Белова (технический жаргон)", supportsGuilty = true, weight = 1, requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_email" },
            new ArgumentData { argumentId = "arg_10", text = "Зотов имел полный доступ как администратор системы", supportsGuilty = false, weight = 2, requiredChoiceType = ChoiceType.Testimony, requiredChoiceId = "Пэйдж" }
        };

        // Timeline — chronology with contradictions
        s.timelineEntries = new TimelineEntryData[]
        {
            new TimelineEntryData { entryId = "tl_1", time = "21:00", description = "Белов задержался по просьбе Зотова (миграция серверов)", source = "Показания Белова", alwaysVisible = true },
            new TimelineEntryData { entryId = "tl_2", time = "22:30", description = "Машина Белова покинула парковку", source = "Камера парковки", requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_usb" },
            new TimelineEntryData { entryId = "tl_3", time = "23:47", description = "Доступ к базе данных с аккаунта Белова", source = "Серверные логи", alwaysVisible = true },
            new TimelineEntryData { entryId = "tl_4", time = "02:15", description = "Файлы записаны на USB-накопитель", source = "Экспертиза USB", requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_usb" },
            new TimelineEntryData { entryId = "tl_5", time = "02:30", description = "Письмо покупателю отправлено с анонимного аккаунта", source = "Экспертиза переписки", requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_email" },
            new TimelineEntryData { entryId = "tl_6", time = "03:00", description = "Зотов покинул здание", source = "Камера парковки", requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_usb" },
            new TimelineEntryData { entryId = "tl_7", time = "23:47", description = "MAC-адрес в логах принадлежит станции Зотова, не Белова", source = "Экспертиза логов", requiredChoiceType = ChoiceType.Evidence, requiredChoiceId = "evidence_serverlogs" },
            new TimelineEntryData { entryId = "tl_8", time = "09:00", description = "Зотов сообщил руководству об утечке", source = "Показания Зотова", alwaysVisible = true }
        };

        s.timelineContradictions = new TimelineContradictionData[]
        {
            new TimelineContradictionData
            {
                entryA = "tl_2", entryB = "tl_3",
                explanation = "Белов уехал в 22:30. Доступ к базе в 23:47 — через час с лишним после отъезда. Кто-то другой использовал его аккаунт."
            },
            new TimelineContradictionData
            {
                entryA = "tl_3", entryB = "tl_7",
                explanation = "Логи показывают аккаунт Белова, но MAC-адрес устройства принадлежит Зотову. Аккаунт использован с чужого компьютера."
            },
            new TimelineContradictionData
            {
                entryA = "tl_4", entryB = "tl_2",
                explanation = "USB записан в 02:15 в офисе. Белов уехал в 22:30. Он физически не мог записать файлы — его не было в здании."
            }
        };
        s.maxContradictionAttempts = 5;

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

        s.documentCompare = new DocumentCompareData
        {
            leftTitle = "ЗАЯВЛЕНИЕ ЗОТОВА",
            rightTitle = "СЕРВЕРНЫЕ ЛОГИ",
            lines = new DocumentLineData[]
            {
                new DocumentLineData { leftText = "Обнаружил утечку: 9:00 утра", rightText = "Первый алерт системы: 9:15 утра", isDiscrepancy = true, revealFact = "Зотов узнал об утечке ДО системного алерта — откуда?" },
                new DocumentLineData { leftText = "Подозреваемый: Артём Белов", rightText = "Учётная запись: belov_a (доступ 23:47)", isDiscrepancy = false },
                new DocumentLineData { leftText = "Метод: скачивание через USB", rightText = "USB подключён: 02:15", isDiscrepancy = true, revealFact = "Зотов пишет про USB, но в логах USB подключён через 3 часа после доступа" },
                new DocumentLineData { leftText = "Белов имел мотив: недовольство зарплатой", rightText = "HR записи: повышение Белова одобрено в прошлом месяце", isDiscrepancy = true, revealFact = "Белов получил повышение — нет мотива недовольства" },
                new DocumentLineData { leftText = "Объём данных: 12,000 записей", rightText = "Скачано: 12,247 записей", isDiscrepancy = false },
                new DocumentLineData { leftText = "Рекомендация: немедленное увольнение Белова", rightText = "IP доступа: внутренний VPN (192.168.1.47)", isDiscrepancy = false }
            }
        };

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
