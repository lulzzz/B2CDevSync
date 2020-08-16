﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace B2CDevSync.Models
{
    /// <summary>
    /// {
    ///     "@odata.context":"https://graph.microsoft.com/beta/applications",
    ///     "value":[{"id":"B2C_1A_PasswordReset"},{"id":"B2C_1A_ProfileEdit"},{"id":"B2C_1A_SignUpOrSignInAAD"},{"id":"B2C_1A_signup_signin"},{"id":"B2C_1A_TrustFrameworkBase"},{"id":"B2C_1A_TrustFrameworkExtensions"}]
    /// }
    /// </summary>
    public class AppList
    {
        [JsonProperty(PropertyName = "@odata.context")]
        public string Context { get; set; }

        [JsonProperty(PropertyName = "value")]
        public IEnumerable<App> Items { get; set; }
    }

    public class App
    {
        [JsonProperty(PropertyName = "api", NullValueHandling = NullValueHandling.Ignore)]
        public ApiApp Api { get; set; }

        [JsonProperty(PropertyName = "appId", NullValueHandling = NullValueHandling.Ignore)]
        public string AppId { get; set; }

        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "createdDateTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CreatedDateTime { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "identifierUris", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> IdentifierUris { get; set; }

        [JsonProperty(PropertyName = "isFallbackPublicClient")]
        public bool? IsFallbackPublicClient { get; set; }

        [JsonProperty(PropertyName = "publicClient")]
        public PublicClient PublicClient { get; set; }

        [JsonProperty(PropertyName = "keyCredentials", NullValueHandling = NullValueHandling.Ignore)]
        public List<KeyCredential> KeyCredentials { get; set; }

        [JsonProperty(PropertyName = "publisherDomain")]
        public string PublisherDomain { get; set; }

        [JsonProperty(PropertyName = "passwordCredentials", NullValueHandling = NullValueHandling.Ignore)]
        public List<PasswordCredential> PasswordCredentials { get; set; }

        [JsonProperty(PropertyName = "requiredResourceAccess", NullValueHandling = NullValueHandling.Ignore)]
        public List<RequiredResourceAccess> RequiredResourceAccess { get; set; }

        [JsonProperty(PropertyName = "tokenEncryptionKeyId", NullValueHandling = NullValueHandling.Ignore)]
        public string TokenEncryptionKeyId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "signInAudience", NullValueHandling = NullValueHandling.Ignore)]
        public Audiences SignInAudience { get; set; }

        [JsonProperty(PropertyName = "web", NullValueHandling = NullValueHandling.Ignore)]
        public AppWeb Web { get; set; }

        public App()
        {
            Web = new AppWeb();
            SignInAudience = new Audiences();
            RequiredResourceAccess = new List<RequiredResourceAccess>();
            PasswordCredentials = new List<PasswordCredential>();
            KeyCredentials = new List<KeyCredential>();
            IdentifierUris = new List<string>();
            PublicClient = new PublicClient();
        }
    }

    public class PublicClient
    {
        [JsonProperty(PropertyName = "redirectUris")]
        public List<string> RedirectUris { get; set; }

        public PublicClient()
        {
            RedirectUris = new List<string>();
        }
    }

    /// <summary>
    /// Well-known resource access IDs:
    ///   openid	        37f7f235-527c-4136-accd-4a02d197296e
    ///   email             64a6cdd6-aab1-4aaf-94b8-3cc8405e90d0
    ///   profile	        14dad69e-099b-42c9-810b-d002981feec1
    ///   offline_access	7427e0e9-2fba-42fe-b0c0-848c9e6a8182
    /// </summary>
    public class ResourceAccess
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }

    public class RequiredResourceAccess
    {
        [JsonProperty(PropertyName = "resourceAppId")]
        public string ResourceAppId { get; set; }

        [JsonProperty(PropertyName = "resourceAccess")]
        public List<ResourceAccess> ResourceAccess { get; set; }

        public RequiredResourceAccess()
        {
            ResourceAccess = new List<ResourceAccess>();
        }
    }

    /// <summary>
    /// https://docs.microsoft.com/en-us/graph/api/resources/apiapplication?view=graph-rest-beta
    /// </summary>
    public class ApiApp
    {
        [JsonProperty(PropertyName = "acceptMappedClaims", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AcceptMappedClaims { get; set; }

        [JsonProperty(PropertyName = "knownClientApplications")]
        public List<string> KnownClientApplications { get; set; }

        [JsonProperty(PropertyName = "oauth2PermissionScopes")]
        public List<OAuth2Permission> Oauth2PermissionScopes { get; set; }

        [JsonProperty(PropertyName = "preAuthorizedApplications")]
        public List<PreAuthedApp> PreAuthorizedApplications { get; set; }

        [JsonProperty(PropertyName = "requestedAccessTokenVersion", NullValueHandling = NullValueHandling.Ignore)]
        public int? RequestedAccessTokenVersion { get; set; }

        public ApiApp(int tokenVersion=2)
        {
            KnownClientApplications = new List<string>();
            PreAuthorizedApplications = new List<PreAuthedApp>();
            Oauth2PermissionScopes = new List<OAuth2Permission>();
            RequestedAccessTokenVersion = tokenVersion;
        }
    }

    public class PreAuthedApp
    {
        [JsonProperty(PropertyName = "appId")]
        public string AppId { get; set; }

        [JsonProperty(PropertyName = "permissionIds")]
        public List<string> PermissionIds { get; set; }

        public PreAuthedApp()
        {
            PermissionIds = new List<string>();
        }
    }
    public class PasswordCredential
    {
        [JsonProperty(PropertyName = "customKeyIdentifier")]
        public byte[] CustomKeyIdentifier { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "endDateTime")]
        public DateTimeOffset? EndDateTime { get; set; }

        [JsonProperty(PropertyName = "hint")]
        public string Hint { get; set; }

        [JsonProperty(PropertyName = "keyId")]
        public string KeyId { get; set; }

        [JsonProperty(PropertyName = "secretText")]
        public string SecretText { get; set; }

        [JsonProperty(PropertyName = "startDateTime")]
        public DateTimeOffset? StartDateTime { get; set; }
    }
    
    public class KeyCredential
    {
        [JsonProperty(PropertyName = "customKeyIdentifier")]
        public byte[] CustomKeyIdentifier { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "endDateTime")]
        public DateTimeOffset? EndDateTime { get; set; }

        [JsonProperty(PropertyName = "keyId")]
        public string KeyId { get; set; }

        [JsonProperty(PropertyName = "startDateTime")]
        public DateTimeOffset? StartDateTime { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "usage")]
        public string Usage { get; set; }

        [JsonProperty(PropertyName = "key")]
        public byte[] Key { get; set; }
    }

    public class ImplicitGrantSettings
    {
        [JsonProperty(PropertyName = "enableIdTokenIssuance")]
        public bool EnableIdTokenIssuance { get; set; }

        [JsonProperty(PropertyName = "enableAccessTokenIssuance")]
        public bool EnableAccessTokenIssuance { get; set; }
    }

    public class AppWeb
    {
        [JsonProperty(PropertyName = "redirectUris")]
        public List<string> RedirectUris { get; set; }

        [JsonProperty(PropertyName = "implicitGrantSettings")]
        public ImplicitGrantSettings ImplicitGrantSettings { get; set; }

        [JsonProperty(PropertyName = "homePageUrl")]
        public string HomePageUrl { get; set; }

        [JsonProperty(PropertyName = "logoutUrl")]
        public string LogoutUrl { get; set; }

        public AppWeb()
        {
            RedirectUris = new List<string>();
            ImplicitGrantSettings = new ImplicitGrantSettings();
            ImplicitGrantSettings.EnableAccessTokenIssuance = true;
        }
    }


    public enum Audiences
    {
        AzureADMyOrg,
        AzureADandPersonalMicrosoftAccount,
        AzureADMultipleOrgs
    }
}
