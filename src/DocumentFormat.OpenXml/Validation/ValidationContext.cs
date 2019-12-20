﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DocumentFormat.OpenXml.Framework;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation.Schema;
using System;
using System.Collections.Generic;

namespace DocumentFormat.OpenXml.Validation
{
    /// <summary>
    /// Validation context.
    /// </summary>
    internal class ValidationContext
    {
        public ValidationContext(FileFormatVersions version = FileFormatVersions.Office2007)
            : this(new ValidationSettings(version), new ValidationCache(version))
        {
        }

        public ValidationContext(ValidationContext other)
            : this(other.Settings, other.Cache)
        {
        }

        public ValidationContext(ValidationSettings settings, ValidationCache cache)
        {
            Cache = cache ?? throw new ArgumentNullException(nameof(cache));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Errors = new List<ValidationErrorInfo>();
            McContext = new MCContext(false);
        }

        public ValidationCache Cache { get; }

        public ValidationSettings Settings { get; }

        public List<ValidationErrorInfo> Errors { get; }

        /// <summary>
        /// Gets target file format.
        /// </summary>
        public FileFormatVersions FileFormat => Settings.FileFormat;

        public bool Valid => Errors.Count == 0;

        public bool IsCancelled => MaxNumberOfErrors > 0 && Errors.Count >= MaxNumberOfErrors;

        public void Clear() => Errors.Clear();

        internal StateManager State { get; } = new StateManager();

        internal IValidationContextEvents Events => State;

        /// <summary>
        /// Gets or sets the target OpenXmlPackage.
        /// </summary>
        internal OpenXmlPackage Package { get; set; }

        /// <summary>
        /// Gets or sets the target OpenXmlPart
        /// </summary>
        internal OpenXmlPart Part { get; set; }

        /// <summary>
        /// Gets or sets the target element.
        /// </summary>
        internal OpenXmlElement Element { get; set; }

        /// <summary>
        /// Gets or sets used to track MC context.
        /// </summary>
        internal MCContext McContext { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether collect ExpectedChildren or not.
        /// </summary>
        internal bool CollectExpectedChildren { get; set; }

        /// <summary>
        /// Get the first child of this.Element according to the MC Mode.
        /// </summary>
        /// <returns>The first child in the MC mode.</returns>
        internal OpenXmlElement GetFirstChildMc()
        {
            return Element.GetFirstChildMc(McContext, FileFormat);
        }

        /// <summary>
        /// Get the next child of this.Element according to the MC Mode.
        /// </summary>
        /// <param name="child">The child after which the next child going to be retrieved.</param>
        /// <returns>The next child after the specified child in the MC mode.</returns>
        internal OpenXmlElement GetNextChildMc(OpenXmlElement child)
        {
            return Element.GetNextChildMc(child, McContext, FileFormat);
        }

        /// <summary>
        /// Gets the maximum number of errors. A zero (0) value means no limitation.
        /// When the errors >= MaxNumberOfErrors, errors will not be recorded, and MaxNumberOfErrorsEvent will be fired.
        /// </summary>
        public int MaxNumberOfErrors => Settings.MaxNumberOfErrors;

        public ValidatorContext ToContext(OpenXmlSimpleType simple, ElementProperty<OpenXmlSimpleType> state, bool isAttribute)
        {
            return new ValidatorContext(simple, FileFormat, Part, Element, state, isAttribute, McContext, AddError);
        }

        public ParticleConstraint GetParticleConstraint() => Cache.GetConstraint(Element);

        public void AddError(ValidationErrorInfo error)
        {
            if (error != null && !IsCancelled)
            {
                Errors.Add(error);
            }
        }
    }
}
