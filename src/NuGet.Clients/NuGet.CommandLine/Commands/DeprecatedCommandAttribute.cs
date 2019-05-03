// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace NuGet.CommandLine
{
    /// <summary>
    /// Attribute for marking nuget.exe commands as deprecated.
    ///
    /// By using this attribute in ICommand-based classes, it will show a warning message each
    /// command invocation and in each help command invocation.
    /// 
    /// <see cref="ICommand"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DeprecatedCommandAttribute : Attribute
    {
        /// <summary>
        /// Placeholder for AlternativeCommand property
        /// </summary>
        /// <see cref="AlternativeCommand"/>
        private Type _alternativeCommand;

        /// <summary>
        /// The concrete Type that is an alternative to the command that has this DeprecatedCommand attribute.
        ///
        /// The Type must fully implement the ICommand interface
        /// 
        /// If no alternative is provided in the attribute, this property will be null
        /// </summary>
        /// <see cref="ICommand">
        public Type AlternativeCommand
        {
            get => _alternativeCommand;
            private set
            {
                if (!typeof(ICommand).IsAssignableFrom(value))
                {
                    var msg = string.Format("{0} must be a {1} that implements {2} interface",
                        nameof(AlternativeCommand),
                        nameof(Type),
                        typeof(ICommand).FullName);
                    throw new ArgumentException(msg);
                }
                _alternativeCommand = value;
            }
        }

        public DeprecatedCommandAttribute()
        {
            _alternativeCommand = null;
        }

        public DeprecatedCommandAttribute(Type AlternativeCommand)
        {
            this.AlternativeCommand = AlternativeCommand;
        }

        /// <summary>
        /// Generates the deprecation warning message
        /// </summary>
        /// <param name="currentCommand">name of the current command that is deprecated</param>
        /// <returns>The deprecation warning message</returns>
        public string GetDeprecationMessage(string currentCommand)
        {
            var binaryName = Assembly.GetExecutingAssembly().GetName().Name;

            if (AlternativeCommand == null)
            {
                var warningResource = LocalizedResourceManager.GetString("CommandDeprecationWarningSimple");

                return string.Format(warningResource, binaryName, currentCommand);
            }

            var cmdAttrAlternative = AlternativeCommand.GetCustomAttributes(typeof(CommandAttribute), false);
            if (cmdAttrAlternative.Length > 1)
            {
                throw new ArgumentException("Multiple CommandAttribute attributes is not allowed");
            }

            if (cmdAttrAlternative.Length == 1)
            {
                var cmdAlternative = cmdAttrAlternative[0] as CommandAttribute;

                var warningResourceAlternative = LocalizedResourceManager.GetString("CommandDeprecationWarningAlternative");

                return string.Format(warningResourceAlternative, binaryName, currentCommand, cmdAlternative.CommandName);
            }

            throw new ArgumentException("No CommandAttribute attribute found");
        }

        /// <summary>
        /// Obtains the string resource appending Deprecated word in help description
        /// </summary>
        /// <returns>A string with the deprecated word</returns>
        public string DeprecatedWord()
        {
            return LocalizedResourceManager.GetString("DeprecatedWord");
        }
    }
}