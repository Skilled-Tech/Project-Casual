handlers.ClearPlayer = function (args?: IClearPlayerArguments)
{
    if (args == null)
    {
        log.error("No Arguments Passed");
        return;
    }

    if (args.playfabID == null)
    {
        log.error("No PlayFabID Passed");
        return;
    }

    if (args.customID == null)
    {
        log.error("No Custom ID Passed");
        return;
    }

    try
    {
        var profile = server.GetPlayerProfile(
            {
                PlayFabId: args.playfabID,
                ProfileConstraints: {
                    ShowLinkedAccounts: true
                }
            },
        ).PlayerProfile;
    }
    catch (ex)
    {
        let error = ex as IPlayFabError;

        if (error.apiErrorInfo?.apiError?.errorMessage != null) log.error(error.apiErrorInfo?.apiError?.errorMessage);

        return;
    }

    if (profile == null)
    {
        log.error("No Player Profile Found");
        return;
    }

    if (profile.LinkedAccounts == null || profile.LinkedAccounts.length == 0)
    {
        log.error("No Linked Accounts Found For Player");
        return;
    }

    if (profile.LinkedAccounts.length > 1)
    {
        log.error("Cannot Clear Account with Multiple Linked Accounts");
        return;
    }

    var ID = profile.LinkedAccounts.find((x) => x.Platform == "Custom");

    if (ID == null)
    {
        log.error("Player Profile Has no Custom ID Link");
        return;
    }

    if (ID.PlatformUserId != args.customID)
    {
        log.error("Incorrect ID");
        return;
    }

    server.DeletePlayer(
        {
            PlayFabId: args.playfabID
        }
    );
}
interface IClearPlayerArguments
{
    playfabID?: string;
    customID?: string;
}