﻿using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CReferenceError : LObject, IBuiltinFunction
    {
        public CReferenceError(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            Class = "Function";
            Extensible = true;
            Prototype = Environment.FunctionPrototype;
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(1.0), false, false, false), false);
            DefineOwnProperty("prototype", Environment.CreateDataDescriptor(Environment.ReferenceErrorPrototype, false, false, false), false);
            base.Initialize();
        }

        IDynamic ICallable.Call(IEnvironment environment, IDynamic thisBinding, IArgs args)
        {
            return ((IConstructable)this).Construct(environment, args);
        }

        IObject IConstructable.Construct(IEnvironment environment, IArgs args)
        {
            var message = args[0].ConvertToString();
            if (message.BaseValue == "undefined")
            {
                message = environment.CreateString("");
            }
            var error = new NReferenceError(environment);
            error.Class = "Error";
            error.Extensible = true;
            error.Prototype = environment.ReferenceErrorPrototype;
            error.Put("message", message, false);
            return error;
        }

        public bool HasInstance(IDynamic value)
        {
            return Environment.Instanceof(value, this);
        }
    }
}
