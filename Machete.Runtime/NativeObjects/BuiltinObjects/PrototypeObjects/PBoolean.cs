﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.RuntimeTypes.Interfaces;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PBoolean : LObject
    {
        internal PBoolean()
        {
            Class = "Boolean";
            Extensible = true;
            DefineOwnProperty("toString", SPropertyDescriptor.Create(new NFunction(null, () => ToString)), false);
            DefineOwnProperty("valueOf", SPropertyDescriptor.Create(new NFunction(null, () => ValueOf)), false);
        }


        private IDynamic ToString(ExecutionContext context, SList args)
        {
            return context.ThisBinding.ConvertToBoolean().ConvertToString();
        }

        private IDynamic ValueOf(ExecutionContext context, SList args)
        {
            return context.ThisBinding.ConvertToBoolean();
        }
    }
}

