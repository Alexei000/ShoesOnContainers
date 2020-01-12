using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace TokenServiceApi
{
    public class Config
    {
        //public static Dictionary<string, string> ClientUrls { get; private set; }
        public static Dictionary<string, string> GetUrls(IConfiguration configuration)
        {
            string mvcClient = configuration.GetValue<string>("MvcClient");
            string basketApiClient = configuration.GetValue<string>("BasketApiClient");
            string orderApiClient = configuration.GetValue<string>("OrderApiClient");
            Console.WriteLine("MvcClient configuration value " + mvcClient);
            Console.WriteLine("BasketApi configuration value " + basketApiClient);
            Console.WriteLine("OrderApi configuration value " + orderApiClient);

            Dictionary<string, string> urls = new Dictionary<string, string>();
            urls.Add("Mvc", mvcClient);
            urls.Add("BasketApi", basketApiClient);
            urls.Add("OrderApi", orderApiClient);
            return urls;
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("basket", "Shopping Cart Api"),
                new ApiResource("order", "Ordering Api"),
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        public static IEnumerable<Client> GetClients(Dictionary<string, string> clientUrls)
        {

            return new List<Client>()
            {
                new Client
                {
                    ClientId = "mvc",
                    ClientSecrets = new [] { new Secret("secret".Sha256())},
                    AllowedGrantTypes = GrantTypes.Hybrid,

                    RedirectUris = {$"{clientUrls["Mvc"]}/signin-oidc"},
                    PostLogoutRedirectUris = {$"{clientUrls["Mvc"]}/signout-callback-oidc"},
                    AllowAccessTokensViaBrowser = false,
                    AllowOfflineAccess = true,
                    RequireConsent = false,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                      //  IdentityServerConstants.StandardScopes.Email,
                        "orders",
                        "basket",
                    }
                },
                new Client
                {
                    ClientId = "basketswaggerui",
                    ClientName = "Basket Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris = { $"{clientUrls["BasketApi"]}/swagger/oauth2-redirect.html" },
                    PostLogoutRedirectUris = { $"{clientUrls["BasketApi"]}/swagger/" },

                    AllowedScopes = new List<string>
                    {
                        "basket"
                    }
                },
                new Client
                {
                    ClientId = "orderswaggerui",
                    ClientName = "Order Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris = { $"{clientUrls["OrderApi"]}/swagger/oauth2-redirect.html" },
                    PostLogoutRedirectUris = { $"{clientUrls["OrderApi"]}/swagger/" },

                    AllowedScopes = new List<string>
                    {
                        "order"
                    }
                }
            };
        }
    }
}