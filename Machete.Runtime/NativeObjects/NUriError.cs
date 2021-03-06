﻿using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NUriError : LObject
    {
        public NUriError(IEnvironment environment)
            : base(environment)
        {
            Class = "Error";
            Extensible = true;
        }
    }
}
