﻿using Microsoft.AspNetCore.Mvc;

namespace Jering.VectorArtKit.WebApi.ResponseModels.Shared
{
    public class ValidateValueResponseModel : IErrorResponseModel
    {
        public bool Valid { get; set; }
        public SerializableError ModelState { get; set; }
        public bool ExpectedError { get; set; }
        public string ErrorMessage { get; set; }
    }
}
