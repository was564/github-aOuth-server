using System;
using Microsoft.AspNetCore.Http;

namespace Was564.MarkdownEditor.Function
{
    public static class AuthUtils
    {
        public static string GetTokenFromRequest(HttpRequest req)
        {
            try
            {
                // req.Headers["Authorization"] => Bearer someSecretAccessToken
                var pureInput = req.Headers["Authorization"].ToString();
                return pureInput.Split(" ")[1];
            }
            catch
            {
                throw new ArgumentException();
            }
        }
    }
}

