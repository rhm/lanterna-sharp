/*
 * This file is part of lanterna (https://github.com/mabe02/lanterna).
 *
 * lanterna is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Copyright (C) 2010-2024 Martin Berglund
 */

using Lanterna.Core;
using Lanterna.Input;

namespace Lanterna.Terminal;

/// <summary>
/// Interface extending ITerminal that explicitly documents that implementations are IO-safe and won't throw IOExceptions
/// during normal operation. In C#, since we don't have checked exceptions like Java, this interface is mainly for
/// documentation purposes and to provide a cleaner API contract. Please note that ReadInput() may still throw exceptions
/// when interrupted or when encountering error conditions.
/// 
/// You can use this interface instead of ITerminal if you use an implementation that guarantees no IO exceptions
/// during normal operation, or if you wrap your terminal in an IOSafeTerminalAdapter.
/// </summary>
public interface IIOSafeTerminal : ITerminal
{
    // All methods inherit from ITerminal but with the explicit contract that they won't throw IOExceptions
    // under normal operating conditions. In C# this is mainly for documentation since we don't have checked exceptions.
    
    // Note: We could explicitly redeclare all methods here without the IOException contract like Java does,
    // but since C# doesn't have checked exceptions, the base interface methods already don't declare IOExceptions.
    // This interface serves primarily as a marker interface and documentation of the implementation's behavior.
}