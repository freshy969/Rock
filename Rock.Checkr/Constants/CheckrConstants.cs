﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Checkr.Constants
{
    /// <summary>
    /// This class holds Checkr settings.
    /// </summary>
    public static class CheckrConstants
    {
        public static readonly string CHECKR_IMAGE_URL = "~/Assets/Images/Checkr.svg";

        /// <summary>
        /// The URL where the Token for the account is retrieved
        /// </summary>
        public static readonly string CHECKR_TOKEN_URL = "https://api.checkr.com/oauth/tokens";

        /// <summary>
        /// The Typename prefix
        /// </summary>
        public static readonly string TYPENAME_PREFIX = "Checkr - ";

        /// <summary>
        /// The login URL
        /// </summary>
        public static readonly string LOGIN_URL = "https://api.checkr.com";

        /// <summary>
        /// The candidates URL
        /// </summary>
        public static readonly string CANDIDATES_URL = "https://api.checkr.com/v1/candidates";

        /// <summary>
        /// The invitations URL
        /// </summary>
        public static readonly string INVITATIONS_URL = "https://api.checkr.com/v1/invitations";

        /// <summary>
        /// The report URL
        /// </summary>
        public static readonly string REPORT_URL = "https://api.checkr.com/v1/reports";

        /// <summary>
        /// The packages URL
        /// </summary>
        public static readonly string PACKAGES_URL = "https://api.checkr.com/v1/packages";

        /// <summary>
        /// The document URL
        /// </summary>
        public static readonly string DOCUMENT_URL = "https://api.checkr.com/v1/documents";
    }
}