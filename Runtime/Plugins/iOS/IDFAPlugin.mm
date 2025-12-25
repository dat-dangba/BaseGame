#import <Foundation/Foundation.h>
#import <AdSupport/AdSupport.h>
#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <UIKit/UIKit.h>

extern "C" {
    void RequestTrackingAndGetIDFA(const char* gameObjectName, const char* callbackMethod);
}

void RequestTrackingAndGetIDFA(const char* gameObjectName, const char* callbackMethod) {
    NSString *goName = [NSString stringWithUTF8String:gameObjectName];
    NSString *cbMethod = [NSString stringWithUTF8String:callbackMethod];
    
    #if TARGET_OS_SIMULATOR
    	// 🚨 Trên iOS Simulator không có IDFA → trả về giá trị giả để test
    	UnitySendMessage([goName UTF8String], [cbMethod UTF8String], "SIMULATOR_NO_IDFA");
    	return;
	#endif

    if (@available(iOS 14, *)) {
        [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
            dispatch_async(dispatch_get_main_queue(), ^{
                if (status == ATTrackingManagerAuthorizationStatusAuthorized) {
                    NSString *idfa = [[[ASIdentifierManager sharedManager] advertisingIdentifier] UUIDString];
                    UnitySendMessage([goName UTF8String], [cbMethod UTF8String], [idfa UTF8String]);
                } else {
                    UnitySendMessage([goName UTF8String], [cbMethod UTF8String], "00000000-0000-0000-0000-000000000000");
                }
            });
        }];
    } else {
        if ([[ASIdentifierManager sharedManager] isAdvertisingTrackingEnabled]) {
            NSString *idfa = [[[ASIdentifierManager sharedManager] advertisingIdentifier] UUIDString];
            UnitySendMessage([goName UTF8String], [cbMethod UTF8String], [idfa UTF8String]);
        } else {
            UnitySendMessage([goName UTF8String], [cbMethod UTF8String], "00000000-0000-0000-0000-000000000000");
        }
    }
}
