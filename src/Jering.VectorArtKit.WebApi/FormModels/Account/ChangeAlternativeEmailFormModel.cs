﻿using Jering.DataAnnotations;
using Jering.DynamicForms;
using Jering.VectorArtKit.WebApi.Controllers;
using Jering.VectorArtKit.WebApi.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.FormModels
{
    [DynamicForm(nameof(Strings.ErrorMessage_Form_Invalid), nameof(Strings.ButtonText_Submit), typeof(Strings))]
    public class ChangeAlternativeEmailFormModel
    {
        [ValidateRequired(nameof(Strings.ErrorMessage_Password_Required), typeof(Strings))]
        [DynamicControl("input", nameof(Strings.DisplayName_CurrentPassword), typeof(Strings), 0)]
        [DynamicControlProperty("type", "password")]
        public string Password { get; set; }

        [ValidateRequired(nameof(Strings.ErrorMessage_NewAlternativeEmail_Required), typeof(Strings))]
        [ValidateEmailAddress(nameof(Strings.ErrorMessage_Email_Invalid), typeof(Strings))]
        [AsyncValidate(nameof(Strings.ErrorMessage_Email_InUse), typeof(Strings), nameof(DynamicFormsController), nameof(DynamicFormsController.ValidateEmailNotInUse))]
        [DynamicControl("input", nameof(Strings.DisplayName_NewAlternativeEmail), typeof(Strings), 1)]
        [DynamicControlProperty("type", "email")]
        public string NewAlternativeEmail { get; set; }
    }
}
