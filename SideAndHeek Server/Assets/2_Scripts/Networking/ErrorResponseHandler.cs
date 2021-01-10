using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ErrorResponseCode
{
    A
}

public class ErrorResponseHandler
{
    public static Dictionary<int, Action> errorResponseHandlers;

    public static void InitialiseErrorResponseData()
    {
        errorResponseHandlers = new Dictionary<int, Action>()
        {
            { (int)ErrorResponseCode.A, A }
        };
    }

    public static void HandleErrorResponse(ErrorResponseCode errorResponseCode)
    {
        errorResponseHandlers[(int)errorResponseCode]?.Invoke();
    }

    private static void A()
    {

    }
}
