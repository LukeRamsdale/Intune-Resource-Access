// Copyright (c) Microsoft Corporation.
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

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.Intune
{
    /// <summary>
    /// Azure Active Directory Authentication Client
    /// </summary>
    public class AdalClient
    {
        public const string DEFAULT_AUTHORITY = "https://login.microsoftonline.com/";

        private TraceSource trace = new TraceSource(nameof(AdalClient));

        /// <summary>
        /// The authority that we are requesting access from.
        /// </summary>
        private string authority = null;

        /// <summary>
        /// The credential to be used for authentication.
        /// </summary>
        private ClientCredential clientCredential = null;

        // Dependencies
        private IAuthenticationContext context = null;

        /// <summary>
        /// Constructor meant to be used for dependency injection of authContext
        /// </summary>
        /// <param name="configProperties">Configuration properties for this class.</param>
        /// <param name="aadTenant">Azure Active Directory Tenant</param>
        /// <param name="clientCredential">Client credential to use for authentication.</param>
        /// <param name="authAuthority">URL of Authorization Authority.</param>
        /// <param name="trace">Trace</param>
        /// <param name="authContext">Authorization Context to use to acquire token.</param>
        public AdalClient(Dictionary<string,string> configProperties, TraceSource trace = null, IAuthenticationContext authContext = null)
        {
            // Required Parameters
            if (configProperties == null)
            {
                throw new ArgumentNullException(nameof(configProperties));
            }

            string tenant = null;
            configProperties.TryGetValue("TENANT", out tenant);
            if (string.IsNullOrWhiteSpace(tenant))
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            string azureAppId = null;
            configProperties.TryGetValue("AAD_APP_ID", out azureAppId);
            if (string.IsNullOrWhiteSpace(azureAppId))
            {
                throw new ArgumentNullException(nameof(azureAppId));
            }

            string azureAppKey = null;
            configProperties.TryGetValue("AAD_APP_KEY", out azureAppKey);
            if (string.IsNullOrWhiteSpace(azureAppKey))
            {
                throw new ArgumentNullException(nameof(azureAppKey));
            }

            clientCredential = new ClientCredential(azureAppId, azureAppKey);

            // Optional Parameters
            if(trace != null)
            {
                this.trace = trace;
            }

            configProperties.TryGetValue("AUTH_AUTHORITY", out authority);
            if(authority == null)
            {
                authority = DEFAULT_AUTHORITY;
            }
            

            // Instantiate Dependencies
            context = authContext ?? new AuthenticationContextWrapper(new AuthenticationContext(authority + tenant, false));
        }

        /// <summary>
        /// Gets an access token from AAD for the specified resource using the ClientCredential passed in.
        /// </summary>
        /// <param name="clientCredential">Credential to use for authentication.</param>
        /// <param name="resource">Resource to get token for.</param>
        /// <returns></returns>
        public async Task<AuthenticationResult> AcquireTokenAsync(string resource)
        {
            if (string.IsNullOrWhiteSpace(resource))
            {
                throw new ArgumentNullException(nameof(resource));
            }

            AuthenticationResult result = await context.AcquireTokenAsync(resource, clientCredential);

            if (result == null)
            {
                throw new IntuneClientException("Authentication result was null");
            }

            return result;
        }
    }
}