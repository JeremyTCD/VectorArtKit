﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Specifies options for implementations of <see cref="IEmailSender"/> .
    /// </summary>
    public class EmailOptions
    {
        /// <summary>
        /// Organization email address.
        /// </summary>
        public string EmailAddress { get; set; } = "noreply@vectorartkit.com";

        /// <summary>
        /// Organization name.
        /// </summary>
        public string Name { get; set; } = "VectorArtKit";

        /// <summary>
        /// Fully qualified domain name of target Smtp server.
        /// </summary>
        public string Host { get; set; } = "mail.jeringcommerce.com";

        /// <summary>
        /// Port that target Smtp server listens on.
        /// </summary>
        public int Port { get; set; } = 587;

        /// <summary>
        /// File to write emails to in development environment.
        /// </summary>
        public string DevelopmentFile { get; set; } = "temp/SmtpTest.txt";

        // Authentication details should be stored somewhere safe
        /// <summary>
        /// Smtp server credentials.
        /// </summary>
        public string Username { get; set; } = "username";

        /// <summary>
        /// Smtp server credentials.
        /// </summary>
        public string Password { get; set; } = "password";
    }
}
