﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PSModule.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PSModule.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Access Token has expired..
        /// </summary>
        internal static string AccessTokenExpired {
            get {
                return ResourceManager.GetString("AccessTokenExpired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Access Token is null..
        /// </summary>
        internal static string AccessTokenIsNull {
            get {
                return ResourceManager.GetString("AccessTokenIsNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Authentication Type is invalid..
        /// </summary>
        internal static string AuthTypeIsInvalid {
            get {
                return ResourceManager.GetString("AuthTypeIsInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid execution token for Mobile, should contain ClientID, SecretKey and TenantID..
        /// </summary>
        internal static string McInvalidToken {
            get {
                return ResourceManager.GetString("McInvalidToken", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Malformed Acees Key, invalid key: [{0}]..
        /// </summary>
        internal static string McMalformedTokenInvalidKey {
            get {
                return ResourceManager.GetString("McMalformedTokenInvalidKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Malformed Acees Key, token with invalid key-value pair: [{0}]..
        /// </summary>
        internal static string McMalformedTokenMissingKeyValuePair {
            get {
                return ResourceManager.GetString("McMalformedTokenMissingKeyValuePair", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid Access Key. The client ID is empty..
        /// </summary>
        internal static string McMissingClientId {
            get {
                return ResourceManager.GetString("McMissingClientId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Missing or invalid Access Key.
        /// </summary>
        internal static string McMissingOrInvalidAcessKey {
            get {
                return ResourceManager.GetString("McMissingOrInvalidAcessKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid Access Key. The tenant ID is empty or not a number..
        /// </summary>
        internal static string McMissingOrInvalidTenant {
            get {
                return ResourceManager.GetString("McMissingOrInvalidTenant", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid Access Key. The Secret Key is empty..
        /// </summary>
        internal static string McMissingSecretKey {
            get {
                return ResourceManager.GetString("McMissingSecretKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Missing username or clientId..
        /// </summary>
        internal static string MissingUsernameOrClientId {
            get {
                return ResourceManager.GetString("MissingUsernameOrClientId", resourceCulture);
            }
        }
    }
}
