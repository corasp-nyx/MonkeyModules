/*
 *  Monkey Modules
 *  Copyright (c) 2026 corasp~nyx
 *
 *  Licensed under the MIT License; you may only use this file in compliance with it.
 */

using System;
using System.Collections.Generic;

#nullable enable
namespace corasp_nyx.MonkeyModules
{
    /// <summary>
    /// Modifiers are applied to an Attribute in order of these priorities.
    /// (A custom integer priority can also be given instead of using an enum value)
    /// </summary>
    internal enum ModifierPriority
    {
        baseValue = 0,
        preAdd = 1000,
        preMul = 2000,
        mainAdd = 3000,
        mainMul = 4000,
        postAdd = 5000,
        postMul = 6000,
        clamp = 7000
    } // (no implicit int conversions for enums ':( )
}
