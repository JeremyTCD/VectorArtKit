﻿using Jering.DataAnnotations;
using Jering.DynamicForms;
using Jering.VectorArtKit.WebApi.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.FormModels
{
    [DynamicForm(nameof(Strings.ErrorMessage_Form_Invalid), nameof(Strings.ButtonText_LogIn), typeof(Strings))]
    public class ForgotPasswordFormModel
    {
        [ValidateRequired(nameof(Strings.ErrorMessage_Email_Required), typeof(Strings))]
        [ValidateEmailAddress(nameof(Strings.ErrorMessage_Email_Invalid), typeof(Strings))]
        [DynamicControl("input", nameof(Strings.DisplayName_Email), typeof(Strings), 0)]
        [DynamicControlProperty("type", "email")]
        public string Email { get; set; }
    }
}
