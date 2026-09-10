mergeInto(LibraryManager.library, {

    // =========================================================
    // INITIALIZE FIREBASE
    // =========================================================

    FirebaseWeb_Initialize: function(
        apiKeyPtr,
        authDomainPtr,
        projectIdPtr,
        storageBucketPtr,
        messagingSenderIdPtr,
        appIdPtr
    ) {
        var apiKey = UTF8ToString(apiKeyPtr);
        var authDomain = UTF8ToString(authDomainPtr);
        var projectId = UTF8ToString(projectIdPtr);
        var storageBucket = UTF8ToString(storageBucketPtr);
        var messagingSenderId = UTF8ToString(messagingSenderIdPtr);
        var appId = UTF8ToString(appIdPtr);

        var config = {
            apiKey: apiKey,
            authDomain: authDomain,
            projectId: projectId,
            storageBucket: storageBucket,
            messagingSenderId: messagingSenderId,
            appId: appId
        };

        if (!firebase.apps.length) {
            firebase.initializeApp(config);
        }

        console.log("Firebase Web inicializado!");
    },


    // =========================================================
    // REGISTER
    // =========================================================

    FirebaseWeb_Register: function(
        usernamePtr,
        emailPtr,
        passwordPtr,
        gameObjectNamePtr
    ) {
        var username = UTF8ToString(usernamePtr);
        var email = UTF8ToString(emailPtr);
        var password = UTF8ToString(passwordPtr);
        var gameObjectName = UTF8ToString(gameObjectNamePtr);

        firebase.auth()
            .createUserWithEmailAndPassword(
                email,
                password
            )
            .then(function(userCredential) {

                var user = userCredential.user;

                return firebase.firestore()
                    .collection("users")
                    .doc(user.uid)
                    .set({
                        username: username,
                        points: 0,
                        streak: 0
                    });
            })
            .then(function() {

                SendMessage(
                    gameObjectName,
                    "OnWebRegisterResult",
                    "success"
                );

            })
            .catch(function(error) {

                console.error(
                    "Erro no registro:",
                    error
                );

                SendMessage(
                    gameObjectName,
                    "OnWebRegisterResult",
                    error.code || "error"
                );
            });
    },


    // =========================================================
    // LOGIN
    // =========================================================

    FirebaseWeb_Login: function(
        emailPtr,
        passwordPtr,
        gameObjectNamePtr,
        callbackMethodPtr
    ) {
        var email = UTF8ToString(emailPtr);
        var password = UTF8ToString(passwordPtr);
        var gameObjectName = UTF8ToString(gameObjectNamePtr);
        var callbackMethod = UTF8ToString(callbackMethodPtr);

        firebase.auth()
            .signInWithEmailAndPassword(
                email,
                password
            )
            .then(function(userCredential) {

                console.log(
                    "Login realizado!"
                );

                SendMessage(
                    gameObjectName,
                    callbackMethod,
                    "success"
                );

            })
            .catch(function(error) {

                console.error(
                    "Erro no login:",
                    error
                );

                SendMessage(
                    gameObjectName,
                    callbackMethod,
                    error.code || "error"
                );
            });
    },


    // =========================================================
    // LOGOUT
    // =========================================================

    FirebaseWeb_Logout: function() {

        firebase.auth()
            .signOut()
            .then(function() {

                console.log(
                    "Logout realizado!"
                );

            })
            .catch(function(error) {

                console.error(
                    "Erro no logout:",
                    error
                );
            });
    },


    // =========================================================
    // GET USER INFO
    // =========================================================

FirebaseWeb_GetUserInfo: function(
    gameObjectNamePtr
) {
    var gameObjectName =
        UTF8ToString(gameObjectNamePtr);

    function sendUserInfo(user) {

        if (!user) {

            console.error(
                "Nenhum usuário está logado."
            );

            return;
        }

        firebase.firestore()
            .collection("users")
            .doc(user.uid)
            .onSnapshot(
                function(snapshot) {

                    if (!snapshot.exists) {

                        console.warn(
                            "Documento do usuário não existe."
                        );

                        return;
                    }

                    var data =
                        snapshot.data();

                    var username =
                        data.username || "";

                    var streak =
                        data.streak || 0;

                    var points =
                        data.points || 0;

                    SendMessage(
                        gameObjectName,
                        "OnWebUserInfo",
                        JSON.stringify({
                            username: username,
                            streak: streak,
                            points: points
                        })
                    );

                },
                function(error) {

                    console.error(
                        "Erro no listener do usuário:",
                        error
                    );
                }
            );
    }

    var currentUser =
        firebase.auth().currentUser;

    if (currentUser) {

        sendUserInfo(currentUser);

    }
    else {

        var unsubscribe =
            firebase.auth().onAuthStateChanged(
                function(user) {

                    unsubscribe();

                    if (user) {
                        sendUserInfo(user);
                    }
                }
            );
    }
},

    // =========================================================
    // GET PURCHASED REWARDS
    // =========================================================

 FirebaseWeb_GetPurchasedRewards: function(
    gameObjectNamePtr
) {
    var gameObjectName =
        UTF8ToString(gameObjectNamePtr);

    function listenRewards(user) {

        if (!user) {

            console.error(
                "Nenhum usuário está logado."
            );

            SendMessage(
                gameObjectName,
                "OnWebPurchasedRewards",
                JSON.stringify({
                    rewards: []
                })
            );

            return;
        }

        firebase.firestore()
            .collection("users")
            .doc(user.uid)
            .onSnapshot(
                function(snapshot) {

                    if (!snapshot.exists) {

                        SendMessage(
                            gameObjectName,
                            "OnWebPurchasedRewards",
                            JSON.stringify({
                                rewards: []
                            })
                        );

                        return;
                    }

                    var data =
                        snapshot.data();

                    var purchasedRewards =
                        data.purchasedRewards || [];

                    SendMessage(
                        gameObjectName,
                        "OnWebPurchasedRewards",
                        JSON.stringify({
                            rewards: purchasedRewards
                        })
                    );

                },
                function(error) {

                    console.error(
                        "Erro no listener das recompensas:",
                        error
                    );

                    SendMessage(
                        gameObjectName,
                        "OnWebPurchasedRewards",
                        JSON.stringify({
                            rewards: []
                        })
                    );
                }
            );
    }

    var currentUser =
        firebase.auth().currentUser;

    if (currentUser) {

        listenRewards(currentUser);

    }
    else {

        var unsubscribe =
            firebase.auth().onAuthStateChanged(
                function(user) {

                    unsubscribe();

                    listenRewards(user);
                }
            );
    }
},


    // =========================================================
    // BUY REWARD
    // =========================================================

    FirebaseWeb_BuyReward: function(
        rewardIdPtr,
        price,
        gameObjectNamePtr
    ) {
        var rewardId =
            UTF8ToString(rewardIdPtr);

        var gameObjectName =
            UTF8ToString(gameObjectNamePtr);

        var user =
            firebase.auth().currentUser;

        if (!user) {

            console.error(
                "Nenhum usuário está logado."
            );

            SendMessage(
                gameObjectName,
                "OnWebBuyRewardResult",
                "not_logged"
            );

            return;
        }

        var userRef =
            firebase.firestore()
                .collection("users")
                .doc(user.uid);

        userRef.get()
            .then(function(snapshot) {

                if (!snapshot.exists) {

                    SendMessage(
                        gameObjectName,
                        "OnWebBuyRewardResult",
                        "user_not_found"
                    );

                    return null;
                }

                var data =
                    snapshot.data();

                var points =
                    data.points || 0;

                var purchasedRewards =
                    data.purchasedRewards || [];

                if (purchasedRewards.includes(rewardId)) {

                    SendMessage(
                        gameObjectName,
                        "OnWebBuyRewardResult",
                        "already_purchased"
                    );

                    return null;
                }

                if (points < price) {

                    SendMessage(
                        gameObjectName,
                        "OnWebBuyRewardResult",
                        "not_enough_points"
                    );

                    return null;
                }

                var newPoints =
                    points - price;

                purchasedRewards.push(rewardId);

                return userRef.update({
                    points: newPoints,
                    purchasedRewards: purchasedRewards
                });
            })
            .then(function(result) {

                if (result === null) {
                    return;
                }

                SendMessage(
                    gameObjectName,
                    "OnWebBuyRewardResult",
                    "success"
                );

            })
            .catch(function(error) {

                console.error(
                    "Erro ao comprar recompensa:",
                    error
                );

                SendMessage(
                    gameObjectName,
                    "OnWebBuyRewardResult",
                    error.code || "error"
                );
            });
    },


    // =========================================================
    // SAVE QUESTIONNAIRE
    // =========================================================

    FirebaseWeb_SaveQuestionnaire: function(
        answersJsonPtr,
        gameObjectNamePtr
    ) {
        var answersJson =
            UTF8ToString(answersJsonPtr);

        var gameObjectName =
            UTF8ToString(gameObjectNamePtr);

        var user =
            firebase.auth().currentUser;

        if (!user) {

            SendMessage(
                gameObjectName,
                "OnWebSaveQuestionnaire",
                "not_logged"
            );

            return;
        }

        var today =
            new Date().toISOString().split("T")[0];

        var userRef =
            firebase.firestore()
                .collection("users")
                .doc(user.uid);

        var questionnaireRef =
            userRef.collection("questionnaires")
                .doc(today);

        userRef.get()
            .then(function(snapshot) {

                var data =
                    snapshot.exists
                        ? snapshot.data()
                        : {};

                var oldStreak =
                    data.streak || 0;

                var lastDate =
                    data.lastQuestionnaireDate || "";

                var currentStreak = 1;

                if (lastDate) {

                    var last =
                        new Date(lastDate);

                    var now =
                        new Date(today);

                    var difference =
                        Math.floor(
                            (now - last) /
                            (1000 * 60 * 60 * 24)
                        );

                    if (difference === 1) {

                        currentStreak =
                            oldStreak + 1;

                    }
                    else if (difference === 0) {

                        currentStreak =
                            oldStreak;

                    }
                    else {

                        currentStreak = 1;
                    }
                }

                var pointsEarned =
                    Math.min(
                        20 +
                        (currentStreak * 10),
                        100
                    );

                var answers = {};

                try {

                    var parsed =
                        JSON.parse(answersJson);

                    if (parsed.answers) {

                        parsed.answers.forEach(
                            function(answer) {

                                answers[answer.key] =
                                    answer.value;
                            }
                        );
                    }

                }
                catch (error) {

                    console.error(
                        "Erro ao ler respostas:",
                        error
                    );
                }

                return questionnaireRef.set({
                    answers: answers,
                    pointsEarned: pointsEarned,
                    createdAt:
                        firebase.firestore.FieldValue
                            .serverTimestamp()
                })
                .then(function() {

                    return userRef.update({

                        points:
                            firebase.firestore.FieldValue
                                .increment(pointsEarned),

                        streak: currentStreak,

                        lastQuestionnaireDate:
                            today
                    });
                });
            })
            .then(function() {

                SendMessage(
                    gameObjectName,
                    "OnWebSaveQuestionnaire",
                    "success"
                );

            })
            .catch(function(error) {

                console.error(
                    "Erro ao salvar questionário:",
                    error
                );

                SendMessage(
                    gameObjectName,
                    "OnWebSaveQuestionnaire",
                    error.code || "error"
                );
            });
    },


    // =========================================================
    // HAS ANSWERED TODAY
    // =========================================================

    FirebaseWeb_HasAnsweredToday: function(
        gameObjectNamePtr
    ) {
        var gameObjectName =
            UTF8ToString(gameObjectNamePtr);

        var user =
            firebase.auth().currentUser;

        if (!user) {

            SendMessage(
                gameObjectName,
                "OnWebHasAnsweredToday",
                "false"
            );

            return;
        }

        var today =
            new Date().toISOString().split("T")[0];

        firebase.firestore()
            .collection("users")
            .doc(user.uid)
            .collection("questionnaires")
            .doc(today)
            .get()
            .then(function(snapshot) {

                SendMessage(
                    gameObjectName,
                    "OnWebHasAnsweredToday",
                    snapshot.exists
                        ? "true"
                        : "false"
                );

            })
            .catch(function(error) {

                console.error(
                    "Erro ao verificar questionário:",
                    error
                );

                SendMessage(
                    gameObjectName,
                    "OnWebHasAnsweredToday",
                    "false"
                );
            });
    },


    // =========================================================
    // GET DAILY QUESTIONS
    // =========================================================

    FirebaseWeb_GetDailyQuestions: function(
        gameObjectNamePtr
    ) {
        var gameObjectName =
            UTF8ToString(gameObjectNamePtr);

        var today =
            new Date().toISOString().split("T")[0];

        firebase.firestore()
            .collection("dailyQuestionnaires")
            .doc(today)
            .get()
            .then(function(snapshot) {

                if (!snapshot.exists) {

                    SendMessage(
                        gameObjectName,
                        "OnWebDailyQuestions",
                        JSON.stringify({
                            questions: []
                        })
                    );

                    return;
                }

                var data =
                    snapshot.data();

                var questions =
                    data.questions || [];

                SendMessage(
                    gameObjectName,
                    "OnWebDailyQuestions",
                    JSON.stringify({
                        questions: questions
                    })
                );

            })
            .catch(function(error) {

                console.error(
                    "Erro ao buscar perguntas:",
                    error
                );

                SendMessage(
                    gameObjectName,
                    "OnWebDailyQuestions",
                    JSON.stringify({
                        questions: []
                    })
                );
            });
    },


    // =========================================================
    // CREATE DAILY QUESTIONS
    // =========================================================

    FirebaseWeb_CreateDailyQuestions: function(
        questionsJsonPtr,
        gameObjectNamePtr
    ) {
        var questionsJson =
            UTF8ToString(questionsJsonPtr);

        var gameObjectName =
            UTF8ToString(gameObjectNamePtr);

        var today =
            new Date().toISOString().split("T")[0];

        var questions = [];

        try {

            var parsed =
                JSON.parse(questionsJson);

            questions =
                parsed.questions || [];

        }
        catch (error) {

            console.error(
                "Erro ao ler perguntas:",
                error
            );

            SendMessage(
                gameObjectName,
                "OnWebCreateDailyQuestions",
                "invalid_json"
            );

            return;
        }

        firebase.firestore()
            .collection("dailyQuestionnaires")
            .doc(today)
            .set({
                questions: questions,
                createdAt:
                    firebase.firestore.FieldValue
                        .serverTimestamp()
            })
            .then(function() {

                SendMessage(
                    gameObjectName,
                    "OnWebCreateDailyQuestions",
                    "success"
                );

            })
            .catch(function(error) {

                console.error(
                    "Erro ao criar perguntas:",
                    error
                );

                SendMessage(
                    gameObjectName,
                    "OnWebCreateDailyQuestions",
                    error.code || "error"
                );
            });
    }

});