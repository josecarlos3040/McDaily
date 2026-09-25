mergeInto(LibraryManager.library, {

    // =========================================================
    // INITIALIZE
    // =========================================================

    FirebaseWeb_Initialize: function(gameObjectNamePtr) {

        var gameObjectName =
            UTF8ToString(gameObjectNamePtr);

        try {

            if (typeof firebase === "undefined") {

                SendMessage(
                    gameObjectName,
                    "OnWebInitializeResult",
                    JSON.stringify({
                        success: false,
                        error: "firebase-not-loaded"
                    })
                );

                return;
            }


            if (!firebase.apps.length) {

                SendMessage(
                    gameObjectName,
                    "OnWebInitializeResult",
                    JSON.stringify({
                        success: false,
                        error: "firebase-not-initialized"
                    })
                );

                return;
            }


            console.log(
                "FirebaseWeb_Initialize: OK"
            );


            SendMessage(
                gameObjectName,
                "OnWebInitializeResult",
                JSON.stringify({
                    success: true
                })
            );

        }
        catch (error) {

            console.error(
                "FirebaseWeb_Initialize:",
                error
            );


            SendMessage(
                gameObjectName,
                "OnWebInitializeResult",
                JSON.stringify({
                    success: false,
                    error: error.message
                })
            );
        }
    },


    // =========================================================
    // LOGIN
    // =========================================================

    FirebaseWeb_Login: function(
        gameObjectNamePtr,
        emailPtr,
        passwordPtr
    ) {

        var gameObjectName =
            UTF8ToString(gameObjectNamePtr);

        var email =
            UTF8ToString(emailPtr);

        var password =
            UTF8ToString(passwordPtr);


        if (
            typeof firebase ===
            "undefined"
        ) {

            SendMessage(
                gameObjectName,
                "OnWebLoginResult",
                JSON.stringify({
                    success: false,
                    error: "firebase-not-loaded"
                })
            );

            return;
        }


        firebase
            .auth()
            .signInWithEmailAndPassword(
                email,
                password
            )
            .then(
                function(result) {

                    SendMessage(
                        gameObjectName,
                        "OnWebLoginResult",
                        JSON.stringify({
                            success: true,

                            uid:
                                result.user.uid,

                            email:
                                result.user.email
                        })
                    );

                }
            )
            .catch(
                function(error) {

                    console.error(
                        "Firebase login:",
                        error
                    );


                    SendMessage(
                        gameObjectName,
                        "OnWebLoginResult",
                        JSON.stringify({
                            success: false,

                            error:
                                error.code,

                            message:
                                error.message
                        })
                    );

                }
            );
    },


    // =========================================================
    // REGISTER
    // =========================================================

    FirebaseWeb_Register: function(
        gameObjectNamePtr,
        emailPtr,
        passwordPtr,
        usernamePtr
    ) {

        var gameObjectName =
            UTF8ToString(
                gameObjectNamePtr
            );


        var email =
            UTF8ToString(
                emailPtr
            );


        var password =
            UTF8ToString(
                passwordPtr
            );


        var username =
            UTF8ToString(
                usernamePtr
            );


        firebase
            .auth()
            .createUserWithEmailAndPassword(
                email,
                password
            )
            .then(
                function(result) {

                    var uid =
                        result.user.uid;


                    return firebase
                        .firestore()
                        .collection("users")
                        .doc(uid)
                        .set({
                            username:
                                username,

                            points:
                                0,

                            streak:
                                0
                        })
                        .then(
                            function() {

                                SendMessage(
                                    gameObjectName,
                                    "OnWebRegisterResult",
                                    JSON.stringify({
                                        success:
                                            true,

                                        uid:
                                            uid,

                                        email:
                                            email,

                                        username:
                                            username
                                    })
                                );

                            }
                        );

                }
            )
            .catch(
                function(error) {

                    console.error(
                        "Firebase registro:",
                        error
                    );


                    SendMessage(
                        gameObjectName,
                        "OnWebRegisterResult",
                        JSON.stringify({
                            success:
                                false,

                            error:
                                error.code,

                            message:
                                error.message
                        })
                    );

                }
            );
    },


    // =========================================================
    // LOGOUT
    // =========================================================

    FirebaseWeb_Logout: function(
        gameObjectNamePtr
    ) {

        var gameObjectName =
            UTF8ToString(
                gameObjectNamePtr
            );


        firebase
            .auth()
            .signOut()
            .then(
                function() {

                    SendMessage(
                        gameObjectName,
                        "OnWebLogoutResult",
                        JSON.stringify({
                            success:
                                true
                        })
                    );

                }
            )
            .catch(
                function(error) {

                    SendMessage(
                        gameObjectName,
                        "OnWebLogoutResult",
                        JSON.stringify({
                            success:
                                false,

                            error:
                                error.code,

                            message:
                                error.message
                        })
                    );

                }
            );
    },


    // =========================================================
    // GET USER INFO
    // =========================================================

    FirebaseWeb_GetUserInfo: function(
        gameObjectNamePtr
    ) {

        var gameObjectName =
            UTF8ToString(
                gameObjectNamePtr
            );


        function sendUserInfo(user) {

            if (!user) {

                console.error(
                    "Nenhum usuário está logado."
                );

                return;
            }


            firebase
                .firestore()
                .collection("users")
                .doc(user.uid)
                .onSnapshot(

                    function(snapshot) {

                        if (
                            !snapshot.exists
                        ) {

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
                                username:
                                    username,

                                streak:
                                    streak,

                                points:
                                    points
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
            firebase
                .auth()
                .currentUser;


        if (currentUser) {

            sendUserInfo(
                currentUser
            );

        }
        else {

            var unsubscribe =
                firebase
                    .auth()
                    .onAuthStateChanged(
                        function(user) {

                            unsubscribe();


                            if (user) {

                                sendUserInfo(
                                    user
                                );

                            }
                        }
                    );
        }
    },


    // =========================================================
    // GET PURCHASED REWARDS + PET MOOD
    // =========================================================

    FirebaseWeb_GetPurchasedRewards: function(
        gameObjectNamePtr
    ) {

        var gameObjectName =
            UTF8ToString(
                gameObjectNamePtr
            );


        function listenRewards(user) {

            if (!user) {

                SendMessage(
                    gameObjectName,
                    "OnWebPurchasedRewards",
                    JSON.stringify({
                        rewards: [],
                        lastQuestionnaireDate: ""
                    })
                );

                return;
            }


            firebase
                .firestore()
                .collection("users")
                .doc(user.uid)
                .onSnapshot(

                    function(snapshot) {

                        if (
                            !snapshot.exists
                        ) {

                            SendMessage(
                                gameObjectName,
                                "OnWebPurchasedRewards",
                                JSON.stringify({
                                    rewards: [],
                                    lastQuestionnaireDate: ""
                                })
                            );

                            return;
                        }


                        var data =
                            snapshot.data();


                        var purchasedRewards =
                            data.purchasedRewards ||
                            [];


                        var lastQuestionnaireDate =
                            data.lastQuestionnaireDate ||
                            "";


                        SendMessage(
                            gameObjectName,
                            "OnWebPurchasedRewards",
                            JSON.stringify({

                                rewards:
                                    purchasedRewards,

                                lastQuestionnaireDate:
                                    lastQuestionnaireDate
                            })
                        );

                    },

                    function(error) {

                        console.error(
                            "Erro ao buscar recompensas:",
                            error
                        );


                        SendMessage(
                            gameObjectName,
                            "OnWebPurchasedRewards",
                            JSON.stringify({
                                rewards: [],
                                lastQuestionnaireDate: ""
                            })
                        );

                    }
                );
        }


        var currentUser =
            firebase
                .auth()
                .currentUser;


        if (currentUser) {

            listenRewards(
                currentUser
            );

        }
        else {

            var unsubscribe =
                firebase
                    .auth()
                    .onAuthStateChanged(
                        function(user) {

                            unsubscribe();


                            listenRewards(
                                user
                            );

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
            UTF8ToString(
                rewardIdPtr
            );


        var gameObjectName =
            UTF8ToString(
                gameObjectNamePtr
            );


        var user =
            firebase
                .auth()
                .currentUser;


        if (!user) {

            SendMessage(
                gameObjectName,
                "OnWebBuyRewardResult",
                "not_logged"
            );

            return;
        }


        var userRef =
            firebase
                .firestore()
                .collection("users")
                .doc(user.uid);


        userRef
            .get()
            .then(
                function(snapshot) {

                    if (
                        !snapshot.exists
                    ) {

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
                        data.purchasedRewards ||
                        [];


                    if (
                        purchasedRewards.includes(
                            rewardId
                        )
                    ) {

                        SendMessage(
                            gameObjectName,
                            "OnWebBuyRewardResult",
                            "already_purchased"
                        );

                        return null;
                    }


                    if (
                        points <
                        price
                    ) {

                        SendMessage(
                            gameObjectName,
                            "OnWebBuyRewardResult",
                            "not_enough_points"
                        );

                        return null;
                    }


                    purchasedRewards.push(
                        rewardId
                    );


                    return userRef.update({

                        points:
                            points - price,

                        purchasedRewards:
                            purchasedRewards
                    });

                }
            )
            .then(
                function(result) {

                    if (
                        result === null
                    ) {
                        return;
                    }


                    SendMessage(
                        gameObjectName,
                        "OnWebBuyRewardResult",
                        "success"
                    );

                }
            )
            .catch(
                function(error) {

                    console.error(
                        "Erro ao comprar recompensa:",
                        error
                    );


                    SendMessage(
                        gameObjectName,
                        "OnWebBuyRewardResult",
                        error.code ||
                        "error"
                    );

                }
            );
    },


    // =========================================================
    // SAVE QUESTIONNAIRE
    // =========================================================

    FirebaseWeb_SaveQuestionnaire: function(
        answersJsonPtr,
        gameObjectNamePtr
    ) {

        var answersJson =
            UTF8ToString(
                answersJsonPtr
            );


        var gameObjectName =
            UTF8ToString(
                gameObjectNamePtr
            );


        var user =
            firebase
                .auth()
                .currentUser;


        if (!user) {

            SendMessage(
                gameObjectName,
                "OnWebSaveQuestionnaire",
                "not_logged"
            );

            return;
        }


        var today =
            new Date()
                .toISOString()
                .split("T")[0];


        var userRef =
            firebase
                .firestore()
                .collection("users")
                .doc(user.uid);


        var questionnaireRef =
            userRef
                .collection("questionnaires")
                .doc(today);


        userRef
            .get()
            .then(
                function(snapshot) {

                    var data =
                        snapshot.exists
                            ? snapshot.data()
                            : {};


                    var oldStreak =
                        data.streak || 0;


                    var lastDate =
                        data.lastQuestionnaireDate ||
                        "";


                    var currentStreak =
                        1;


                    if (lastDate) {

                        var last =
                            new Date(
                                lastDate
                            );


                        var now =
                            new Date(
                                today
                            );


                        var difference =
                            Math.floor(
                                (now - last) /
                                (
                                    1000 *
                                    60 *
                                    60 *
                                    24
                                )
                            );


                        if (
                            difference ===
                            1
                        ) {

                            currentStreak =
                                oldStreak +
                                1;

                        }
                        else if (
                            difference ===
                            0
                        ) {

                            currentStreak =
                                oldStreak;

                        }
                        else {

                            currentStreak =
                                1;

                        }
                    }


                    var pointsEarned =
                        Math.min(
                            20 +
                            (
                                currentStreak *
                                10
                            ),
                            100
                        );


                    var answers =
                        {};


                    try {

                        var parsed =
                            JSON.parse(
                                answersJson
                            );


                        if (
                            parsed.answers
                        ) {

                            parsed.answers.forEach(
                                function(answer) {

                                    answers[
                                        answer.key
                                    ] =
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


                    return questionnaireRef
                        .set({

                            answers:
                                answers,

                            pointsEarned:
                                pointsEarned,

                            createdAt:
                                firebase
                                    .firestore
                                    .FieldValue
                                    .serverTimestamp()
                        })
                        .then(
                            function() {

                                return userRef
                                    .update({

                                        points:
                                            firebase
                                                .firestore
                                                .FieldValue
                                                .increment(
                                                    pointsEarned
                                                ),

                                        streak:
                                            currentStreak,

                                        lastQuestionnaireDate:
                                            today
                                    });

                            }
                        );

                }
            )
            .then(
                function() {

                    SendMessage(
                        gameObjectName,
                        "OnWebSaveQuestionnaire",
                        "success"
                    );

                }
            )
            .catch(
                function(error) {

                    console.error(
                        "Erro ao salvar questionário:",
                        error
                    );


                    SendMessage(
                        gameObjectName,
                        "OnWebSaveQuestionnaire",
                        error.code ||
                        "error"
                    );

                }
            );
    },


    // =========================================================
    // HAS ANSWERED TODAY
    // =========================================================

    FirebaseWeb_HasAnsweredToday: function(
        gameObjectNamePtr
    ) {

        var gameObjectName =
            UTF8ToString(
                gameObjectNamePtr
            );


        var user =
            firebase
                .auth()
                .currentUser;


        if (!user) {

            SendMessage(
                gameObjectName,
                "OnWebHasAnsweredToday",
                "false"
            );

            return;
        }


        var today =
            new Date()
                .toISOString()
                .split("T")[0];


        firebase
            .firestore()
            .collection("users")
            .doc(user.uid)
            .collection("questionnaires")
            .doc(today)
            .get()
            .then(
                function(snapshot) {

                    SendMessage(
                        gameObjectName,
                        "OnWebHasAnsweredToday",
                        snapshot.exists
                            ? "true"
                            : "false"
                    );

                }
            )
            .catch(
                function(error) {

                    console.error(
                        "Erro ao verificar questionário:",
                        error
                    );


                    SendMessage(
                        gameObjectName,
                        "OnWebHasAnsweredToday",
                        "false"
                    );

                }
            );
    },


    // =========================================================
    // GET DAILY QUESTIONS
    // =========================================================

    FirebaseWeb_GetDailyQuestions: function(
        gameObjectNamePtr
    ) {

        var gameObjectName =
            UTF8ToString(
                gameObjectNamePtr
            );


        var today =
            new Date()
                .toISOString()
                .split("T")[0];


        firebase
            .firestore()
            .collection("dailyQuestionnaires")
            .doc(today)
            .get()
            .then(
                function(snapshot) {

                    if (
                        !snapshot.exists
                    ) {

                        SendMessage(
                            gameObjectName,
                            "OnWebDailyQuestions",
                            JSON.stringify({
                                questions:
                                    []
                            })
                        );

                        return;
                    }


                    var data =
                        snapshot.data();


                    var questions =
                        data.questions ||
                        [];


                    SendMessage(
                        gameObjectName,
                        "OnWebDailyQuestions",
                        JSON.stringify({

                            questions:
                                questions
                        })
                    );

                }
            )
            .catch(
                function(error) {

                    console.error(
                        "Erro ao buscar perguntas:",
                        error
                    );


                    SendMessage(
                        gameObjectName,
                        "OnWebDailyQuestions",
                        JSON.stringify({
                            questions:
                                []
                        })
                    );

                }
            );
    },


    // =========================================================
    // CREATE DAILY QUESTIONS
    // =========================================================

    FirebaseWeb_CreateDailyQuestions: function(
        questionsJsonPtr,
        gameObjectNamePtr
    ) {

        var questionsJson =
            UTF8ToString(
                questionsJsonPtr
            );


        var gameObjectName =
            UTF8ToString(
                gameObjectNamePtr
            );


        var today =
            new Date()
                .toISOString()
                .split("T")[0];


        var questions =
            [];


        try {

            var parsed =
                JSON.parse(
                    questionsJson
                );


            questions =
                parsed.questions ||
                [];

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


        firebase
            .firestore()
            .collection("dailyQuestionnaires")
            .doc(today)
            .set({

                questions:
                    questions,

                createdAt:
                    firebase
                        .firestore
                        .FieldValue
                        .serverTimestamp()
            })
            .then(
                function() {

                    SendMessage(
                        gameObjectName,
                        "OnWebCreateDailyQuestions",
                        "success"
                    );

                }
            )
            .catch(
                function(error) {

                    console.error(
                        "Erro ao criar perguntas:",
                        error
                    );


                    SendMessage(
                        gameObjectName,
                        "OnWebCreateDailyQuestions",
                        error.code ||
                        "error"
                    );

                }
            );
    },

    // =========================================================
    // ADD POINTS
    // =========================================================

    FirebaseWeb_AddPoints: function(
        points
    ) {

        var user =
            firebase
                .auth()
                .currentUser;


        if (!user) {

            console.error(
                "Nenhum usuário logado para adicionar pontos."
            );

            return;
        }


        var userRef =
            firebase
                .firestore()
                .collection("users")
                .doc(user.uid);


        userRef
            .update({

                points:
                    firebase
                        .firestore
                        .FieldValue
                        .increment(
                            points
                        )

            })
            .then(
                function() {

                    console.log(
                        "Pontos adicionados: +" +
                        points
                    );

                }
            )
            .catch(
                function(error) {

                    console.error(
                        "Erro ao adicionar pontos:",
                        error
                    );

                }
            );
    }

});
