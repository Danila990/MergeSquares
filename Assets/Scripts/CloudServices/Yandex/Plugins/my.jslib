mergeInto(LibraryManager.library, {
    GetLanguageExtern: function () {
        var lang = ysdk.environment.i18n.lang;
        var bufferSize = lengthBytesUTF8(lang) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(lang, buffer, bufferSize);
        return buffer;
    },

    SaveExtern: function (date) {
        try {
            var dateString = UTF8ToString(date);
            var myobj = JSON.parse(dateString);
            player.setData(myobj);
        } catch (e) {
            console.error('[YandexService][SaveExtern] CRASH Save Cloud: ', e.message);
        }
    },

    LoadExtern: function () {
        try {
            player.getData().then(_data => {
                myGameInstance.SendMessage(yandexServiceName, 'OnLoad', JSON.stringify(_data));
            }).catch(() => {
                console.error('[YandexService][LoadExtern] getData Error!');
                myGameInstance.SendMessage(yandexServiceName, 'OnLoad', '');
            });
        } catch (e) {
            console.error('[YandexService][LoadExtern] CRASH Load saves Cloud: ', e.message);
            myGameInstance.SendMessage(yandexServiceName, 'OnLoad', '');
        }
    },

    RateExtern: function () {
        try {
            ysdk.feedback.canReview().then(({ value, reason }) => {
                if (value) {
                    ysdk.feedback.requestReview().then(({feedbackSent}) => {
                        console.log('[YandexService][RateExtern] feedbackSent ', feedbackSent);
                        if (feedbackSent)
                            myGameInstance.SendMessage(yandexServiceName, 'OnReview', 'true');
                        else myGameInstance.SendMessage(yandexServiceName, 'OnReview', 'false');
                        window.focus();
                    })
                }
                else {
                    console.log('[YandexService][RateExtern] reviewCanShow = false', reason)
                    window.focus();
                }
            })
        } catch (e) {
            console.error('[YandexService][RateExtern] CRASH Review: ', e.message);
            window.focus();
        }
    },

    CanReviewExtern: function () {
        ysdk.feedback.canReview()
            .then(({value, reason}) => {
                console.log('[YandexService][CanReviewExtern] Got can review result: ' + value + ' ' + reason);
                if (value) myGameInstance.SendMessage(yandexServiceName, 'OnCanReview', 'true');
                else myGameInstance.SendMessage(yandexServiceName, 'OnCanReview', 'false');
            })
            .catch(err => {
                console.log('[YandexService][CanReviewExtern] Error while canReview:', err);
                myGameInstance.SendMessage(yandexServiceName, 'OnCanReview', 'false');
            })
    },

    ShowRewardAdExtern: function (id) {
        var idString = UTF8ToString(id);
        try {
            ysdk.adv.showRewardedVideo({
                callbacks: {
                    onOpen: () => {
                        console.log('[YandexService][ShowRewardAdExtern] Reward ad opened. Id: ' + idString);
                    },
                    onRewarded: () => {
                        console.log('[YandexService][ShowRewardAdExtern] Rewarded! Id: ' + idString);
                        myGameInstance.SendMessage(yandexServiceName, 'OnRewardAdFinished', idString + '.rewarded');
                        window.focus();
                    },
                    onClose: () => {
                        console.log('[YandexService][ShowRewardAdExtern] Reward ad closed. Id: ' + idString);
                        myGameInstance.SendMessage(yandexServiceName, 'OnRewardAdFinished', idString + '.closed');
                    },
                    onError: (e) => {
                        console.log('[YandexService][ShowRewardAdExtern] Error while open reward ad: Id: ' + idString, e);
                        myGameInstance.SendMessage(yandexServiceName, 'OnRewardAdFinished', idString + '.error');
                    }
                }
            });
        } catch (err) {
            console.error('[YandexService][ShowRewardAdExtern] CRASH Rewarded Video Ad Show: ', err.message);
        }
    },

    ShowFullscreenExtern: function () {
        try {
            window.ysdk.adv.showFullscreenAdv({
                callbacks: {
                    onOpen: () => {
                        console.log('[YandexService][ShowFullscreenExtern] Open Fullscreen Ad');
                    },
                    onClose: (wasShown) => {
                        if (wasShown) myGameInstance.SendMessage(yandexServiceName, 'OnFullscreenAdFinished', 'true');
                        else myGameInstance.SendMessage(yandexServiceName, 'OnFullscreenAdFinished', 'false');
                        window.focus();
                    },
                    onError: (error) => {
                        console.log('[YandexService][ShowFullscreenExtern] Error Fullscreen Ad', error);
                        myGameInstance.SendMessage(yandexServiceName, 'OnFullscreenAdFinished', 'false');
                        window.focus();
                    }
                }
            });
        } catch (e) {
            console.error('[YandexService][ShowFullscreenExtern] CRASH FullAd Show: ', e.message);
        }
    },

    CheckPurchaseExtern: function (id) {
        var idString = UTF8ToString(id);
        payments.getPurchases().then(purchases => {
            if (purchases.some(purchase => purchase.productID === idString)) {
                myGameInstance.SendMessage(yandexServiceName, 'OnPurchaseCheck', (idString + '.' + 1));
            }
            else{
                myGameInstance.SendMessage(yandexServiceName, 'OnPurchaseCheck', (idString + '.' + 0));
            }
        }).catch(err => {
            console.log('[YandexService][CheckPurchaseExtern] Error while check adsDisable:', err);
            myGameInstance.SendMessage(yandexServiceName, 'OnPurchaseCheck', (idString + '.' + 0));
        })
    },

    CheckAuthStateExtern: function () {
        return player.getMode() !== 'lite';
    },

    AuthExtern: function () {
        Auth();
    },

    InitSDKExtern: function () {
        InitSDK();
    },

    GameReadyExtern: function () {
        GameReady();
    },

    PurchaseExtern: function (id) {
        Purchase(UTF8ToString(id));
    },

    ConsumePurchaseExtern: function (id) {
        var idString = UTF8ToString(id);
        try {
            if (payments != null) {
                payments.getPurchases().then(purchases => {
                    for(i = 0; i < purchases.length; i++){
                        if (purchases[i].productID === idString)
                            payments.consumePurchase(purchases[i].purchaseToken);
                    }
                });
            }
            else console.log('[YandexService][ConsumePurchaseExtern] Delete Purchase: payments == null');
        } catch (e) {
            console.error('[YandexService][ConsumePurchaseExtern] CRASH Delete Purchase: ', e.message);
        }
    },

    GetPurchasesExtern: function () {
        GetPurchases();
    }
});

const metricaLibrary = {

    // Class definition.

    $yandexMetrica: {
        yandexMetricaSend: function (eventName, eventData) {
            const eventDataJson = eventData === '' ? undefined : JSON.parse(eventData);
            // console.log('[YandexService][yandexMetricaSend] Send counterid: ' + yandexMetricaCounterId + ' event: ' + eventName + ' data: ' + eventDataJson);
            ym(yandexMetricaCounterId, 'reachGoal', eventName, eventDataJson);
        },
    },

    // External C# calls.

    YandexMetricaSendExtern: function (eventNamePtr, eventDataPtr) {
        const eventName = UTF8ToString(eventNamePtr);
        const eventData = UTF8ToString(eventDataPtr);
        try {
            yandexMetrica.yandexMetricaSend(eventName, eventData);
            // console.log('[YandexService][YandexMetricaSendExtern] Send event: ' + eventName + ' data: ' + eventData);
        } catch (e) {
            console.error('Yandex Metrica send evnet error: ', e.message);
        }
    },
}

autoAddDeps(metricaLibrary, '$yandexMetrica');
mergeInto(LibraryManager.library, metricaLibrary);
