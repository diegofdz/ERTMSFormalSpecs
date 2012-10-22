// ------------------------------------------------------------------------------
// -- Copyright ERTMS Solutions
// -- Licensed under the EUPL V.1.1
// -- http://joinup.ec.europa.eu/software/page/eupl/licence-eupl
// --
// -- This file is part of ERTMSFormalSpec software and documentation
// --
// --  ERTMSFormalSpec is free software: you can redistribute it and/or modify
// --  it under the terms of the EUPL General Public License, v.1.1
// --
// -- ERTMSFormalSpec is distributed in the hope that it will be useful,
// -- but WITHOUT ANY WARRANTY; without even the implied warranty of
// -- MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// --
// ------------------------------------------------------------------------------
using System.Collections.Generic;

namespace DataDictionary.Functions.PredefinedFunctions
{
    /// <summary>
    /// Indicates whether a entry X is before another entry Y in a list
    /// </summary>
    public class Before : PredefinedFunction
    {
        /// <summary>
        /// The expected first entry
        /// </summary>
        public Parameter ExpectedFirst { get; private set; }

        /// <summary>
        /// The expected second entry
        /// </summary>
        public Parameter ExpectedSecond { get; private set; }

        /// <summary>
        /// The collection which contains the two entries
        /// </summary>
        public Parameter Collection { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Before(EFSSystem efsSystem)
            : base(efsSystem, "Before")
        {
            ExpectedFirst = (Parameter)Generated.acceptor.getFactory().createParameter();
            ExpectedFirst.Name = "ExpectedFirst";
            ExpectedFirst.Type = EFSSystem.AnyType;
            ExpectedFirst.setFather(this);
            FormalParameters.Add(ExpectedFirst);

            ExpectedSecond = (Parameter)Generated.acceptor.getFactory().createParameter();
            ExpectedSecond.Name = "ExpectedSecond";
            ExpectedSecond.Type = EFSSystem.AnyType;
            ExpectedSecond.setFather(this);
            FormalParameters.Add(ExpectedSecond);

            Collection = (Parameter)Generated.acceptor.getFactory().createParameter();
            Collection.Name = "Collection";
            Collection.Type = EFSSystem.GenericCollection;
            Collection.setFather(this);
            FormalParameters.Add(Collection);
        }

        /// <summary>
        /// The return type of the before function
        /// </summary>
        public override Types.Type ReturnType
        {
            get { return EFSSystem.BoolType; }
        }

        /// <summary>
        /// Provides the value of the function
        /// </summary>
        /// <param name="instance">the instance on which the function is evaluated</param>
        /// <param name="actuals">the actual parameters values</param>
        /// <param name="localScope">the values of local variables</param>
        /// <returns>The value for the function application</returns>
        public override Values.IValue Evaluate(Interpreter.InterpretationContext context, Dictionary<string, Values.IValue> actuals)
        {
            Values.IValue retVal = EFSSystem.BoolType.False;

            context.LocalScope.PushContext();
            AssignParameters(context, actuals);

            Values.ListValue collection = Collection.Value as Values.ListValue;
            if (collection != null)
            {
                if (ExpectedFirst.Value != null)
                {
                    int firstIndex = collection.Val.IndexOf(ExpectedFirst.Value);
                    if (firstIndex >= 0)
                    {
                        if (ExpectedSecond.Value != null)
                        {
                            int secondIndex = collection.Val.IndexOf(ExpectedSecond.Value);

                            if (secondIndex >= 0)
                            {
                                if (firstIndex < secondIndex)
                                {
                                    retVal = EFSSystem.BoolType.True;
                                }
                            }
                            else
                            {
                                Collection.AddError("Cannot find " + ExpectedSecond.Value.FullName + " in " + collection.ToString() + " to evaluate " + Name);
                            }
                        }
                        else
                        {
                            Collection.AddError("Cannot evaluate second element to evaluate " + Name);
                        }
                    }
                    else
                    {
                        Collection.AddError("Cannot find " + ExpectedFirst.Value.FullName + " in " + collection.ToString() + " to evaluate " + Name);
                    }
                }
                else
                {
                    Collection.AddError("Cannot evaluate first element to evaluate " + Name);
                }
            }
            context.LocalScope.PopContext();

            return retVal;
        }
    }
}
