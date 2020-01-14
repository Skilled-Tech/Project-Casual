"use strict";
handlers.ClearPlayer = function (args) {
    var _a, _b, _c, _d;
    if (args == null) {
        log.error("No Arguments Passed");
        return;
    }
    if (args.playfabID == null) {
        log.error("No PlayFabID Passed");
        return;
    }
    if (args.customID == null) {
        log.error("No Custom ID Passed");
        return;
    }
    try {
        var profile = server.GetPlayerProfile({
            PlayFabId: args.playfabID,
            ProfileConstraints: {
                ShowLinkedAccounts: true
            }
        }).PlayerProfile;
    }
    catch (ex) {
        let error = ex;
        if (((_b = (_a = error.apiErrorInfo) === null || _a === void 0 ? void 0 : _a.apiError) === null || _b === void 0 ? void 0 : _b.errorMessage) != null)
            log.error((_d = (_c = error.apiErrorInfo) === null || _c === void 0 ? void 0 : _c.apiError) === null || _d === void 0 ? void 0 : _d.errorMessage);
        return;
    }
    if (profile == null) {
        log.error("No Player Profile Found");
        return;
    }
    if (profile.LinkedAccounts == null || profile.LinkedAccounts.length == 0) {
        log.error("No Linked Accounts Found For Player");
        return;
    }
    if (profile.LinkedAccounts.length > 1) {
        log.error("Cannot Clear Account with Multiple Linked Accounts");
        return;
    }
    var ID = profile.LinkedAccounts.find((x) => x.Platform == "Custom");
    if (ID == null) {
        log.error("Player Profile Has no Custom ID Link");
        return;
    }
    if (ID.PlatformUserId != args.customID) {
        log.error("Incorrect ID");
        return;
    }
    server.DeletePlayer({
        PlayFabId: args.playfabID
    });
};
var MyJSON;
(function (MyJSON) {
    MyJSON.IgnoreCharacter = '$';
    function Write(object) {
        function Replacer(key, value) {
            if (key[0] == MyJSON.IgnoreCharacter)
                return undefined;
            return value;
        }
        var json = JSON.stringify(object, Replacer);
        return json;
    }
    MyJSON.Write = Write;
    function Read(constructor, json) {
        var object = JSON.parse(json);
        var instance = new constructor(object);
        return instance;
    }
    MyJSON.Read = Read;
})(MyJSON || (MyJSON = {}));
var Utility;
(function (Utility) {
    let Dates;
    (function (Dates) {
        function DaysFrom(date) {
            return DaysBetween(date, new Date());
        }
        Dates.DaysFrom = DaysFrom;
        function DaysBetween(date1, date2) {
            return Math.round((date2.valueOf() - date1.valueOf()) / (86400000));
        }
        Dates.DaysBetween = DaysBetween;
    })(Dates = Utility.Dates || (Utility.Dates = {}));
    let Class;
    (function (Class) {
        function Assign(ctor, props) {
            return Object.assign(new ctor(), props);
        }
        Class.Assign = Assign;
        function WriteProperty(target, name, value) {
            let descriptor = {
                value: value,
                enumerable: true,
                writable: false,
            };
            Object.defineProperty(target, name, descriptor);
        }
        Class.WriteProperty = WriteProperty;
    })(Class = Utility.Class || (Utility.Class = {}));
})(Utility || (Utility = {}));
