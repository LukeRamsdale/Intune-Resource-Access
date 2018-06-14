﻿// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portionas of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace Microsoft.Management.Powershell.PFXImport
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Security;
    using IdentityModel.Clients.ActiveDirectory;

    public class Authenticate
    {
        public const string AuthURI = "login.windows-ppe.net";//"login.microsoftonline.com";
        public const string GraphURI = "https://graph.microsoft-ppe.com";// "https://graph.microsoft.com";
        public const string SchemaVersion = "test_Intune_OneDF";//"beta";

        public const string ClientId = "1950a258-227b-4e31-a9cf-717495945fc2";

        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Declaring it as a function helps to test a code path.")]
        [SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible", Justification = "Needs to be public and can't make functions consts")]
        public static Func<AuthenticationResult, bool> AuthTokenIsValid = (AuthRes) =>
        {
            if (AuthRes != null && AuthRes.AccessToken != null && AuthRes.ExpiresOn > DateTimeOffset.UtcNow)
            {
                return true;
            }

            return false;
        };

        private static Uri redirectUri = new Uri("urn:ietf:wg:oauth:2.0:oob");

        public static AuthenticationResult GetAuthToken(string user, SecureString password)
        {
            string authority = string.Format("https://{0}/common", AuthURI);
            AuthenticationContext authContext = new AuthenticationContext(authority);
            PlatformParameters platformParams = new PlatformParameters(PromptBehavior.Auto);

            if(password == null)
            {
                UserIdentifier userId = new UserIdentifier(user, UserIdentifierType.OptionalDisplayableId);
                return authContext.AcquireTokenAsync(GraphURI, ClientId, redirectUri, platformParams, userId).Result;
            }
            else
            {
                UserPasswordCredential userCreds = new UserPasswordCredential(user, password);
                return AuthenticationContextIntegratedAuthExtensions.AcquireTokenAsync(authContext, GraphURI, ClientId, userCreds).Result;
            }
        }
    }
}
