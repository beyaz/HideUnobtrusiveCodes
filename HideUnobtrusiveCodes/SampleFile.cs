    Comment Begin
    /// line1
    /// line2
    /// line3
    /// line4
    Comment End
    
    Comment Begin
    /// line1
    /// line2
    /// line3
    /// line4
    Comment End
    
    Log-Trace Begin
        Logger.Log("1");
        Logger.Log("2");
        Debug.Assert("2");
        Trace.Info("2");
        Debug.Assert("2");
    Log-Trace End
    
    Region Begin
    #region x
    region body
    #endregion
    Region End
    
    
    ReplaceTextRange Begin
    -(Scope scope)-
    ReplaceTextRange End
    
    ReplaceTextRange Begin
    public static void Process(Scope scope)
    {}
    ReplaceTextRange End
    
    SummaryCommont Begin
    /// <summary>
    ///     Initializes a new instance of the <see cref="AdornmentTagger" /> class.
    /// </summary>
    SummaryCommont End
    
    ReplaceLineWithAnotherTextWhenLineContains Begin
    var returnObject = objectHelper.InitializeGenericResponse<string>("ttt");
    ReplaceLineWithAnotherTextWhenLineContains End
    
    BOA Call Begin
    var userNameResponse = functionCall();
    if(!userNameResponse.Success)
    {
        returnObject.Results.AddRange(userNameResponse.Results);
        return returnObject;
    }
    var userName = userNameResponse.Value.First();
    BOA Call End
    
    BOA Call Begin
    var userNameResponse = functionCall();
    if(!userNameResponse.Success)
    {
        returnObject.Results.AddRange(userNameResponse.Results);
        return returnObject;
    }
    var userName = userNameResponse.Value.First();
    
    userNameResponse = functionCall();
    if(!userNameResponse.Success)
    {
        returnObject.Results.AddRange(userNameResponse.Results);
        return returnObject;
    }
    var userName = userNameResponse.Value.First();
    
    var userNameResponse2 = functionCall();
    if(!userNameResponse2.Success)
    {
        returnObject.Results.AddRange(userNameResponse2.Results);
        return returnObject;
    }
    returnObject.Value = userNameResponse2.Value.Last();
    
    var userNameResponse2 = functionCall_1(A,B);
    if(!userNameResponse2.Success)
    {
        
        returnObject.Results.AddRange(userNameResponse2.Results);
        
        return returnObject;
    
    }
    
    returnObject.Value = userNameResponse2.Value.Last();
    
    var userNameResponse2 = functionCall();
    if(!userNameResponse2.Success)
    {
        returnObject.Results.AddRange(userNameResponse2.Results);
        return returnObject;
    }
    
    
    returnObject.Value = UserName = userNameResponse2.Value.Last();
    
    var userNameResponse2_1 = functionCall();
    if(!userNameResponse2_1.Success)
    {
        return returnObject.Add(userNameResponse2_1);
    }   
    
    returnObject.Value = UserName = userNameResponse2_1.Value.Last();
    
    var userNameResponse3 = functionCall3();
    if(!userNameResponse3.Success)
    {
        returnObject.Results.AddRange(userNameResponse3.Results);
        return returnObject;
    }
    
    
    returnObject.Value = UserName = userNameResponse3.Value.Last();
    
    AA(Scope scope, int i)
    
    -- Invalid
    userNameResponse3 = functionCall3();
    if(!userNameResponse2.Success)
    {
        returnObject.Results.AddRange(userNameResponse3.Results);
        return returnObject;
    }
    -- Invalid
    
    
    var userNameResponse3 = functionCall3();
    
    if(!userNameResponse3.Success)
    {
        returnObject.Results.AddRange(userNameResponse3.Results);
        
        return returnObject;
        
    }
    BOA Call End
    
    Scope Assignments Begin
    var a = scope.Get("Aloha");
    var b = scope.Get("Aloha");
    var c = scope.Get("Aloha");
    Scope Assignments End
    
    Begin Should see one empty line
    var pushResponse = PushPersonData(objectHelper, request, map);
    if (!pushResponse.Success)
    {
        returnObject.Results.AddRange(pushResponse.Results);
        return returnObject;
    }

    pushResponse = PushCustomerData(objectHelper, request, map);
    if (!pushResponse.Success)
    {
        returnObject.Results.AddRange(pushResponse.Results);
        return returnObject;
    }
    End