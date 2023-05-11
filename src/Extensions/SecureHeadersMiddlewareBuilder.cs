﻿// Note:  some commments (especially those which explain what the different
//        parameters for each header) are taken from the OWASP Secure Headers
//        page. The original comments can be found at:
//                https://www.owasp.org/index.php/OWASP_Secure_Headers_Project
// Note:  the description of the Expect-CT header (used above the UseExpectCt
//        method) is taken from the MDN page for the header, which can be found
//        at the following url:
//          https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Expect-CT

using System;
using System.Collections.Generic;
using OwaspHeaders.Core.Enums;
using OwaspHeaders.Core.Helpers;
using OwaspHeaders.Core.Models;

namespace OwaspHeaders.Core.Extensions
{
    public static class SecureHeadersMiddlewareBuilder
    {
        private static SecureHeadersMiddlewareConfiguration Config
            => new SecureHeadersMiddlewareConfiguration();

        public static SecureHeadersMiddlewareConfiguration CreateBuilder()
        {
            return Config;
        }

        /// <summary>
        /// Includes the HTTP Strict Transport Security header in all responses
        /// generated by the application which consumes this middleware
        /// </summary>
        /// <param name="maxAge">
        /// The The time, in seconds, that the browser should remember that this
        /// site is only to be accessed using HTTPS
        /// </param>
        /// <param name="includeSubDomains">
        /// If this optional parameter is specified, this rule applies to all of
        /// the site's subdomains as well
        /// </param>
        /// <remarks>
        /// If no values for <param name="maxAge"/> or <param name="includeSubDomains"/>
        /// are provided, then default ones will be used. These default values will be
        /// based on the OWASP best practises values for HSTS.
        /// </remarks>
        public static SecureHeadersMiddlewareConfiguration UseHsts
            (this SecureHeadersMiddlewareConfiguration config,
                int maxAge = 63072000, bool includeSubDomains = true)
        {
            config.UseHsts = true;
            config.HstsConfiguration = new HstsConfiguration(maxAge, includeSubDomains);

            return config;
        }

        /// <summary>
        /// Declares a policy communicated from a host to the client browser on whether
        /// the browser must not display the transmitted content in frames of other web pages
        /// </summary>
        /// <param name="xFrameOption">
        /// Whether or not we should allow rendering this site within a frame.
        /// Applicable values are: deny; sameorigin; and allowfrom
        /// </param>
        /// <param name="domain">
        /// If allowfrom is supplied, this optional parameter describes the domain in which
        /// our site is permitted to be loaded within a frame 
        /// </param>
        /// <remarks>
        /// If no value for <param name="xFrameOption"/> is rovided, then default one will be
        /// used. This default value is based on the OWASP best practises value for X-Frame-Options.
        /// </remarks>
        public static SecureHeadersMiddlewareConfiguration UseXFrameOptions
            (this SecureHeadersMiddlewareConfiguration config,
                XFrameOptions xFrameOption = XFrameOptions.Deny,
                string domain = null)
        {
            config.UseXFrameOptions = true;
            config.XFrameOptionsConfiguration = new XFrameOptionsConfiguration(xFrameOption, domain);

            return config;
        }

        /// <summary>
        /// Enables the Cross Site Scripting protection filter in the client browser.
        /// </summary>
        /// <remarks>
        /// This overload will use the X-XSS-Protection value of 0. This effectively disables
        /// the XSS Auditor, and is required for modern browsers. Please ensure that you have
        /// a value Content-Security Policy enabled, otherwise you are opening yourself up to
        /// a world of trouble.
        /// The XSS Auditor needs to be disabled because it can lead to client-side security
        /// issues in modern browsers.
        /// </remarks>
        public static SecureHeadersMiddlewareConfiguration UseXssProtection
        (this SecureHeadersMiddlewareConfiguration config)
        {
            config.UseXssProtection = true;
            config.XssConfiguration = new XssConfiguration();
            return config;
        }

        /// <summary>
        /// Setting this header will prevent the browser from interpreting files as something
        /// else than declared by the content type in the HTTP headers
        /// </summary>
        /// <remarks>
        /// There is no value to pass in here, OWASP recommends that if you use this header
        /// (X-ContentType-Options), then the value of "nosniff" be used. "nosniff" is the default
        /// value for this header when using this middleware class.
        /// </remarks>
        public static SecureHeadersMiddlewareConfiguration UseContentTypeOptions
            (this SecureHeadersMiddlewareConfiguration config)
        {
            config.UseXContentTypeOptions = true;

            return config;
        }

        /// <summary>
        /// CSP prevents a wide range of attacks, including Cross-site scripting and other
        /// cross-site injections.
        /// </summary>
        /// <remarks>
        /// This method sets up a CSP header with:
        ///  - all mixed content blocked
        ///  - all insecure
        ///  - requests upgraded to HTTPS
        ///  - a ScriptSrc of "self"
        ///  - an ObjectSrc of "self"
        /// </remarks>
        public static SecureHeadersMiddlewareConfiguration UseContentDefaultSecurityPolicy
        (this SecureHeadersMiddlewareConfiguration config)
        {
            config.UseContentSecurityPolicy = true;

            config.ContentSecurityPolicyConfiguration = new ContentSecurityPolicyConfiguration
                (null, true, true, null, null);

            config.SetCspUris(
                new List<ContentSecurityPolicyElement> { ContentSecurityPolicyHelpers.CreateSelfDirective() },
                CspUriType.Script);

            config.SetCspUris(
                new List<ContentSecurityPolicyElement> { ContentSecurityPolicyHelpers.CreateSelfDirective() },
                CspUriType.Object);

            return config;
        }

        /// <summary>
        /// CSP prevents a wide range of attacks, including Cross-site scripting and other
        /// cross-site injections.
        /// </summary>
        /// <param name="pluginTypes">
        /// The set of plugins that can be invoked by the protected resource by limiting the
        /// types of resources that can be embedded
        /// </param>
        /// <param name="blockAllMixedContent">
        /// Prevent user agent from loading mixed content.
        /// </param>
        /// <param name="upgradeInsecureRequests">
        /// Instructs user agent to download insecure resources using HTTPS.
        /// </param>
        /// <param name="reportUri">
        /// Specifies a URI to which the user agent sends reports about policy violation.
        /// </param>
        /// <param name="useXContentSecurityPolicy">
        /// Specifies if we should use X-Content-Security-Policy header as well for compatibility with Internet Explorer.
        /// </param>
        /// <remarks>
        /// Requires consumer to set up their own Content Security Policy Rules via calls to
        /// SetCspUris, which is an extension method on the <see cref="SecureHeadersMiddlewareConfiguration"/> object
        /// </remarks>
        public static SecureHeadersMiddlewareConfiguration UseContentSecurityPolicy
            (this SecureHeadersMiddlewareConfiguration config,
                string pluginTypes = null, bool blockAllMixedContent = true,
                bool upgradeInsecureRequests = true, string referrer = null,
                string reportUri = null, bool useXContentSecurityPolicy = false)
        {
            config.UseContentSecurityPolicy = true;
            config.UseXContentSecurityPolicy = useXContentSecurityPolicy;

            config.ContentSecurityPolicyConfiguration = new ContentSecurityPolicyConfiguration
                (pluginTypes, blockAllMixedContent, upgradeInsecureRequests, referrer, reportUri);

            return config;
        }

        public static SecureHeadersMiddlewareConfiguration UseContentSecurityPolicyReportOnly
        (this SecureHeadersMiddlewareConfiguration config, string reportUri,
            string pluginTypes = null, bool blockAllMixedContent = true,
            bool upgradeInsecureRequests = true, string referrer = null, bool useXContentSecurityPolicy = false)
        {
            // Check whether the URI is valid before continuing
            if (!reportUri.IsValidHttpsUri())
            {
                ArgumentExceptionHelper.RaiseException(nameof(reportUri));
            }

            config.UseContentSecurityPolicyReportOnly = true;
            config.UseXContentSecurityPolicy = useXContentSecurityPolicy;

            config.ContentSecurityPolicyReportOnlyConfiguration = new ContentSecurityPolicyReportOnlyConfiguration
                (pluginTypes, blockAllMixedContent, upgradeInsecureRequests, referrer, reportUri);

            return config;
        }

        /// <summary>
        /// A cross-domain policy grants a web client permission to handle data across domains
        /// </summary>
        /// <remarks>
        /// If a <see cref="XPermittedCrossDomainOptionValue"/> is not supplied, then the default value of "none" will
        /// be used
        /// </remarks>
        public static SecureHeadersMiddlewareConfiguration UsePermittedCrossDomainPolicies
        (this SecureHeadersMiddlewareConfiguration config,
            XPermittedCrossDomainOptionValue xPermittedCrossDomainOptionValue =
                XPermittedCrossDomainOptionValue.none)
        {
            config.UsePermittedCrossDomainPolicy = true;

            config.PermittedCrossDomainPolicyConfiguration =
                new PermittedCrossDomainPolicyConfiguration(xPermittedCrossDomainOptionValue);

            return config;
        }

        /// <summary>
        /// Governs which referrer information, sent in the Referer header, should be included with requests made
        /// </summary>
        /// <remarks>
        /// If a <see cref="ReferrerPolicyOptions"/> value is not supplied, then the default value of "no-referrer"
        /// will be used.
        /// </remarks>
        public static SecureHeadersMiddlewareConfiguration UseReferrerPolicy
            (this SecureHeadersMiddlewareConfiguration config,
                ReferrerPolicyOptions referrerPolicyOption =
                    ReferrerPolicyOptions.noReferrer)
        {
            config.UseReferrerPolicy = true;

            config.ReferrerPolicy = new ReferrerPolicy(referrerPolicyOption);
            return config;
        }

        /// <summary>
        /// The server did not return or returned an invalid 'Cache-Control' header which means page
        /// containing sensitive information (password, credit card, personal data, social security 
        /// number, etc) could be stored on client side disk and then be exposed to unauthorised persons. 
        /// This URL is flagged as a specific example.
        /// </summary>
        /// <param name="private">
        /// [OPTIONAL]
        /// Whether all or part of the HTTP response message is intended for a single user and must 
        /// not be cached by a shared cache.
        /// </param>
        /// <param name="maxAge">
        /// [OPTIONAL]
        /// The maximum age, specified in seconds, that the HTTP client is willing to accept a response.
        /// </param>
        /// <exception cref="ArgumentException">
        /// An ArgumentException is thrown when no Report URI is supplied 
        /// </exception>
        public static SecureHeadersMiddlewareConfiguration UseCacheControl
        (this SecureHeadersMiddlewareConfiguration config,
            bool @private = true, int maxAge = 31536000, bool noCache = false, bool noStore = false,
            bool mustRevalidate = false)
        {
            config.UseCacheControl = true;
            config.CacheControl = new CacheControl(@private, maxAge, noCache, noStore, mustRevalidate);
            return config;
        }

        /// <summary>
        /// Governs whether the site can opt-into reporting or enforcement of certificate
        /// transparency requirements, which prevents the use of misissued certificates
        /// for that site from going unnoticed
        /// </summary>
        /// <param name="reportUri">
        /// [REQUIRED]
        /// Specifies the URI to which the user agent should report Expect-CT failures.
        /// </param>
        /// <param name="maxAge">
        /// [REQUIRED, HAS DEFAULT]
        /// Specifies the number of seconds after reception of the Expect-CT header field
        /// during which the user agent should regard the host from whom the message was
        /// received as a known Expect-CT host
        /// </param>
        /// <param name="enforce">
        /// [OPTIONAL]
        /// Signals to the user agent that compliance with the Certificate Transparency
        /// policy should be enforced (rather than only reporting compliance) and that the
        /// user agent should refuse future connections that violate its Certificate
        /// Transparency policy.
        /// </param>
        /// <exception cref="ArgumentException">
        /// An ArgumentException is thrown when no Report URI is supplied 
        /// </exception>
        public static SecureHeadersMiddlewareConfiguration UseExpectCt
            (this SecureHeadersMiddlewareConfiguration config,
                string reportUri, int maxAge = 86400, bool enforce = false)
        {
            config.UseExpectCt = true;
            config.ExpectCt = new ExpectCt(reportUri, maxAge, enforce);
            return config;
        }

        /// <summary>
        /// Thanks to Stack Overflow userrawb for this description:
        /// This is a common non-standard HTTP response header. It's often included by default in responses constructed via a particular scripting technology.
        /// Source: https://stackoverflow.com/a/33580769/1143474
        /// </summary>
        /// <remarks>
        /// A lot of web security experts recommend removing this header as it exposes the version
        /// of the server software. Malicious actors can target your application with attacks relevant
        /// to the version of the server software you are using. 
        /// </remarks>
        public static SecureHeadersMiddlewareConfiguration RemovePoweredByHeader
            (this SecureHeadersMiddlewareConfiguration config)
        {
            config.RemoveXPoweredByHeader = true;

            return config;
        }

        /// <summary>
        /// Return the completed <see cref="SecureHeadersMiddlewareConfiguration"/> ready for consumption by the
        /// <see cref="SecureHeadersMiddleware"/> class
        /// </summary>
        public static SecureHeadersMiddlewareConfiguration Build
            (this SecureHeadersMiddlewareConfiguration config)
        {
            return config;
        }
    }
}
