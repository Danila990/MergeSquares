mergeInto(LibraryManager.library, {
    GPFetchProductsExtern : function() {
        try {
            window.GamePush.gp.payments.fetchProducts().then(function (result) {
                if(result.products != null) {
                    console.log('[GamePushService][FetchProductsExtern] Got result:', result);
                    window.unityInstance.SendMessage('CloudService(Clone)', 'OnGPFetchProductsExtern', result.products);
                }
            })
            .catch(err => {
                console.log('[GamePushService][FetchProductsExtern] Error while fetchProducts:', err);
                window.unityInstance.SendMessage('CloudService(Clone)', 'OnGPFetchProductsExtern', '[]');
            });
        } catch (e) {
            console.error('[GamePushService][FetchProductsExtern] CRASH rate ', e.message);
            window.unityInstance.SendMessage('CloudService(Clone)', 'OnGPFetchProductsExtern', '[]');
        }
    },
    GPRateExtern : function() {
        try {
            window.GamePush.gp.app.requestReview().then(function (result) {
                try {
                    if(result.success != null) {
                        if (result.success) {
                            window.unityInstance.SendMessage('CloudService(Clone)', 'OnGPReviewSuccess', result.rating);
                            console.log('[GamePushService][RateExtern] success with rating: ', result.rating);
                        }
                    }
                    if(result.error != null){
                        window.unityInstance.SendMessage('CloudService(Clone)', 'OnGPReviewError', 'false');
                        console.log('[GamePushService][RateExtern] Error rate: ', result.error);
                    }
                } catch (e) {
                    window.unityInstance.SendMessage('CloudService(Clone)', 'OnGPReviewError', 'false');
                    console.error('[GamePushService][RateExtern] Crash on exec rate: ', e.message);
                }
            });
        } catch (e) {
            console.error('[GamePushService][RateExtern] CRASH rate ', e.message);
            window.unityInstance.SendMessage('CloudService(Clone)', 'OnGPReviewError', 'false');
        }
    },
    GPGetCanvasWidthExtern: function () {
        return canvas.getBoundingClientRect().width;
    },
    GPGetCanvasHeightExtern: function () {
        return canvas.getBoundingClientRect().height;
    },
    GPCanRequestReviewExtern: function () {
        return window.GamePush.gp.app.canRequestReview;
    },
    GPIsAlreadyReviewedExtern: function () {
        return window.GamePush.gp.app.isAlreadyReviewed;
    }
});
